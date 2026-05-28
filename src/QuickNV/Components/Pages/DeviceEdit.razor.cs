using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Utils;
using Quick.EntityFrameworkCore.Plus;
using QuickNV.Components.Controls;
using QuickNV.Core;

namespace QuickNV.Components.Pages
{
    public partial class DeviceEdit
    {
        private RenderFragment Body { get; set; }
        private string DeviceId { get; set; }
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

            //显示
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<DeviceCreateControl>
            (
                DeviceCreateControl.PrepareParameter(device, t =>
                {
                    modalLoading.Show("编辑设备", $"正在编辑设备[{t.Name}]...", true, null);
                    Task.Run(async () =>
                    {
                        try
                        {
                            await DeviceManager.Instance.EditDevice(t);
                            await jsRuntime.InvokeVoidAsync(
                                "QuickNV.postMessageToParent",
                                JsonSerializer.Serialize(new
                                {
                                    code = 0,
                                    message = $"编辑设备[{t.Name}]成功",
                                    data = t
                                }),
                                "*");
                            modalAlert.Show("成功", $"编辑设备[{t.Name}]成功");
                        }
                        catch (Exception ex)
                        {
                            modalAlert.Show("错误", $"编辑设备[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                        }
                        modalLoading.Close();
                    });
                }).ToDictionary()
            );
            InvokeAsync(StateHasChanged);
        }
    }
}
