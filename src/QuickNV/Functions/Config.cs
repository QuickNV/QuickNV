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
                    getDriverInterfaceGroup(request,requestModel,isReadOnly),
                    getNorthInterfaceGroup(request,requestModel,isReadOnly)                    
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

    protected FieldForGet getDriverInterfaceGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "驱动服务",
            Children =
            [
                new()
                {
                    Id = nameof(ConfigModel.DriverInterfacePassword),
                    Name = "密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = model.DriverInterfacePassword,
                    Input_ReadOnly = isReadOnly
                },
                new ()
                {
                    Id = "Pipe",
                    Name = "管道",
                    Type = FieldType.ContainerGroup,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(ConfigModel.DriverInterfacePipeEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.pipe://./QuickNV.DriverInterface",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.DriverInterfacePipeEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(ConfigModel.DriverInterfacePipeName),
                            Name = "管道名称",
                            Input_AllowBlank = false,
                            Type = model.DriverInterfacePipeEnable ? FieldType.InputText: FieldType.InputHidden,
                            Value = model.DriverInterfacePipeName,
                            Input_ReadOnly = isReadOnly
                        },
                    ]
                },
                new ()
                {
                    Id = "WebSocket",
                    Name = "WebSocket",
                    Type = FieldType.ContainerGroup,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(ConfigModel.DriverInterfaceWebSocketEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.ws://127.0.0.1:8097/ws/driver",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.DriverInterfaceWebSocketEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        }      
                    ]
                },
                new ()
                {
                    Id = "TCP",
                    Name = "TCP",
                    Type = FieldType.ContainerGroup,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(ConfigModel.DriverInterfaceTcpEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.tcp://127.0.0.1:8097",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.DriverInterfaceTcpEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(ConfigModel.DriverInterfaceTcpListenAddress),
                            Name = "监听地址",
                            Input_AllowBlank = false,
                            Type = model.DriverInterfaceTcpEnable ? FieldType.InputText: FieldType.InputHidden,
                            Value = model.DriverInterfacePassword,
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(ConfigModel.DriverInterfaceTcpListenPort),
                            Name = "监听端口",
                            Input_AllowBlank = false,
                            Type = model.DriverInterfaceTcpEnable ? FieldType.InputText: FieldType.InputHidden,
                            Value = model.DriverInterfaceTcpListenPort.ToString(),
                            Input_ReadOnly = isReadOnly
                        }
                    ]
                }
            ]
        };
    }

    protected FieldForGet getNorthInterfaceGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "北向服务",
            Children =
            [
                new()
                {
                    Id = nameof(ConfigModel.NorthInterfacePassword),
                    Name = "密码",
                    Description = "默认密码：123456",
                    Input_AllowBlank = false,
                    Type = FieldType.InputText,
                    Value = model.NorthInterfacePassword,
                    Input_ReadOnly = isReadOnly
                },
                new ()
                {
                    Id = "Pipe",
                    Name = "管道",
                    Type = FieldType.ContainerGroup,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(ConfigModel.NorthInterfacePipeEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.pipe://./QuickNV.NorthInterface",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.NorthInterfacePipeEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(ConfigModel.NorthInterfacePipeName),
                            Name = "管道名称",
                            Input_AllowBlank = false,
                            Type = model.NorthInterfacePipeEnable? FieldType.InputText: FieldType.InputHidden,
                            Value = model.NorthInterfacePipeName,
                            Input_ReadOnly = isReadOnly
                        },
                    ]
                },
                new ()
                {
                    Id = "WebSocket",
                    Name = "WebSocket",
                    Type = FieldType.ContainerGroup,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(ConfigModel.NorthInterfaceWebSocketEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.ws://127.0.0.1:8097/ws/north",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.NorthInterfaceWebSocketEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        }      
                    ]
                },
                new ()
                {
                    Id = "TCP",
                    Name = "TCP",
                    Type = FieldType.ContainerGroup,
                    Children =
                    [
                        new()
                        {
                            Id = nameof(ConfigModel.NorthInterfaceTcpEnable),
                            Name = "启用",
                            Description = "接口地址示例：qp.tcp://127.0.0.1:8097",
                            Input_AllowBlank = false,
                            Type = FieldType.InputSelect,
                            InputSelect_Options = new Dictionary<string,string>()
                            {
                                [true.ToString()] = "是",
                                [false.ToString()] = "否"
                            },
                            PostOnChanged = true,
                            Value = model.NorthInterfaceTcpEnable.ToString(),
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(ConfigModel.NorthInterfaceTcpListenAddress),
                            Name = "监听地址",
                            Input_AllowBlank = false,
                            Type = model.NorthInterfaceTcpEnable? FieldType.InputText: FieldType.InputHidden,
                            Value = model.NorthInterfacePassword,
                            Input_ReadOnly = isReadOnly
                        },
                        new()
                        {
                            Id = nameof(ConfigModel.NorthInterfaceTcpListenPort),
                            Name = "监听端口",
                            Input_AllowBlank = false,
                            Type = model.NorthInterfaceTcpEnable? FieldType.InputNumber: FieldType.InputHidden,
                            Value = model.NorthInterfaceTcpListenPort.ToString(),
                            Input_ReadOnly = isReadOnly
                        }
                    ]
                }
            ]
        };
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
