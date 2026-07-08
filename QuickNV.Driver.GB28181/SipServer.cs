using SIPSorcery.SIP;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using YiQiDong.Agent;
using QuickNV.Driver.Agent;
using QuickNV.Driver.GB28181.Command;
using QuickNV.Driver.GB28181.Utils;
using Quick.Utils;

namespace QuickNV.Driver.GB28181
{
    public class SipServer
    {
        //请求处理器字典
        private Dictionary<SIPMethodsEnum, ISipRequestHandler> requestHandlerDict = new Dictionary<SIPMethodsEnum, ISipRequestHandler>();
        //SIP传输器
        private SIPTransport sipTransport;
        private ConcurrentDictionary<string, CommandContext> commandDict = new ConcurrentDictionary<string, CommandContext>();

        private Dictionary<string, DeviceContext> deviceDict = new Dictionary<string, DeviceContext>();
        private Dictionary<string, DeviceContext> deviceDict_Cache = new Dictionary<string, DeviceContext>();
        private CancellationTokenSource cts;
        private int currentCSeq = 0;

        /// <summary>
        /// 设备在线状态改变事件
        /// </summary>
        public event EventHandler<DeviceOnlineStateChangedEventArgs> DeviceOnlineStateChanged;
        /// <summary>
        /// 新设备注册事件
        /// </summary>
        public event EventHandler<DriverDevice<DeviceConfig, ChannelConfig>> NewDeviceRegistered;

        /// <summary>
        /// 选项
        /// </summary>
        public ConfigModel Options { get; private set; }
        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding { get; private set; }
        /// <summary>
        /// 服务端联系方式
        /// </summary>
        public SIPContactHeader ServerContactHeader { get; private set; }

        public SipServer(ConfigModel options)
        {
            Options = options;
            Encoding = Encoding.GetEncoding(options.Encoding);
            ServerContactHeader = new SIPContactHeader(Options.SipDeviceId,
                new SIPURI(SIPSchemesEnum.sip, IPAddress.Parse(Options.SipServerIpAddress), Options.SipServerPort));
            requestHandlerDict[SIPMethodsEnum.REGISTER] = new SipRequestHandlers.Register(this);
            requestHandlerDict[SIPMethodsEnum.MESSAGE] = new SipRequestHandlers.Message(this);
        }

        public void Start()
        {
            cts = new CancellationTokenSource();

            IPAddress listenAddress = IPAddress.Any;
            IPAddress listenIPv6Address = IPAddress.IPv6Any;

            sipTransport = new SIPTransport(Encoding, Encoding);
            sipTransport.EnableTraceLogs();
            // IPv4 channels.
            sipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(listenAddress, Options.SipServerPort)));
            AgentContext.LogInfo($"开始监听IPv4 UDP端口: {Options.SipServerPort}");
            sipTransport.AddSIPChannel(new SIPTCPChannel(new IPEndPoint(listenAddress, Options.SipServerPort)));
            AgentContext.LogInfo($"开始监听IPv4 TCP端口: {Options.SipServerPort}");
            // IPv6 channels.
            //sipTransport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(listenIPv6Address, Agent.ConfigModel.SipServerPort)));
            //sipTransport.AddSIPChannel(new SIPTCPChannel(new IPEndPoint(listenIPv6Address, Agent.ConfigModel.SipServerPort)));

