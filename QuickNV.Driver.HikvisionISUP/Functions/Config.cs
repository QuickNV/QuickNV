using Quick.Fields;
using YiQiDong.Protocol.V1.Model;
using QuickNV.Driver.Agent.Functions;

namespace QuickNV.Driver.HikvisionISUP.Functions;

public class Config : DriverModelJsonConfig<ConfigModel>
{
    public static Config Instance { get; private set; }

    public Config() : base(
        ConfigModelSerializerContext.Default.ConfigModel)
    {
        Instance = this;
    }

    private FieldForGet getCmsServiceGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "ISUP中心服务",
            Children =
            [
                new()
                {
                    Id = nameof(ConfigModel.CmsListenIPAddress),
                    Name = "监听IP地址",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.CmsListenIPAddress,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.CmsListenPort),
                    Name = "监听端口",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputNumber,
                    Value = model.CmsListenPort.ToString(),
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.CmsEncoding),
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
                    Value = model.CmsEncoding,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.CmsAccessSecurity),
                    Name = "访问安全模式",
                    Input_AllowBlank = false,
                    Type = FieldType.InputSelect,
                    InputSelect_Options = new Dictionary<string,string>()
                    {
                        ["CompatibleMode"] = "兼容模式（允许任意版本的协议接入）",
                        ["NormalMode"] = "普通模式（只支持4.0以下版本，不支持协议安全的版本接入）",
                        ["SecurityMode"] = "安全模式（只允许4.0以上版本，支持协议安全的版本接入）"
                    },
                    Value = model.CmsAccessSecurity.ToString(),
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.CmsPublicIPAddress),
                    Description = "ISUP 5.0及以上版本时必须填写公开IP地址",
                    Name = "公开IP地址",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.CmsPublicIPAddress,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.CmsPublicPort),
                    Name = "公开端口",
                    Description = "ISUP 5.0及以上版本时必须填写公开端口",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputNumber,
                    Value = model.CmsPublicPort.ToString(),
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.CmsPassword),
                    Name = "认证密码",
                    Description = "ISUP 5.0及以上版本时必须填写认证密码",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.CmsPassword,
                    Input_ReadOnly = isReadOnly
                }
            ]
        };
    }

    private FieldForGet getSmsServiceGroup(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false)
    {
        var model = requestModel ?? Model;
        return new FieldForGet()
        {
            Type = FieldType.ContainerGroup,
            Name = "ISUP媒体转发服务",
            Children =
            [
                new()
                {
                    Id = nameof(ConfigModel.SmsListenIPAddress),
                    Name = "监听IP地址",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsListenIPAddress,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.SmsListenPort),
                    Name = "监听端口",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputNumber,
                    Value = model.SmsListenPort.ToString(),
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.SmsPublicIPAddress),
                    Name = "公开IP地址",
                    Input_AllowBlank = false,
                    Type =  FieldType.InputText,
                    Value = model.SmsPublicIPAddress,
                    Input_ReadOnly = isReadOnly
                },
                new()
                {
                    Id = nameof(ConfigModel.SmsLinkMode),
                    Name = "连接模式",
                    Input_AllowBlank = false,
                    Type = FieldType.InputSelect,
                    InputSelect_Options = new Dictionary<string,string>()
                    {
                        ["TCP"] = "TCP",
                        ["UDP"] = "UDP",
                        ["HRUDP"] = "HRUDP"
                    },
                    Value = model.SmsLinkMode.ToString(),
                    Input_ReadOnly = isReadOnly
                }

            ]
        };
    }

    protected override FieldForGet[] getOtherGroups(FunctionRequest request, ConfigModel requestModel, bool isReadOnly = false) =>
    [
        getCmsServiceGroup(request,requestModel,isReadOnly),
        getSmsServiceGroup(request,requestModel,isReadOnly)
    ];
}
