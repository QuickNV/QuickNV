using QuickNV.DahuaNetSDK;
using QuickNV.DahuaNetSDK.Api;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Protocol.QpCommands.ImportDevices;
using QuickNV.Driver.Protocol.QpModels;
using System.Text.Json;
using Quick.Utils;

namespace QuickNV.Driver.DahuaDeviceNetwork.Functions
{
    public class ImportDevicesFunction
    {
        private const string VAR_STEP = nameof(VAR_STEP);
        private const string STEP_1 = nameof(STEP_1);
        private const string STEP_1_VAR_HOST = nameof(STEP_1_VAR_HOST);
        private const string STEP_1_VAR_PORT = nameof(STEP_1_VAR_PORT);
        private const string STEP_1_VAR_USERNAME = nameof(STEP_1_VAR_USERNAME);
        private const string STEP_1_VAR_PASSWORD = nameof(STEP_1_VAR_PASSWORD);
        private const string STEP_1_VAR_LOGINTYPE = nameof(STEP_1_VAR_LOGINTYPE);
        private const string STEP_1_BUTTON_NEXT = nameof(STEP_1_BUTTON_NEXT);

        private const string STEP_2 = nameof(STEP_2);
        private const string STEP_2_VAR_DEVICE_ID = nameof(STEP_2_VAR_DEVICE_ID);
        private const string STEP_2_VAR_DEVICE_NAME = nameof(STEP_2_VAR_DEVICE_NAME);
        private const string STEP_2_VAR_RTSP_PORT = nameof(STEP_2_VAR_RTSP_PORT);
        private const string STEP_2_BUTTON_IMPORT = nameof(STEP_2_BUTTON_IMPORT);

