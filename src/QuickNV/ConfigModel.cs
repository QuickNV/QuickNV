using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using YiQiDong.Core.JsonConverters;

namespace QuickNV;

[JsonSerializable(typeof(ConfigModel))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ConfigModelSerializerContext : JsonSerializerContext { }

public class ConfigModel
{
    /// <summary>
    /// Web服务地址
    /// </summary>
    public string WebUrls { get; set; } = "http://*:8097";
    /// <summary>
    /// 管理密码
    /// </summary>
    public string WebPassword { get; set; } = "123456";
    /// <summary>
    /// Session的Cookie名称
    /// </summary>
    public string SessionCookieName { get; set; } = "QuickNV.SID";

    /// <summary>
    /// 应用数据库类型
    /// </summary>
    public string AppDbType { get; set; } = "Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler";
    /// <summary>
    /// 应用数据库配置
    /// </summary>
    public JsonNode AppDbConfig { get; set; }    

    /// <summary>
    /// 驱动接口密码
    /// </summary>
    public string DriverInterfacePassword { get; set; } = "123456";
    /// <summary>
    /// 驱动接口是否启用WebSocket
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool DriverInterfaceWebSocketEnable { get; set; } = false;
    /// <summary>
    /// 驱动接口是否启用管道
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool DriverInterfacePipeEnable { get; set; } = true;
    /// <summary>
    /// 驱动接口管道名称
    /// </summary>
    public string DriverInterfacePipeName { get; set; } = "QuickNV.DriverInterface";
    /// <summary>
    /// 驱动接口是否启用TCP
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool DriverInterfaceTcpEnable { get; set; } = false;
    /// <summary>
    /// 驱动接口TCP监听地址
    /// </summary>
    public string DriverInterfaceTcpListenAddress { get; set; } = "0.0.0.0";
    /// <summary>
    /// 驱动接口TCP监听端口
    /// </summary>
    [JsonConverter(typeof(JsonInt32Converter))]
    public int DriverInterfaceTcpListenPort { get; set; } = 8098;
    /// <summary>
    /// 北向接口密码
    /// </summary>
    public string NorthInterfacePassword { get; set; } = "123456";
    /// <summary>
    /// 北向接口是否启用WebSocket
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool NorthInterfaceWebSocketEnable { get; set; } = false;
    /// <summary>
    /// 北向接口是否启用管道
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool NorthInterfacePipeEnable { get; set; } = true;
    /// <summary>
    /// 北向接口管道名称
    /// </summary>
    public string NorthInterfacePipeName { get; set; } = "QuickNV.NorthInterface";
    /// <summary>
    /// 北向接口是否启用TCP
    /// </summary>
    [JsonConverter(typeof(JsonBoolConverter))]
    public bool NorthInterfaceTcpEnable { get; set; } = false;
    /// <summary>
    /// 北向接口TCP监听地址
    /// </summary>
    public string NorthInterfaceTcpListenAddress { get; set; } = "0.0.0.0";
    /// <summary>
    /// 北向接口TCP监听端口
    /// </summary>
    [JsonConverter(typeof(JsonInt32Converter))]
    public int NorthInterfaceTcpListenPort { get; set; } = 8098;
    /// <summary>
    /// 易认证接口URL
    /// </summary>
    public string YiRenZhengInterfaceUrl { get; set; }
    /// <summary>
    /// 易认证接口密码
    /// </summary>
    public string YiRenZhengInterfacePassword { get; set; } = "123456";
}
