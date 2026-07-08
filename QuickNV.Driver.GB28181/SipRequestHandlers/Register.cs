using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using System.Security.Cryptography;
using System.Text;
using YiQiDong.Agent;

namespace QuickNV.Driver.GB28181.SipRequestHandlers
{
    public class Register : ISipRequestHandler
    {
        private SipServer sipServer;

        public bool RequireRegisterBeforeExecute => false;

        public Register(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }

        public static string GetMD5(string str)
        {
            string retVal = null;
            using (var md5 = MD5.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                buffer = md5.ComputeHash(buffer);
                retVal = BitConverter.ToString(buffer).Replace("-", string.Empty).ToLower();
            }
            return retVal;
        }

        public static bool Authenticate(SIPRequest sipRequest, string password)
        {
            var sipDigest = sipRequest.Header.AuthenticationHeaders[0].SIPDigest;
            string ha1 = GetMD5($"{sipDigest.Username}:{sipDigest.Realm}:{password}");
            string ha2 = GetMD5($"REGISTER:{sipDigest.URI}");
            string ha3 = GetMD5($"{ha1}:{sipDigest.Nonce}:{ha2}");
            return ha3.Equals(sipDigest.Response);
        }

        public async Task Execute(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest, DeviceContext device)
        {
            //如果没有在头信息中没有找到Contact字段
            if (sipRequest.Header.Contact?.Count <= 0)
            {
                var message = $"在头信息中没有找到Contact字段";
                AgentContext.LogDebug($"从[SIP设备编号:{device.Model.Id},远程端点:{remoteEndPoint}]接收到注册请求。认证失败，原因: {message}");
                await sipServer.SendResponseAsync(sipRequest, SIPResponseStatusCodesEnum.BadRequest, message);
                return;
            }

            var headerContact = sipRequest.Header.Contact[0];
            //过期时间
            long expiry = Math.Max(headerContact.Expires, sipRequest.Header.Expires);

            //如果是注销设备
            if (expiry <= 0)
            {
                //设备注销
                var reason = $"接收到取消注册请求";
                AgentContext.LogDebug($"从[SIP设备编号:{device.Model.Id},远程端点:{remoteEndPoint}]{reason}");
                device.Unregister(reason);

                //发送200 OK响应
                await sipServer.SendResponseAsync(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
                return;
            }
            //如果是注册设备
            else
            {
                if (sipRequest.Header.AuthenticationHeaders.Count <= 0)
                {
                    SIPAuthenticationHeader authHeader =
                        new SIPAuthenticationHeader(SIPAuthorisationHeadersEnum.WWWAuthenticate,
                            sipServer.Options.SipRealm, SIPRequestAuthenticator.GetNonce());
                    var unAuthorisedHead =
                        new SIPRequestAuthenticationResult(SIPResponseStatusCodesEnum.Unauthorised, authHeader);
                    unAuthorisedHead.AuthenticationRequiredHeader.SIPDigest.Opaque = "";
                    authHeader.SIPDigest.DigestAlgorithm = DigestAlgorithmsEnum.MD5;
                    unAuthorisedHead.AuthenticationRequiredHeader.SIPDigest.DigestAlgorithm = DigestAlgorithmsEnum.MD5;

                    await sipServer.SendResponseAsync(sipRequest, SIPResponseStatusCodesEnum.Unauthorised, null, response =>
                    {
                        response.Header.AuthenticationHeaders.Add(unAuthorisedHead.AuthenticationRequiredHeader);
                        response.Header.Allow = null;
                        response.Header.Expires = 7200;
                    });
                    return;
                }
                else
                {
                    try
                    {
                        //如果验证不通过
                        if (!Authenticate(sipRequest, sipServer.Options.SipPassword))
                            throw new ApplicationException("错误的用户名或密码");
                    }
                    catch (Exception ex)
                    {
                        //注册失败
                        AgentContext.LogDebug($"从[SIP设备编号:{device.Model.Id},远程端点:{remoteEndPoint}]接收到注册请求。认证失败，原因: {ex.Message}");
                        SIPRequest req = SIPRequest.GetRequest(SIPMethodsEnum.BYE, sipRequest.URI);
                        req.Header.CallId = sipRequest.Header.CallId;
                        req.Header.From.FromTag = sipRequest.Header.From.FromTag;
                        req.Header.To.ToTag = sipRequest.Header.To.ToTag;
                        await sipServer.SendRequestAsync(remoteEndPoint, req);
                        return;
                    }
                }

                //设备注册成功
                AgentContext.LogDebug($"从[SIP设备编号:{device.Model.Id},远程端点:{remoteEndPoint}]接收到注册请求。认证成功。");
                //发送200 OK响应                
                await sipServer.SendResponseAsync(sipRequest, SIPResponseStatusCodesEnum.Ok, null, response =>
                {
                    response.Header.Contact = sipRequest.Header.Contact;
                    response.Header.Expires = sipRequest.Header.Expires;
                    response.Header.SetDateHeader(true, "s");
                });

                //如果是新设备注册
                if (sipServer.GetDevice(device.Model.Id) == null)
                    device = sipServer.RegisterNewDevice(device.Model);

                //注册设备
                await device.Register(localEndPoint, remoteEndPoint, sipRequest, expiry);
            }
        }
    }
}
