using System;

namespace UnityCore.GameModule.Coroutine
{
    public static class CoroutineManagerExtensions
    {
        public static long ScheduleAction(this CoroutineManager manager, Action action, double delay = 0)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            
            var task = Wjybxx.BigCat.Co.TaskBuilder.NewAction(action);
            task.SetOnlyOnce(delay);
            
            var future = manager.TimerMgr.Schedule(task);
            return future.TaskId;
        }
        
        public static long ScheduleAction(this CoroutineManager manager, Action action, double initialDelay, double period, int countLimit = -1)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            
            var task = Wjybxx.BigCat.Co.TaskBuilder.NewAction(action);
            task.SetFixedDelay(initialDelay, period);
            if (countLimit > 0)
            {
                task.CountLimit = countLimit;
            }
            
            var future = manager.TimerMgr.Schedule(task);
            return future.TaskId;
        }
        
        public static long StartCoroutine(this CoroutineManager manager, Func<Wjybxx.BigCat.Co.CoroutineTaskContext, Wjybxx.Commons.Concurrent.ValueFuture> func)
        {
            if (manager.CoroutineMgr is Wjybxx.BigCat.Co.CoroutineMgr coroutineMgr)
            {
                var context = coroutineMgr.StartCoroutine(func, new Wjybxx.BigCat.Co.CoroutineStartArgs());
                return context.CoroutineId;
            }
            
            throw new InvalidOperationException("CoroutineMgr is not initialized");
        }
        
        public static void CancelTask(this CoroutineManager manager, long taskId)
        {
            if (manager.CoroutineMgr is Wjybxx.BigCat.Co.CoroutineMgr coroutineMgr)
            {
                coroutineMgr.Cancel(taskId);
            }
        }
    }
}


