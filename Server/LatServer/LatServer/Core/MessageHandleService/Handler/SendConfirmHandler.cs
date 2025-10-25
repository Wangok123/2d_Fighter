using GameProtocol;
using LatServer.Core.MessageHandleService.Dispatcher;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;

namespace LatServer.Core.MessageHandleService.Handler;

public class SendConfirmHandler : MessageHandler<SendConfirmRequest>
{
    public SendConfirmHandler(uint cmdId) : base(cmdId)
    {
    }

    protected override void Process(ServerSession session, SendConfirmRequest message)
    {
        uint roomId = message.RoomId;
        SendConfirmArgs args = new SendConfirmArgs(roomId, session);
        EventManager.Publish(args);
    }
}