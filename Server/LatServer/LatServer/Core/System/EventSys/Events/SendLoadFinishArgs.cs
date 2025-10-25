using LatServer.Core.Service.NetService;

namespace LatServer.Core.System.EventSys.Events;

public class SendLoadFinishArgs : IEvent
{
    public uint RoomId { get; set; }
    public ServerSession Session { get; set; }

    public SendLoadFinishArgs()
    {
    }

    public SendLoadFinishArgs(uint roomId, ServerSession session)
    {
        RoomId = roomId;
        Session = session;
    }
}