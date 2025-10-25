// See https://aka.ms/new-console-template for more information

using KCPExampleProtocol;
using KCPServerTest;
using LATLog;
using LATNet;

GameDebug.InitSettings();

string ip = "0.0.0.0";
KCPNet<Serversession, NetMsg> server = new KCPNet<Serversession, NetMsg>();
server.StartAsServer(ip, 17666);
while (true)
{
    string ipt = Console.ReadLine();
    if (ipt == "exit")
    {
        server.CloseServer();
        break;
    }else
    {
        NetMsg msg = new NetMsg();
        msg.Info = ipt;
        server.BroadCastMsg(msg);
    }
    
    Console.ReadKey();
}