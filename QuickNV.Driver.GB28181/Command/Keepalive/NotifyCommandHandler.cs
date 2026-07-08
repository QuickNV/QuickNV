using SIPSorcery.SIP;
using System.Xml.Linq;

namespace QuickNV.Driver.GB28181.Command.Keepalive
{
    public class NotifyCommandHandler : ICommandHandler
    {
        public string MessageType => nameof(Notify);
        public string CmdType => nameof(Keepalive);

        private SipServer sipServer;

        public NotifyCommandHandler(SipServer sipServer)
        {
            this.sipServer = sipServer;
        }

        public async Task Execute(
            SIPEndPoint localEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest,
            DeviceContext device,
            XElement cmdBody)
        {
            if (device.Keepalive(localEndPoint, remoteEndPoint, sipRequest))
                await sipServer.SendResponseAsync(sipRequest, SIPResponseStatusCodesEnum.Ok, null);
            else
                await sipServer.SendResponseAsync(sipRequest, SIPResponseStatusCodesEnum.BadRequest, null);
        }
    }
}
