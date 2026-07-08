using Quick.Protocol;
using YiQiDong.Agent;
using Quick.Protocol.InterfaceService;

namespace QuickNV.Interfaces
{
    public class Manager
    {
        public static Manager Instance { get; } = new Manager();
        private QpInterfaceServiceContext allInterface;
        private CommandExecuterManager commandExecuterManager;

        private Manager()
        {
            commandExecuterManager = new CommandExecuterManager();
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.Register.Request(), Driver.Manager.Instance.ExecuteRegister);
            commandExecuterManager.Register(new Protocol.North.QpCommands.Register.Request(), North.Manager.Instance.ExecuteRegister);
        }

        public void Start(IApplicationBuilder app)
        {
            var config = Agent.Instance.Config.QpInterface;
            allInterface = new QpInterfaceServiceContext(new()
            {
                InterfaceName = "对外接口",
                WebBuilder = app,
                Config = config,
                InstructionSet =
                [
                    Protocol.Driver.Instruction.Instance,
                    Protocol.North.Instruction.Instance
                ],
                Logger = AgentContext.LogInfo,
                CommandExecuterManager = commandExecuterManager
            });
            allInterface.Start();
        }

        public void Stop()
        {
            allInterface.Stop();
        }
    }
}
