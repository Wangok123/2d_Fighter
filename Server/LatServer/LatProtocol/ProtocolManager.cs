using Google.Protobuf;

namespace LatProtocol
{
    public class ProtocolManager
    {
        private readonly Dictionary<uint, MessageParser> _parsers = new Dictionary<uint, MessageParser>();
        private readonly Dictionary<uint, Action<IMessage>> _handlers = new Dictionary<uint, Action<IMessage>>();

        public void RegisterProtocol<T>(uint protocolId, MessageParser<T> parser, Action<T> handler) where T : IMessage<T>
        {
            _parsers[protocolId] = parser;
            _handlers[protocolId] = msg => handler((T)msg);
        }

        public bool TryParse(uint protocolId, byte[] data, out IMessage message)
        {
            message = null;
            if (!_parsers.TryGetValue(protocolId, out var parser))
                return false;

            try
            {
                message = parser.ParseFrom(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryHandle(uint protocolId, IMessage message)
        {
            if (!_handlers.TryGetValue(protocolId, out var handler))
                return false;

            handler(message);
            return true;
        }
    }
}