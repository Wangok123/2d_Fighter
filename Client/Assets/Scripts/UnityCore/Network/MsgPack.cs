namespace UnityCore.Network
{
    public class MsgPack
    {
        public ushort CMDID;
        public byte[] Bytes;
    
        public MsgPack(ushort cmdId, byte[] bytes)
        {
            CMDID = cmdId;
            Bytes = bytes;
        }
    }
}