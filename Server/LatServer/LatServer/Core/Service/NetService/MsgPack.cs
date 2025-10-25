namespace LatServer.Core.Service.NetService;

public class MsgPack
{
    public ServerSession Session;
    public ushort CMDID;
    public byte[] Bytes;
    
    public MsgPack(ServerSession session, ushort cmdId, byte[] bytes)
    {
        Session = session;
        CMDID = cmdId;
        Bytes = bytes;
    }
}