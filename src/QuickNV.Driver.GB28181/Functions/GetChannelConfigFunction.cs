using Quick.Fields;
using Quick.Protocol;
using QuickNV.Driver.Protocol.QpCommands.GetChannelConfig;

namespace QuickNV.Driver.GB28181.Functions
{
    public static class GetChannelConfigFunction
    {
        public static Response Invoke(QpChannel channel, Request request)
        {
            return new Response()
            {
                Fields = new FieldForGet[0]
            };
        }
    }
}
