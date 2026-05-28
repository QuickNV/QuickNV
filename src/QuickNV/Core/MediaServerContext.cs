using System.Text.Json;
using Quick.EntityFrameworkCore.Plus;
using Quick.ZLMediaKit.HttpApi;
using Quick.ZLMediaKit.WebHook.Model;
using System.Net;
using YiQiDong.Core.Utils;
using QuickNV.Core.Utils;
using QuickNV.Driver.Protocol.QpModels;
using QuickNV.Model;

namespace QuickNV.Core
{
    public class MediaServerContext : WithLogContext, IDisposable
    {
        private ZLMediaKitClientOptions clientOptions;
        private ZLMediaKitClient client;
        private CancellationTokenSource cts;

        //空闲媒体编号队列
        private Queue<int> idleMediaIdQueue = new Queue<int>();
        private Dictionary<int, MediaInfo> mediaInfoDict = new Dictionary<int, MediaInfo>();
        public Quick.ZLMediaKit.HttpApi.Model.ServerConfig Config { get; private set; }

        public int GetMediaCount()
        {
            lock (idleMediaIdQueue)
                return mediaInfoDict.Count;
        }

        public MediaInfo[] GetMediaInfos()
        {
            lock (idleMediaIdQueue)
                return mediaInfoDict.Values.ToArray();
        }

        public MediaInfo GetMediaInfo(int mediaId)
        {
            lock (idleMediaIdQueue)
            {
                if (mediaInfoDict.ContainsKey(mediaId))
                    return mediaInfoDict[mediaId];
            }
            return null;
        }

        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event EventHandler StateChanged;
        public DateTime KeepaliveTime { get; private set; }
        public bool IsConnected { get; private set; } = false;
        public Model.MediaServer Model { get; private set; }

        private void RaiseEvent_StateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
        }

        public MediaServerContext(Model.MediaServer model)
        {
            Model = model;

            //初始化媒体编号队列
            var mediaIds = new int[10000];
            for (var i = 0; i < mediaIds.Length; i++)
                mediaIds[i] = i;
            lock (idleMediaIdQueue)
                foreach (var mediaId in mediaIds.OrderBy(t => Random.Shared.Next()))
                    idleMediaIdQueue.Enqueue(mediaId);

            clientOptions = new ZLMediaKitClientOptions()
            {
                ApiUrl = Model.ApiUrl,
                ApiSecret = Model.ApiSecret
            };
            client = new ZLMediaKitClient(clientOptions);
            cts = new CancellationTokenSource();
            beginCheckKeepalive(cts.Token);
        }

        private async Task<bool> ValidateConfig()
        {
            try
            {
                var ret = await client.GetServerConfig();
                if (ret.Code != Quick.ZLMediaKit.HttpApi.Model.ApiCodeEnum.Success)
                    throw new ApplicationException(ret.Msg);
                Config = ret.Data;
                if (Config.General.MediaServerId != Model.Id)
                    throw new ApplicationException($"媒体服务器实际编号[{Config.General.MediaServerId}]与配置编号[{Model.Id}]不匹配");
                if (!Config.Hook.Enable)
                    throw new ApplicationException($"媒体服务器未开启鉴权");
                PushLog($"验证媒体服务器配置通过");
                return true;
            }
            catch (Exception ex)
            {
                PushLog($"验证媒体服务器配置失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                return false;
            }
        }

        private void beginCheckKeepalive(CancellationToken token)
        {
            Task.Delay(1000, token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;

                //当前是否连接
                bool currentIsConnected = (DateTime.Now - KeepaliveTime).TotalSeconds < 30;
                //如果连接状态有变化
                if (IsConnected != currentIsConnected)
                {
                    //如果当前已断开，则释放MediaId
                    if (!currentIsConnected)
                    {
                        PushLog("媒体服务器连接已断开");
                        int[] mediaIds = null;
                        lock (idleMediaIdQueue)
                            mediaIds = mediaInfoDict.Keys.ToArray();
                        foreach (var mediaId in mediaIds)
                            DestoryMediaId(mediaId);
                    }
                    IsConnected = currentIsConnected;
                    RaiseEvent_StateChanged();
                }
                beginCheckKeepalive(token);
            });
        }

        public ResultBase Keepalive(IPAddress remoteIpAddress)
        {
            //如果当前已连接
            if (IsConnected)
            {
                KeepaliveTime = DateTime.Now;
            }
            //如果当前未连接
            else
            {
                //验证配置 
                if (ValidateConfig().Result)
                {
                    KeepaliveTime = DateTime.Now;
                    IsConnected = true;
                    RaiseEvent_StateChanged();
                    PushLog("媒体服务器连接成功");
                }
            }
            return new ResultBase();
        }

