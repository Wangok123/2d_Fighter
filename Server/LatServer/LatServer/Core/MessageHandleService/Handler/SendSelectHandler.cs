using GameProtocol;
using LatServer.Core.MessageHandleService.Dispatcher;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;

namespace LatServer.Core.MessageHandleService.Handler;

public class SendSelectHandler : MessageHandler<SendSelectRequest>
{
    public SendSelectHandler(uint cmdId) : base(cmdId)
    {
    }

    protected override void Process(ServerSession session, SendSelectRequest message)
    {
        uint roomId = message.RoomId;
        int heroId = message.HeroId;
        SendSelectArgs args = new SendSelectArgs(roomId, heroId,session);
        EventManager.Publish(args);
    }
}