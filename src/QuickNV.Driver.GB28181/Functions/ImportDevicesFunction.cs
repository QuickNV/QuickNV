using Quick.Fields;
using Quick.Protocol;
using System.Collections.Generic;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Agent;
using QuickNV.Protocol.Driver.QpCommands.ImportDevices;
using QuickNV.Protocol.Driver.QpModels;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static QuickNV.Driver.GB28181.DeviceConfig;

namespace QuickNV.Driver.GB28181.Functions
{
    public static class ImportDevicesFunction
    {
        private const string VAR_DEVICES = nameof(VAR_DEVICES);
        private const string BTN_IMPORT_ALL = nameof(BTN_IMPORT_ALL);
        private const string BTN_IMPORT_SELECTED = nameof(BTN_IMPORT_SELECTED);

        private static List<FieldForGet> innerGet(FunctionRequest functionRequest)
        {
            var list = new List<FieldForGet>();
            var sipDevices = Agent.Instance.SipServer.GetDevices().Select(t => t.Model).ToArray();
            var waitImportDeviceList = new List<DriverDevice<DeviceConfig, ChannelConfig>>();
            foreach (var sipDevice in sipDevices)
            {
                if (Agent.Instance.GetDevice(sipDevice.Id) != null)
                    continue;
                waitImportDeviceList.Add(sipDevice);
            }
            if (waitImportDeviceList.Count > 0)
            {
                list.Add(new FieldForGet()
                {
                    Id = BTN_IMPORT_SELECTED,
                    Name = "导入选中设备",
                    Type = FieldType.Button
                });
                list.Add(new FieldForGet()
                {
                    Id = BTN_IMPORT_ALL,
                    Name = "导入全部",
                    MarginLeft = 1,
                    Type = FieldType.Button
                });
                var itemList = new List<FieldForGet>();
                foreach (var sipDevice in waitImportDeviceList)
                {
                    var id = sipDevice.Id;
                    var name = $"{sipDevice.Name}({sipDevice.Id})";
                    var value = functionRequest == null ? false.ToString() : functionRequest.GetFieldValue(VAR_DEVICES, id);
                    if (value == false.ToString())
                        name = $"[ ] {name}";
                    else
                        name = $"[*] {name}";
                    itemList.Add(new FieldForGet()
                    {
                        Id = id,
                        Name = name,
                        Type = FieldType.Button,
                        PostOnChanged = true,
                        Value = value,
                        InputButton_IsBlock = true
                    });
                }
                list.Add(new FieldForGet()
                {
                    Id = VAR_DEVICES,
                    Name = "设备列表",
                    Type = FieldType.ContainerGroup,
                    Children = itemList.ToArray()
                });
            }
            else
            {
                list.Add(new FieldForGet()
                {
                    Type = FieldType.Alert,
                    Name = "提示",
                    Description = "暂时未发现可以导入的设备"
                });
            }
            return list;
        }
        public static Response Invoke(QpChannel channel, Request request)
        {
            FunctionRequest functionRequest = null;
            if (request.Fields != null)
            {
                functionRequest = new FunctionRequest();
                functionRequest.Fields = request.Fields;
                functionRequest.FieldIds = request.FieldIds;
            }

            if (request.FieldIds != null)
            {
                if (functionRequest.FieldIds[0] == VAR_DEVICES)
                {
                    var deviceId = functionRequest.FieldIds[1];
                    var devicesField = functionRequest.Fields.FirstOrDefault(t => t.Id == VAR_DEVICES);
                    var deviceField = devicesField.Children.FirstOrDefault(t => t.Id == deviceId);
                    if (deviceField.Value == false.ToString())
                        deviceField.Value = true.ToString();
                    else
                        deviceField.Value = false.ToString();
                    return new Response()
                    {
                        Fields = innerGet(functionRequest).ToArray()
                    };
                }
                else if (functionRequest.IsFieldIdsMatch(BTN_IMPORT_SELECTED))
                {
                    var devicesField = functionRequest.Fields.FirstOrDefault(t => t.Id == VAR_DEVICES);
                    var deviceAndChannelsList = new List<DeviceAndChannelsInfo>();
                    foreach (var deviceField in devicesField.Children)
                    {
                        if (deviceField.Value == false.ToString())
                            continue;
                        var deviceContext = Agent.Instance.SipServer.GetDevice(deviceField.Id);
                        if (deviceContext == null)
                            continue;
                        var model = deviceContext.Model;
                        model.DriverId = Agent.Instance.DriverInfo.Id;
                        deviceAndChannelsList.Add(new DeviceAndChannelsInfo()
                        {
                            Device = model,
                            Channels = deviceContext.GetChannels().Select(t => t.Model).ToArray()
                        });
                    }
                    return new Response() { Devices = deviceAndChannelsList.ToArray() };
                }
                else if (functionRequest.IsFieldIdsMatch(BTN_IMPORT_ALL))
                {
                    var devicesField = functionRequest.Fields.FirstOrDefault(t => t.Id == VAR_DEVICES);
                    var deviceAndChannelsList = new List<DeviceAndChannelsInfo>();
                    foreach (var deviceField in devicesField.Children)
                    {
                        var deviceContext = Agent.Instance.SipServer.GetDevice(deviceField.Id);
                        if (deviceContext == null)
                            continue;
                        var model = deviceContext.Model;
                        model.DriverId = Agent.Instance.DriverInfo.Id;
                        deviceAndChannelsList.Add(new DeviceAndChannelsInfo()
                        {
                            Device = model,
                            Channels = deviceContext.GetChannels().Select(t => t.Model).ToArray()
                        });
                    }
                    return new Response() { Devices = deviceAndChannelsList.ToArray() };
                }
            }
            return new Response()
            {
                Fields = innerGet(functionRequest).ToArray()
            };
        }
    }
}
