using Quick.Protocol;

namespace QuickNV.North.Protocol;

public class Instruction
{
    public static QpInstruction Instance = new QpInstruction()
    {
        Id = typeof(Instruction).Namespace,
        Name = "QuickNV北向协议",
        CommandInfos = new[]
        {
            QpCommandInfo.Create(new QpCommands.Register.Request()),
            QpCommandInfo.Create(new QpCommands.GetAddressData.Request()),
            QpCommandInfo.Create(new QpCommands.GetDeviceData.Request()),
            QpCommandInfo.Create(new QpCommands.GetChannelData.Request()),
            QpCommandInfo.Create(new QpCommands.Sync.Request())
        }
    };
}