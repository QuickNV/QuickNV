using Microsoft.AspNetCore.Components;

namespace QuickNV.Components.Controls.ViewGrids
{
    public partial class Grid24
    {
        [Parameter]
        public IViewGridContainer Container { get; set; }
        public ViewGrid ViewModel { get; private set; }

        public Grid24()
        {
            ViewModel = new ViewGrid(this.GetType().FullName, "二十四画面", 24);
        }

        protected override void OnParametersSet()
        {
            Container.SetViewGrid(ViewModel);
        }
    }
}
