using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.EntityFrameworkCore.Plus;
using QuickNV.Controls;
using QuickNV.Core;

namespace QuickNV.Pages
{
    public partial class DeviceChannelList
    {
        private RenderFragment Body { get; set; }
        private string DeviceId { get; set; }
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
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<ChannelManage>
            (
                ChannelManage.PrepareParameter(device, () => { }).ToDictionary()
            );
            InvokeAsync(StateHasChanged);
        }
    }
}
