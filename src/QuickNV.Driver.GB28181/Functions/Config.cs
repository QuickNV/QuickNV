using Quick.Fields;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Agent.Functions;

namespace QuickNV.Driver.GB28181.Functions;

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
            Id = "SipServiceConfig",
            Type = FieldType.ContainerGroup,
            Name = "SIP服务",
            Children =
            [
                new()
                {
                    Id = nameof(ConfigModel.SipServerIpAddress),
                    Name = "SIP服务IP地址",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SipServerIpAddress,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.SipServerPort),
                    Name = "SIP服务端口",
                    Description = "SIP服务监听的端口",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputNumber,
                    Value = model.SipServerPort.ToString(),
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.SipDeviceId),
                    Name = "SIP设备编号",
                    Description = "格式：总共20位。1-8位：中心编码，9-10位：行业编码，11-13位：类型编码，14位：网络编码，15-20位：设备序号。参考文档《GB/T 28181-2016》附录D.1编码规则A",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SipDeviceId,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.SipRealm),
                    Name = "SIP服务域编号",
                    Description = "格式：总共10位。1-8位：中心编码，9-10位：行业编码。参考文档《GB/T 28181-2016》附录D.1编码规则A",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SipRealm,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.SipPassword),
                    Name = "SIP密码",
                    Description = "SIP设备注册时验证的密码",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SipPassword,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.Encoding),
                    Name = "字符编码",
                    Input_AllowBlank = false,
                    Type = FieldType.InputSelect,
                    InputSelect_Options = new Dictionary<string,string>()
                    {
                        ["ASCII"] = "ASCII",
                        ["GB2312"] = "GB2312",
                        ["GB18030"] = "GB18030",
                        ["UTF-8"] = "UTF-8"
                    },
                    Value = model.Encoding,
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
