namespace UnityCore.Network.Dispatcher
{
    public interface IMessageHandler
    {
        uint CommandId { get; }
        void Handle(MsgPack message);
    }
}