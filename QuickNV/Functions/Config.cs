using Quick.EntityFrameworkCore.Plus;
using Quick.Fields;
using YiQiDong.Agent;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Model;

namespace QuickNV.Functions;

public class Config : YiQiDong.Core.Functions.ModelJsonConfig<ConfigModel>
{
    public Config() : base(
        ConfigModelSerializerContext.Default.ConfigModel,
        AgentContext.Container?.ContainerFolder ?? AppContext.BaseDirectory,
        () => AgentContext.Container.AutoStart,
        "config.json")
    {}

    public override string Name => "配置";
    private const string TAB_APP_DB = nameof(TAB_APP_DB);

    protected override List<FieldForGet> innerGet(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        return new List<FieldForGet>()
        {
            new FieldForGet()
            {
                Type = FieldType.ContainerTab,
                Children =
                [
                    getWebServiceGroup(request,requestModel,isReadOnly),
                    getAppDbGroup(request,requestModel,isReadOnly),
                    getYiRenZhengGroup(request,requestModel,isReadOnly),
                    getInterfaceConfigGroup(request,requestModel,isReadOnly),
                ]
            }
        };
    }

    protected FieldForGet getWebServiceGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "Web服务",
            Children =
            [
                new ()
                {
                    Id = nameof(ConfigModel.WebUrls),
                    Name = "Web服务地址",
                    Input_AllowBlank = false,
                    Input_RegularExpression = "^http://((\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])\\.(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])|\\*)(\\:([0-9]|[1-9]\\d{1,3}|[1-5]\\d{4}|6[0-5]{2}[0-3][0-5]))?$",
                    Type = FieldType.InputText,
                    Value = model.WebUrls,
                    Input_ReadOnly = isReadOnly
                },
                new ()
                {
                    Id = nameof(ConfigModel.WebPassword),
                    Name = "管理密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = model.WebPassword,
                    Input_ReadOnly = isReadOnly
                },
                new ()
                {
                    Id = nameof(ConfigModel.SessionCookieName),
                    Name = "Session的Cookie名称",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = model.SessionCookieName,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }


    private IDbContextConfigHandler configHandler;

    public override ConfigModel ReadConfig()
    {
        var config = base.ReadConfig();
        configHandler = DbUtils.GetDbContextConfigHandler(
            config.AppDb.DbType,
            t => ModelsJsonSerializerContext.Default2,
            config.AppDb.DbConnectionParameter);
        return config;
    }

    public override void WriteConfig(ConfigModel model)
    {
        if (configHandler != null)
            model.AppDb.DbConnectionParameter = DbUtils.SerializerConfigHandler(configHandler);
        base.WriteConfig(model);
    }

    protected FieldForGet getAppDbGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "数据库",
            Children =
            [
                new ()
                {
                    Type = FieldType.ContainerTab,
                    Children = [
                        model.AppDb.GetDbConfigGroup(request,isReadOnly,nameof(ConfigModel.AppDb),"数据库连接",
                            t=>new ConfigDbContext(t),
                            t => ModelsJsonSerializerContext.Default2,
                            ()=> configHandler,
                            t=>configHandler=t)
                    ]
                }
            ]
        };
    }

    protected FieldForGet getInterfaceConfigGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        var defaultModel = ConfigModel.Default;
        return model.QpInterface.GetConfigGroup(request, isReadOnly, nameof(model.QpInterface), "对外接口", defaultModel.QpInterface);
    }

    protected FieldForGet getYiRenZhengGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "易认证连接",
            Children =
            [
                new ()
                {
                    Id = nameof(ConfigModel.YiRenZhengInterfaceUrl),
                    Name = "URL",
                    Description = "如果不配置，则使用ApiKey进行页面访问认证。",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = model.YiRenZhengInterfaceUrl,
                    Input_ReadOnly = isReadOnly
                },
                new ()
                {
                    Id = nameof(ConfigModel.YiRenZhengInterfacePassword),
                    Name = "密码",
                    Input_AllowBlank = false,
                    Description = "默认密码为:123456",
                    Type = FieldType.InputText,
                    Value = model.YiRenZhengInterfacePassword,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

}
