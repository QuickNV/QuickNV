using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Quick.Blazor.Bootstrap;
using Quick.Fields;
using System.ComponentModel;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Core;
using QuickNV.Utils;

namespace QuickNV.Components.Controls
{
    public partial class ChannelCreateControl : IDisposable
    {
        private Model.Channel createModel = new Model.Channel();
        [Parameter]
        public Model.Device Device { get; set; }
        [Parameter]
        public Model.Channel Model { get; set; }
        [Parameter]
        public Action<Model.Channel> OkAction { get; set; }
        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ToastStack toastStack;
        private DriverContext driverContext;

        private void Ok()
        {
            OkAction?.Invoke(createModel);
        }

        protected override void OnParametersSet()
        {
            driverContext = DriverManager.Instance.GetDriverContext(Device.DriverId);
            if (Model != null)
            {
                createModel = DataUtils.Clone(Model);
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                _ = loadDriverConfig();
            }
        }

        public static DialogParameters<ChannelCreateControl> PrepareParameter(Model.Device device, Model.Channel model, Action<Model.Channel> okAction)
        {
            return new DialogParameters<ChannelCreateControl>()
            {
                {t=>t.Device,device},
                {t=>t.Model,model},
                {t=>t.OkAction,okAction}
            };
        }

        private FieldForGet[] fieldForGetArray;


        private void travelFields(FieldForGet[] fields, Action<FieldForGet> action)
        {
            if (fields == null)
                return;
            foreach (var field in fields)
            {
                action.Invoke(field);
                travelFields(field.Children, action);
            }
        }

        private void setFields(FieldForGet[] fields)
        {
            travelFields(fieldForGetArray, field =>
            {
                if (field.PostOnChanged.HasValue && field.PostOnChanged.Value)
                    field.PropertyChanged -= OnFieldValueChanged;
            });
            fieldForGetArray = fields;
            travelFields(fieldForGetArray, field =>
            {
                switch (field.Type)
                {
                    case FieldType.MessageBox:
                        modalAlert.Show(field.Name, field.Description, null, null);
                        return;
                    case FieldType.Toast:
                        toastStack.AddToast(field.Name, field.Description, BackgroundTheme.info);
                        return;
                    case FieldType.Button:
                        field.PostOnChanged = true;
                        break;
                }
                if (field.PostOnChanged.HasValue && field.PostOnChanged.Value)
                    field.PropertyChanged += OnFieldValueChanged;
            });
        }

        private void OnFieldValueChanged(object sender, PropertyChangedEventArgs e)
        {
            var field = (FieldForGet)sender;
            OnFieldChanged(field);
        }

        private void OnFieldChanged(FieldForGet field)
        {
            modalLoading.Show("处理中", null, true, null);
            Task.Run(async () =>
            {
                try
                {
                    var rep = await driverContext.GetChannelConfig(
                        Device.Id,
                        null,
                        field.GetFullFieldIds().Where(t => t != null).ToArray(),
                        fieldForGetArray.Select(t => t.ToPost()).ToArray());
                    createModel.DriverConfig = rep.Config;
                    setFields(rep.Fields);
                }
                catch (Exception ex)
                {
                    setFields(new[] {
                        new FieldForGet()
                        {
                            Type = FieldType.Alert,
                            Name = "错误",
                            Description=ExceptionUtils.GetExceptionString(ex)
                        }
                    });
                }
                modalLoading.Close();
                await InvokeAsync(StateHasChanged);
            });
        }

        private async Task loadDriverConfig()
        {
            if (driverContext == null)
                return;
            modalLoading.Show("加载中", "正在加载驱动配置...", true);
            try
            {
                var rep = await driverContext.GetChannelConfig(Device.Id, createModel.DriverConfig, null, null);
                createModel.DriverConfig = rep.Config;
                setFields(rep.Fields);
            }
            catch (Exception ex)
            {
                setFields(new[] {
                        new FieldForGet()
                        {
                            Type = FieldType.Alert,
                            Name = "错误",
                            Description=ExceptionUtils.GetExceptionString(ex)
                        }
                    });
            }
            await InvokeAsync(StateHasChanged);
            modalLoading.Close();
        }

        public void Dispose()
        {
            travelFields(fieldForGetArray, field =>
            {
                if (field.PostOnChanged.HasValue && field.PostOnChanged.Value)
                    field.PropertyChanged -= OnFieldValueChanged;
            });
        }
    }
}
