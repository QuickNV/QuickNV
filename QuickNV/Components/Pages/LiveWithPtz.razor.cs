using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using QuickNV.Components.Controls;

namespace QuickNV.Components.Pages
{
    public partial class LiveWithPtz
    {
        private RenderFragment Body { get; set; }
        private string DeviceId { get; set; }
        private string ChannelId { get; set; }
        private ModalAlert modalAlert;

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (!firstRender)
                return;

            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(navigationManager.Uri).Query);
            //找到设备
            if (!query.ContainsKey(nameof(DeviceId)))
            {
                modalAlert.Show("参数错误", $"未找到查询参数：{nameof(DeviceId)}");
                return;
            }
            DeviceId = query[nameof(DeviceId)];
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(DeviceId));
            if (device == null)
            {
                modalAlert.Show("参数错误", $"未找到编号为[{DeviceId}]的设备");
            }
            //找到通道
            if (!query.ContainsKey(nameof(ChannelId)))
            {
                modalAlert.Show("参数错误", $"未找到查询参数：{nameof(ChannelId)}");
            }
            ChannelId = query[nameof(ChannelId)];
            var channel = device.GetChannel(ChannelId);
            if (channel == null)
            {
                modalAlert.Show("参数错误", $"设备[{device.Name}]中未找到编号为[{ChannelId}]的通道");
            }

            //显示
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<ChannelLiveView>
            (
                ChannelLiveView.PrepareParameter(channel, true).ToDictionary()
            );
            InvokeAsync(StateHasChanged);
        }
    }
}
