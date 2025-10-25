using LatServer.Core.Common;
using LatServer.Core.MessageHandleService;
using LatServer.Core.Service.CacheService;
using LatServer.Core.Service.ConfigService;
using LatServer.Core.Service.NetService;
using LatServer.Core.Service.TimerService;
using LatServer.Core.System.LoginSys;
using LatServer.Core.System.MatchSys;
using LatServer.Core.System.RoomSys;
using LogLib;

namespace LatServer.Common;

public class ServerRoot : Singleton<ServerRoot>
{
    public override void Init()
    {
        GameDebug.InitSettings();
        
        MessageHandlerService.Instance.Init();
        CacheService.Instance.Init(); 
        NetService.Instance.Init();
        TimerService.Instance.Init();
        CfgService.Instance.Init();
        
        LoginSystem.Instance.Init();
        MatchSystem.Instance.Init();
        RoomSystem.Instance.Init();
        
        
        GameDebug.LogColor(LogColorType.Green,"ServerRoot Init");
    }
    
    public override void Update()
    {
        CacheService.Instance.Update();
        NetService.Instance.Update();
        TimerService.Instance.Update();
        
        LoginSystem.Instance.Update();
        MatchSystem.Instance.Update();
        RoomSystem.Instance.Update();
    }
}