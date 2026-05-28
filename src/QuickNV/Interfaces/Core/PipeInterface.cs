using Quick.Protocol;
using Quick.Protocol.Pipeline;
using Quick.Utils;
using System;
using System.Collections.Generic;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;

namespace QuickNV.Interfaces.Core
{
    public class PipeInterface
    {
        public const string INTERFACE_TYPE = "管道";
        private AllInterfaceConfig config;
        private QpPipelineServerOptions options;
        private QpPipelineServer server;
        public QpChannel[] GetAllChannels() => server?.Channels ?? new QpChannel[0];

        public PipeInterface(AllInterfaceConfig config, Quick.Protocol.CommandExecuterManager commandExecuterManager, Quick.Protocol.NoticeHandlerManager noticeHandlerManager)
        {
            this.config = config;
            options = new QpPipelineServerOptions()
            {
                PipeName = config.PipeName,
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
            server = new QpPipelineServer(options);
            try
            {
                server.Start();
                AgentContext.LogInfo($"[{config.InterfaceName}][{INTERFACE_TYPE}]已启动，地址：qp.pipe://./{config.PipeName}");
            }
            catch (Exception ex)
            {
                AgentContext.LogWarn($"[{config.InterfaceName}][{INTERFACE_TYPE}]启动失败，地址：qp.pipe://./{config.PipeName}，原因：{ExceptionUtils.GetExceptionMessage(ex)}。");
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
