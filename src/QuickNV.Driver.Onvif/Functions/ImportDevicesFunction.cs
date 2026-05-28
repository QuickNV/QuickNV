using System.Text.Json;
using Quick.Fields;
using QuickNV.Onvif;
using QuickNV.Onvif.Discovery;
using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Protocol.QpCommands.ImportDevices;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Onvif.Functions
{
    public class ImportDevicesFunction
    {
        private const string VAR_STEP = nameof(VAR_STEP);
        private const string STEP_1 = nameof(STEP_1);
        private const string STEP_1_VAR_INTERFACE_IPADDRSS = nameof(STEP_1_VAR_INTERFACE_IPADDRSS);
        private const string STEP_1_BUTTON_DISCOVERY = nameof(STEP_1_BUTTON_DISCOVERY);
        private const string STEP_1_VAR_SERVICE_ADDRESS = nameof(STEP_1_VAR_SERVICE_ADDRESS);
        private const string STEP_1_BUTTON_NEXT = nameof(STEP_1_BUTTON_NEXT);

        private const string STEP_2 = nameof(STEP_2);
        private const string STEP_2_VAR_DEVICE_ID = nameof(STEP_2_VAR_DEVICE_ID);
        private const string STEP_2_VAR_DEVICE_NAME = nameof(STEP_2_VAR_DEVICE_NAME);
        private const string STEP_2_BUTTON_IMPORT = nameof(STEP_2_BUTTON_IMPORT);

        private static List<FieldForGet> step_1(FunctionRequest functionRequest)
        {
            var list = new List<FieldForGet>
            {
                new FieldForGet()
                {
                    Id = VAR_STEP,
                    Type = FieldType.InputHidden,
                    Value = STEP_1
                }
            };
            Dictionary<string, string> addressDict = new Dictionary<string, string>();
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up
                    || ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    continue;
                foreach (var unicastAddress in ni.GetIPProperties().UnicastAddresses)
                {
                    var key = unicastAddress.Address.ToString();
                    addressDict[key] = $"{key}({ni.Name})";
                }
            }

            var interfaceIpAddress = functionRequest == null ? addressDict.Keys.FirstOrDefault() : functionRequest.GetFieldValue(STEP_1_VAR_INTERFACE_IPADDRSS);
            list.Add(new FieldForGet()
            {
                Id = STEP_1_VAR_INTERFACE_IPADDRSS,
                Name = "网络接口",
                Type = FieldType.InputSelect,
                Value = interfaceIpAddress,
                PostOnChanged = true,
                Input_AllowBlank = false,
                InputSelect_Options = addressDict
            });
            if (!string.IsNullOrEmpty(interfaceIpAddress))
            {
                list.Add(new FieldForGet()
                {
                    Id = STEP_1_BUTTON_DISCOVERY,
                    Name = "搜索发现",
                    Type = FieldType.Button,
                    PostOnChanged = true,
                    Html_Class = "m-1"
                });
                //如果是点击了“搜索发现”按钮
                if (functionRequest != null && functionRequest.IsFieldIdsMatch(STEP_1_BUTTON_DISCOVERY))
                {
                    DiscoveryController2 controller = new DiscoveryController2();
                    try
                    {
                        var devices = controller.RunDiscovery(IPAddress.Parse(interfaceIpAddress)).Result;
                        var deviceDict = new Dictionary<string, string>();
                        foreach (var device in devices)
                        {
                            foreach (var serviceAddress in device.ServiceAddresses)
                                deviceDict[serviceAddress] = $"{device.EndPointAddress}({serviceAddress})";
                        }
                        list.Add(new FieldForGet()
                        {
                            Id = STEP_1_VAR_SERVICE_ADDRESS,
                            Name = "设备",
                            Type = FieldType.InputSelect,
                            Input_AllowBlank = false,
                            InputSelect_Options = deviceDict,
                            Value = deviceDict.Keys.FirstOrDefault()
                        });
                        list.Add(new FieldForGet()
                        {
                            Id = STEP_1_BUTTON_NEXT,
                            Name = "下一步",
                            Type = FieldType.Button,
                            PostOnChanged = true,
                            Html_Class = "m-1"
                        });
                    }
                    catch
                    {
                        list.Add(new FieldForGet()
                        {
                            Type = FieldType.Alert,
                            Name = "提示",
                            Description = "暂时未发现可以导入的设备"
                        });
                    }
                }
            }
            return list;
        }

        private static List<FieldForGet> step_2(FunctionRequest functionRequest)
        {
            var list = new List<FieldForGet>
            {
                new FieldForGet()
                {
                    Id = VAR_STEP,
                    Type = FieldType.InputHidden,
                    Value = STEP_2
                }
            };


            var tmpKey = STEP_2_VAR_DEVICE_ID;
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "编号",
                Type = FieldType.InputText,
                Value = functionRequest == null ? null : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });

            tmpKey = STEP_2_VAR_DEVICE_NAME;
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "名称",
                Type = FieldType.InputText,
                Value = functionRequest == null ? null : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });

            var step_1_serviceAddress = functionRequest.GetFieldValue(STEP_1_VAR_SERVICE_ADDRESS);
            if (step_1_serviceAddress != null)
            {
                var uri = new Uri(step_1_serviceAddress);
                var fieldList = new List<FieldForPost>(functionRequest.Fields)
                {
                    new FieldForPost() { Id = nameof(DeviceConfig.Host), Value = uri.Host },
                    new FieldForPost() { Id = nameof(DeviceConfig.Port), Value = uri.Port.ToString() },
                    new FieldForPost() { Id = nameof(DeviceConfig.ClientCredentialType), Value = "Digest" },
                    new FieldForPost() { Id = nameof(DeviceConfig.UserName) },
                    new FieldForPost() { Id = nameof(DeviceConfig.Password) },
                    new FieldForPost() { Id = nameof(DeviceConfig.Scheme), Value = uri.Scheme }
                };
                functionRequest.Fields = fieldList.ToArray();
            }

            var rep = GetDeviceConfigFunction.Invoke(null, new Protocol.QpCommands.GetDeviceConfig.Request()
            {
                FieldIds = functionRequest.FieldIds,
                Fields = functionRequest.Fields
            });
            list.AddRange(rep.Fields);
            list.Add(new FieldForGet()
            {
                Id = STEP_2_BUTTON_IMPORT,
                Name = "导入",
                Type = FieldType.Button,
                PostOnChanged = true,
                Html_Class = "m-1"
            });
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
            var currentStep = functionRequest == null ? STEP_1 : functionRequest.GetFieldValue(VAR_STEP);
            if (functionRequest != null)
            {
                if (functionRequest.IsFieldIdsMatch(STEP_1_BUTTON_NEXT))
                {
                    currentStep = STEP_2;
                }
                else if (functionRequest.IsFieldIdsMatch(STEP_2_BUTTON_IMPORT))
                {
                    var rep = GetDeviceConfigFunction.Invoke(null, new Protocol.QpCommands.GetDeviceConfig.Request()
                    {
                        FieldIds = functionRequest.FieldIds,
                        Fields = functionRequest.Fields
                    });

                    var ovnifClient = new OnvifClient(JsonSerializer.Deserialize<OnvifClientOptions>(rep.Config));
                    try
                    {
                        ovnifClient.ConnectAsync().Wait();
                    }
                    catch { }

                    return new Response()
                    {
                        Devices = new DeviceAndChannelsInfo[]
                        {
                            new DeviceAndChannelsInfo()
                            {
                                Device = new DeviceInfo()
                                {
                                    Id = functionRequest.GetFieldValue(STEP_2_VAR_DEVICE_ID),
                                    Name=functionRequest.GetFieldValue(STEP_2_VAR_DEVICE_NAME),
                                    Manufacturer=ovnifClient.DeviceInformation?.Manufacturer,
                                    Model = ovnifClient.DeviceInformation?.Model,
                                    SerialNumber=ovnifClient.DeviceInformation?.SerialNumber,
                                    DriverId =Agent.Instance.DriverInfo.Id,
                                    DriverConfig = rep.Config
                                }
                            }
                        }
                    };
                }
            }
            FieldForGet[] fields = null;

            switch (currentStep)
            {
                case STEP_1:
                    fields = step_1(functionRequest).ToArray();
                    break;
                case STEP_2:
                    fields = step_2(functionRequest).ToArray();
                    break;
            }
            return new Response()
            {
                Fields = fields
            };
        }
    }
}
