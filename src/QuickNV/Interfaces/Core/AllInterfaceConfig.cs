using Quick.Protocol;

namespace QuickNV.Interfaces.Core
{
    public class AllInterfaceConfig
    {
        public string InterfaceName { get; set; }
        public QpInstruction[] InstructionSet { get; set; }
        public string WebSocketPath { get; set; }

        public string Password { get; set; } = "123456";

        public bool WebSocketEnable { get; set; } = false;
        public bool PipeEnable { get; set; } = false;
        public string PipeName { get; set; }

        public bool TcpEnable { get; set; } = false;
        public string TcpListenAddress { get; set; } = "0.0.0.0";
        public int TcpListenPort { get; set; }
    }
}
