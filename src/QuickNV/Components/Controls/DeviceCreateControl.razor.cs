using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Quick.Blazor.Bootstrap;
using Quick.Fields;
using System.ComponentModel;
using System.Text;
using YiQiDong.Core.Utils;
using QuickNV.Core;
using QuickNV.Utils;

namespace QuickNV.Components.Controls
{
    public partial class DeviceCreateControl : IDisposable
    {
        private bool isCreate;
        private Model.Device createModel = new Model.Device();
        [Parameter]
        public Model.Device Model { get; set; }
        [Parameter]
        public Action<Model.Device> OkAction { get; set; }
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
            base.OnParametersSet();
            isCreate = Model == null;
            if (Model != null)
            {
                createModel = DataUtils.Clone(Model);
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if(firstRender)
            {
                if (!string.IsNullOrEmpty(createModel.DriverId))
                    _ = onDriverIdChanged(createModel.DriverId);
            }
        }

        public static DialogParameters<DeviceCreateControl> PrepareParameter(Model.Device model, Action<Model.Device> okAction)
        {
            return new DialogParameters<DeviceCreateControl>()
            {
                {t=>t.Model,model},
                {t=>t.OkAction,okAction}
            };
        }
        private string ParameterDriverId
        {
            get { return createModel.DriverId; }
            set
            {
                createModel.DriverId = value;
                _ = onDriverIdChanged(value);
            }
        }

        private string GetHardwareInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("厂商：");
            sb.AppendLine(createModel.Manufacturer);
            sb.Append("型号：");
            sb.AppendLine(createModel.Model);
            sb.Append("序列号：");
            sb.AppendLine(createModel.SerialNumber);
            sb.Append("固件版本：");
            sb.AppendLine(createModel.FirmwareVersion);
            return sb.ToString();
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
                    var rep = await driverContext.Channel.SendCommand(new Driver.Protocol.QpCommands.GetDeviceConfig.Request()
                    {
                        FieldIds = field.GetFullFieldIds().Where(t => t != null).ToArray(),
                        Fields = fieldForGetArray.Select(t => t.ToPost()).ToArray()
                    });
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

        private async Task onDriverIdChanged(string driverId)
        {
            modalLoading.Show("加载中", "正在加载驱动配置...", true);
            try
            {
                driverContext = DriverManager.Instance.GetDriverContext(driverId);
                if (driverContext != null)
                {
                    var rep = await driverContext.Channel.SendCommand(new Driver.Protocol.QpCommands.GetDeviceConfig.Request()
                    {
                        Config = createModel.DriverConfig
                    });
                    createModel.DriverConfig = rep.Config;
                    setFields(rep.Fields);
                }
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
