using LatServer.Core.Service.NetService;

namespace LatServer.Core.MessageHandleService.Dispatcher;

public interface IMessageDispatcher
{
    void RegisterHandler(Core.MessageHandleService.Dispatcher.IMessageHandler handler);
    void Dispatch(MsgPack message);
}