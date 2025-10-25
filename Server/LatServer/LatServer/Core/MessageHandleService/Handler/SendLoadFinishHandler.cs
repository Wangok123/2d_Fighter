using GameProtocol;
using LatServer.Core.MessageHandleService.Dispatcher;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;

namespace LatServer.Core.MessageHandleService.Handler;

public class SendLoadFinishHandler : MessageHandler<LoadingFinishRequest>
{
    public SendLoadFinishHandler(uint cmdId) : base(cmdId)
    {
    }

    protected override void Process(ServerSession session, LoadingFinishRequest message)
    {
        var roomId = message.RoomId;
        var args = new SendLoadFinishArgs(roomId, session);
        EventManager.Publish(args);
    }
}