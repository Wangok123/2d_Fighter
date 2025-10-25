using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using LATLog;

namespace LATNet;

public class KCPNet<T, K> where T : KCPSession<K>, new() where K : KCPMsg, new()
{
    private UdpClient udp;
    private IPEndPoint remoteEndPoint;

    private CancellationTokenSource? cts;
    private CancellationToken ct;

    public KCPNet()
    {
        cts = new CancellationTokenSource();
        ct = cts.Token;
    }

    #region Server

    private Dictionary<uint, T> sessionDic;

    public void StartAsServer(string ip, int port)
    {
        sessionDic = new Dictionary<uint, T>();
        udp = new UdpClient(port);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] {0, 0, 0, 0}, null);
        }
        
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        KCPTool.ColorLog(KCPLogColor.Green, "Server Start...");

        Task.Run(ServerReceive, ct);
    }

    public void CloseServer()
    {
        foreach (var session in sessionDic)
        {
            session.Value.CloseSession();
        }
        
        sessionDic.Clear();
        if (udp != null)
        {
            udp.Close();
            udp = null;
            cts?.Cancel();
        }
    }

    private async void ServerReceive()
    {
        UdpReceiveResult result;
        while (true)
        {
            try
            {
                if (ct.IsCancellationRequested)
                {
                    KCPTool.ColorLog(KCPLogColor.Cyan, "SeverReceive Task is Cancelled.");
                    break;
                }

                result = await udp.ReceiveAsync();


                uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                if (sid == 0)
                {
                    sid = GenerateUniqueSessionID();
                    byte[] sid_bytes = BitConverter.GetBytes(sid);
                    byte[] conv_bytes = new byte[8];
                    Array.Copy(sid_bytes, 0, conv_bytes, 4, 4);

                    SendUdpMsg(conv_bytes, result.RemoteEndPoint);
                }
                else
                {
                    if (!sessionDic.TryGetValue(sid, out T session))
                    {
                        session = new T();
                        session.InitSession(sid, SendUdpMsg, result.RemoteEndPoint);
                        session.OnSessionClose = OnServerSessionClosed;
                        lock (sessionDic)
                        {
                            sessionDic.Add(sid, session);
                        }
                    }
                    else
                    {
                        session = sessionDic[sid];
                    }

                    session.ReceiveData(result.Buffer);
                }
            }
            catch (Exception e)
            {
                KCPTool.Error(e.ToString());
                break;
            }
        }
    }

    private void OnServerSessionClosed(uint sid)
    {
        if (sessionDic.ContainsKey(sid))
        {
            lock (sessionDic)
            {
                sessionDic.Remove(sid);
                KCPTool.Error("Server Session Not Found, SessionID: " + sid);
            }
        }
        else
        {
            KCPTool.Error("Server Session Not Found, SessionID: " + sid);
        }
    }

    #endregion

    #region Client

    public T ClientSession;

    public void StartAsClient(string ip, int port)
    {
        udp = new UdpClient(0);
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] {0, 0, 0, 0}, null);
        }
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        KCPTool.ColorLog(KCPLogColor.Green, "Client Start...");

        Task.Run(ClientReceive, ct);
    }

    public Task<bool> ConnectServer(int interval, int maxIntervalSum = 5000)
    {
        SendUdpMsg(new byte[4], remoteEndPoint);

        int sum = 0;
        Task<bool> task = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(interval);
                sum += interval;
                if (ClientSession != null && ClientSession.IsConnected)
                {
                    return true;
                }
                else
                {
                    if (sum >= maxIntervalSum)
                    {
                        KCPTool.Error("Connect Server Timeout");
                        return false;
                    }
                }
            }
        });
        return task;
    }

    public void CloseClient()
    {
        cts?.Cancel();
        udp?.Close();
    }

    async void ClientReceive()
    {
        UdpReceiveResult result;
        while (true)
        {
            try
            {
                if (ct.IsCancellationRequested)
                {
                    KCPTool.ColorLog(KCPLogColor.Green, "Client Receive Task Cancel");
                    break;
                }

                result = await udp.ReceiveAsync();

                if (Equals(remoteEndPoint, result.RemoteEndPoint))
                {
                    uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                    if (sid == 0)
                    {
                        if (ClientSession != null && ClientSession.IsConnected)
                        {
                            KCPTool.Warn("Client Already Connected");
                            return;
                        }
                        else
                        {
                            sid = BitConverter.ToUInt32(result.Buffer, 4);
                            KCPTool.ColorLog(KCPLogColor.Green,$"Client Connect Server Success, SessionID: {sid}");

                            //会话处理
                            ClientSession = new T();
                            ClientSession.InitSession(sid, SendUdpMsg, remoteEndPoint);
                            ClientSession.OnSessionClose = OnClientSessionClosed;
                        }
                    }
                    else
                    {
                        if (ClientSession != null && ClientSession.IsConnected)
                        {
                            ClientSession.ReceiveData(result.Buffer);
                        }
                        else
                        {
                            KCPTool.Warn("Client is Initing...");
                        }
                    }
                }
                else
                {
                    KCPTool.ColorLog(KCPLogColor.Red,
                        $"Client Receive Illegal Message From {result.RemoteEndPoint}");
                }
            }
            catch (Exception e)
            {
                KCPTool.Error(e.ToString());
                break;
            }
        }
    }

    private void OnClientSessionClosed(uint sessionId)
    {
        cts?.Cancel();
        if (udp != null)
        {
            udp.Close();
            udp = null;
        }

        GameDebug.LogColor(LogColorType.Green, $"Client Session Close, SessionID: {sessionId}");
    }

    #endregion

    private void SendUdpMsg(byte[] bytes, IPEndPoint remotePoint)
    {
        udp?.SendAsync(bytes, bytes.Length, remotePoint);
    }

    public void BroadCastMsg(K msg)
    {
        byte[] bytes = KCPTool.Serialize<K>(msg);
        foreach (var session in sessionDic)
        {
            session.Value.SendMsg(bytes);
        }
    }

    private uint _sid = 0;

    public uint GenerateUniqueSessionID()
    {
        lock (sessionDic)
        {
            while (true)
            {
                _sid++;
                if (_sid == uint.MaxValue)
                {
                    _sid = 1;
                }

                if (!sessionDic.ContainsKey(_sid))
                {
                    break;
                }
            }
        }

        return _sid;
    }
}