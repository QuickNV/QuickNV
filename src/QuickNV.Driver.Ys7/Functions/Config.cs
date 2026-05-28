using Quick.Fields;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Agent.Functions;

namespace QuickNV.Driver.Ys7.Functions;

public class Config : DriverModelJsonConfig<ConfigModel>
{
    public static Config Instance { get; private set; }

    public Config() : base(
        ConfigModelSerializerContext.Default.ConfigModel)
    {
        Instance = this;
    }

    private FieldForGet getDriverGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Id = "DriverConfig",
            Type = FieldType.ContainerGroup,
            Name = "萤石开放平台",
            Children =
            [
                new()
                {
                    Id = nameof(ConfigModel.Ys7ServerUrl),
                    Name = "地址",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.Ys7ServerUrl,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.Ys7AppKey),
                    Name = "AppKey",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.Ys7AppKey,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.Ys7Secret),
                    Name = "Secret",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.Ys7Secret,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

    protected override List<FieldForGet> innerGet(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        return new List<FieldForGet>()
        {
            new FieldForGet()
            {
                Id="Tab",
                Type = FieldType.ContainerTab,
                Children =
                [
                    getQuickNVDriverInterfaceGroup(request,requestModel,isReadOnly),
                    getDriverGroup(request,requestModel,isReadOnly)
                ]
            }
        };
    }
}
