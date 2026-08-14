using NPOI.SS.Formula.Functions;
using Quick.Protocol;
using Quick.Utils;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;

namespace QuickNV.Core
{
    /// <summary>
    /// 易认证管理器
    /// </summary>
    public class YiRenZhengManager
    {
        public static YiRenZhengManager Instance { get; } = new YiRenZhengManager();
        public bool Connected { get; private set; } = false;

        private QpClient client;
        private QpClientOptions clientOptions;
        private CancellationTokenSource cts;


        private void Client_Disconnected(object sender, EventArgs e)
        {
            lock (this)
            {
                if (Connected)
                {
                    Connected = false;
                    AgentContext.LogInfo($"到易认证接口[{Agent.Instance.Config.YiRenZhengInterfaceUrl}]的连接已断开。原因：{ExceptionUtils.GetExceptionString(client?.LastException)}");
                    delayToConnect(cts.Token);
                }
            }
        }

        private void delayToConnect(CancellationToken token)
        {
            clean();
            Task.Delay(5000, token).ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                beginConnect(token);
            });
        }

        private void clean()
        {
            if (client != null)
            {
                client.Disconnected -= Client_Disconnected;
                client.Dispose();
                client = null;
            }
        }

        private void beginConnect(CancellationToken token)
        {
            client = clientOptions.CreateClient();
            client.Disconnected += Client_Disconnected;
            AgentContext.LogInfo($"开始连接到易认证接口[{Agent.Instance.Config.YiRenZhengInterfaceUrl}]...");
            client.ConnectAsync().ContinueWith(task =>
            {
                if (token.IsCancellationRequested)
                    return;
                if (task.IsFaulted)
                {
                    AgentContext.LogWarn($"连接到易认证接口[{Agent.Instance.Config.YiRenZhengInterfaceUrl}]失败，原因：{task.Exception.InnerException.Message}");
                    delayToConnect(token);
                    return;
                }
                //连接成功
                AgentContext.LogInfo($"连接到易认证接口[{Agent.Instance.Config.YiRenZhengInterfaceUrl}]]成功。");
                Connected = true;
            });
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(Agent.Instance.Config.YiRenZhengInterfaceUrl))
                return;

            cts = new CancellationTokenSource();
            var uri = new Uri(Agent.Instance.Config.YiRenZhengInterfaceUrl);
            clientOptions = QpClientOptions.Parse(uri);
            clientOptions.Password = Agent.Instance.Config.YiRenZhengInterfacePassword;
            clientOptions.InstructionSet = new QpInstruction[] { YiRenZheng.Protocol.Instruction.Instance };
            beginConnect(cts.Token);
        }

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
            clean();
        }

        /// <summary>
        /// 认证
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<YiRenZheng.Protocol.QpCommands.Authenticate.Response> Authenticate(
            string origin,
            Dictionary<string, string> parameters)
        {
            return await client.SendCommand(new YiRenZheng.Protocol.QpCommands.Authenticate.Request()
            {
                Origin = origin,
                Parameters = parameters
            });
        }
    }
}
