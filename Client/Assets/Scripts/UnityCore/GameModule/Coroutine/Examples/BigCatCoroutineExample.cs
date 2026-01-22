using UnityEngine;
using Wjybxx.BigCat.Co;
using Wjybxx.BigCat.Gameplay;
using Wjybxx.Commons.Concurrent;

namespace UnityCore.GameModule.Coroutine.Examples
{
    /// <summary>
    /// BigCat 协程系统独立示例
    /// 不依赖 Game 全局环境，可以在测试场景中独立运行
    /// </summary>
    public class BigCatCoroutineExample : MonoBehaviour
    {
        private CoroutineMgr _coroutineMgr;
        private GTime _gTime;
        
        private long _timerId;
        private long _coroutineId;
        private bool _examplesStarted;

        private void Awake()
        {
            Debug.Log("[BigCat] Example Awake - Initializing BigCat");
            
            _gTime = new GTime();
            _gTime.Restart(timeScale: 1.0);
            
            _coroutineMgr = new CoroutineMgr(
                eventLoop: GlobalEventLoop.Inst,
                time: _gTime,
                minPeriod: 0.01,
                unscaledMinPeriod: 0.01,
                enableUnscaledQueue: true,
                enableFrameQueue: false
            );
            
            Debug.Log("[BigCat] BigCat initialized successfully");
        }

        private void Update()
        {
            if (_gTime != null && _coroutineMgr != null)
            {
                _gTime.Update(Time.unscaledDeltaTime);
                
                if (!_examplesStarted)
                {
                    _examplesStarted = true;
                    Debug.Log($"[BigCat] Starting examples at GTime={_gTime.Time:F4}, UnityTime={Time.time:F4}");
                    ExampleScheduleAction();
                    ExampleSchedulePeriodic();
                    ExampleCoroutine();
                }
                
                _coroutineMgr.Update(GameLoopPhase.EarlyUpdate);
                _coroutineMgr.Update(GameLoopPhase.PostEarlyUpdate);
                _coroutineMgr.Update(GameLoopPhase.Update);
                _coroutineMgr.Update(GameLoopPhase.PostUpdate);
                _coroutineMgr.Update(GameLoopPhase.LateUpdate);
                _coroutineMgr.Update(GameLoopPhase.PostLateUpdate);
            }
        }

        private void FixedUpdate()
        {
            if (_gTime != null && _coroutineMgr != null)
            {
                _gTime.FixedUpdate(Time.fixedUnscaledDeltaTime);
                
                _coroutineMgr.Update(GameLoopPhase.FixedUpdate);
                _coroutineMgr.Update(GameLoopPhase.PostFixedUpdate);
            }
        }

        private void ExampleScheduleAction()
        {
            Debug.Log($"[BigCat] Example: Schedule delayed action (current GTime={_gTime.Time:F4})");
            
            var task = Wjybxx.BigCat.Co.TaskBuilder.NewAction(() =>
            {
                Debug.Log($"[BigCat] Delayed action executed at GTime={_gTime.Time:F4}, UnityTime={Time.time:F4}");
            });
            task.SetOnlyOnce(2.0);
            
            var future = _coroutineMgr.TimerMgr.Schedule(task);
            _timerId = future.TaskId;
            
            Debug.Log($"[BigCat] Scheduled action with ID: {_timerId}, will trigger at GTime={_gTime.Time + 2.0:F4}");
        }

        private void ExampleSchedulePeriodic()
        {
            Debug.Log($"[BigCat] Example: Schedule periodic action (current GTime={_gTime.Time:F4})");
            
            var task = Wjybxx.BigCat.Co.TaskBuilder.NewAction(() =>
            {
                Debug.Log($"[BigCat] Periodic action at GTime={_gTime.Time:F4}, UnityTime={Time.time:F4}");
            });
            task.SetFixedDelay(1.0, 1.0);
            task.CountLimit = 5;
            
            var future = _coroutineMgr.TimerMgr.Schedule(task);
            Debug.Log($"[BigCat] Scheduled periodic action with ID: {future.TaskId}, first trigger at GTime={_gTime.Time + 1.0:F4}");
        }

        private void ExampleCoroutine()
        {
            Debug.Log($"[BigCat] Example: Start coroutine (current GTime={_gTime.Time:F4})");
            
            var context = _coroutineMgr.StartCoroutine(async ctx =>
            {
                Debug.Log($"[BigCat] Coroutine started at GTime={_gTime.Time:F4}");
                
                await ctx.Sleep(1.0, timingType: TimingType.Time);
                Debug.Log($"[BigCat] After 1 second at GTime={_gTime.Time:F4}");
                
                await ctx.Sleep(2.0, timingType: TimingType.Time);
                Debug.Log($"[BigCat] After 3 seconds total at GTime={_gTime.Time:F4}");
            }, new CoroutineStartArgs());
            
            _coroutineId = context.CoroutineId;
            Debug.Log($"[BigCat] Started coroutine with ID: {_coroutineId}");
        }

        private void OnDestroy()
        {
            Debug.Log("[BigCat] Example OnDestroy - Cleaning up");
            
            if (_coroutineMgr != null)
            {
                _coroutineMgr.Cancel(_timerId);
                _coroutineMgr.Cancel(_coroutineId);
                _coroutineMgr.Shutdown();
                _coroutineMgr = null;
            }
            
            _gTime = null;
        }
    }
}



