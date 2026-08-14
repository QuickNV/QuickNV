using QuickNV.HikvisionNetSDK.Api;
using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Protocol.Driver.QpCommands.GetDeviceConfig;

namespace QuickNV.Driver.HikvisionDeviceNetwork.Functions
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
                Description = "默认值：8000",
                Type = FieldType.InputNumber,
                Value = functionRequest == null ? config.Port.ToString() : CorrectPort(functionRequest.GetFieldValue(tmpKey)),
                PostOnChanged = true
            });

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
            tmpKey = nameof(DeviceConfig.Encoding);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "字符编码",
                Type = FieldType.InputSelect,
                PostOnChanged = true,
                Value = functionRequest == null ? config.Encoding : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    ["GB18030"] = "GB18030",
                    ["GB2312"] = "GB2312",
                    ["UTF8"] = "UTF-8"
                }
            });
            tmpKey = nameof(DeviceConfig.RtspPathFormat);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "RTSP路径格式",
                Type = FieldType.InputSelect,
                PostOnChanged = true,
                Value = functionRequest == null ? config.RtspPathFormat.ToString() : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false,
                InputSelect_OptionsEnumIdUseIntValue = false,
                InputSelect_OptionsEnum = typeof(HvRtspPathFormat)
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

            if (functionRequest != null)
            {
                functionRequest = new FunctionRequest()
                {
                    Fields = [..list.Select(t => t.ToPost())]
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
