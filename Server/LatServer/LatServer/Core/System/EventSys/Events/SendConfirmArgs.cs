using LatServer.Core.Service.NetService;

namespace LatServer.Core.System.EventSys.Events;

public class SendConfirmArgs : IEvent
{
    public uint RoomId { get; set; }
    public ServerSession Session { get; set; }
    
    public SendConfirmArgs()
    {
    }
    
    public SendConfirmArgs(uint roomId, ServerSession session)
    {
        RoomId = roomId;
        Session = session;
    }
}