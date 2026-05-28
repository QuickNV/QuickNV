using Microsoft.AspNetCore.Components;

namespace QuickNV.Controls.Pages
{
    public partial class Exit
    {
        [Inject]
        private NavigationManager navigationManager { get; set; }

        private void exit()
        {
            navigationManager.NavigateTo("./api/login/logout", true);
        }
    }
}
