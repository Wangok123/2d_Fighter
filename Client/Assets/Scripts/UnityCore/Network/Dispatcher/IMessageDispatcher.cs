namespace UnityCore.Network.Dispatcher
{
    public interface IMessageDispatcher
    {
        void RegisterHandler(IMessageHandler handler);
        void Dispatch(MsgPack message);
    }
}