using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Utils;
using Quick.Utils;

namespace QuickNV.Components.Controls.Pages
{
    public partial class ConfigManage
    {
        private string ApiKey;
        private ModalAlert modalAlert;

        protected override void OnInitialized()
        {
            ApiKey = Core.ApiKeyManager.Instance.GetApiKey();
        }

        private void Save()
        {
            try
            {
                Core.ApiKeyManager.Instance.SetApiKey(ApiKey);
                modalAlert.Show("成功", "保存ApiKey成功！");
            }
            catch (Exception ex)
            {
                modalAlert.Show("错误", "保存ApiKey失败，原因：" + ExceptionUtils.GetExceptionMessage(ex));
            }
        }
    }
}
