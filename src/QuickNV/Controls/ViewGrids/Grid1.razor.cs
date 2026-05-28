using Microsoft.AspNetCore.Components;

namespace QuickNV.Controls.ViewGrids
{
    public partial class Grid1
    {
        [Parameter]
        public IViewGridContainer Container { get; set; }
        [Parameter]
        public ViewGrid ViewModel { get; set; }

        public Grid1()
        {
            ViewModel = new ViewGrid(this.GetType().FullName, "单画面", 1);
        }

        protected override void OnParametersSet()
        {
            Container.SetViewGrid(ViewModel);
        }
    }
}
