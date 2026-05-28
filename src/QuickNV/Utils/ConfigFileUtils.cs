using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Utils
{
    public class ConfigFileUtils
    {
        private static string getTypeFilePath(Type type, string fileSuffix)
        {
            var fileName = type.FullName;
            if (!string.IsNullOrEmpty(fileSuffix))
                fileName += "." + fileSuffix;
            fileName += ".json";
            return fileName;
        }

        public static string GetTypeFilePath<T>(string fileSuffix)
        {
            return getTypeFilePath(typeof(T), fileSuffix);
        }

        public static T Load<T>(string fileSuffix = null)
        {
            try
            {
                var file = GetTypeFilePath<T>(fileSuffix);
                if (!File.Exists(file))
                    return default(T);
                var content = File.ReadAllText(file);
                return JsonSerializer.Deserialize<T>(content);
            }
            catch
            {
                return default(T);
            }
        }

        public static void Save(object configObj, string fileSuffix = null)
        {
            if (configObj == null)
                return;

            var file = getTypeFilePath(configObj.GetType(), fileSuffix);
            if (File.Exists(file))
                File.Delete(file);
            var content = JsonSerializer.Serialize(configObj);
            File.WriteAllText(file, content, Encoding.UTF8);
        }
    }
}
