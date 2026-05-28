using SIPSorcery.SIP;
using System.Xml.Linq;
using QuickNV.Driver.GB28181.Command;

namespace QuickNV.Driver.GB28181.SipRequestHandlers
{
    public class Message : ISipRequestHandler
    {
        private SipServer sipServer;
        private Dictionary<string, Dictionary<string, ICommandHandler>> messageCommandDict = new Dictionary<string, Dictionary<string, ICommandHandler>>();
        public bool RequireRegisterBeforeExecute => false;

        public Message(SipServer sipServer)
        {
            this.sipServer = sipServer;

            registerCommand(new Command.Keepalive.NotifyCommandHandler(sipServer));
            registerCommand(new Command.Catalog.ResponseCommandHandler(sipServer));
            registerCommand(new Command.DeviceInfo.ResponseCommandHandler(sipServer));
            registerCommand(new Command.ConfigDownload.ResponseCommandHandler(sipServer));
            registerCommand(new Command.RecordInfo.ResponseCommandHandler(sipServer));
        }

        public ICommandHandler GetCommand(string messageType, string cmdType)
        {
            if (!messageCommandDict.ContainsKey(messageType))
                return null;
            var commandDict = messageCommandDict[messageType];
            if (!commandDict.ContainsKey(cmdType))
                return null;
            return commandDict[cmdType];
        }

        private void registerCommand(ICommandHandler handler)
        {
            Dictionary<string, ICommandHandler> commandDict = null;
            if (messageCommandDict.ContainsKey(handler.MessageType))
                commandDict = messageCommandDict[handler.MessageType];
            else
                commandDict = messageCommandDict[handler.MessageType] = new Dictionary<string, ICommandHandler>();
            commandDict[handler.CmdType] = handler;
        }

        public async Task Execute(SIPEndPoint localEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest, DeviceContext device)
        {
            if (string.IsNullOrEmpty(sipRequest.Body))
                throw new ArgumentNullException("Body中没有内容");
            XElement bodyXml = null;
            try
            {
                bodyXml = XElement.Parse(sipRequest.Body);
            }
            catch (Exception ex)
            {
                throw new IOException($"XML解析出错。XML内容：{sipRequest.Body}", ex);
            }
            var messageType = bodyXml.Name.LocalName;
            string cmdType = null;
            try
            {
                cmdType = bodyXml.Element("CmdType")?.Value;
                var handler = GetCommand(messageType, cmdType);
                await handler.Execute(localEndPoint, remoteEndPoint, sipRequest, device, bodyXml);
            }
            catch (Exception ex)
            {
                throw new IOException($"处理类型为[{messageType}]命令类型为[{cmdType}]的消息时出错。XML内容：{sipRequest.Body}", ex);
            }
        }
    }
}
