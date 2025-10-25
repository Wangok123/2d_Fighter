using LatServer.Core.Service.NetService;

namespace LatServer.Core.System.EventSys.Events;

public class SendLoadProgressArgs: IEvent
{
    public uint RoomId { get; set; }
    public int Percent { get; set; }
    public ServerSession Session { get; set; }

    public SendLoadProgressArgs()
    {
    }

    public SendLoadProgressArgs(uint roomId, int percent, ServerSession session)
    {
        RoomId = roomId;
        Percent = percent;
        Session = session;
    }
}