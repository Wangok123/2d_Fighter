using LatServer.Core.Common;
using LATTimer;
using LogLib;

namespace LatServer.Core.Service.TimerService;

public class TimerService : Singleton<TimerService>
{
    TickTimer _timer = new TickTimer(0, false);
    
    public override void Init()
    {
        _timer.LogCallback = GameDebug.Log;
        _timer.WarningCallback = GameDebug.LogWarning;
        _timer.ErrorCallback = GameDebug.LogError;
        
        GameDebug.Log("TimerService Init Done!!!!!");
    }

    public override void Update()
    {
        base.Update();
        
        _timer.UpdateTasks();
    }
    
    public int AddTask(uint delay, Action<int> taskCallback, Action<int> cancelCallback = null, int count = 1)
    {
        return _timer.AddTask(delay, taskCallback, cancelCallback, count);
    }
    
    public bool DeleteTask(int taskId)
    {
        return _timer.DeleteTask(taskId);
    }   
}