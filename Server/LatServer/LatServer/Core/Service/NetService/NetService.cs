using LATNet;
using LatProtocol;
using LatServer.Core.Common;
using LatServer.Core.MessageHandleService;
using LogLib;

namespace LatServer.Core.Service.NetService;

public class NetService : Singleton<NetService>
{
    public static readonly string PkgqueLock = "pkgque_lock";
    
    private KCPNet<ServerSession> _server = new KCPNet<ServerSession>();
    private Queue<MsgPack> _msgPackQueue = new Queue<MsgPack>();

    public override void Init()
    {
        _msgPackQueue.Clear();
        
        KCPTool.LogFunc = GameDebug.Log;
        KCPTool.ErrorFunc = GameDebug.LogError;
        KCPTool.WarnFunc = GameDebug.LogWarning;
        KCPTool.ColorLogFunc = (color, s) =>
        {
            GameDebug.LogColor((LogColorType)color, s);
        };
        
        _server.StartAsServer("172.16.1.31", ServerConfig.UdpPort);
        GameDebug.Log("NetService Init Done!!!!");
    }

    public override void Update()
    {
        if (_msgPackQueue.Count > 0)
        {
            lock (PkgqueLock)
            {
                MsgPack msg = _msgPackQueue.Dequeue();
                HandOutMsg(msg);
            }
        }
    }
    
    public void AddMsgQueue(ServerSession session, ushort cmdId, byte[] msg)
    {
        lock (PkgqueLock)
        {
            _msgPackQueue.Enqueue(new MsgPack(session, cmdId, msg));
        }
    }
    
    /// <summary>
    /// 分发消息
    /// </summary>
    /// <param name="msgPack"></param>
    private void HandOutMsg(MsgPack msgPack)
    {
        MessageHandlerService.Instance.Dispatch(msgPack);
    }
}