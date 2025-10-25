using GameProtocol;
using LatServer.Core.Common;
using LatServer.Core.Service.NetService;
using LogLib;

namespace LatServer.Core.Service.CacheService;

public class CacheService : Singleton<CacheService>
{
    private Dictionary<string, ServerSession> _onLineUser;
    private Dictionary<ServerSession, UserDto> _userData;

    public override void Init()
    {
        _onLineUser = new Dictionary<string, ServerSession>();
        _userData = new Dictionary<ServerSession, UserDto>();
        GameDebug.Log("CacheService Done!!!!!");
    }

    public override void Update()
    {
        base.Update();
    }
    
    public bool IsAcctOnline(string acct, out ServerSession session)
    {
        session = null;
        if (_onLineUser.TryGetValue(acct, out var serverSession))
        {
            session = serverSession;
            return true;
        }
        
        return false;
    }

    public UserDto GetUserDataBySession(ServerSession roomDataSession)
    {
        if (_userData.TryGetValue(roomDataSession, out var userData))
        {
            return userData;
        }
        
        GameDebug.LogError($"GetUserDataBySession: Session {roomDataSession.GetSessionID()} not found in cache.");
        return null;
    }
}