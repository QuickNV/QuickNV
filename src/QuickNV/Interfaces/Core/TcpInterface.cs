using Quick.Protocol;
using Quick.Protocol.Tcp;
using System;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;

namespace QuickNV.Interfaces.Core
{
    public class TcpInterface
    {
        public const string INTERFACE_TYPE = "TCP";
        private AllInterfaceConfig config;
        private QpTcpServerOptions options;
        private QpTcpServer server;
        public QpChannel[] GetAllChannels() => server?.Channels ?? new QpChannel[0];

        public TcpInterface(AllInterfaceConfig config, Quick.Protocol.CommandExecuterManager commandExecuterManager, Quick.Protocol.NoticeHandlerManager noticeHandlerManager)
        {
            this.config = config;
            options = new QpTcpServerOptions()
            {
                Address = System.Net.IPAddress.Parse(config.TcpListenAddress),
                Port = config.TcpListenPort,
                Password = config.Password,
                ServerProgram = config.InterfaceName,
                InstructionSet = config.InstructionSet,
                MaxPackageSize = 100 * 1024 * 1024
            };
            if (commandExecuterManager != null)
                options.RegisterCommandExecuterManager(commandExecuterManager);
            if (noticeHandlerManager != null)
                options.RegisterNoticeHandlerManager(noticeHandlerManager);
        }

        public void Start()
        {
            server = new QpTcpServer(options);
            try
            {
                server.Start();
                AgentContext.LogInfo($"[{config.InterfaceName}][{INTERFACE_TYPE}]已启动，地址：qp.tcp://{config.TcpListenAddress}:{config.TcpListenPort}");
            }
            catch (Exception ex)
            {
                AgentContext.LogWarn($"[{config.InterfaceName}][{INTERFACE_TYPE}]启动失败，地址：qp.tcp://{config.TcpListenAddress}:{config.TcpListenPort}，原因：{ExceptionUtils.GetExceptionMessage(ex)}。");
                Stop();
                return;
            }
        }

        public void Stop()
        {
            server?.Stop();
            server = null;
            AgentContext.LogInfo($"[{config.InterfaceName}][{INTERFACE_TYPE}]已停止");
        }
    }
}
