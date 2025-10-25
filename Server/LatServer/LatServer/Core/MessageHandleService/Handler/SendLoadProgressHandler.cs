using GameProtocol;
using LatServer.Core.MessageHandleService.Dispatcher;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;

namespace LatServer.Core.MessageHandleService.Handler;

public class SendLoadProgressHandler : MessageHandler<SendLoadProgressRequest>
{
    public SendLoadProgressHandler(uint cmdId) : base(cmdId)
    {
    }

    protected override void Process(ServerSession session, SendLoadProgressRequest message)
    {
        uint roomId = message.RoomId;
        int percent = message.Percent;
        SendLoadProgressArgs args = new SendLoadProgressArgs(roomId, percent, session);
        EventManager.Publish(args);
    }
}