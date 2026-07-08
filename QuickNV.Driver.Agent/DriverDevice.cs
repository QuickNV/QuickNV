using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Driver.Agent
{
    public class DriverDevice<TConfig, TChannelConfig> : DeviceInfo
        where TConfig : new()
        where TChannelConfig : new()
    {
        public TConfig Config { get; private set; }        

        private Dictionary<string, DriverChannel<TChannelConfig>> channelDict = new Dictionary<string, DriverChannel<TChannelConfig>>();

        public DriverChannel<TChannelConfig>[] GetChannels()
        {
            lock (channelDict)
                return channelDict.Values.ToArray();
        }

        public DriverChannel<TChannelConfig> GetChannel(string channelId)
        {
            lock (channelDict)
            {
                if (channelDict.TryGetValue(channelId, out var channel))
                    return channel;
                return null;
            }
        }

        public void AddChannel(DriverChannel<TChannelConfig> channel)
        {
            lock (channelDict)
                channelDict[channel.Id] = channel;
        }

        public void DeleteChannel(DriverChannel<TChannelConfig> channel)
        {
            lock (channelDict)
            {
                if (channelDict.ContainsKey(channel.Id))
                    channelDict.Remove(channel.Id);
            }
        }

        public DriverDevice(DeviceInfo deviceInfo)
        {
            Id = deviceInfo.Id;
            Name = deviceInfo.Name;
            DriverConfig = deviceInfo.DriverConfig;
            Manufacturer = deviceInfo.Manufacturer;
            Model = deviceInfo.Model;
            SerialNumber = deviceInfo.SerialNumber;
            if (!string.IsNullOrEmpty(DriverConfig))
                try { Config = JsonSerializer.Deserialize<TConfig>(DriverConfig); }
                catch { }
            if (Config == null)
            {
                Config = new TConfig();
                DriverConfig = JsonSerializer.Serialize(Config, new JsonSerializerOptions() { WriteIndented = true });
            }
        }
    }
}
