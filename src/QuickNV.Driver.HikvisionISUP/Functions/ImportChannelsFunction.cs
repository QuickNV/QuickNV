using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Agent;
using QuickNV.Protocol.Driver.QpCommands.ImportChannels;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Driver.HikvisionISUP.Functions
{
    public class ImportChannelsFunction
    {
        private const string VAR_CHANNELS = nameof(VAR_CHANNELS);
        private const string BTN_IMPORT_ALL = nameof(BTN_IMPORT_ALL);
        private const string BTN_IMPORT_SELECTED = nameof(BTN_IMPORT_SELECTED);

        public static ChannelInfo ApiChannelInfo2ChannelInfo(string deviceId, QuickNV.HikvisionISUPSDK.Api.ChannelInfo t)
        {
            return new ChannelInfo()
            {
                DeviceId = deviceId,
                Id = t.Id.ToString(),
                Name = t.Name,
                DriverConfig = JsonSerializer.Serialize(new ChannelConfig()
                {
                    ChannelId = t.Id,
                    StreamType = QuickNV.HikvisionISUPSDK.Api.SmsStreamType.Sub
                })
            };
        }

        private static List<FieldForGet> innerGet(DeviceContext deviceContext, FunctionRequest functionRequest)
        {
            var list = new List<FieldForGet>();
            if (deviceContext.ApiContext == null)
            {
                list.Add(new FieldForGet()
                {
                    Type = FieldType.Alert,
                    Name = "提示",
                    Description = $"设备当前不在线，无法导入通道。"
                });
                return list;
            }

            var alreadyImportChannelDict = deviceContext.Model.GetChannels().ToDictionary(t => t.Id, t => t);
            var waitImportChannelList = new List<DriverChannel<ChannelConfig>>();
            foreach (var channel in deviceContext.ApiContext.Channels)
            {
                if (alreadyImportChannelDict.ContainsKey(channel.Id.ToString()))
                    continue;
                waitImportChannelList.Add(new DriverChannel<ChannelConfig>(ApiChannelInfo2ChannelInfo(deviceContext.Model.Id, channel)));
            }
            if (waitImportChannelList.Count > 0)
            {
                list.Add(new FieldForGet()
                {
                    Id = BTN_IMPORT_SELECTED,
                    Name = "导入选中通道",
                    Type = FieldType.Button,
                    PostOnChanged = true,
                    Html_Class = "m-1"
                });
                list.Add(new FieldForGet()
                {
                    Id = BTN_IMPORT_ALL,
                    Name = "导入全部",
                    Type = FieldType.Button,
                    PostOnChanged = true,
                    Html_Class = "m-1"
                });
                var itemList = new List<FieldForGet>();
                foreach (var channel in waitImportChannelList)
                {
                    var id = channel.Id;
                    var name = $"{channel.Name}({channel.Id})";
                    var value = functionRequest == null ? false.ToString() : functionRequest.GetFieldValue(VAR_CHANNELS, id);
                    if (value == false.ToString())
                        name = $"[ ] {name}";
                    else
                        name = $"[*] {name}";
                    itemList.Add(new FieldForGet()
                    {
                        Id = id,
                        Name = name,
                        Type = FieldType.Button,
                        PostOnChanged = true,
                        Value = value,
                        Html_Class = "btn-block"
                    });
                }
                list.Add(new FieldForGet()
                {
                    Id = VAR_CHANNELS,
                    Name = "通道列表",
                    Type = FieldType.ContainerGroup,
                    Children = itemList.ToArray()
                });
            }
            else
            {
                list.Add(new FieldForGet()
                {
                    Type = FieldType.Alert,
                    Name = "提示",
                    Description = "暂时未发现可以导入的通道"
                });
            }
            return list;
        }

        public static Response Invoke(QpChannel channel, Request request)
        {
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
                            Name = "错误",
                            Description = $"未找到编号为[{request.DeviceId}]的设备。"
                        }
                     }
                };
            }

            FunctionRequest functionRequest = null;
            if (request.Fields != null)
            {
                functionRequest = new FunctionRequest();
                functionRequest.Fields = request.Fields;
                functionRequest.FieldIds = request.FieldIds;
            }

            if (request.FieldIds != null)
            {
                if (functionRequest.FieldIds[0] == VAR_CHANNELS)
                {
                    var channelId = functionRequest.FieldIds[1];
                    var channelsField = functionRequest.Fields.FirstOrDefault(t => t.Id == VAR_CHANNELS);
                    var channelField = channelsField.Children.FirstOrDefault(t => t.Id == channelId);
                    if (channelField.Value == false.ToString())
                        channelField.Value = true.ToString();
                    else
                        channelField.Value = false.ToString();
                    return new Response()
                    {
                        Fields = innerGet(deviceContext, functionRequest).ToArray()
                    };
                }
                else if (functionRequest.IsFieldIdsMatch(BTN_IMPORT_SELECTED))
                {
                    var channelsField = functionRequest.Fields.FirstOrDefault(t => t.Id == VAR_CHANNELS);
                    var channelsList = new List<ChannelInfo>();
                    foreach (var channelField in channelsField.Children)
                    {
                        if (channelField.Value == false.ToString())
                            continue;
                        var channelId = channelField.Id;

                        var channelModel = deviceContext.ApiContext.Channels.First(t => t.Id.ToString() == channelId);
                        if (channel == null)
                            continue;
                        channelsList.Add(ApiChannelInfo2ChannelInfo(request.DeviceId, channelModel));
                    }
                    return new Response() { Channels = channelsList.ToArray() };
                }
                else if (functionRequest.IsFieldIdsMatch(BTN_IMPORT_ALL))
                {
                    var channelsField = functionRequest.Fields.FirstOrDefault(t => t.Id == VAR_CHANNELS);
                    var channelsList = new List<ChannelInfo>();
                    foreach (var channelField in channelsField.Children)
                    {
                        var channelId = channelField.Id;
                        var channelModel = deviceContext.ApiContext.Channels.First(t => t.Id.ToString() == channelId);
                        if (channelModel == null)
                            continue;
                        channelsList.Add(ApiChannelInfo2ChannelInfo(request.DeviceId, channelModel));
                    }
                    return new Response() { Channels = channelsList.ToArray() };
                }
            }
            return new Response()
            {
                Fields = innerGet(deviceContext, functionRequest).ToArray()
            };
        }
    }
}
