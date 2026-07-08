using Quick.Fields;
using Quick.Protocol;
using QuickNV.Protocol.Driver.QpCommands.ImportDevices;

namespace QuickNV.Driver.RTSP.Functions
{
    public class ImportDevicesFunction
    {
        public static Response Invoke(QpChannel channel, Request request)
        {
            return new Response()
            {
                Fields = new[]
                {
                    new FieldForGet()
                    {
                        Type = FieldType.Alert,
                        Name = "信息",                        
                        Description = "本驱动不支持设备导入"
                    }
                }
            };
        }
    }
}
