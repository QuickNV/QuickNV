using Quick.Fields;
using YiQiDong.Protocol.V1.Model;
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

    protected override List<FieldForGet> innerGet(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        return new List<FieldForGet>()
        {
            new FieldForGet()
            {                
                Type = FieldType.ContainerTab,
                Children =
                [
                    getQuickNVDriverInterfaceGroup(request,requestModel,isReadOnly)
                ]
            }
        };
    }
}
