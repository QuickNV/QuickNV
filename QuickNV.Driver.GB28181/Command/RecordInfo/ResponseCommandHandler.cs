using SIPSorcery.SIP;
using System.Xml.Linq;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;
using QuickNV.Driver.GB28181.Utils;

namespace QuickNV.Driver.GB28181.Command.RecordInfo
{
    public class ResponseCommandHandler : ICommandHandler
    {
        public string MessageType => nameof(Response);
        public string CmdType => nameof(RecordInfo);

        private SipServer sipServer;

        public ResponseCommandHandler(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }

        public async Task Execute(
            SIPEndPoint localSIPEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest,
            DeviceContext device,
            XElement cmdBody)
        {
            //发送200 OK响应                
            await sipServer.SendResponseAsync(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
            var recordInfo = XmlConverter.DeserializeObject<Response>(cmdBody);
            device.SetResult_QueryRecordInfo(recordInfo);
        }
    }
}
