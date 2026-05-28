using QuickNV.HikvisionISUPSDK.Api;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.HikvisionISUP
{
    public class DeviceContext : IDisposable
    {
        public DriverDevice<DeviceConfig, ChannelConfig> Model { get; private set; }
        public QuickNV.HikvisionISUPSDK.Api.DeviceContext ApiContext { get; set; }
        public bool IsOnline => ApiContext != null;

        public DeviceContext(DriverDevice<DeviceConfig, ChannelConfig> device)
        {
            Model = device;
        }

        public void Dispose()
        {

        }

        private CmsPTZCommand lastCommand = CmsPTZCommand.PTZ_RIGHT;
        public void PtzControl(int channelId, PTZCommandType commandType, float moveSpeed)
        {
            var start = true;
            switch (commandType)
            {
                case PTZCommandType.Stop:
                    start = false;
                    break;
                case PTZCommandType.Up:
                    lastCommand = CmsPTZCommand.PTZ_UP;
                    break;
                case PTZCommandType.Down:
                    lastCommand = CmsPTZCommand.PTZ_DOWN;
                    break;
                case PTZCommandType.Left:
                    lastCommand = CmsPTZCommand.PTZ_LEFT;
                    break;
                case PTZCommandType.Right:
                    lastCommand = CmsPTZCommand.PTZ_RIGHT;
                    break;
                case PTZCommandType.ZoomIn:
                    lastCommand = CmsPTZCommand.PTZ_ZOOMIN;
                    break;
                case PTZCommandType.ZoomOut:
                    lastCommand = CmsPTZCommand.PTZ_ZOOMOUT;
                    break;
                case PTZCommandType.FocusFar:
                    lastCommand = CmsPTZCommand.PTZ_FOCUSFAR;
                    break;
                case PTZCommandType.FocusNear:
                    lastCommand = CmsPTZCommand.PTZ_FOCUSNEAR;
                    break;
                case PTZCommandType.IrisOpen:
                    lastCommand = CmsPTZCommand.PTZ_IRISSTARTUP;
                    break;
                case PTZCommandType.IrisClose:
                    lastCommand = CmsPTZCommand.PTZ_IRISSTOPDOWN;
                    break;
            }
            ApiContext.PtzControl(channelId, lastCommand, start, moveSpeed);
        }
    }
}
