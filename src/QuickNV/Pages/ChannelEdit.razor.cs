using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Utils;
using Quick.EntityFrameworkCore.Plus;
using QuickNV.Controls;
using QuickNV.Core;

namespace QuickNV.Pages
{
    public partial class ChannelEdit
    {
        private RenderFragment Body { get; set; }
        private string DeviceId { get; set; }
        private string ChannelId { get; set; }
        private ModalLoading modalLoading;
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
                return;
            }
            //找到通道
            if (!query.ContainsKey(nameof(ChannelId)))
            {
                modalAlert.Show("参数错误", $"未找到查询参数：{nameof(ChannelId)}");
                return;
            }
            ChannelId = query[nameof(ChannelId)];
            var channel = device.GetChannel(ChannelId);
            if (channel == null)
            {
                modalAlert.Show("参数错误", $"设备[{device.Name}]中未找到编号为[{ChannelId}]的通道");
                return;
            }
            var driverContext = device.GetDriverContext();
            if (driverContext == null)
            {
                modalAlert.Show("错误", $"设备[{device.Name}]使用的驱动[{device.DriverId}]当前没有连接。");
                return;
            }
            //显示
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<ChannelCreateControl>
            (
                ChannelCreateControl.PrepareParameter(device, channel, t =>
                {
                    modalLoading.Show("编辑通道", $"正在编辑通道[{t.Name}]...", true, null);
                    Task.Run(async () =>
                    {
                        try
                        {
                            await ChannelManager.Instance.EditChannel(driverContext, device, t);
                            await jsRuntime.InvokeVoidAsync(
                                    "QuickNV.postMessageToParent",
                                    JsonSerializer.Serialize(new
                                    {
                                        code = 0,
                                        message = $"编辑通道[{t.Name}]成功",
                                        data = t
                                    }),
                                    "*");
                            modalAlert.Show("成功", $"编辑通道[{t.Name}]成功");
                        }
                        catch (Exception ex)
                        {
                            modalAlert.Show("错误", $"编辑通道[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionString(ex)}");
                        }
                        modalLoading.Close();
                    });
                }).ToDictionary()
            );
            InvokeAsync(StateHasChanged);
        }
    }
}
