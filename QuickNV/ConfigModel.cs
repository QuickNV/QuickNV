using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol.InterfaceService;
using YiQiDong.Core.JsonConverters;

namespace QuickNV;

[JsonSerializable(typeof(ConfigModel))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class ConfigModelSerializerContext : JsonSerializerContext { }

public class ConfigModel
{
    public static ConfigModel Default { get; } = new();
    
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
    /// 配置数据库
    /// </summary>
    public DbConfigInfo AppDb { get; set; } = new()
    {
#if DEBUG
        DbType = "Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler",
        DbConnectionParameter = new JsonObject()
        {
            ["DataSource"] = "Config.db"
        }
#else
        DbType = "Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler",
#endif
    };

    /// <summary>
    /// 接口配置
    /// </summary>
    public QpInterfaceServiceConfig QpInterface { get; set; } = new()
    {
        EnablePipeline = true,
        PipelineServerOptions = new()
        {
            Password = "123456",
            PipeName = $"{nameof(QuickNV)}.{nameof(QpInterface)}"
        },
        WebSocketServerOptions = new()
        {
            Password = "123456",
            Path = "/qp/ws"
        },
        TcpServerOptions = new()
        {
            Password = "123456",
            Port = 8098
        }
    };

    /// <summary>
    /// 易认证接口URL
    /// </summary>
    public string YiRenZhengInterfaceUrl { get; set; }
    /// <summary>
    /// 易认证接口密码
    /// </summary>
    public string YiRenZhengInterfacePassword { get; set; } = "123456";
}
