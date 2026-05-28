using Microsoft.AspNetCore.Components;

namespace QuickNV.Components.Controls.ViewGrids
{
    public partial class Grid16
    {
        [Parameter]
        public IViewGridContainer Container { get; set; }
        public ViewGrid ViewModel { get; private set; }

        public Grid16()
        {
            ViewModel = new ViewGrid(this.GetType().FullName, "十六画面", 16);
        }

        protected override void OnParametersSet()
        {
            Container.SetViewGrid(ViewModel);
        }
    }
}
