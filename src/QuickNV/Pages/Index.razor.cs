using Microsoft.AspNetCore.Components;

namespace QuickNV.Pages
{
    public partial class Index
    {
        [Parameter]
        public RenderFragment Body { get; set; }

        private void Show<T>()
        {
            Body = Quick.Blazor.Bootstrap.Utils.BlazorUtils.GetRenderFragment<T>();
        }

        public string Title => "QuickNV";
    }
}
