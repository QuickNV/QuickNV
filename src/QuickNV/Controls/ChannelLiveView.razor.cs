using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using QuickNV.Core;
using QuickNV.Driver.Protocol.QpModels;
using QuickNV.Model;

namespace QuickNV.Controls
{
    public partial class ChannelLiveView
    {
        private bool isCurrentAutoPan = false;
        private float moveSpeed = 0.5f;

        [Inject]
        private NavigationManager navigationManager { get; set; }

        [Parameter]
        public Model.Channel Channel { get; set; }
        [Parameter]
        public bool LiveWithPtz { get; set; }

        private DriverContext driverContext;

        private string liveUrl;
        private string liveWithPtzUrl;
        protected override void OnParametersSet()
        {
            liveUrl = $"{navigationManager.BaseUri}preview/{Channel.DeviceId}/{Channel.Id}";
            liveWithPtzUrl = $"{navigationManager.BaseUri}preview/{Channel.DeviceId}/{Channel.Id}?withPtz=true";

            var device = ConfigDbContext.CacheContext.Find(new Device(Channel.DeviceId));
            driverContext = device?.GetDriverContext();
        }

        public static DialogParameters<ChannelLiveView> PrepareParameter(
            Model.Channel channel,
            bool liveWithPtz = false)
        {
            return new DialogParameters<ChannelLiveView>()
            {
                {t=>t.Channel,channel},
                {t=>t.LiveWithPtz,liveWithPtz}
            };
        }

        private async Task moveUp() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.Up, moveSpeed);
        private async Task moveDown() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.Down, moveSpeed);
        private async Task moveLeft() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.Left, moveSpeed);
        private async Task moveRight() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.Right, moveSpeed);
        private async Task zoomIn() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.ZoomIn, moveSpeed);
        private async Task zoomOut() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.ZoomOut, moveSpeed);
        private async Task focusFar() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.FocusFar, moveSpeed);
        private async Task focusNear() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.FocusNear, moveSpeed);
        private async Task irisIn() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.IrisOpen, moveSpeed);
        private async Task irisOut() => await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.IrisClose, moveSpeed);

        private async Task autoPan()
        {
            if (isCurrentAutoPan)
            {
                await stopMove();
            }
            else
            {
                await moveRight();
                isCurrentAutoPan = true;
            }
        }

        private async Task stopMove()
        {
            await driverContext.PtzControl(Channel.DeviceId, Channel.Id, PTZCommandType.Stop, moveSpeed);
            isCurrentAutoPan = false;
        }
    }
}
