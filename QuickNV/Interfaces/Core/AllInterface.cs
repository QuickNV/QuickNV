using Microsoft.AspNetCore.Builder;
using Quick.Protocol;
using Quick.Protocol.WebSocket.Server.AspNetCore;
using System;
using System.Collections.Generic;

namespace QuickNV.Interfaces.Core
{
    public class AllInterface
    {
        private PipeInterface pipeInterface;
        private TcpInterface tcpInterface;
        private WebSocketInterface webSocketInterface;

        private QpWebSocketServerOptions webSocketServerOptions;
        private QpWebSocketServer webSocketServer;

        public AllInterface(AllInterfaceConfig config, IApplicationBuilder app)
        {
            webSocketServerOptions = new QpWebSocketServerOptions()
            {
                Path = config.WebSocketPath,
                Password = config.Password,
                ServerProgram = config.InterfaceName,
                InstructionSet = config.InstructionSet
            };
            app.UseQuickProtocolWebSocketServer(webSocketServerOptions, out webSocketServer);
        }

        public QpChannel[] GetAllChannels()
        {
            List<QpChannel> list = new List<QpChannel>();
            if (pipeInterface != null)
                list.AddRange(pipeInterface.GetAllChannels());
            if (tcpInterface != null)
                list.AddRange(tcpInterface.GetAllChannels());
            if (webSocketInterface != null)
                list.AddRange(webSocketInterface.GetAllChannels());
            return list.ToArray();
        }

        public void Start(AllInterfaceConfig config, CommandExecuterManager commandExecuterManager, NoticeHandlerManager noticeHandlerManager)
        {
            if (config.PipeEnable)
                pipeInterface = new PipeInterface(config, commandExecuterManager, noticeHandlerManager);
            if (config.TcpEnable)
                tcpInterface = new TcpInterface(config, commandExecuterManager, noticeHandlerManager);
            if (config.WebSocketEnable)
                webSocketInterface = new WebSocketInterface(config, commandExecuterManager, noticeHandlerManager, webSocketServer, webSocketServerOptions);

            pipeInterface?.Start();
            tcpInterface?.Start();
            webSocketInterface?.Start();
        }

        public void Stop()
        {
            pipeInterface?.Stop();
            pipeInterface = null;
            tcpInterface?.Stop();
            tcpInterface = null;
            webSocketInterface?.Stop();
            webSocketInterface = null;
        }
    }
}
