using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Protocol.QpCommands.GetDeviceConfig;

namespace QuickNV.Driver.RTSP.Functions
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
            var tmpKey = nameof(config.RtspUrlTemplate);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "RTSP地址模板",
                Description = "示例：rtsp://10.122.165.67/{0}",
                Type = FieldType.InputText,
                Value = functionRequest == null ? config.RtspUrlTemplate : functionRequest.GetFieldValue(tmpKey),
                PostOnChanged = true,
                Input_AllowBlank = false
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
