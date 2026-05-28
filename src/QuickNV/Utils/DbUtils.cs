using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Quick.EntityFrameworkCore.Plus;
using YiQiDong.Agent;

namespace QuickNV.Utils;

public class DbUtils
{
    private static Dictionary<string, IDbContextConfigHandler> configHandlerDict;
    private static Dictionary<string, JsonTypeInfo> configHandlerTypeInfoDict;

    public static void Init()
    {
        AbstractDbContextConfigHandler.BackupFilePrefix = "QuickNV数据库备份";
        var sqliteDbFile = "Config.db";
        if (DebugUtils.IsInDebugMode())
            sqliteDbFile = Path.Combine("bin", "Debug", sqliteDbFile);
        configHandlerDict = new Dictionary<string, IDbContextConfigHandler>()
        {
            [typeof(Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler).FullName] =
                new Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler() { DataSource =  sqliteDbFile},
            [typeof(Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler).FullName] =
                new Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler(),
            [typeof(Quick.EntityFrameworkCore.Plus.Dm.DmDbContextConfigHandler).FullName] =
                new Quick.EntityFrameworkCore.Plus.Dm.DmDbContextConfigHandler()
            //[typeof(SqlServerDbContextConfigHandler).FullName] = new SqlServerDbContextConfigHandler()
        };
        configHandlerTypeInfoDict = new Dictionary<string, JsonTypeInfo>()
        {
            [typeof(Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandler).FullName] =
                Quick.EntityFrameworkCore.Plus.SQLite.SQLiteDbContextConfigHandlerSerializerContext.Default.SQLiteDbContextConfigHandler,
            [typeof(Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandler).FullName] =
                Quick.EntityFrameworkCore.Plus.MySql.MySqlDbContextConfigHandlerSerializerContext.Default.MySqlDbContextConfigHandler,
            [typeof(Quick.EntityFrameworkCore.Plus.Dm.DmDbContextConfigHandler).FullName] =
                Quick.EntityFrameworkCore.Plus.Dm.DmDbContextConfigHandlerSerializerContext.Default.DmDbContextConfigHandler,
            //[typeof(SqlServerDbContextConfigHandler).FullName] = SqlServerDbContextConfigHandlerSerializerContext.Default.SqlServerDbContextConfigHandler
        };
    }

    public static JsonNode SerializerConfigHandler(IDbContextConfigHandler configHandler)
    {
        var configHandlerTypeName = configHandler.GetType().FullName;
        if (!configHandlerTypeInfoDict.TryGetValue(configHandlerTypeName, out var configHandlerJsonTypeInfo))
            throw new ArgumentException($"GatewayDbType参数值错误，类型[{configHandlerTypeName}]未知！");
        return JsonSerializer.SerializeToNode(configHandler, configHandlerJsonTypeInfo);
    }

    private static string GetDatabaseName(string name)
    {
        if (string.IsNullOrEmpty(name))
            name = AgentContext.Container?.Id;
        if (string.IsNullOrEmpty(name))
            name = Assembly.GetEntryAssembly().GetName().Name;
        return name;
    }

    public static Dictionary<string, string> GetDbTypeDict() => configHandlerDict.ToDictionary(t => t.Key, t => t.Value.Name);

    public static IDbContextConfigHandler GetDbContextConfigHandler(ConfigModel Config)
    {
        if (string.IsNullOrEmpty(Config.AppDbType))
            Config.AppDbType = configHandlerDict.FirstOrDefault().Key;

        if (configHandlerDict.TryGetValue(Config.AppDbType, out var configHandler))
        {
            if (Config.AppDbConfig != null)
            {
                if (!configHandlerTypeInfoDict.TryGetValue(Config.AppDbType, out var configHandlerJsonTypeInfo))
                    throw new ArgumentException($"AppDbType参数值错误，类型[{Config.AppDbType}]未知！");
                configHandler = (IDbContextConfigHandler)JsonSerializer.Deserialize(Config.AppDbConfig, configHandlerJsonTypeInfo);
                configHandlerDict[Config.AppDbType] = configHandler;
            }
        }
        else
        {
            var item = configHandlerDict.FirstOrDefault();
            Config.AppDbType = item.Key;
            configHandler = item.Value;
            if (!configHandlerTypeInfoDict.TryGetValue(Config.AppDbType, out var configHandlerJsonTypeInfo))
                throw new ArgumentException($"AppDbType参数值错误，类型[{Config.AppDbType}]未知！");
            Config.AppDbConfig = JsonSerializer.SerializeToNode(configHandler, configHandlerJsonTypeInfo);
        }
        return configHandler;
    }
}