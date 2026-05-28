using Microsoft.AspNetCore.Components;
using Quick.Blazor.Bootstrap;
using Quick.Fields;
using System.ComponentModel;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Core;
using QuickNV.Model;
using QuickNV.Utils;

namespace QuickNV.Components.Controls
{
    public partial class ChannelImportControl : IDisposable
    {
        [Parameter]
        public Model.Device Device { get; set; }
        [Parameter]
        public Action<DriverContext, Model.Channel[]> OkAction { get; set; }

        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ToastStack toastStack;
        private DriverContext driverContext;

        public static DialogParameters<ChannelImportControl> PrepareParameter(Model.Device device, Action<DriverContext, Model.Channel[]> okAction)
        {
            return new DialogParameters<ChannelImportControl>()
            {
                {t=>t.Device,device},
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
            modalLoading.Show("处理中", null, true);
            Task.Run(async () =>
            {
                try
                {
                    var rep = await driverContext.ImportChannels(
                        Device.Id,
                        field.GetFullFieldIds().Where(t => t != null).ToArray(),
                        fieldForGetArray.Select(t => t.ToPost()).ToArray()
                    );
                    if (rep.Channels != null)
                    {
                        var channels = rep.Channels.Select(t =>
                        {
                            var model = DataUtils.Convert<Model.Channel>(t);
                            model.DeviceId = Device.Id;
                            return model;
                        }).ToArray();
                        OkAction(driverContext, channels);
                    }
                    else
                    {
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
                modalLoading.Close();
                await InvokeAsync(StateHasChanged);
            });
        }

        protected override void OnParametersSet()
        {
            driverContext = Device.GetDriverContext();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                modalLoading.Show("处理中", null, true);
                if (driverContext != null)
                {
                    try
                    {
                        var rep = await driverContext.ImportChannels(Device.Id, null, null);
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
                }
                modalLoading.Close();
                await InvokeAsync(StateHasChanged);
            }
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
