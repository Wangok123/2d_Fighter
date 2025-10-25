using System;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;

namespace LATNet
{
    public abstract class KCPSession<T> where T : KCPMsg , new()
{
    public Action<uint>? OnSessionClose;

    protected uint m_sessionID;
    protected SessionState m_state = SessionState.None;

    private Action<byte[], IPEndPoint>? m_udpSender;
    private IPEndPoint? m_remoteEndPoint;
    public KCPHandle? Handle;
    public Kcp? Kcp { get; set; }

    public bool IsConnected => m_state == SessionState.Connected;

    private CancellationTokenSource? cts;
    private CancellationToken ct;

    public KCPSession()
    {
        cts = new CancellationTokenSource();
        ct = cts.Token;
    }

    public void InitSession(uint sessionId, Action<byte[], IPEndPoint> udpSender, IPEndPoint remotePoint)
    {
        m_sessionID = sessionId;
        m_udpSender = udpSender;
        m_remoteEndPoint = remotePoint;
        m_state = SessionState.Connected;

        Handle = new KCPHandle();
        Kcp = new Kcp(sessionId, Handle);
        Kcp.NoDelay(1, 10, 2, 1);
        Kcp.WndSize(64, 64);
        Kcp.SetMtu(512);

        Handle.OutputAction = buffer => { m_udpSender(buffer.ToArray(), m_remoteEndPoint); };

        Handle.ReceiveAction = bytes =>
        {
            byte[] buffer = KCPTool.Decompress(bytes);
            T msg = KCPTool.DeSerialize<T>(buffer);
            OnReceiveMsg(msg);
        };

        OnConnected();
        
        Task.Run(Update);
    }

    public void ReceiveData(byte[] buffer)
    {
        Kcp.Input(buffer.AsSpan());
    }

    async void Update()
    {
        try
        {
            while (true)
            {
                DateTime now = DateTime.UtcNow;
                OnUpdate(now);
                if (ct.IsCancellationRequested)
                {
                    KCPTool.ColorLog(KCPLogColor.Cyan, "SessionUpdate Task is Cancelled.");
                    break;
                }
                else
                {
                    Kcp.Update(now);
                    int len;
                    while ((len = Kcp.PeekSize()) > 0)
                    {
                        byte[] buffer = new byte[len];
                        if (Kcp.Recv(buffer) >= 0)
                        {
                            Handle?.Receive(buffer);
                        }
                    }

                    await Task.Delay(10);
                }
            }
        }
        catch (Exception e)
        {
            KCPTool.Warn("Session Update Exception:{0}", e.ToString());
            throw;
        }
    }

    public void SendMsg(T msg)
    {
        if (m_state != SessionState.Connected)
        {
            KCPTool.Warn("Session Disconnected.Can not send msg.");
            return;
        }
        
        if (IsConnected)
        {
            byte[] buffer = KCPTool.Compress(KCPTool.Serialize(msg));
            SendMsg(buffer);
        }
        else
        {
            KCPTool.Warn("Session Disconnected.Can not send msg.");
        }
    }
    
    public void SendMsg(byte[] buffer)
    {
        if (m_state != SessionState.Connected)
        {
            KCPTool.Warn("Session Disconnected.Can not send msg.");
            return;
        }
        
        if (IsConnected)
        {
            Kcp.Send(buffer.AsSpan());
        }
    }
    
    public void CloseSession()
    {
        cts?.Cancel();
        OnDisConnected();

        OnSessionClose?.Invoke(m_sessionID);
        OnSessionClose = null;

        m_state = SessionState.Disconnected;
        m_remoteEndPoint = null;
        m_udpSender = null;
        m_sessionID = 0;

        Handle = null;
        Kcp = null;
        cts = null;
    }

    protected abstract void OnConnected();
    protected abstract void OnDisConnected(); 
    protected abstract void OnReceiveMsg(T msg);
    protected abstract void OnUpdate(DateTime now);
    
    public override bool Equals(object obj)
    {
        if (obj is KCPSession<T> us)
        {
            return us.m_sessionID == m_sessionID;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return m_sessionID.GetHashCode();
    }
    
    public uint GetSessionID()
    {
        return m_sessionID;
    }
}

public enum SessionState
{
    None,
    Connected,
    Disconnected
}k
}

