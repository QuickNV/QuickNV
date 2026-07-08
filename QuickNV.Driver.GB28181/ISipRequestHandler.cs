using SIPSorcery.SIP;

namespace QuickNV.Driver.GB28181
{
    public interface ISipRequestHandler
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="localEndPoint"></param>
        /// <param name="remoteEndPoint"></param>
        /// <param name="sipRequest"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public Task Execute(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest, DeviceContext device);
    }
}
