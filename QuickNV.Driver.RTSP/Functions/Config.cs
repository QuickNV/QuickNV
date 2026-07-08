using QuickNV.Driver.Agent.Functions;

namespace QuickNV.Driver.RTSP.Functions;

public class Config : DriverModelJsonConfig<ConfigModel>
{
    public static Config Instance { get; private set; }

    public Config() : base(
        ConfigModelSerializerContext.Default.ConfigModel)
    {
        Instance = this;
    }
}
