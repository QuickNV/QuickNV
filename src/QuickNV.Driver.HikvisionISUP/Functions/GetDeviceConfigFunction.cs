using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using QuickNV.Protocol.Driver.QpCommands.GetDeviceConfig;
using YiQiDong.Protocol.V1.Model;

namespace QuickNV.Driver.HikvisionISUP.Functions
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

            var tmpKey = nameof(config.SdkDeviceId);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "设备ID",
                Description = "海康设备的ID，9位字符串",
                Type = FieldType.InputText,
                PostOnChanged = true,
                Value = functionRequest == null ? config.SdkDeviceId : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });

            tmpKey = nameof(config.StreamTransferMode);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "媒体传输模式",
                Type = FieldType.InputSelect,
                Value = functionRequest == null ? config.StreamTransferMode.ToString() : functionRequest.GetFieldValue(tmpKey),
                PostOnChanged = true,
                Input_AllowBlank = false,
                InputSelect_OptionsEnumIdUseIntValue = false,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    ["UDP"] = "UDP",
                    ["TCP"] = "TCP"
                }
            });
            return new Response()
            {
                Config = JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }),
                Fields = list.ToArray()
            };
        }
    }
}
