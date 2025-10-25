using LatServer.Core.Service.NetService;

namespace LatServer.Core.System.EventSys.Events;

public class SendSelectArgs : IEvent
{
    public uint RoomId { get; set; }
    public int HeroId { get; set; }
    public ServerSession Session { get; set; }

    public SendSelectArgs()
    {
    }

    public SendSelectArgs(uint roomId, int heroId, ServerSession session)
    {
        RoomId = roomId;
        HeroId = heroId;
        Session = session;
    }
}