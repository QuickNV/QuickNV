using QuickNV.DahuaNetSDK.Api;
using Quick.Fields;
using Quick.Protocol;
using System.Collections.Generic;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Protocol.QpCommands.GetChannelConfig;
using System.Text.Json;

namespace QuickNV.Driver.DahuaDeviceNetwork.Functions
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
            var tmpKey = nameof(ChannelConfig.ChannelId);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "大华通道编号",
                Type = FieldType.InputText,
                PostOnChanged = true,
                Value = functionRequest == null ? config.ChannelId.ToString() : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false
            });
            tmpKey = nameof(ChannelConfig.StreamType);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "码流类型",
                Description = "一般选择子码流",
                Type = FieldType.InputSelect,
                PostOnChanged = true,
                Value = functionRequest == null ? config.StreamType.ToString() : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    [DhStreamType.Main.ToString()] = "主码流",
                    [DhStreamType.Sub.ToString()] = "子码流",
                    [DhStreamType.StreamType3.ToString()] = "第三码流",
                    [DhStreamType.StreamType4.ToString()] = "第四码流"
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
