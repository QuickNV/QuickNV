using Quick.Protocol;

namespace QuickNV.Protocol.Driver;

public class Instruction
{
    public static QpInstruction Instance = new QpInstruction()
    {
        Id = typeof(Instruction).Namespace,
        Name = "QuickNV驱动协议",
        NoticeInfos = new[]
        {
            QpNoticeInfo.Create(new QpNotices.DeviceAddedNotice()),
            QpNoticeInfo.Create(new QpNotices.DeviceDeletedNotice()),
            QpNoticeInfo.Create(new QpNotices.DeviceOnlineNotice()),
            QpNoticeInfo.Create(new QpNotices.DeviceOfflineNotice()),
            QpNoticeInfo.Create(new QpNotices.DeviceLogNotice()),
            QpNoticeInfo.Create(new QpNotices.ChannelAddedNotice()),
            QpNoticeInfo.Create(new QpNotices.ChannelDeletedNotice())
        },
        CommandInfos = new[]
        {
            QpCommandInfo.Create(new QpCommands.Register.Request()),
            QpCommandInfo.Create(new QpCommands.ImportDevices.Request()),
            QpCommandInfo.Create(new QpCommands.GetDeviceConfig.Request()),
            QpCommandInfo.Create(new QpCommands.ImportChannels.Request()),
            QpCommandInfo.Create(new QpCommands.GetChannelConfig.Request()),
            QpCommandInfo.Create(new QpCommands.CreateChannelLiveStream.Request()),
            QpCommandInfo.Create(new QpCommands.ChangeLiveStreamSSRC.Request()),
            QpCommandInfo.Create(new QpCommands.DestoryChannelStream.Request()),
            QpCommandInfo.Create(new QpCommands.GetMediaServerStreamInfo.Request()),
            QpCommandInfo.Create(new QpCommands.MediaServerAddStreamProxy.Request()),
            QpCommandInfo.Create(new QpCommands.PtzControl.Request()),
            QpCommandInfo.Create(new QpCommands.Snapshot.Request()),
            QpCommandInfo.Create(new QpCommands.FindPlaybackFiles.Request()),
            QpCommandInfo.Create(new QpCommands.CreateChannelPlaybackStream.Request())
        }
    };
}