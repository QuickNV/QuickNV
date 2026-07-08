using Microsoft.AspNetCore.Components;

namespace QuickNV.Components.Controls.ViewGrids
{
    public partial class ViewGridCell : ComponentBase
    {
        [Parameter]
        public ViewGrid ViewModel { get; set; }
        [Parameter]
        public int Index { get; set; }
        private bool showAlertBorder = false;

        protected override void OnParametersSet()
        {
            ViewModel.SetViewGridCell(Index, this);
        }

        private string GetCellCssClass()
        {
            if (showAlertBorder)
                return "Alert";
            return null;
        }

        public async Task FlashAsync()
        {
            for (var i = 0; i < 5; i++)
            {
                showAlertBorder = true;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(200);
                showAlertBorder = false;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(200);
            }
        }
    }
}
