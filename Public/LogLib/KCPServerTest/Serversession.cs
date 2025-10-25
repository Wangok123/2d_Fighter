using KCPExampleProtocol;
using LATLog;
using LATNet;

namespace KCPServerTest;

public class Serversession : KCPSession<NetMsg>
{
    protected override void OnConnected()
    {
        GameDebug.LogColor(LogColorType.Green, "Server Connected");
    }

    protected override void OnDisConnected()
    {
        GameDebug.LogColor(LogColorType.Red, "Server DisConnected");
    }

    protected override void OnReceiveMsg(NetMsg msg)
    {
        GameDebug.LogColor(LogColorType.Magenta, $"SessionId:{m_sessionID} Receive Msg:{msg.Info}");
        
        if (msg.Cmd == CMD.NetPing)
        {
            if (msg.Ping.IsOver)
            {
                CloseSession();
            }else
            {
                checkCount = 0;
                NetMsg pingMsg = new NetMsg();
                pingMsg.Cmd = CMD.NetPing;
                pingMsg.Ping = new NetPing()
                {
                    IsOver = false
                };
                
                SendMsg(pingMsg);
            }
        }
    }

    private int checkCount = 0;
    private DateTime checkTime = DateTime.UtcNow.AddSeconds(5);
    protected override void OnUpdate(DateTime now)
    {
        if (now > checkTime)
        {
            checkTime = now.AddSeconds(5);
            checkCount++;
            if (checkCount > 3)
            {
                NetMsg msg = new NetMsg();
                msg.Cmd = CMD.NetPing;
                msg.Ping = new NetPing()
                {
                    IsOver = true
                };
                OnReceiveMsg(msg);
            }
        }
    }
}