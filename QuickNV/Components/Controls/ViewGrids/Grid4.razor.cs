using Microsoft.AspNetCore.Components;

namespace QuickNV.Components.Controls.ViewGrids
{
    public partial class Grid4
    {
        [Parameter]
        public IViewGridContainer Container { get; set; }
        public ViewGrid ViewModel { get; private set; }

        public Grid4()
        {
            ViewModel = new ViewGrid(this.GetType().FullName, "四画面", 4);
        }

        protected override void OnParametersSet()
        {
            Container.SetViewGrid(ViewModel);
        }
    }
}
