using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Protocol.Driver.QpCommands.GetDeviceConfig;

namespace QuickNV.Driver.Onvif.Functions
{
    public class GetDeviceConfigFunction
    {
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

        public static Response Invoke(QpChannel channel, Request request)
        {
            FunctionRequest functionRequest = null;
            DeviceConfig config = null;
            if (request.Fields == null)
            {
                if (!string.IsNullOrEmpty(request.Config))
                    try { config = JsonSerializer.Deserialize<DeviceConfig>(request.Config); }
                    catch { }
                if (config == null)
                    config = new DeviceConfig();
            }
            else
            {
                functionRequest = new FunctionRequest()
                {
                    FieldIds = request.FieldIds,
                    Fields = request.Fields
                };
                config = functionRequest.Convert(DeviceConfigSerializerContext.Default.DeviceConfig);
            }
            var list = new List<FieldForGet>();
            var tmpKey = nameof(config.Host);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "主机",
                Type = FieldType.InputText,
                Value = functionRequest == null ? config.Host : functionRequest.GetFieldValue(tmpKey),
                PostOnChanged = true,
                Input_AllowBlank = false
            });
            tmpKey = nameof(config.Port);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "端口",
                Description = "一般摄像机的http协议使用80端口，https协议使用443端口",
                Type = FieldType.InputNumber,
                Value = functionRequest == null ? config.Port.ToString() : CorrectPort(functionRequest.GetFieldValue(tmpKey)),
                PostOnChanged = true
            });
            tmpKey = nameof(config.ClientCredentialType);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "凭证类型",
                Description = "一般摄像机使用Digest凭证，部分老摄像机使用Basic或者不使用凭证",
                Type = FieldType.InputSelect,
                Value = functionRequest == null ? config.ClientCredentialType.ToString() : functionRequest.GetFieldValue(tmpKey),
                PostOnChanged = true,
                Input_AllowBlank = false,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    ["None"] = "无",
                    ["Digest"] = "Digest",
                    ["Basic"] = "Basic"
                }
            });
            if (config.ClientCredentialType != System.ServiceModel.HttpClientCredentialType.None)
            {
                tmpKey = nameof(config.UserName);
                list.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "用户名",
                    Type = FieldType.InputText,
                    Value = functionRequest == null ? config.UserName : functionRequest.GetFieldValue(tmpKey),
                    PostOnChanged = true,
                    Input_AllowBlank = false
                });
                tmpKey = nameof(config.Password);
                list.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "密码",
                    Type = FieldType.InputPassword,
                    Value = functionRequest == null ? config.Password : functionRequest.GetFieldValue(tmpKey),
                    PostOnChanged = true,
                    Input_AllowBlank = false
                });
            }
            tmpKey = nameof(config.Scheme);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "协议",
                Description = "默认值：http",
                Type = FieldType.InputSelect,
                Value = functionRequest == null ? config.Scheme.ToString() : functionRequest.GetFieldValue(tmpKey),
                PostOnChanged = true,
                Input_AllowBlank = false,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    ["http"] = "http",
                    ["https"] = "https"
                }
            });
            tmpKey = nameof(config.RtspPort);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "RTSP端口",
                Description = "默认值：0",
                Type = FieldType.InputNumber,
                Value = functionRequest == null ? config.RtspPort.ToString() : CorrectPort(functionRequest.GetFieldValue(tmpKey)),
                PostOnChanged = true
            });
            tmpKey = nameof(config.SnapshotPort);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "快照端口",
                Description = "默认值：0",
                Type = FieldType.InputNumber,
                Value = functionRequest == null ? config.SnapshotPort.ToString() : CorrectPort(functionRequest.GetFieldValue(tmpKey)),
                PostOnChanged = true
            });
            if(functionRequest!=null)
            {
                functionRequest = new FunctionRequest()
                {
                    Fields = list.Select(t => t.ToPost()).ToArray()
                };
                config = functionRequest.Convert(DeviceConfigSerializerContext.Default.DeviceConfig);
            }
            return new Response()
            {
                Config = JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }),
                Fields = list.ToArray()
            };
        }
    }
}
