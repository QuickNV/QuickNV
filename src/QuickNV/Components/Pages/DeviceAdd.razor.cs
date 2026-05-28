using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Utils;
using QuickNV.Components.Controls;
using QuickNV.Core;
using Quick.Utils;

namespace QuickNV.Components.Pages
{
    public partial class DeviceAdd
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

            //显示
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<DeviceCreateControl>
            (
                DeviceCreateControl.PrepareParameter(
                    null,
                    t =>
                    {
                        modalLoading.Show("添加设备", $"正在添加设备[{t.Name}]...", true, null);
                        Task.Run(async () =>
                        {
                            try
                            {
                                await DeviceManager.Instance.AddDevice(t);
                                await jsRuntime.InvokeVoidAsync(
                                    "QuickNV.postMessageToParent",
                                    JsonSerializer.Serialize(new
                                    {
                                        code = 0,
                                        message = $"添加设备[{t.Name}]成功",
                                        data = t
                                    }),
                                    "*");
                                modalAlert.Show("成功", $"添加设备[{t.Name}]成功");
                            }
                            catch (Exception ex)
                            {
                                modalAlert.Show("错误", $"添加设备[{t.Name}]时出错！原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                            }
                            modalLoading.Close();
                        });
                    }).ToDictionary()
            );
            InvokeAsync(StateHasChanged);
        }
    }
}
