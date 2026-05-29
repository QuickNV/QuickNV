using QuickNV.DahuaNetSDK.Api;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Protocol.Driver.QpCommands.ImportChannels;
using QuickNV.Protocol.Driver.QpModels;
using System.Text.Json;

namespace QuickNV.Driver.DahuaDeviceNetwork.Functions
{
    public class ImportChannelsFunction
    {
        private const string VAR_CHANNEL_ID = nameof(VAR_CHANNEL_ID);
        private const string VAR_STREAM_TYPE = nameof(VAR_STREAM_TYPE);
        private const string BUTTON_IMPORT = nameof(BUTTON_IMPORT);

        private static List<FieldForGet> innerGet(DeviceContext deviceContext, FunctionRequest functionRequest)
        {
            var list = new List<FieldForGet>();

            if (!deviceContext.IsOnline)
            {
                list.Add(new FieldForGet()
                {
                    Type = FieldType.Alert,
                    Name = "提示",
                    Description = $"设备当前不在线，无法导入通道。"
                });
                return list;
            }

            var channelDict = new Dictionary<string, string>();
            foreach (var channel in deviceContext.DhSession.ChannelService.AllChannels)
            {
                channelDict[channel.Id.ToString()] = channel.Name;
            }
            var channelId = functionRequest == null ? channelDict.Keys.FirstOrDefault() : functionRequest.GetFieldValue(VAR_CHANNEL_ID);
            list.Add(new FieldForGet()
            {
                Id = VAR_CHANNEL_ID,
                Name = "大华通道编号",
                Type = FieldType.InputSelect,
                Value = channelId,
                PostOnChanged = true,
                Input_AllowBlank = false,
                InputSelect_Options = channelDict
            });
            var tmpKey = VAR_STREAM_TYPE;
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "码流类型",
                Description = "一般选择子码流",
                Type = FieldType.InputSelect,
                PostOnChanged = true,
                Value = functionRequest == null ? DhStreamType.Sub.ToString() : functionRequest.GetFieldValue(tmpKey),
                Input_AllowBlank = false,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    [DhStreamType.Main.ToString()] = "主码流",
                    [DhStreamType.Sub.ToString()] = "子码流",
                    [DhStreamType.StreamType3.ToString()] = "第三码流",
                    [DhStreamType.StreamType4.ToString()] = "第四码流"
                }
            });
            tmpKey = nameof(ChannelInfo.Id);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "通道编号",
                Type = FieldType.InputText,
                Value = channelId,
                Input_AllowBlank = false
            });
            var defaultChannelName = channelDict[channelId];            
            tmpKey = nameof(ChannelInfo.Name);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "通道名称",
                Type = FieldType.InputText,
                Value = defaultChannelName,
                Input_AllowBlank = false
            });
            list.Add(new FieldForGet()
            {
                Id = BUTTON_IMPORT,
                Name = "导入",
                Type = FieldType.Button
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
            if (functionRequest != null)
            {
                if (functionRequest.IsFieldIdsMatch(BUTTON_IMPORT))
                {
                    var channelConfig = new ChannelConfig()
                    {
                        StreamType = Enum.Parse<DhStreamType>(functionRequest.GetFieldValue(VAR_STREAM_TYPE)),
                        ChannelId = int.Parse(functionRequest.GetFieldValue(VAR_CHANNEL_ID))
                    };
                    return new Response()
                    {
                        Channels = new ChannelInfo[]
                        {
                            new ChannelInfo()
                            {
                                DeviceId = request.DeviceId,
                                Id = functionRequest.GetFieldValue(nameof(ChannelInfo.Id)),
                                Name=functionRequest.GetFieldValue(nameof(ChannelInfo.Name)),
                                DriverConfig = JsonSerializer.Serialize(channelConfig, new JsonSerializerOptions(){ WriteIndented=true})
                            }
                        }
                    };
                }
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
            var fields = innerGet(deviceContext, functionRequest);
            return new Response()
            {
                Fields = fields.ToArray()
            };
        }
    }
}
