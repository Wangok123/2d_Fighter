using LatServer.Core.Service.NetService;

namespace LatServer.Core.MessageHandleService.Dispatcher;

public interface IMessageHandler
{
    uint CommandId { get; }
    void Handle(MsgPack message);
}