        //rtsp/rtmp/rtp推流鉴权
        public object OnPublish(PublishInfo content)
        {
            var streamId = content.Stream;
            var mediaId = MediaStreamUtils.GetMediaIdFromStreamId(streamId);
            var mediaInfo = GetMediaInfo(mediaId);
            if (mediaInfo == null)
                return new { code = -1, msg = $"Stream[Id:{streamId},MediaId:{mediaId}] not exist." };
            mediaInfo.PublishTime = DateTime.Now;
            PushLog($"媒体流推流鉴权成功。{mediaInfo}");
            return new
            {
                code = 0,
                msg = "success",
                enable_hls = false,
                enable_rtsp = false,
                enable_rtmp = true,
                enable_ts = false,
                enable_fmp4 = false,
                enable_mp4 = false,
                enable_audio = true
            };
        }

        public ResultBase OnStreamNotFound(StreamNotFoundInfo content)
        {
            var streamId = content.Stream;
            var mediaId = MediaStreamUtils.GetMediaIdFromStreamId(streamId);
            var mediaInfo = GetMediaInfo(mediaId);
            if (mediaInfo == null)
                return new ResultBase { Code = -1, Msg = $"Stream[Id:{streamId},MediaId:{mediaId}] not exist." };
            DestoryMediaId(mediaId);
            return new ResultBase();
        }

        private class StreamRegisteredInfoContext
        {
            public StreamChangedInfo Info { get; private set; }
            public Task<StreamChangedInfo> Task { get; private set; }
            public StreamRegisteredInfoContext()
            {
                Task = new Task<StreamChangedInfo>(() => Info);
            }

            public void SetStreamRegisteredInfo(StreamChangedInfo info)
            {
                Info = info;
                Task.Start();
            }
        }

        private Dictionary<int, StreamRegisteredInfoContext> streamRegisteredContextDict = new Dictionary<int, StreamRegisteredInfoContext>();

        public Task<StreamChangedInfo> GetStreamRegisteredTask(int mediaId)
        {
            lock (streamRegisteredContextDict)
            {
                var context = new StreamRegisteredInfoContext();
                streamRegisteredContextDict[mediaId] = context;
                return context.Task;
            }
        }

        public void OnStreamChanged(StreamChangedInfo content)
        {
            var streamId = content.Stream;
            var mediaId = MediaStreamUtils.GetMediaIdFromStreamId(streamId);
            var mediaInfo = GetMediaInfo(mediaId);

            //如果是流注册
            if (content.Regist)
            {
                StreamRegisteredInfoContext context = null;
                lock (streamRegisteredContextDict)
                {
                    if (streamRegisteredContextDict.ContainsKey(mediaId))
                    {
                        context = streamRegisteredContextDict[mediaId];
                        streamRegisteredContextDict.Remove(mediaId);
                    }
                }
                if (context == null)
                {
                    PushLog($"WARN:在streamRegisteredContextDict中未找到mediaId:{mediaId}");
                    return;
                }
                context.SetStreamRegisteredInfo(content);

                var message = $"媒体流已注册。";
                if (mediaInfo == null)
                {
                    message += $"StreamId: {streamId}, MediaId: {mediaId}";
                }
                else
                {
                    mediaInfo.StreamRegistTime = DateTime.Now;
                    message += mediaInfo.ToString();
                }
                PushLog(message);
                var channel = GetChannel(mediaInfo);
                channel?.PushLog(message);
            }
            //如果是流注销
            else
            {
                var message = $"媒体流已注销。";
                if (mediaInfo == null)
                {
                    message += $"StreamId: {streamId}, MediaId: {mediaId}";
                }
                else
                {
                    message += mediaInfo.ToString();
                    var channel = GetChannel(mediaInfo);
                    if (channel != null)
                    {
                        channel.LiveStreamInfo = null;
                        channel.PushLog(message);
                    }
                }
                PushLog(message);
                DestoryMediaId(mediaId);
            }
            RaiseEvent_StateChanged();
        }

