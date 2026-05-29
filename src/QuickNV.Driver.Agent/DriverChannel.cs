using System.Text.Json;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Driver.Agent
{
    public class DriverChannel<TConfig> : ChannelInfo
        where TConfig : new()
    {
        public TConfig Config { get; private set; }

        public DriverChannel(ChannelInfo channelInfo)
        {
            Id = channelInfo.Id;
            Name = channelInfo.Name;
            DriverConfig = channelInfo.DriverConfig;
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