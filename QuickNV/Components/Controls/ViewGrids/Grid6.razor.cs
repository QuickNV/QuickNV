using Microsoft.AspNetCore.Components;

namespace QuickNV.Components.Controls.ViewGrids
{
    public partial class Grid6
    {
        [Parameter]
        public IViewGridContainer Container { get; set; }
        public ViewGrid ViewModel { get; private set; }

        public Grid6()
        {
            ViewModel = new ViewGrid(this.GetType().FullName, "六画面", 6);
        }

        protected override void OnParametersSet()
        {
            Container.SetViewGrid(ViewModel);
        }
    }
}
