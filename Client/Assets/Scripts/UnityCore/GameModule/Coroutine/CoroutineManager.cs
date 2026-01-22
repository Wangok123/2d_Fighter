using Core;
using Wjybxx.BigCat.Co;
using Wjybxx.BigCat.Gameplay;
using Wjybxx.Commons.Concurrent;

namespace UnityCore.GameModule.Coroutine
{
    public class CoroutineManager : CoreModule
    {
        private CoroutineMgr _coroutineMgr;
        private GTime _gTime;
        private bool _isInitialized;

        public override int Priority => 90;

        public ICoroutineMgr CoroutineMgr => _coroutineMgr;
        public ITimerMgr TimerMgr => _coroutineMgr?.TimerMgr;
        public ITimerMgr UnscaledTimerMgr => _coroutineMgr?.UnscaledTimerMgr;

        public void Initialize()
        {
            if (_isInitialized) return;

            _gTime = new GTime();
            _gTime.Restart(timeScale: 1.0);
            _gTime.Update(0.001);

            _coroutineMgr = new CoroutineMgr(
                eventLoop: GlobalEventLoop.Inst,
                time: _gTime,
                minPeriod: 0.01,
                unscaledMinPeriod: 0.01,
                enableUnscaledQueue: true,
                enableFrameQueue: false
            );

            _isInitialized = true;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!_isInitialized) return;

            _gTime.Update(realElapseSeconds);

            _coroutineMgr.Update(GameLoopPhase.EarlyUpdate);
            _coroutineMgr.Update(GameLoopPhase.PostEarlyUpdate);
            _coroutineMgr.Update(GameLoopPhase.Update);
            _coroutineMgr.Update(GameLoopPhase.PostUpdate);
            _coroutineMgr.Update(GameLoopPhase.LateUpdate);
            _coroutineMgr.Update(GameLoopPhase.PostLateUpdate);
        }

        public override void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (!_isInitialized) return;

            _gTime.FixedUpdate(realElapseSeconds);

            _coroutineMgr.Update(GameLoopPhase.FixedUpdate);
            _coroutineMgr.Update(GameLoopPhase.PostFixedUpdate);
        }

        internal override void Shutdown()
        {
            if (_coroutineMgr != null)
            {
                _coroutineMgr.Shutdown();
                _coroutineMgr = null;
            }

            _gTime = null;
            _isInitialized = false;
        }

        public void SetTimeScale(float timeScale)
        {
            if (_gTime != null)
            {
                _gTime.TimeScale = timeScale;
            }
        }
    }
}
