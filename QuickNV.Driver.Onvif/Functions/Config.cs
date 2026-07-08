using QuickNV.Driver.Agent.Functions;

namespace QuickNV.Driver.Onvif.Functions;

public class Config : DriverModelJsonConfig<ConfigModel>
{
    public static Config Instance { get; private set; }

    public Config() : base(
        ConfigModelSerializerContext.Default.ConfigModel)
    {
        Instance = this;
    }
}
