using GameProtocol;
using LatServer.Core.MessageHandleService.Dispatcher;
using LatServer.Core.Service.NetService;
using LatServer.Core.System.EventSys;
using LatServer.Core.System.EventSys.Events;

namespace LatServer.Core.MessageHandleService.Handler;

public class MatchHandler : MessageHandler<MatchRequest>
{
    public MatchHandler(uint cmdId) : base(cmdId)
    {
    }

    protected override void Process(ServerSession session, MatchRequest message)
    {
        MatchArgs matchArgs = new MatchArgs(session, message.Status);
        EventManager.Publish(matchArgs);
    }
}