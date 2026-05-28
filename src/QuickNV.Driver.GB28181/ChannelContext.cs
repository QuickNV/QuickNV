using SIPSorcery.Net;
using SIPSorcery.SIP;
using System.Net;
using YiQiDong.Agent;
using QuickNV.Driver.GB28181.Utils;
using QuickNV.Driver.Protocol.QpModels;
using Quick.Utils;

namespace QuickNV.Driver.GB28181
{
    public class ChannelContext : IDisposable
    {
        private SipServer sipServer;
        private Semaphore semaphore = new Semaphore(1, 1);
        private const int ChannelMinCreateStreamSeconds = 10;

        //----------------
        //实时视频流相关
        //----------------
        private Model.GbStreamInfo liveStream_GbStreamInfo;
        
        //上次创建实时视频流时间
        private DateTime liveStream_LastCreateTime = DateTime.MinValue;
        //----------------
        //回放视频流相关
        //----------------
        private Model.GbStreamInfo playbackStream_GbStreamInfo;

        public DeviceContext Device { get; private set; }
        public ChannelModel Model { get; private set; }

        public ChannelContext(SipServer sipServer, DeviceContext device, ChannelModel model)
        {
            this.sipServer = sipServer;
            Device = device;
            Model = model;
        }

        public const string SSRC_PREFIX = "y=";

