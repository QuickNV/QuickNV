using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Controls.Pages
{
    public partial class Dashboard : IDisposable
    {
        private int OnlineDeviceCount = 0;
        private int AllDeviceCount = 0;
        private int AllChannelCount = 0;

        private Timer refreshTimer;
        
        protected override void OnInitialized()
        {
            refreshCount();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                refreshTimer = new Timer(refresh, null, 0, 1000);
        }

        private void refreshCount()
        {
            var devices = ConfigDbContext.CacheContext.Query<Model.Device>();
            AllDeviceCount = devices.Length;
            OnlineDeviceCount = devices.Count(t => t.IsOnline);
            AllChannelCount = devices.Sum(t => t.ChannelsCount);
        }
        private void refresh(object _)
        {
            refreshCount();
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            refreshTimer.Dispose();
        }
    }
}
