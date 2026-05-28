using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Protocol.QpCommands.GetChannelConfig;

namespace QuickNV.Driver.RTSP.Functions
{
    public static class GetChannelConfigFunction
    {
        public static Response Invoke(QpChannel channel, Request request)
        {
            FunctionRequest functionRequest = null;
            ChannelConfig config = null;
            if (request.Fields == null)
            {
                if (!string.IsNullOrEmpty(request.Config))
                    try { config = JsonSerializer.Deserialize<ChannelConfig>(request.Config); }
                    catch { }
                if (config == null)
                    config = new ChannelConfig();
            }
            else
            {
                functionRequest = new FunctionRequest()
                {
                    FieldIds = request.FieldIds,
                    Fields = request.Fields
                };
                config = functionRequest.Convert(ChannelConfigSerializerContext.Default.ChannelConfig);
            }
            var deviceContext = Agent.Instance.GetDeviceContext(request.DeviceId);
            if (deviceContext == null)
            {
                return new Response()
                {
                    Fields = new[]
                    {
                        new FieldForGet()
                        {
                            Type = FieldType.Alert,
                            Name = "提示",
                            Description = $"未找到编号为[{request.DeviceId}]的设备上下文"
                        }
                    }
                };
            }

            var list = new List<FieldForGet>();
            var tmpKey = nameof(ChannelConfig.RtspUrl);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "RTSP流地址",
                Description = "示例：rtsp://10.122.165.67/100",
                Type = FieldType.InputText,
                PostOnChanged = true,
                Value = functionRequest == null ? config.RtspUrl : functionRequest.GetFieldValue(tmpKey),
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
