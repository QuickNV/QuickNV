using System.Text.Json;
using Quick.Fields;
using QuickNV.Onvif;
using Quick.Protocol;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Protocol.QpCommands.ImportChannels;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Onvif.Functions
{
    public class ImportChannelsFunction
    {
        private const string VAR_RAW_PROFILE_DICT = nameof(VAR_RAW_PROFILE_DICT);
        private const string VAR_PROFILE_TOKEN = nameof(VAR_PROFILE_TOKEN);
        private const string VAR_PROFILE_TOKEN_DISPLAY = nameof(VAR_PROFILE_TOKEN_DISPLAY);
        private const string VAR_VIDEOSOURCE_TOKEN = nameof(VAR_VIDEOSOURCE_TOKEN);
        private const string BUTTON_IMPORT = nameof(BUTTON_IMPORT);
        private static Regex getChannelNumberRegex = new Regex(@"(?<number>\d+)");
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

            Dictionary<string, QuickNV.Onvif.Media.Profile> rawProfileDict = null;
            if (functionRequest!=null)
            {
                var rawProfileDictJson = functionRequest.GetFieldValue(VAR_RAW_PROFILE_DICT);
                if (rawProfileDictJson != null)
                {
                    rawProfileDict = JsonSerializer.Deserialize<Dictionary<string, QuickNV.Onvif.Media.Profile>>(rawProfileDictJson);
                }
            }
            if (rawProfileDict == null)
            {
                var profiles = deviceContext.GetMediaProfilesAsync().Result;
                rawProfileDict = profiles.ToDictionary(t => t.token, t => t);
            }
            list.Add(new FieldForGet()
            {
                Id = VAR_RAW_PROFILE_DICT,
                Type = FieldType.InputHidden,
                Value = JsonSerializer.Serialize(rawProfileDict)
            });

            var profileDict = new Dictionary<string, string>();
            foreach (var kv in rawProfileDict)
            {
                var key = kv.Key;
                var profile = kv.Value;

                var videoEncoderConfiguration = profile.VideoEncoderConfiguration;
                var resolution = videoEncoderConfiguration?.Resolution;
                profileDict[key] = $"{profile.Name}({videoEncoderConfiguration?.Encoding}_{resolution?.Width}x{resolution?.Height})";
            }

            var profileToken = functionRequest == null ? profileDict.Keys.FirstOrDefault() : functionRequest.GetFieldValue(VAR_PROFILE_TOKEN);
            list.Add(new FieldForGet()
            {
                Id = VAR_PROFILE_TOKEN,
                Name = "配置",
                Type = FieldType.InputSelect,
                Value = profileToken,
                PostOnChanged = true,
                Input_AllowBlank = false,
                InputSelect_Options = profileDict
            });
            list.Add(new FieldForGet()
            {
                Id = VAR_PROFILE_TOKEN_DISPLAY,
                Name = "配置源令牌",
                Type = FieldType.InputText,
                Input_ReadOnly = true,
                Value = profileToken
            });
            var currentProfile = rawProfileDict[profileToken];
            var videoSourceConfigurationToken = currentProfile.VideoSourceConfiguration.token;
            list.Add(new FieldForGet()
            {
                Id = VAR_VIDEOSOURCE_TOKEN,
                Name = "视频源令牌",
                Type = FieldType.InputText,
                Input_ReadOnly = true,
                Value = videoSourceConfigurationToken
            });
            
            var tmpKey = nameof(ChannelInfo.Id);
            var defaultChannelId = currentProfile.VideoSourceConfiguration.SourceToken;
            if (getChannelNumberRegex.IsMatch(defaultChannelId))
            {
                var match = getChannelNumberRegex.Match(defaultChannelId);
                defaultChannelId = match.Groups["number"].Value;
            }
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "通道编号",
                Type = FieldType.InputText,
                Value = defaultChannelId,
                Input_AllowBlank = false
            });
            var defaultChannelName = currentProfile.VideoSourceConfiguration.SourceToken;
            try
            {
                var osds = deviceContext.GetOSDs(videoSourceConfigurationToken).Result;
                defaultChannelName = string.Join(',', osds.Where(t => t.TextString.Type == "Plain").Select(t => t.TextString.PlainText));
            }
            catch { }
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
                Type = FieldType.Button,
                PostOnChanged = true,
                Html_Class = "m-1"
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
                        ProfileToken = functionRequest.GetFieldValue(VAR_PROFILE_TOKEN),
                        VideoSourceToken = functionRequest.GetFieldValue(VAR_VIDEOSOURCE_TOKEN),
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
                                DriverConfig = JsonSerializer.Serialize(channelConfig, new JsonSerializerOptions() { WriteIndented = true })
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
