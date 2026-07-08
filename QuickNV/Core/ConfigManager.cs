using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Core
{
    public class ConfigManager
    {


        public static ConfigManager Instance { get; } = new ConfigManager();


        public string GetConfig(string id)
        {
            var config = ConfigDbContext.CacheContext.Find(new Model.Config(id));
            return config?.Value;
        }

        public void SetConfig(string id, string value)
        {
            var config = ConfigDbContext.CacheContext.Find(new Model.Config(id));
            if (config == null)
            {
                config = new Model.Config(id);
                config.Value = value;
                ConfigDbContext.CacheContext.Add(config);
            }
            else
            {
                config.Value = value;
                ConfigDbContext.CacheContext.Update(config);
            }
        }
    }
}
