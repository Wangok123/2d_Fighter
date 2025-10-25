using LatServer.Core.Service.NetService;

namespace LatServer.Core.System.EventSys.Events;

public class MatchArgs : IEvent
{
    public ServerSession Session { get; }
    public GameProtocol.MatchType MatchType { get; }

    public MatchArgs()
    {
    }

    public MatchArgs(ServerSession session, GameProtocol.MatchType type)
    {
        MatchType = type;
        Session = session;
    }
}