using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Protocol.Driver.QpCommands.GetChannelConfig;

namespace QuickNV.Driver.Onvif.Functions
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
            var tmpKey = nameof(ChannelConfig.ProfileToken);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "配置令牌",
                Description = "海康威视摄像机示例：\r\n通道1主码流：Profile_101\r\n通道2主码流：Profile_201\r\n通道1子码流：Profile_102\r\n通道2子码流：Profile_202",
                Type = FieldType.InputText,
                PostOnChanged = true,
                Value = functionRequest == null ? config.ProfileToken : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });
            tmpKey = nameof(ChannelConfig.VideoSourceToken);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "视频源令牌",
                Description = "海康威视摄像机示例：\r\n通道1视频源：VideoSourceToken_1\r\n通道2视频源：VideoSourceToken_2",
                Type = FieldType.InputText,
                PostOnChanged = true,
                Value = functionRequest == null ? config.VideoSourceToken : functionRequest.GetFieldValue(tmpKey),
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
