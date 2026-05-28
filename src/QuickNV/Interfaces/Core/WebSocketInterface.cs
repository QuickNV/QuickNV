using Quick.Protocol;
using Quick.Protocol.Pipeline;
using Quick.Protocol.WebSocket.Server.AspNetCore;
using System;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;

namespace QuickNV.Interfaces.Core
{
    public class WebSocketInterface
    {
        public const string INTERFACE_TYPE = "WebSocket";
        private AllInterfaceConfig config;
        private QpWebSocketServer server;
        public QpChannel[] GetAllChannels() => server?.Channels ?? new QpChannel[0];

        public WebSocketInterface(AllInterfaceConfig config, CommandExecuterManager commandExecuterManager, NoticeHandlerManager noticeHandlerManager, QpWebSocketServer server, QpWebSocketServerOptions options)
        {
            this.config = config;
            this.server = server;

            options.Password = config.Password;
            options.MaxPackageSize = 100 * 1024 * 1024;
            if (commandExecuterManager != null && !options.CommandExecuterManagerList.Contains(commandExecuterManager))
                options.RegisterCommandExecuterManager(commandExecuterManager);
            if (noticeHandlerManager != null && !options.NoticeHandlerManagerList.Contains(noticeHandlerManager))
                options.RegisterNoticeHandlerManager(noticeHandlerManager);
        }

        public void Start()
        {
            var uri = new Uri(Agent.Instance.Config.WebUrls.Replace("*", "127.0.0.1"));
            var wsUrl = $"qp.ws://{uri.Host}:{uri.Port}{config.WebSocketPath}";

            try
            {
                server.Start();
                AgentContext.LogInfo($"[{config.InterfaceName}][{INTERFACE_TYPE}]已启动，地址：{wsUrl}");
            }
            catch (Exception ex)
            {
                AgentContext.LogWarn($"[{config.InterfaceName}][{INTERFACE_TYPE}]启动失败，地址：{wsUrl}，原因：{ExceptionUtils.GetExceptionMessage(ex)}。");
                Stop();
                return;
            }
        }

        public void Stop()
        {
            server.Stop();
            AgentContext.LogInfo($"[{config.InterfaceName}][{INTERFACE_TYPE}]已停止");
        }
    }
}
