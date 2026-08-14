using Quick.Protocol;
using YiQiDong.Agent;
using YiQiDong.Core;
using QuickNV.Protocol.North.QpModels;
using Quick.Utils;

namespace QuickNV.North.Agent
{
    public abstract class AbstractNorthAgent : AbstractAgent
    {
        protected abstract string QuickNVNorthInterfaceUrl { get; }
        protected abstract string QuickNVNorthInterfacePassword { get; }
        protected virtual string QuickNVNorthInterfaceName => AgentContext.Container.Name;

        private CancellationTokenSource cts;
        private CommandExecuterManager commandExecuterManager;
        private NoticeHandlerManager noticeHandlerManager;

        protected bool IsConnected { get; private set; } = false;
        private QpClientOptions clientOptions = null;
        protected QpClient Client { get; private set; }

        public AbstractNorthAgent()
        {
            Quick.Protocol.Pipeline.QpPipelineClientOptions.RegisterUriSchema();
            Quick.Protocol.Tcp.QpTcpClientOptions.RegisterUriSchema();
            Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions.RegisterUriSchema();
            commandExecuterManager = new CommandExecuterManager();
            noticeHandlerManager = new NoticeHandlerManager();
        }

        public override void Start()
        {
            base.Start();
            cts = new CancellationTokenSource();
            clientOptions = QpClientOptions.Parse(new Uri(QuickNVNorthInterfaceUrl));
            clientOptions.Password = QuickNVNorthInterfacePassword;
            clientOptions.InstructionSet = new[] { Protocol.North.Instruction.Instance };
            clientOptions.RegisterCommandExecuterManager(commandExecuterManager);
            clientOptions.RegisterNoticeHandlerManager(noticeHandlerManager);
            _ = beginConnect(cts.Token);
        }

        private void clean()
        {
            var client = Client;
            if (client != null)
            {
                client.Disconnected -= Client_Disconnected;
                client.Dispose();
                Client = null;
            }
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                IsConnected = false;
                AgentContext.LogDebug($"[QuickNV北向接口]到[{QuickNVNorthInterfaceUrl}]的连接已断开。原因：{ExceptionUtils.GetExceptionMessage(Client.LastException)}");
                try
                {
                    OnDisconnected();
                }
                catch (Exception ex)
                {
                    AgentContext.LogError(ExceptionUtils.GetExceptionString(ex));
                }
                delayToConnect(cts.Token);
            }
        }

        private void delayToConnect(CancellationToken token)
        {
            clean();
            if (token.IsCancellationRequested)
                return;
            AgentContext.LogDebug($"[QuickNV北向接口]将在5秒后重试连接到[{QuickNVNorthInterfaceUrl}]...");
            Task.Delay(5000, token).ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                _ = beginConnect(token);
            });
        }
        //当连接上QuickNV时
        protected abstract void OnConnected();
        //当与QuickNV的连接断开时
        protected abstract void OnDisconnected();

        private async Task beginConnect(CancellationToken token)
        {
            try
            {
                AgentContext.LogDebug($"[QuickNV北向接口]正在连接到[{QuickNVNorthInterfaceUrl}]...");
                var client = Client = clientOptions.CreateClient();
                client.Disconnected += Client_Disconnected;
                await client.ConnectAsync();
                try
                {
                    var rep = await client.SendCommand(new Protocol.North.QpCommands.Register.Request()
                    {
                        Name = QuickNVNorthInterfaceName
                    });
                    OnConnected();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("注册北向接口失败",ex);
                }
                AgentContext.LogDebug($"[QuickNV北向接口]连接到[{QuickNVNorthInterfaceUrl}]成功.");
                IsConnected = true;
            }
            catch (Exception ex)
            {
                AgentContext.LogDebug($"[QuickNV北向接口]连接到[{QuickNVNorthInterfaceUrl}]失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                delayToConnect(token);
            }
        }

        public override void Stop()
        {
            cts?.Cancel();
            cts = null;
            clean();
            IsConnected = false;

            base.Stop();
        }

        public async Task<AddressInfo[]> GetAddressData()
        {
            var rep = await Client.SendCommand(new Protocol.North.QpCommands.GetAddressData.Request());
            return rep.Data;
        }

        public async Task<DeviceInfo[]> GetDeviceData()
        {
            var rep = await Client.SendCommand(new Protocol.North.QpCommands.GetDeviceData.Request());
            return rep.Data;
        }

        public async Task<ChannelInfo[]> GetChannelData()
        {
            var rep = await Client.SendCommand(new Protocol.North.QpCommands.GetChannelData.Request());
            return rep.Data;
        }

        public async Task Sync(Protocol.North.QpCommands.Sync.Request request)
        {
            await Client.SendCommand(request);
        }
    }
}