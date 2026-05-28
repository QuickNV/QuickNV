using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using QuickNV.Model;

namespace QuickNV.Components.Controls;

public partial class ChannelPlaybackManage : ComponentBase
{
    [Parameter]
    public Model.Channel Channel { get; set; }

    public static DialogParameters<ChannelPlaybackManage> PrepareParameter(Channel channel)
    {
        return new DialogParameters<ChannelPlaybackManage>()
        {
            {t=>t.Channel,channel}
        };
    }
}