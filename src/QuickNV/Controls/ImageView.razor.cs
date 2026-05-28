using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;

namespace QuickNV.Controls
{
    public partial class ImageView
    {
        [Parameter]
        public string ImageUrl { get; set; }

        public static DialogParameters<ImageView> PrepareParameter(string imageUrl)
        {
            return new DialogParameters<ImageView>()
            {
                {t=>t.ImageUrl,imageUrl}
            };
        }
    }
}
