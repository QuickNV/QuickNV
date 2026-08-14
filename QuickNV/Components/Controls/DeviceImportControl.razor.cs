using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Quick.Blazor.Bootstrap;
using Quick.Fields;
using System.ComponentModel;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Core;
using QuickNV.Utils;
using Quick.Utils;

namespace QuickNV.Components.Controls
{
    public partial class DeviceImportControl : IDisposable
    {
        [Parameter]
        public Action<DriverContext, Model.Device[], Model.Channel[]> OkAction { get; set; }
        private ModalLoading modalLoading;
        private ModalAlert modalAlert;
        private ToastStack toastStack;
        private DriverContext driverContext;

        public static DialogParameters<DeviceImportControl> PrepareParameter(Action<DriverContext,Model.Device[], Model.Channel[]> okAction)
        {
            return new DialogParameters<DeviceImportControl>()
            {
                {t=>t.OkAction,okAction}
            };
        }
        private string _DriverId;
        private string DriverId
        {
            get { return _DriverId; }
            set
            {
                _DriverId = value;
                _ = onDriverIdChanged(value);
            }
        }

        private FieldForGet[] fieldForGetArray;


        private void travelFields(IEnumerable<FieldForGet> fields, Action<FieldForGet> action)
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
                        modalAlert.Show(field.Name, field.Description);
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
                    var rep = await driverContext.ImportDevices(
                        field.GetFullFieldIds().Where(t => t != null).ToArray(),
                        [..fieldForGetArray.Select(t => t.ToPost())]
                    );
                    if (rep.Devices != null)
                    {
                        List<Model.Device> deviceList = new List<Model.Device>();
                        List<Model.Channel> channelList = new List<Model.Channel>();
                        foreach (var item in rep.Devices)
                        {
                            var device = DataUtils.Convert<Model.Device>(item.Device);
                            device.DriverId = DriverId;
                            deviceList.Add(device);

                            if (item.Channels != null)
                            {
                                foreach (var channelInfo in item.Channels)
                                {
                                    var channel = DataUtils.Convert<Model.Channel>(channelInfo);
                                    channel.DeviceId = device.Id;
                                    channelList.Add(channel);
                                }
                            }
                        }
                        OkAction(driverContext, deviceList.ToArray(), channelList.ToArray());
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

        private async Task onDriverIdChanged(string driverId)
        {
            driverContext = DriverManager.Instance.GetDriverContext(driverId);
            if (driverContext == null)
                return;
            modalLoading.Show("加载中", "正在加载驱动配置...", true);
            try
            {
                var rep = await driverContext.ImportDevices(null, null);
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
