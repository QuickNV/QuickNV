using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using QuickNV.Protocol.Driver.QpCommands.GetDeviceConfig;
using YiQiDong.Protocol.V1.Model;

namespace QuickNV.Driver.Ys7.Functions
{
    public static class GetDeviceConfigFunction
    {
        public static Response Invoke(QpChannel channel, Request request)
        {
            FunctionRequest functionRequest = null;
            DeviceConfig config = null;
            if (request.Fields == null)
            {
                if (!string.IsNullOrEmpty(request.Config))
                    try { config = JsonSerializer.Deserialize<DeviceConfig>(request.Config); }
                    catch { }
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
            if (config == null)
                config = new DeviceConfig();

            var list = new List<FieldForGet>();

            var tmpKey = nameof(config.Ys7DeviceSerial);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "设备序列号",
                Description = "萤石设备的序列号，9位字符串",
                Type = FieldType.InputText,
                PostOnChanged = true,
                Value = functionRequest == null ? config.Ys7DeviceSerial : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });

            return new Response()
            {
                Config = JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }),
                Fields = list.ToArray()
            };
        }
    }
}
