using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using QuickNV.Core;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Controls
{
    public partial class MediaServerMediaInfosView
    {
        private string keywords;
        private MediaInfo[] mediaInfos;

        [Parameter]
        public MediaServerContext MediaServer { get; set; }

        private void search()
        {
            mediaInfos = MediaServer.GetMediaInfos();
            if (!string.IsNullOrEmpty(keywords))
                mediaInfos = mediaInfos.Where(t => t.MediaId.ToString() == keywords
                || t.SSRC.Contains(keywords)
                || t.StreamId.Contains(keywords)
                || t.Channel.Name.Contains(keywords)).ToArray();
            InvokeAsync(StateHasChanged);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
                search();
        }

        public static DialogParameters<MediaServerMediaInfosView> PrepareParameter(MediaServerContext mediaServer)
        {
            return new DialogParameters<MediaServerMediaInfosView>()
            {
                {t=>t.MediaServer,mediaServer}
            };
        }
    }
}