        private async Task<StreamInfo> innerCreateLiveStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo)
        {
            DestoryLiveStream();
            var mediaServerIpAddress = IPAddress.Parse(mediaServerInfo.PublicIpAddress);
            var sdpConn = new SDPConnectionInformation(mediaServerIpAddress);
            var sdp = new SDP(mediaServerIpAddress)
            {
                Version = 0,
                SessionId = "0",
                Username = Device.Model.Id,
                SessionName = "Play",
                Connection = sdpConn,
                Timing = "0 0"
            };
            var media = new SDPMediaAnnouncement()
            {
                Media = SDPMediaTypesEnum.video,
                Port = mediaServerInfo.RtpProxyPort
            };

            switch (Device.Model.Config.StreamTransferMode)
            {
                case DeviceConfig.TransferMode.UDP_Passive:
                    break;
                //如果是TCP被动推流
                case DeviceConfig.TransferMode.TCP_Passive:
                    media.Transport = "TCP/RTP/AVP";
                    media.AddExtra("a=setup:passive");
                    media.AddExtra("connection:new");
                    break;
                case DeviceConfig.TransferMode.TCP_Active:
                    throw new NotImplementedException();
            }
            media.MediaFormats.Add(96, new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.video, 96, "PS/90000"));
            media.MediaFormats.Add(98, new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.video, 98, "H264/90000"));
            media.MediaFormats.Add(97, new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.video, 97, "MPEG4/90000"));
            media.AddExtra("a=recvonly");
            try
            {
                media.AddExtra($"{SSRC_PREFIX}{mediaInfo.SSRC}");
                sdp.Media.Add(media);

                //发送INVITE
                var rep = await sipServer.SendRequestAsync(
                    Device,
                    SIPMethodsEnum.INVITE,
                    Model.Id,
                    "application/sdp",
                    sdp.ToString(),
                    req2 => sipServer.AddHeaderContact(req2));

                //如果返回码不是成功
                if (!rep.IsSuccessStatusCode)
                {
                    var message = rep.Header.Warning;
                    if (string.IsNullOrEmpty(message))
                        message = $"Code:{rep.StatusCode}, Status:{rep.Status}, Reason:{rep.ReasonPhrase}";

                    //发送BYE指令
                    _ = sipServer.SendRequestAsync(Device, SIPMethodsEnum.BYE, requestHandler: request =>
                    {
                        request.Header.To.ToTag = rep.Header.To.ToTag;
                        request.Header.CallId = rep.Header.CallId;
                    }).Wait(1000);

                    throw new IOException($"设备返回错误。{message}");
                }
                //解析响应中的SSRC
                var repSSRC = rep.Body?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)?.FirstOrDefault(t => t.StartsWith(SSRC_PREFIX))?.Substring(SSRC_PREFIX.Length);
                //如果响应的SSRC与我方生成的SSRC不一致，则使用响应的SSRC
                if (repSSRC != null && repSSRC != mediaInfo.SSRC)
                {
                    mediaInfo = await Agent.Instance.ChangeLiveStreamSSRC(mediaServerInfo.Id, mediaInfo.MediaId, repSSRC);
                }
                liveStream_GbStreamInfo = new Model.GbStreamInfo()
                {
                    DeviceId = Device.Model.Id,
                    ChannelId = Model.Id,
                    CallId = rep.Header.CallId,
                    ToTag = rep.Header.To.ToTag,
                    CreateTime = new DateTime()
                };
                //发送ACK
                await sipServer.SendRequestAsync(Device, SIPMethodsEnum.ACK, requestHandler: request =>
                {
                    request.Header.To.ToURI.User = Model.Id;
                    request.Header.To.ToTag = liveStream_GbStreamInfo.ToTag;
                    request.Header.CSeq = rep.Header.CSeq;
                    request.Header.CallId = liveStream_GbStreamInfo.CallId;
                }, waitForResponse: false);
            }
            catch (TimeoutException ex)
            {
                throw new ApplicationException("等待INVITE指令响应超时", ex);
            }
            catch (IOException ex)
            {
                throw new ApplicationException($"出现IO错误", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"出现错误", ex);
            }
            //等待媒体服务器的on_publish回调
            StreamInfo liveStream_StreamInfo = null;
            try
            {
                //等待流注册
                liveStream_StreamInfo = await Agent.Instance.GetMediaServerStreamInfo(mediaServerInfo.Id, mediaInfo.MediaId);
            }
            catch (TimeoutException)
            {
                throw new TimeoutException("等待流注册超时");
            }
            return liveStream_StreamInfo;
        }

        public async Task<StreamInfo> CreateLiveStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo)
        {
            try
            {
                semaphore.WaitOne();
                var intervalSeconds = (DateTime.Now - liveStream_LastCreateTime).TotalSeconds;
                if (intervalSeconds < ChannelMinCreateStreamSeconds)
                    throw new TimeoutException($"创建视频流过于频繁");
                var ret = await innerCreateLiveStream(mediaServerInfo, mediaInfo);
                liveStream_LastCreateTime = DateTime.Now;
                return ret;
            }
            catch
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<StreamInfo> CreatePlaybackStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo, DateTime startTime, DateTime endTime)
        {
            if (playbackStream_GbStreamInfo != null)
                DestoryPlaybackStream();
                
            var mediaServerIpAddress = IPAddress.Parse(mediaServerInfo.PublicIpAddress);
            var sdpConn = new SDPConnectionInformation(mediaServerIpAddress);
            string sdbStr = null;

            switch (Device.Model.Config.StreamTransferMode)
            {
                //如果是TCP被动推流
                case DeviceConfig.TransferMode.TCP_Passive:
                    sdbStr = @$"v=0
o={Device.Model.Id} 0 0 IN IP4 {mediaServerIpAddress}
s=Playback
u={Model.Id}:0
c=IN IP4 {mediaServerIpAddress}
t={DateUtils.ToUnixTimestamp(startTime) / 1000} {DateUtils.ToUnixTimestamp(endTime) / 1000}
m=video {mediaServerInfo.RtpProxyPort} TCP/RTP/AVP 96 98 97
a=setup:passive
connection:new
a=recvonly
a=rtpmap:96 PS/90000
a=rtpmap:98 H264/90000
a=rtpmap:97 MPEG4/90000
y={mediaInfo.SSRC}";
                    break;
                case DeviceConfig.TransferMode.TCP_Active:
                    throw new NotImplementedException();
                case DeviceConfig.TransferMode.UDP_Passive:
                default:
                    sdbStr = @$"v=0
o={Device.Model.Id} 0 0 IN IP4 {mediaServerIpAddress}
s=Playback
u={Model.Id}:0
c=IN IP4 {mediaServerIpAddress}
t={DateUtils.ToUnixTimestamp(startTime) / 1000} {DateUtils.ToUnixTimestamp(endTime) / 1000}
m=video {mediaServerInfo.RtpProxyPort} RTP/AVP 96 98 97
a=recvonly
a=rtpmap:96 PS/90000
a=rtpmap:98 H264/90000
a=rtpmap:97 MPEG4/90000
y={mediaInfo.SSRC}";
                    break;
            }
            try
            {
                //发送INVITE
                var rep = await sipServer.SendRequestAsync(
                    Device,
                    SIPMethodsEnum.INVITE,
                    Model.Id,
                    "application/sdp",
                    sdbStr.ToString(),
                    req =>
                    {
                        sipServer.AddHeaderContact(req);
                        req.Header.Subject = $"{Model.Id}:{mediaInfo.SSRC},{sipServer.Options.SipDeviceId}:{mediaInfo.SSRC}";
                    });

                //如果返回码不是成功
                if (!rep.IsSuccessStatusCode)
                {
                    var message = rep.Header.Warning;
                    if (string.IsNullOrEmpty(message))
                        message = $"Code:{rep.StatusCode}, Status:{rep.Status}, Reason:{rep.ReasonPhrase}";

                    //发送BYE指令
                    _ = sipServer.SendRequestAsync(Device, SIPMethodsEnum.BYE, requestHandler: request =>
                    {
                        request.Header.To.ToTag = rep.Header.To.ToTag;
                        request.Header.CallId = rep.Header.CallId;
                    }).Wait(1000);

                    throw new IOException($"设备返回错误。{message}");
                }
                //解析响应中的SSRC
                var repSSRC = rep.Body?.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)?.FirstOrDefault(t => t.StartsWith(SSRC_PREFIX))?.Substring(SSRC_PREFIX.Length);
                //如果响应的SSRC与我方生成的SSRC不一致，则使用响应的SSRC
                if (repSSRC != null && repSSRC != mediaInfo.SSRC)
                {
                    mediaInfo = await Agent.Instance.ChangeLiveStreamSSRC(mediaServerInfo.Id, mediaInfo.MediaId, repSSRC);
                }
                playbackStream_GbStreamInfo = new Model.GbStreamInfo()
                {
                    DeviceId = Device.Model.Id,
                    ChannelId = Model.Id,
                    CallId = rep.Header.CallId,
                    ToTag = rep.Header.To.ToTag,
                    CreateTime = new DateTime()
                };
                //发送ACK
                await sipServer.SendRequestAsync(Device, SIPMethodsEnum.ACK, requestHandler: request =>
                {
                    request.Header.To.ToURI.User = Model.Id;
                    request.Header.To.ToTag = playbackStream_GbStreamInfo.ToTag;
                    request.Header.CSeq = rep.Header.CSeq;
                    request.Header.CallId = playbackStream_GbStreamInfo.CallId;
                }, waitForResponse: false);
            }
            catch (TimeoutException ex)
            {
                throw new ApplicationException("等待INVITE指令响应超时", ex);
            }
            catch (IOException ex)
            {
                throw new ApplicationException($"出现IO错误", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"出现错误", ex);
            }
            try
            {
                //等待流注册
                return await Agent.Instance.GetMediaServerStreamInfo(mediaServerInfo.Id, mediaInfo.MediaId);
            }
            catch (TimeoutException)
            {
                throw new TimeoutException("等待流注册超时");
            }
        }

        public void DestoryLiveStream()
        {
            if (liveStream_GbStreamInfo != null)
            {
                //发送BYE
                _ = sipServer.SendRequestAsync(Device, SIPMethodsEnum.BYE, requestHandler: request =>
                {
                    request.Header.To.ToTag = liveStream_GbStreamInfo.ToTag;
                    request.Header.CallId = liveStream_GbStreamInfo.CallId;
                }).Wait(1000);
                liveStream_GbStreamInfo = null;
            }
        }

        public void DestoryPlaybackStream()
        {
            if (playbackStream_GbStreamInfo != null)
            {
                //发送BYE
                _ = sipServer.SendRequestAsync(Device, SIPMethodsEnum.BYE, requestHandler: request =>
                {
                    request.Header.To.ToTag = playbackStream_GbStreamInfo.ToTag;
                    request.Header.CallId = playbackStream_GbStreamInfo.CallId;
                }).Wait(1000);
                playbackStream_GbStreamInfo = null;
            }
        }

        public async Task<bool> SendPtzCommandAsync(PTZCommandType cmdType, byte speed = 0x80)
        {
            try
            {
                string ptzCmdStr = Command.DeviceControl.PTZCommandUtils.GetPtzCmd(cmdType, speed);
                var body = new Command.DeviceControl.Control()
                {
                    DeviceID = Model.Id,
                    SN = new Random().Next(1, ushort.MaxValue),
                    PTZCmd = ptzCmdStr
                };
                var rep = await sipServer.SendMessageRequestAsync(Device, body);
                return rep.Status == SIPResponseStatusCodesEnum.Ok;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送云台控制命令时出错，原因：{ExceptionUtils.GetExceptionString(ex)}");
                return false;
            }
        }

        public void Dispose()
        {
            DestoryLiveStream();
        }

        private List<VideoFileInfo> resultFindPlaybackFileList;
        private Task<VideoFileInfo[]> taskFindPlaybackFiles;

        internal void SetResult_QueryRecordInfo(Command.RecordInfo.Response recordInfo)
        {
            if (resultFindPlaybackFileList == null
                || taskFindPlaybackFiles == null)
                return;
            if (recordInfo.RecordList != null)
                resultFindPlaybackFileList.AddRange(
                    recordInfo.RecordList.Item.Select(t => new VideoFileInfo()
                    {
                        Id = t.FilePath,
                        Name = t.Name,
                        StartTime = t.DT_StartTime,
                        EndTime = t.DT_EndTime,
                        Size = t.FileSize
                    }));
            //如果结果已经接收完整
            if (resultFindPlaybackFileList.Count >= recordInfo.SumNum)
            {
                taskFindPlaybackFiles?.Start();
            }
        }

        public async Task<VideoFileInfo[]> FindPlaybackFiles(DateTime startTime, DateTime endTime)
        {
            AgentContext.LogDebug($"开始向[SIP设备编号:{Device.Model.Id},远程端点:{Device.RemoteEndPoint}]，通道[{Model.Id}]发送查询录像信息指令。时间：{startTime} - {endTime}");
            var body = new Command.RecordInfo.Query()
            {
                DeviceID = Model.Id,
                SN = new Random().Next(1, ushort.MaxValue),
                StartTime = startTime,
                EndTime = endTime
            };
            resultFindPlaybackFileList = new List<VideoFileInfo>();
            taskFindPlaybackFiles = new Task<VideoFileInfo[]>(() => resultFindPlaybackFileList.ToArray());

            var rep = await sipServer.SendMessageRequestAsync(Device, body);
            if (rep.Status == SIPResponseStatusCodesEnum.Ok)
                AgentContext.LogDebug($"向[SIP设备编号:{Device.Model.Id},远程端点:{Device.RemoteEndPoint}]发送查询录像信息指令成功");
            else
                AgentContext.LogDebug($"向[SIP设备编号:{Device.Model.Id},远程端点:{Device.RemoteEndPoint}]发送查询录像信息指令失败。Status:{rep.StatusCode} {rep.Status}, Reson:{rep.ReasonPhrase}");
            var result = await taskFindPlaybackFiles;
            resultFindPlaybackFileList = null;
            taskFindPlaybackFiles = null;
            return result;
        }
    }
}
