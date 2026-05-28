using SIPSorcery.SIP;
using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using YiQiDong.Agent;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.GB28181
{
    public class DeviceContext
    {
        private CancellationTokenSource cts;
        private SipServer sipServer;

        /// <summary>
        /// 在线状态改变事件
        /// </summary>
        public event EventHandler<DeviceOnlineStateChangedEventArgs> OnlineStateChanged;
        private void RaiseEvent_OnlineStateChanged(string reason)
        {
            OnlineStateChanged?.Invoke(this, new DeviceOnlineStateChangedEventArgs()
            {
                DeviceContext = this,
                IsOnline = IsOnline,
                Reason = reason
            });
        }

        private Command.DeviceInfo.Response DeviceInfo;
        private Command.ConfigDownload.Response DeviceConfig;

        public DriverDevice<DeviceConfig, ChannelConfig> Model { get; private set; }

        public DeviceContext(SipServer sipServer, DriverDevice<DeviceConfig, ChannelConfig> model)
        {
            this.sipServer = sipServer;
            Model = model;
        }

        public bool IsOnline { get; private set; } = false;
        public SIPEndPoint LocalEndPoint { get; private set; }
        public SIPEndPoint RemoteEndPoint { get; private set; }
        public SIPURI ContactUri { get; private set; }
        public SIPRequest LastRequest { get; private set; }
        public long RegisterExpiry { get; private set; }
        public DateTime RegisterTime { get; private set; }
        public DateTime KeepAliveTime { get; private set; }

        private ConcurrentDictionary<string, ChannelContext> channelDict = new ConcurrentDictionary<string, ChannelContext>();


        public int ChannelsCount => channelDict.Count;
        public ChannelContext[] GetChannels() => channelDict.Values.OrderBy(t => t.Model.Id).ToArray();

        public ChannelContext GetChannel(string id)
        {
            ChannelContext ret = null;
            if (channelDict.TryGetValue(id, out ret))
                return ret;
            return null;
        }

        public async Task Register(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest, long expiry)
        {
            //如果本次注册时间与上次注册时间间隔小于5秒，则忽略
            if ((DateTime.Now - RegisterTime).TotalSeconds < 5)
                return;

            RegisterExpiry = expiry;

            cts?.Cancel();
            cts = new CancellationTokenSource();

            //如果请求头中没有Contact信息
            if (sipRequest.Header.Contact == null
                || sipRequest.Header.Contact.Count == 0)
            {
                ContactUri = new SIPURI(SIPSchemesEnum.sip, remoteEndPoint);
            }
            //否则请求头中有Contact信息，直接使用Contact信息
            else
            {
                var headerContact = sipRequest.Header.Contact[0];
                ContactUri = headerContact.ContactURI;
            }
            LocalEndPoint = localEndPoint;
            RemoteEndPoint = remoteEndPoint;
            LastRequest = sipRequest;
            RegisterTime = DateTime.Now;
            KeepAliveTime = DateTime.Now;
            IsOnline = true;

            //设备信息查询
            await QueryDeviceInfo();
            //设备配置查询
            await QueryDeviceConfig();
            //设备目录信息查询
            await QueryCatalog();

            //开始定时发送设备状态信息
            beginCheckKeepalive(cts.Token);

            RaiseEvent_OnlineStateChanged(null);
        }

        //取消注册
        public void Unregister(string reason)
        {
            cts?.Cancel();
            cts = null;
            IsOnline = false;
            foreach (var channel in GetChannels())
                channel.Dispose();
            channelDict.Clear();
            RaiseEvent_OnlineStateChanged(reason);
        }

        private void beginCheckKeepalive(CancellationToken token)
        {
            //每5秒检测一次心跳是否超时
            Task.Delay(5000, token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                //如果离上次心跳时间大于约定超时时间，则取消注册设备
                var keepaliveTimeoutTimeSpan = TimeSpan.FromSeconds(DeviceConfig.BasicParam.HeartBeatInterval * DeviceConfig.BasicParam.HeartBeatCount);
                if ((DateTime.Now - KeepAliveTime) > keepaliveTimeoutTimeSpan)
                {
                    var reason = $"心跳超时时间已到";
                    AgentContext.LogDebug($"[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]{reason}");
                    AgentContext.LogDebug(reason);
                    Unregister(reason);
                    return;
                }
                //如果离上次注册时间大于约定的注册超时时间，则取消注册设备
                if ((DateTime.Now - RegisterTime) > TimeSpan.FromSeconds(RegisterExpiry))
                {
                    var reason = $"注册超时时间已到";
                    AgentContext.LogDebug($"[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]{reason}");
                    Unregister(reason);
                    return;
                }
                beginCheckKeepalive(token);
            });
        }

        private async Task QueryCatalog()
        {
            var body = new Command.Catalog.Query()
            {
                DeviceID = Model.Id,
                SN = new Random().Next(1, ushort.MaxValue)
            };
            var rep = await sipServer.SendMessageRequestAsync(this, body);
            if (rep.Status == SIPResponseStatusCodesEnum.Ok)
                AgentContext.LogDebug($"向[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]发送查询目录指令成功");
            else
                AgentContext.LogDebug($"向[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]发送查询目录指令失败。Status:{rep.StatusCode} {rep.Status}, Reson:{rep.ReasonPhrase}");
        }

        public void UpdateCatalog(Command.Catalog.Response catalog)
        {
            var channels = catalog.AllChannelsInfo.Items
                .Where(t =>
                {
                    if (string.IsNullOrEmpty(t.ChannelId) || t.ChannelId.Length < 20)
                        return false;
                    var type = t.ChannelId.Substring(10, 3);
                    switch (type)
                    {
                        case "131":
                        case "132":
                            return true;
                        default:
                            return false;
                    }
                })
                .ToArray();
            AgentContext.LogDebug($"从[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]接收到查询目录指令响应。其中包含[{channels.Length}]个通道.");
            foreach (var channel in channels)
            {
                var channelModel = new ChannelModel()
                {
                    DeviceId = Model.Id,
                    Id = channel.ChannelId,
                    Name = channel.Name,
                    DriverConfig = JsonSerializer.Serialize(new ChannelConfig(), new JsonSerializerOptions() { WriteIndented = true })
                };
                channelDict.TryAdd(channel.ChannelId, new ChannelContext(sipServer, this, channelModel));
            }
        }

        private async Task QueryDeviceInfo()
        {
            var body = new Command.DeviceInfo.Query()
            {
                DeviceID = Model.Id,
                SN = new Random().Next(1, ushort.MaxValue)
            };
            var rep = await sipServer.SendMessageRequestAsync(this, body);
            if (rep.Status == SIPResponseStatusCodesEnum.Ok)
                AgentContext.LogDebug($"向[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]发送查询设备信息指令成功");
            else
                AgentContext.LogDebug($"向[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]发送查询设备信息指令失败。Status:{rep.StatusCode} {rep.Status}, Reson:{rep.ReasonPhrase}");
        }

        private async Task QueryDeviceConfig()
        {
            var body = new Command.ConfigDownload.Query()
            {
                DeviceID = Model.Id,
                SN = new Random().Next(1, ushort.MaxValue)
            };
            var rep = await sipServer.SendMessageRequestAsync(this, body);
            if (rep.Status == SIPResponseStatusCodesEnum.Ok)
                AgentContext.LogDebug($"向[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]发送查询设备配置指令成功");
            else
                AgentContext.LogDebug($"向[SIP设备编号:{Model.Id},远程端点:{RemoteEndPoint}]发送查询设备配置指令失败。Status:{rep.StatusCode} {rep.Status}, Reson:{rep.ReasonPhrase}");
        }

        public void UpdateDeviceInfo(Command.DeviceInfo.Response deviceInfo)
        {
            DeviceInfo = deviceInfo;
            Model.Manufacturer = DeviceInfo.Manufacturer;
            Model.Model = DeviceInfo.Model;
            Model.FirmwareVersion = DeviceInfo.Firmware;
        }

        public void UpdateDeviceConfig(Command.ConfigDownload.Response config)
        {
            DeviceConfig = config;

            //更新模型中的信息
            if (!DeviceConfig.BasicParam.Name.Equals(Model.Name))
                Model.Name = DeviceConfig.BasicParam.Name;
        }

        public bool Keepalive(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
        {
            //如果当前未注册，返回保活失败
            if (!IsOnline)
                return false;
            KeepAliveTime = DateTime.Now;
            return true;
        }

        public void SetResult_QueryRecordInfo(QuickNV.Driver.GB28181.Command.RecordInfo.Response recordInfo)
        {
            if (recordInfo == null)
                return;
            var channelContext = GetChannel(recordInfo.DeviceID);
            if (channelContext == null)
                return;
            channelContext.SetResult_QueryRecordInfo(recordInfo);
        }
    }
}
