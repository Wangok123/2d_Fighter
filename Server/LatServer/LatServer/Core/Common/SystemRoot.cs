using LatServer.Core.Service.CacheService;
using LatServer.Core.Service.NetService;
using LatServer.Core.Service.TimerService;

namespace LatServer.Core.Common;

public abstract class SystemRoot<T> : Singleton<T> where T : class, new()
{
    protected NetService NetService;
    protected CacheService CacheService;
    protected TimerService TimerService;

    public override void Init()
    { 
        NetService = NetService.Instance;
        CacheService = CacheService.Instance;
        TimerService = TimerService.Instance;
    }
}