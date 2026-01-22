using System;
using UnityCore.Base;
using UnityEngine;
using Wjybxx.BigCat.Co;
using Wjybxx.Commons.Concurrent;

namespace UnityCore.GameModule.Coroutine
{
    [DisallowMultipleComponent]
    public class CoroutineComponent : LatComponent
    {
        private CoroutineManager _manager;

        protected override void Awake()
        {
            base.Awake();
            _manager = GameModuleManager.GetModule<CoroutineManager>();
            _manager.Initialize();
        }

        public CoroutineManager GetManager()
        {
            return _manager;
        }

        public ValueFuture DelayedCall(Action action, double delaySeconds, ICancelToken cancelToken = null)
        {
            return _manager.TimerMgr.ScheduleAction(action, delaySeconds, cancelToken);
        }

        public ValueFuture DelayedCallUnscaled(Action action, double delaySeconds, ICancelToken cancelToken = null)
        {
            return _manager.UnscaledTimerMgr.ScheduleAction(action, delaySeconds, cancelToken);
        }

        public ValueFuture<T> DelayedFunc<T>(Func<T> func, double delaySeconds, ICancelToken cancelToken = null)
        {
            return _manager.TimerMgr.ScheduleFunc(func, delaySeconds, cancelToken);
        }

        public ValueFuture RepeatCall(Action action, double initialDelay, double interval, ICancelToken cancelToken = null)
        {
            return _manager.TimerMgr.ScheduleWithFixedDelay(action, initialDelay, interval, cancelToken);
        }

        public ValueFuture RepeatCallAtFixedRate(Action action, double initialDelay, double interval, ICancelToken cancelToken = null)
        {
            return _manager.TimerMgr.ScheduleAtFixedRate(action, initialDelay, interval, cancelToken);
        }

        public CoroutineUserContext StartCoroutine(Func<CoroutineTaskContext, ValueFuture> routine)
        {
            return _manager.CoroutineMgr.StartCoroutine(routine, new CoroutineStartArgs());
        }

        public CoroutineUserContext StartCoroutine(Func<CoroutineTaskContext, ValueFuture> routine, CoroutineStartArgs args)
        {
            return _manager.CoroutineMgr.StartCoroutine(routine, args);
        }

        public void CancelTask(long taskId)
        {
            _manager.CoroutineMgr.Cancel(taskId);
        }

        public void SetTimeScale(float timeScale)
        {
            _manager.SetTimeScale(timeScale);
        }
    }
}
