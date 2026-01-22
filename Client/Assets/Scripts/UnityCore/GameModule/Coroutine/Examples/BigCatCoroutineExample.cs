using UnityCore.Base;
using UnityEngine;
using Wjybxx.BigCat.Co;

namespace UnityCore.GameModule.Coroutine.Examples
{
    public class BigCatCoroutineExample : MonoBehaviour
    {
        private long _timerId;
        private long _coroutineId;

        private void Start()
        {
            ExampleScheduleAction();
            ExampleSchedulePeriodic();
            ExampleCoroutine();
        }

        private void ExampleScheduleAction()
        {
            var manager = Game.Coroutine.GetManager();
            
            _timerId = manager.ScheduleAction(() =>
            {
                Debug.Log($"[BigCat] Delayed action executed at {Time.time}");
            }, delay: 2.0);
            
            Debug.Log($"[BigCat] Scheduled action with ID: {_timerId}");
        }

        private void ExampleSchedulePeriodic()
        {
            var manager = Game.Coroutine.GetManager();
            
            manager.ScheduleAction(() =>
            {
                Debug.Log($"[BigCat] Periodic action at {Time.time}");
            }, initialDelay: 1.0, period: 1.0, countLimit: 5);
        }

        private void ExampleCoroutine()
        {
            var manager = Game.Coroutine.GetManager();
            
            _coroutineId = manager.StartCoroutine(async ctx =>
            {
                Debug.Log("[BigCat] Coroutine started");
                
                await ctx.Sleep(1.0, timingType: TimingType.Time);
                Debug.Log("[BigCat] After 1 second");
                
                await ctx.Sleep(2.0, timingType: TimingType.Time);
                Debug.Log("[BigCat] After 3 seconds total");
            });
            
            Debug.Log($"[BigCat] Started coroutine with ID: {_coroutineId}");
        }

        private void OnDestroy()
        {
            var manager = Game.Coroutine?.GetManager();
            if (manager != null)
            {
                manager.CancelTask(_timerId);
                manager.CancelTask(_coroutineId);
            }
        }
    }
}



