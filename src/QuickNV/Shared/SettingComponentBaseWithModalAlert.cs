using Quick.Blazor.Bootstrap;
using Quick.Blazor.Bootstrap.Utils;

namespace QuickNV.Shared
{
    public abstract class SettingComponentBaseWithModalAlert<T> : SettingComponentBase<T>
            where T : new()
    {
        protected ModalAlert modalAlert;

        protected new void save()
        {
            try
            {
                base.save();
                modalAlert?.Show("成功", "保存成功！");
            }
            catch (Exception ex)
            {
                modalAlert?.Show("保存", "保存失败，原因：" + ExceptionUtils.GetExceptionMessage(ex));
            }
        }
    }
}
