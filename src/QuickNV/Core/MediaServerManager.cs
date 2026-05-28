using Quick.EntityFrameworkCore.Plus;
using QuickNV.Core.Web;
using QuickNV.Model;

namespace QuickNV.Core
{
    public class MediaServerManager
    {
        public static MediaServerManager Instance { get; } = new MediaServerManager();
        private Dictionary<string, MediaServerContext> mediaServerDict = new Dictionary<string, MediaServerContext>();
        private Dictionary<string, MediaServerContext> mediaServerDict_Cache = new Dictionary<string, MediaServerContext>();

        public void Start()
        {
            foreach (var model in ConfigDbContext.CacheContext.Query<MediaServer>())
                AddMediaServer(model);
        }

        public void Stop()
        {
            foreach (var mediaServer in GetMediaServers())
                RemoveMediaServer(mediaServer.Model.Id);
        }

        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event EventHandler StateChanged;

        private void RaiseEvent_StateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public MediaServerContext[] GetMediaServers() => mediaServerDict_Cache.Values.ToArray();
        public int GetMediaServerCount() => mediaServerDict_Cache.Count;
        public int GetConnectedMediaServersCount() => mediaServerDict_Cache.Count(t => t.Value.IsConnected);
        public int GetMediaCount() => mediaServerDict_Cache.Sum(t => t.Value.GetMediaCount());

        public MediaServerContext[] QueryMediaServers(string keywords)
        {
            if (keywords == null)
                return GetMediaServers();

            return mediaServerDict_Cache.Values
                .Where(t => t.Model.Id.Contains(keywords) || t.Model.Name.Contains(keywords))
                .ToArray();
        }

        public MediaServerContext GetMediaServer(string mediaServerId)
        {
            if (mediaServerDict_Cache.ContainsKey(mediaServerId))
                return mediaServerDict_Cache[mediaServerId];
            return null;
        }

        public void AddMediaServer(MediaServer model)
        {
            var mediaServer = new MediaServerContext(model);
            lock (mediaServerDict)
            {
                if (mediaServerDict.ContainsKey(mediaServer.Model.Id))
                    throw new ApplicationException($"Already has MediaServer with id[{mediaServer.Model.Id}]");
                mediaServerDict[mediaServer.Model.Id] = mediaServer;
                mediaServerDict_Cache = mediaServerDict.ToDictionary(t => t.Key, t => t.Value);
            }
            mediaServer.StateChanged += MediaServer_StateChanged;
            RaiseEvent_StateChanged();
            ReverseProxyManager.Instance.AddMediaServer(model);
        }

        private void MediaServer_StateChanged(object sender, EventArgs e)
        {
            RaiseEvent_StateChanged();
        }

        public void RemoveMediaServer(string mediaServerId)
        {
            using (var mediaServer = GetMediaServer(mediaServerId))
            {
                if (mediaServer == null)
                    return;
                lock (mediaServerDict)
                {
                    if (mediaServerDict.ContainsKey(mediaServerId))
                        mediaServerDict.Remove(mediaServerId);
                    mediaServerDict_Cache = mediaServerDict.ToDictionary(t => t.Key, t => t.Value);
                }
                mediaServer.StateChanged -= MediaServer_StateChanged;
                RaiseEvent_StateChanged();
                ReverseProxyManager.Instance.RemoveMediaServer(mediaServer.Model);
            }
        }

        //获取下一个媒体服务器
        public MediaServerContext GetNext()
        {
            var array = GetMediaServers();
            return array
                //已连接的媒体服务器
                .Where(t => t.IsConnected)
                //打乱顺序
                .OrderBy(t => Random.Shared.Next())
                //按媒体流数量排序
                .OrderBy(t => t.GetMediaCount())
                //取第一个即媒体流数量最少的
                .FirstOrDefault();
        }
    }
}