            sipTransport.SIPTransportRequestReceived += SipTransport_SIPTransportRequestReceived;
            sipTransport.SIPTransportResponseReceived += SipTransport_SIPTransportResponseReceived;
        }

        public string GetAddressId(string id)
        {
            return id.Substring(0, 8);
        }

        private async Task SipTransport_SIPTransportRequestReceived(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            AgentContext.LogTrace($"[接收到请求][{remoteEndPoint}]{Environment.NewLine}----------------{Environment.NewLine}{sipRequest}{Environment.NewLine}----------------");
            if (!requestHandlerDict.ContainsKey(sipRequest.Method))
                return;

            string deviceId = sipRequest.Header.From.FromURI.User;
            var device = GetDevice(deviceId);
            if (device == null)
                device = new DeviceContext(this, new DriverDevice<DeviceConfig, ChannelConfig>(new Protocol.Driver.QpModels.DeviceInfo()
                {
                    Id = deviceId,
                    Name = deviceId
                }));
            var handler = requestHandlerDict[sipRequest.Method];
            if (localEndPoint.Address == IPAddress.Any)
                localEndPoint = new SIPEndPoint(new IPEndPoint(IPAddress.Parse(Options.SipServerIpAddress), localEndPoint.Port));
            try
            {
                await handler.Execute(localEndPoint, remoteEndPoint, sipRequest, device);
            }
            catch (Exception ex)
            {
                AgentContext.LogError("处理SIP请求时出错，原因：" + ExceptionUtils.GetExceptionString(ex));
                throw;
            }
        }

        private Task SipTransport_SIPTransportResponseReceived(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPResponse sipResponse)
        {
            AgentContext.LogTrace($"[接收到响应][{remoteEndPoint}]{Environment.NewLine}----------------{Environment.NewLine}{sipResponse}{Environment.NewLine}----------------");

            if (sipResponse.Status == SIPResponseStatusCodesEnum.Trying)
                return Task.CompletedTask;

            var callId = sipResponse.Header.CallId;
            if (callId == null)
                return Task.CompletedTask;

            //设置指令响应
            CommandContext commandContext;
            if (!commandDict.TryRemove(callId, out commandContext))
                return Task.CompletedTask;
            commandContext.SetResponse(sipResponse);
            return Task.CompletedTask;
        }

        public void Stop()
        {
            if (sipTransport != null)
            {
                sipTransport.Shutdown();
                sipTransport.Dispose();
                sipTransport = null;
            }

            cts?.Cancel();
            cts = null;

            lock (deviceDict)
                deviceDict.Clear();
            deviceDict_Cache = new Dictionary<string, DeviceContext>();
        }

        private void RaiseEvent_DeviceOnlineStateChanged(DeviceOnlineStateChangedEventArgs e)
        {
            DeviceOnlineStateChanged?.Invoke(this, e);
        }

        public DeviceContext[] GetDevices() => deviceDict_Cache.Values.ToArray();
        public int GetDevicesCount() => deviceDict_Cache.Count;
        public int GetConnectedDevicesCount() => deviceDict_Cache.Count(t => t.Value.IsOnline);
        public int GetDevicesChannelsCount() => deviceDict_Cache.Sum(t => t.Value.ChannelsCount);

        public DeviceContext[] QueryDevices(string keywords = null, bool? isRegistered = null)
        {
            return deviceDict_Cache.Values
                .Where(t => string.IsNullOrEmpty(keywords) || t.Model.Id.Contains(keywords) || t.Model.Name.Contains(keywords))
                .Where(t => isRegistered == null || t.IsOnline == isRegistered.Value)
                .ToArray();
        }

        public DeviceContext GetDevice(string deviceId)
        {
            if (deviceDict_Cache.ContainsKey(deviceId))
                return deviceDict_Cache[deviceId];
            return null;
        }

        public DeviceContext AddDevice(DriverDevice<DeviceConfig, ChannelConfig> model)
        {
            var device = new DeviceContext(this, model);
            lock (deviceDict)
            {
                if (deviceDict.ContainsKey(device.Model.Id))
                    throw new ApplicationException($"Already has device with id[{device.Model.Id}]");
                deviceDict[device.Model.Id] = device;
                deviceDict_Cache = deviceDict.ToDictionary(t => t.Key, t => t.Value);
            }
            device.OnlineStateChanged += Device_OnlineStateChanged;
            RaiseEvent_DeviceOnlineStateChanged(new DeviceOnlineStateChangedEventArgs()
            {
                DeviceContext = device,
                IsOnline = device.IsOnline
            });
            return device;
        }

        private void Device_OnlineStateChanged(object sender, DeviceOnlineStateChangedEventArgs e)
        {
            RaiseEvent_DeviceOnlineStateChanged(e);
        }

        public void RemoveDevice(string deviceId)
        {
            DeviceContext device = GetDevice(deviceId);
            if (device == null)
                return;
            lock (deviceDict)
            {
                if (deviceDict.ContainsKey(deviceId))
                    deviceDict.Remove(deviceId);
                deviceDict_Cache = deviceDict.ToDictionary(t => t.Key, t => t.Value);
            }
            device.OnlineStateChanged -= Device_OnlineStateChanged;
        }

        /// <summary>
        /// 增加Contact头
        /// </summary>
        /// <param name="request"></param>
        public void AddHeaderContact(SIPRequest request)
        {
            if (request.Header.Contact == null)
                request.Header.Contact = new List<SIPContactHeader>();
            if (request.Header.Contact.Count == 0)
                request.Header.Contact.Add(ServerContactHeader);
        }

        public async Task SendResponseAsync(
            SIPRequest sipRequest,
            SIPResponseStatusCodesEnum responseCode,
            string reasonPhrase,
            Action<SIPResponse> responseHandler = null)
        {
            var response = SIPResponse.GetResponse(sipRequest, responseCode, reasonPhrase);
            AddHeader_UserAgent(response);
            response.Header.Allow = null;
            responseHandler?.Invoke(response);
            AgentContext.LogTrace($"[发送响应][{sipRequest.RemoteSIPEndPoint}]{Environment.NewLine}----------------{Environment.NewLine}{response}{Environment.NewLine}----------------");
            await sipTransport.SendResponseAsync(response);
        }

        public void SendResponse(SIPRequest sipRequest,
            SIPResponseStatusCodesEnum responseCode,
            string reasonPhrase,
            Action<SIPResponse> responseHandler = null)
        {
            var response = SIPResponse.GetResponse(sipRequest, responseCode, reasonPhrase);
            AddHeader_UserAgent(response);
            responseHandler?.Invoke(response);
            SIPNonInviteTransaction transaction = new SIPNonInviteTransaction(sipTransport, sipRequest, null);
            transaction.SendResponse(response);
        }

        public async Task<SIPResponse> SendMessageRequestAsync(
            DeviceContext deviceContext,
            object bodyObj,
            Action<SIPRequest> requestHandler = null,
            bool waitForResponse = true)
        {
            return await SendMessageRequestAsync(
                deviceContext.LocalEndPoint,
                deviceContext.RemoteEndPoint,
                deviceContext.ContactUri,
                deviceContext.Model.Id,
                bodyObj,
                requestHandler,
                waitForResponse);
        }

        public async Task<SIPResponse> SendMessageRequestAsync(
            SIPEndPoint localEndpoint,
            SIPEndPoint remoteEndpoint,
            SIPURI contactUri,
            string deviceId,
            object bodyObj,
            Action<SIPRequest> requestHandler = null,
            bool waitForResponse = true)
        {
            return await SendRequestAsync(
                localEndpoint,
                remoteEndpoint,
                SIPMethodsEnum.MESSAGE,
                contactUri,
                Options.SipDeviceId,
                deviceId,
                "Application/MANSCDP+xml",
                XmlConverter.SerializeObject(bodyObj, Encoding),
                requestHandler,
                waitForResponse);
        }

        public async Task<SIPResponse> SendRequestAsync(
            DeviceContext deviceContext,
            SIPMethodsEnum method,
            string contentType = null,
            string body = null,
            Action<SIPRequest> requestHandler = null,
            bool waitForResponse = true)
        {
            return await SendRequestAsync(
                deviceContext.LocalEndPoint,
                deviceContext.RemoteEndPoint,
                method,
                deviceContext.ContactUri,
                Options.SipDeviceId,
                deviceContext.Model.Id,
                contentType,
                body,
                requestHandler,
                waitForResponse);
        }

        public async Task<SIPResponse> SendRequestAsync(
            DeviceContext deviceContext,
            SIPMethodsEnum method,
            string to,
            string contentType = null,
            string body = null,
            Action<SIPRequest> requestHandler = null,
            bool waitForResponse = true)
        {
            return await SendRequestAsync(
                deviceContext.LocalEndPoint,
                deviceContext.RemoteEndPoint,
                method,
                deviceContext.ContactUri,
                Options.SipDeviceId,
                to,
                contentType,
                body,
                requestHandler,
                waitForResponse);
        }

        public async Task<SIPResponse> SendRequestAsync(
            SIPEndPoint localEndpoint,
            SIPEndPoint remoteEndpoint,
            SIPMethodsEnum method,
            SIPURI uri,
            string from,
            string to,
            string contentType = null,
            string body = null,
            Action<SIPRequest> requestHandler = null,
            bool waitForResponse = true)
        {
            //准备From
            IPAddress sipServerIpAddress = IPAddress.Parse(Options.SipServerIpAddress);
            var fromSipUri = new SIPURI(SIPSchemesEnum.sip, sipServerIpAddress, Options.SipServerPort);
            fromSipUri.User = Options.SipDeviceId;
            SIPFromHeader fromHeader = new SIPFromHeader(null, fromSipUri, nameof(QuickNV));

            //准备To
            var toSipUri = new SIPURI(SIPSchemesEnum.sip, remoteEndpoint);
            toSipUri.User = to;
            SIPToHeader toHeader = new SIPToHeader(null, toSipUri, null);

            return await SendRequestAsync(
                localEndpoint,
                remoteEndpoint,
                method,
                uri,
                fromHeader,
                toHeader,
                contentType,
                body,
                requestHandler,
                waitForResponse);
        }
        private void AddHeader_UserAgent(SIPMessageBase message)
        {
            message.Header.UserAgent = $"{nameof(QuickNV)} v3.7";
        }

        public async Task<SIPResponse> SendRequestAsync(
            SIPEndPoint localEndpoint,
            SIPEndPoint remoteEndpoint,
            SIPMethodsEnum method,
            SIPURI uri,
            SIPFromHeader from,
            SIPToHeader to,
            string contentType = null,
            string body = null,
            Action<SIPRequest> requestHandler = null,
            bool waitForResponse = true)
        {
            //准备Request
            SIPRequest request = SIPRequest.GetRequest(method, uri, to, from);
            //每次发送指令CSeq增加1
            currentCSeq++;
            if (currentCSeq >= int.MaxValue)
                currentCSeq = 0;

            AddHeader_UserAgent(request);
            request.Header.CSeq = currentCSeq;
            request.Header.Vias = new SIPViaSet();
            request.Header.Vias.PushViaHeader(new SIPViaHeader(localEndpoint, CallProperties.CreateBranchId()));
            request.Header.Allow = null;
            if (!string.IsNullOrEmpty(contentType)
                && !string.IsNullOrEmpty(body))
            {
                request.Header.ContentType = contentType;
                request.Header.ContentLength = Encoding.GetByteCount(body);
                request.Body = body;
            }
            //发送请求
            return await SendRequestAsync(
                remoteEndpoint,
                request,
                requestHandler,
                waitForResponse);
        }

        public async Task<SIPResponse> SendRequestAsync(
            SIPEndPoint endpoint,
            SIPRequest request,
            Action<SIPRequest> requestHandler = null,
            bool waitForResponse = true,
            int timeout = 5 * 1000)
        {
            requestHandler?.Invoke(request);
            if (string.IsNullOrEmpty(request.Header.CallId))
                request.Header.CallId = CommandContext.GenerateNewId();            

            AgentContext.LogTrace($"[发送请求][{endpoint}]{Environment.NewLine}----------------{Environment.NewLine}{request}{Environment.NewLine}----------------");

            //如果不等待响应，则直接发送请求后直接返回
            if (!waitForResponse)
            {
                await sipTransport.SendRequestAsync(endpoint, request);
                return null;
            }

            //准备指令上下文
            var commandContext = new CommandContext(request.Header.CallId);
            commandDict.TryAdd(commandContext.Id, commandContext);

            //如果没有设置超时
            if (timeout <= 0)
            {
                await sipTransport.SendRequestAsync(endpoint, request);
                return await commandContext.ResponseTask;
            }
            try
            {
                await sipTransport.SendRequestAsync(endpoint, request).WaitAsync(TimeSpan.FromMilliseconds(timeout));
            }
            catch
            {
                if (commandContext.ResponseTask.Status == TaskStatus.Created)
                {
                    commandContext.Timeout();
                    commandDict.TryRemove(commandContext.Id, out _);
                }
            }
            return await commandContext.ResponseTask.WaitAsync(TimeSpan.FromMilliseconds(timeout));
        }

        public DeviceContext RegisterNewDevice(DriverDevice<DeviceConfig, ChannelConfig> model)
        {
            NewDeviceRegistered?.Invoke(this, model);
            return AddDevice(model);
        }
    }
}
