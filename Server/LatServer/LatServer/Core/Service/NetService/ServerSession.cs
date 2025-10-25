using LATNet;
using LatProtocol;
using LogLib;

namespace LatServer.Core.Service.NetService;

public class ServerSession : KCPSession
{
    protected override void OnConnected()
    {
        GameDebug.LogColor(LogColorType.Green, $"Client Online, sessionID:{m_sessionID}");
    }

    protected override void OnDisConnected()
    {
        GameDebug.LogColor(LogColorType.Red, $"Client Offline, sessionID:{m_sessionID}");
    }

    protected override void OnReceiveMsg(byte[] bytes)
    {
        ProtocolProcessor.ProcessPacket(bytes, out ushort cmdId, out byte[] message);
        NetService.Instance.AddMsgQueue(this, cmdId, message);
    }

    protected override void OnUpdate(DateTime now)
    {
    }
}