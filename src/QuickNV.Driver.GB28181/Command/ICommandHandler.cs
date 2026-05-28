using SIPSorcery.SIP;
using System.Xml.Linq;

namespace QuickNV.Driver.GB28181.Command
{
    public interface ICommandHandler
    {
        public string MessageType { get; }
        public string CmdType { get; }

        public Task Execute(
            SIPEndPoint localSIPEndPoint,
            SIPEndPoint remoteEndPoint,
            SIPRequest sipRequest,
            DeviceContext device,
            XElement cmdBody);
    }
}
