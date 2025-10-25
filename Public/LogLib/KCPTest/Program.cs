using KCPExampleProtocol;using KCPTest;
using LATLog;
using LATNet;

public class Progress
{
    static KCPNet<ClientSession, NetMsg> client;
    static Task<bool> checkTask;
    
    static void Main(string[] args)
    {
        GameDebug.InitSettings();
        
        string ip = "172.16.1.31";
        int port = 17666;
        client = new KCPNet<ClientSession, NetMsg>();
        client.StartAsClient(ip, port);
        checkTask = client.ConnectServer(200);
        
        Task.Run(ConnectCheck);

        while (true)
        {
            string ipt = Console.ReadLine();
            if (ipt == "exit")
            {
                client.CloseClient();
                break;
            }
            else
            {
                NetMsg msg = new NetMsg();
                msg.Info = ipt;
                client.ClientSession.SendMsg(msg);
            }
        }

        Console.ReadKey();
    }
    
    private static int count = 0;
    static async void ConnectCheck()
    {
        while (true)
        {
            await Task.Delay(3000);
            if (checkTask!= null && checkTask.IsCompleted)
            {
                if (checkTask.Result)
                {
                    GameDebug.Log("Connect Success");
                    checkTask = null;
                    await Task.Run(SendPingMsg);
                }
                else
                {
                    count++;
                    if (count > 3)
                    {
                        GameDebug.LogError("Connect Fail");
                        checkTask = null;
                        break;
                    }
                    else
                    {
                        GameDebug.LogWarning("Reconnect......Count:" + count);
                        checkTask = client.ConnectServer(200);
                    }
                }
            }
        }
    }
    
    static async void SendPingMsg()
    {
        while (true)
        {
            await Task.Delay(5000);
            if (client != null && client.ClientSession != null)
            {
                NetMsg msg = new NetMsg();
                msg.Info = "Ping";
                msg.Cmd = CMD.NetPing;
                msg.Ping = new NetPing()
                {
                    IsOver = false
                };
                GameDebug.LogColor(LogColorType.Green, "Send Ping");
            }else
            {
                GameDebug.LogColor(LogColorType.Red, "Client is null");
                break;
            }
        }
    }
}

 