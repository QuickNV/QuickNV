using Microsoft.AspNetCore.Components;

namespace QuickNV.Controls.ViewGrids
{
    public partial class Grid9
    {
        [Parameter]
        public IViewGridContainer Container { get; set; }
        public ViewGrid ViewModel { get; private set; }

        public Grid9()
        {
            ViewModel = new ViewGrid(this.GetType().FullName, "九画面", 9);
        }

        protected override void OnParametersSet()
        {
            Container.SetViewGrid(ViewModel);
        }
    }
}
