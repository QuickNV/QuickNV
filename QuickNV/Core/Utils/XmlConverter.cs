using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace QuickNV.Core.Utils
{
    public class XmlConverter
    {
        public static string SerializeObject(object obj)
        {
            return SerializeObject(obj, Encoding.UTF8);
        }

        public static string SerializeObject(object obj, Encoding encoding)
        {
            var xmlSerializer = new XmlSerializer(obj.GetType(), (string)null);

            using (var stream = new MemoryStream())
            using (var writer = new XmlTextWriter(stream, encoding))
            {
                writer.Formatting = Formatting.Indented;
                xmlSerializer.Serialize(writer, obj);
                var buffer = stream.ToArray();
                return encoding.GetString(buffer);
            }
        }

        public static T DeserializeObject<T>(XElement element)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = element.CreateReader())
                return (T)xmlSerializer.Deserialize(reader);
        }

        public static T DeserializeObject<T>(string xml)
        {
            var element = XElement.Parse(xml);
            return DeserializeObject<T>(element);
        }
    }
}