        public StreamNoneReaderInfoResult OnStreamNoneReader(StreamNoneReaderInfo content)
        {
            var streamId = content.Stream;
            var mediaId = MediaStreamUtils.GetMediaIdFromStreamId(streamId);
            var mediaInfo = GetMediaInfo(mediaId);

            var message = $"媒体流无人观看。";
            if (mediaInfo == null)
            {
                message += $"StreamId: {streamId}, MediaId: {mediaId}";
            }
            else
            {
                message += mediaInfo.ToString();
                var channel = GetChannel(mediaInfo);
                if (channel != null)
                {
                    channel.LiveStreamInfo = null;
                    channel.PushLog(message);
                }
            }
            PushLog(message);
            DestoryMediaId(mediaId);
            RaiseEvent_StateChanged();
            return new StreamNoneReaderInfoResult();
        }

        private Channel GetChannel(MediaInfo mediaInfo)
        {
            if (mediaInfo.Channel is Channel)
                return (Channel)mediaInfo.Channel;
            return ConfigDbContext.CacheContext.Find(new Channel(mediaInfo.Device.Id, mediaInfo.Channel.Id));
        }

        public void DestoryMediaId(int mediaId)
        {
            MediaInfo mediaInfo = null;
            lock (idleMediaIdQueue)
            {
                if (!mediaInfoDict.ContainsKey(mediaId))
                    return;
                mediaInfo = mediaInfoDict[mediaId];
                mediaInfoDict.Remove(mediaId);
                idleMediaIdQueue.Enqueue(mediaId);
            }
            var driverContext = DriverManager.Instance.GetDriverContext(mediaInfo.Device.DriverId);
            _ = driverContext?.DestoryStream(
                mediaInfo.Device.Id,
                mediaInfo.Channel.Id,
                mediaId);
            
            PushLog($"媒体编号已销毁。{mediaInfo}");
        }

        public async Task DelStreamProxy(string streamProxyKey)
        {
            if (string.IsNullOrEmpty(streamProxyKey))
                throw new ArgumentNullException(nameof(streamProxyKey));
            await client.DelStreamProxy(streamProxyKey);
        }

        public MediaInfo GenerateMediaInfo(DeviceInfo device, ChannelInfo channel, bool isLiveMedia)
        {
            MediaInfo media = null;
            lock (idleMediaIdQueue)
            {
                if (idleMediaIdQueue.Count == 0)
                    throw new ApplicationException("当前没有空闲的MediaId");
                var mediaId = idleMediaIdQueue.Dequeue();
                var ssrc = MediaStreamUtils.GetSSRC(isLiveMedia ? 0 : 1, mediaId);
                var streamId = MediaStreamUtils.GetStreamId(ssrc);
                media = new MediaInfo()
                {
                    MediaId = mediaId,
                    SSRC = ssrc,
                    StreamId = streamId,
                    Device = device,
                    Channel = channel,
                    CreateTime = DateTime.Now
                };
                mediaInfoDict[mediaId] = media;
            }
            var message = $"媒体已创建。{media}";
            if (channel is Channel logger)
                logger.PushLog(message);
            PushLog(message);
            return media;
        }

        public MediaInfo GenerateMediaInfo(DeviceInfo device, ChannelInfo channel, string ssrc)
        {
            MediaInfo media = null;
            var mediaId = MediaStreamUtils.GetMediaIdFromSSRC(ssrc);

            lock (idleMediaIdQueue)
            {
                if(!idleMediaIdQueue.Contains(mediaId))
                    throw new ApplicationException($"SSRC:{ssrc},MediaId[{mediaId}]已被使用！");
                var idleMediaIds = idleMediaIdQueue.Where(t => t != mediaId).ToArray();
                idleMediaIdQueue.Clear();
                foreach (var idleMediaId in idleMediaIds)
                    idleMediaIdQueue.Enqueue(idleMediaId);
                var streamId = MediaStreamUtils.GetStreamId(ssrc);
                media = new MediaInfo()
                {
                    MediaId = mediaId,
                    SSRC = ssrc,
                    StreamId = streamId,
                    Device = device,
                    Channel = channel,
                    CreateTime = DateTime.Now
                };
                mediaInfoDict[mediaId] = media;
            }
            var message = $"媒体已创建。{media}";
            if (channel is Channel logger)
                logger.PushLog(message);
            PushLog(message);
            return media;
        }

        public async Task<string> AddStreamProxy(StreamInfo streamInfo, string streamUrl)
        {
            var rep = await client.AddStreamProxy(
                "__defaultVhost__",
                streamInfo.App,
                streamInfo.Stream,
                streamUrl,
                enable_rtmp: true);
            if (rep.Code != Quick.ZLMediaKit.HttpApi.Model.ApiCodeEnum.Success)
                throw new ApplicationException("添加流代理出错，原因：" + JsonSerializer.Serialize(rep));
            return rep.Data.Key;
        }
    }
}
