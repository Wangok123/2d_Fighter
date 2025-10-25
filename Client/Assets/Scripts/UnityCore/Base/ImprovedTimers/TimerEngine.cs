using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace UnityCore.Base.ImprovedTimers
{
    internal static class TimerBootstrapper
    {
        private static PlayerLoopSystem timerSystem;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            
            if (!InsertTimerManager<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogError("Failed to insert TimerManager into PlayerLoop.");
                return;
            }
            
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            PlayerLoopUtils.PrintPlayerLoop(currentPlayerLoop);
        }

        static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            timerSystem = new PlayerLoopSystem
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers,
                subSystemList = null
            };
            
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }
    }
}