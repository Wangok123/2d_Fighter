using LatServer.Core.Service.NetService;

namespace LatServer.Core.MessageHandleService;

public interface IMessageHandler
{
    void Handle(MsgPack pack);
}