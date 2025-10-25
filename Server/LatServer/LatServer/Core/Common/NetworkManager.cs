using Google.Protobuf;
using LATNet;
using LatProtocol;

namespace LatServer.Core.Common;

public static class NetworkManager
{
    public static void SendMsg<T>(this KCPSession session, ushort protocolId, T message) where T : IMessage
    {
        var buffer = ProtocolProcessor.SendMessage(protocolId, message);
        session.SendMsg(buffer);
    }
}