        public static string CorrectPort(string portStr)
        {
            if (string.IsNullOrEmpty(portStr))
                return ushort.MinValue.ToString();
            if (!int.TryParse(portStr, out var port))
                return ushort.MinValue.ToString();
            if (port > ushort.MaxValue)
                port = ushort.MaxValue;
            if (port < ushort.MinValue)
                port = ushort.MinValue;
            return port.ToString();
        }

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
            var tmpKey = nameof(STEP_1_VAR_HOST);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "主机",
                Type = FieldType.InputText,
                Value = functionRequest == null ? string.Empty : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });
            tmpKey = nameof(STEP_1_VAR_PORT);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "端口",
                Description = "TCP默认值：37777，UDP默认值：37778",
                Type = FieldType.InputNumber,
                Value = functionRequest == null ? "37777" : CorrectPort(functionRequest.GetFieldValue(tmpKey))
            });
            tmpKey = nameof(STEP_1_VAR_USERNAME);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "用户名",
                Type = FieldType.InputText,
                Value = functionRequest == null ? string.Empty : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });
            tmpKey = nameof(STEP_1_VAR_PASSWORD);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "密码",
                Type = FieldType.InputPassword,
                Value = functionRequest == null ? string.Empty : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });
            tmpKey = nameof(STEP_1_VAR_LOGINTYPE);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "登录类型",
                Type = FieldType.InputSelect,
                Value = functionRequest == null ? EM_LOGIN_SPAC_CAP_TYPE.TCP.ToString() : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    [EM_LOGIN_SPAC_CAP_TYPE.TCP.ToString()] = EM_LOGIN_SPAC_CAP_TYPE.TCP.ToString(),
                    [EM_LOGIN_SPAC_CAP_TYPE.UDP.ToString()] = EM_LOGIN_SPAC_CAP_TYPE.UDP.ToString()
                }
            });
            list.Add(new FieldForGet()
            {
                Id = STEP_1_BUTTON_NEXT,
                Name = "下一步",
                Type = FieldType.Button
            });
            return list;
        }

        private static List<FieldForGet> step_2(FunctionRequest functionRequest)
        {
            var host = functionRequest.GetFieldValue(STEP_1_VAR_HOST);
            var port = int.Parse(functionRequest.GetFieldValue(STEP_1_VAR_PORT));
            var username = functionRequest.GetFieldValue(STEP_1_VAR_USERNAME);
            var password = functionRequest.GetFieldValue(STEP_1_VAR_PASSWORD);
            var loginType = functionRequest.GetFieldValue(STEP_1_VAR_LOGINTYPE);
            var list = new List<FieldForGet>
            {
                new FieldForGet()
                {
                    Id = VAR_STEP,
                    Type = FieldType.InputHidden,
                    Value = STEP_2
                },
                new FieldForGet()
                {
                    Id = STEP_1_VAR_HOST,
                    Type = FieldType.InputHidden,
                    Value = host
                },
                new FieldForGet()
                {
                    Id = STEP_1_VAR_PORT,
                    Type = FieldType.InputHidden,
                    Value = port.ToString()
                },
                new FieldForGet()
                {
                    Id = STEP_1_VAR_USERNAME,
                    Type = FieldType.InputHidden,
                    Value = username
                },
                new FieldForGet()
                {
                    Id = STEP_1_VAR_PASSWORD,
                    Type = FieldType.InputHidden,
                    Value = password
                },
                new FieldForGet()
                {
                    Id = STEP_1_VAR_PASSWORD,
                    Type = FieldType.InputHidden,
                    Value = password
                },
                new FieldForGet()
                {
                    Id = STEP_1_VAR_LOGINTYPE,
                    Type = FieldType.InputHidden,
                    Value = loginType
                }
            };
            var defaultDeviceId = string.Empty;
            var defaultDeviceName = string.Empty;
            using (var session = DhSession.Login(host, port, username, password,Enum.Parse<EM_LOGIN_SPAC_CAP_TYPE>(loginType)))
            {
                defaultDeviceId = session.ConfigService.GetDeviceSerialNumber();
                defaultDeviceName = session.ConfigService.GetMachineName();
            }
            var tmpKey = STEP_2_VAR_DEVICE_ID;
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "编号",
                Type = FieldType.InputText,
                Value = defaultDeviceId,
                Input_AllowBlank = false
            });
            tmpKey = STEP_2_VAR_DEVICE_NAME;
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "名称",
                Type = FieldType.InputText,
                Value = defaultDeviceName,
                Input_AllowBlank = false
            });
            tmpKey = nameof(STEP_2_VAR_RTSP_PORT);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "RTSP端口",
                Description = "默认值：0",
                Type = FieldType.InputNumber,
                Value = functionRequest == null ? "0" : CorrectPort(functionRequest.GetFieldValue(tmpKey))
            });
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
                    var host = functionRequest.GetFieldValue(STEP_1_VAR_HOST);
                    var port = int.Parse(functionRequest.GetFieldValue(STEP_1_VAR_PORT));
                    var username = functionRequest.GetFieldValue(STEP_1_VAR_USERNAME);
                    var password = functionRequest.GetFieldValue(STEP_1_VAR_PASSWORD);
                    var loginType = functionRequest.GetFieldValue(STEP_1_VAR_LOGINTYPE);
                    var rtspPort = int.Parse(functionRequest.GetFieldValue(STEP_2_VAR_RTSP_PORT));

                    DeviceInfo deviceInfo = null;
                    var channelList = new List<ChannelInfo>();
                    using (var session = DhSession.Login(host, port, username, password, Enum.Parse<EM_LOGIN_SPAC_CAP_TYPE>(loginType)))
                    {
                        deviceInfo = new DeviceInfo()
                        {
                            Id = functionRequest.GetFieldValue(STEP_2_VAR_DEVICE_ID),
                            Name = functionRequest.GetFieldValue(STEP_2_VAR_DEVICE_NAME),
                            Manufacturer = "Hikvision",
                            Model = session.ConfigService.GetDeviceType(),
                            SerialNumber = session.ConfigService.GetDeviceSerialNumber(),
                            DriverId = Agent.Instance.DriverInfo.Id,
                            DriverConfig = JsonSerializer.Serialize(new DeviceConfig()
                            {
                                Host = host,
                                Port = port,
                                UserName = username,
                                Password = password,
                                RtspPort = rtspPort,
                                LoginType = Enum.Parse<EM_LOGIN_SPAC_CAP_TYPE>(loginType)
                            }, new JsonSerializerOptions() { WriteIndented = true })
                        };
                        session.ChannelService.RefreshChannelsName();
                        foreach (var hvChannel in session.ChannelService.AllChannels)
                        {
                            channelList.Add(new ChannelInfo()
                            {
                                DeviceId = deviceInfo.Id,
                                Id = hvChannel.Id.ToString(),
                                Name = hvChannel.Name,
                                DriverConfig = JsonSerializer.Serialize(new ChannelConfig()
                                {
                                    ChannelId = hvChannel.Id,
                                    StreamType = DhStreamType.Sub
                                }, new JsonSerializerOptions() { WriteIndented = true })
                            });
                        }
                        session.Logout();
                    }

                    return new Response()
                    {
                        Devices = new DeviceAndChannelsInfo[]
                        {
                            new DeviceAndChannelsInfo()
                            {
                                Device = deviceInfo,
                                Channels=channelList.ToArray()
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
                    try
                    {
                        fields = step_2(functionRequest).ToArray();
                    }
                    catch (Exception ex)
                    {
                        var list = step_1(functionRequest);
                        list.Insert(0, new FieldForGet()
                        {
                            Type = FieldType.Alert,
                            Name = "错误",
                            Description = "导入时出错，原因：" + ExceptionUtils.GetExceptionMessage(ex)
                        });
                        fields = list.ToArray();
                    }
                    break;
            }
            return new Response()
            {
                Fields = fields
            };
        }
    }
}
