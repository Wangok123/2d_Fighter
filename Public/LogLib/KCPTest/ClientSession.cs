using KCPExampleProtocol;
using LATLog;
using LATNet;

namespace KCPTest;

public class ClientSession :KCPSession<NetMsg>
{
    protected override void OnConnected()
    {
        throw new NotImplementedException();
    }

    protected override void OnDisConnected()
    {
        throw new NotImplementedException();
    }

    protected override void OnReceiveMsg(NetMsg msg)
    {
        GameDebug.LogColor(LogColorType.Magenta, "Session id: " + m_sessionID + " Receive Msg: " + msg.Info);
    }

    protected override void OnUpdate(DateTime now)
    {
        throw new NotImplementedException();
    }
}