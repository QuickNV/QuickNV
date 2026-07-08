namespace QuickNV.Components.Controls.Pages
{
    public partial class DriverManage : IDisposable
    {
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                Core.DriverManager.Instance.DriverChanged += DriverChanged_DriverChanged;
            }
        }

        private void DriverChanged_DriverChanged(object sender, EventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Core.DriverManager.Instance.DriverChanged -= DriverChanged_DriverChanged;
        }
    }
}
