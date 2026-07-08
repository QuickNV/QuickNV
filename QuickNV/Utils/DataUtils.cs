using System.Text.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace QuickNV.Utils
{
    public class DataUtils
    {

        /// <summary>
        /// 克隆对象，利用Json组件的序列化和反序列化来克隆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T Clone<T>(T t)
        {
            ICloneable cloneable = t as ICloneable;
            if (cloneable == null)
            {
                return Convert<T>(t);
            }
            else
            {
                return (T)cloneable.Clone();
            }
        }

        /// <summary>
        /// 复制属性的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyPropertyValue<T>(T source, T target)
        {
            Type type = typeof(T);
            foreach (PropertyInfo pi in type.GetProperties())
            {
                if (pi.GetMethod == null
                    || pi.SetMethod == null
                    || pi.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                    continue;
                Object value = pi.GetValue(source, null);
                pi.SetValue(target, value, null);
            }
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Convert<T>(Object source)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source));
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object Convert(Type type, object obj)
        {
            if (obj == null)
                return null;
            return JsonSerializer.Deserialize(JsonSerializer.Serialize(obj), type);
        }
    }
}
