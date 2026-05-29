using System.Text.Json;
using Quick.Fields;
using Quick.Protocol;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Protocol.Driver.QpCommands.ImportChannels;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Driver.RTSP.Functions
{
    public class ImportChannelsFunction
    {
        private const string VAR_RTSP_URL_TEMPLATE = nameof(VAR_RTSP_URL_TEMPLATE);
        private const string VAR_CHANNEL_MAP = nameof(VAR_CHANNEL_MAP);
        private const string BUTTON_IMPORT = nameof(BUTTON_IMPORT);
        private static Regex getChannelNumberRegex = new Regex(@"(?<number>\d+)");
        private static List<FieldForGet> innerGet(DeviceContext deviceContext, FunctionRequest functionRequest)
        {
            var list = new List<FieldForGet>();
            list.Add(new FieldForGet()
            {
                Id = VAR_RTSP_URL_TEMPLATE,
                Name = "RTSP流地址模板",
                Description = "示例：rtsp://10.122.165.67/{0}",
                Type = FieldType.InputText,
                Value = functionRequest == null ? deviceContext.Model.Config.RtspUrlTemplate : functionRequest.GetFieldValue(VAR_RTSP_URL_TEMPLATE)
            });
            list.Add(new FieldForGet()
            {
                Id = VAR_CHANNEL_MAP,
                Name = "通道映射",
                Description = "一行一个通道，格式：通道编号->通道名称。示例：100->100号通道",
                Type = FieldType.InputTextArea,
                InputTextArea_Rows = 10,
                Value = functionRequest == null ? null : functionRequest.GetFieldValue(VAR_CHANNEL_MAP)
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
                    var rtspUrlTemplate = functionRequest.GetFieldValue(VAR_RTSP_URL_TEMPLATE);
                    var channelMap = functionRequest.GetFieldValue(VAR_CHANNEL_MAP);
                    var list = new List<ChannelInfo>();
                    foreach (var line in channelMap.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var strs = line.Split("->", StringSplitOptions.RemoveEmptyEntries);
                        if (strs.Length < 2)
                            continue;
                        var id = strs[0].Trim();
                        var name = strs[1].Trim();
                        var channelConfig = new ChannelConfig()
                        {
                            RtspUrl = string.Format(rtspUrlTemplate, id)
                        };
                        list.Add(new ChannelInfo()
                        {
                            DeviceId = request.DeviceId,
                            Id = id,
                            Name = name,
                            DriverConfig = JsonSerializer.Serialize(channelConfig, new JsonSerializerOptions() { WriteIndented = true })
                        });
                    }
                    return new Response()
                    {
                        Channels = list.ToArray()
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
