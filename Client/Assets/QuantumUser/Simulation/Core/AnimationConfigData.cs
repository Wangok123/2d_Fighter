namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    /// <summary>
    /// 动画状态数据 - 定义单个动画状态的属性
    /// Animation State Data - Defines properties of a single animation state
    /// </summary>
    [System.Serializable]
    public partial struct AnimationStateData
    {
        public string StateName;
        public int StateHash;
        public int Priority;
        public int CancelPolicy; // 0=NonCancellable, 1=AlwaysCancellable, 2=CancellableInWindow, 3=CancellableOnEnd, 4=OnlyByHigherPriority
        public FP CrossfadeDuration;
        public CancelWindowData[] CancelWindows;
    }

    /// <summary>
    /// 取消窗口数据
    /// Cancel Window Data
    /// </summary>
    [System.Serializable]
    public partial struct CancelWindowData
    {
        public FP StartTime;      // 归一化时间 0-1
        public FP EndTime;        // 归一化时间 0-1
        public int[] AllowedTargetHashes; // 允许取消到的目标状态哈希值，null表示任意
    }

    /// <summary>
    /// 动画配置数据资源 - 包含所有动画状态定义
    /// Animation Config Data Asset - Contains all animation state definitions
    /// </summary>
    public partial class AnimationConfigData : AssetObject
    {
        public AnimationStateData[] States;
        public int DefaultStateHash;

        // 根据哈希值查找状态数据
        public bool TryGetState(int stateHash, out AnimationStateData stateData)
        {
            for (int i = 0; i < States.Length; i++)
            {
                if (States[i].StateHash == stateHash)
                {
                    stateData = States[i];
                    return true;
                }
            }
            stateData = default;
            return false;
        }

        // 检查是否可以从当前状态取消到目标状态
        public bool CanCancel(AnimationStateData currentState, AnimationStateData targetState, FP normalizedTime)
        {
            // 1. 检查优先级
            if (targetState.Priority > currentState.Priority)
            {
                return true; // 高优先级可以打断低优先级
            }

            if (targetState.Priority < currentState.Priority)
            {
                return false; // 低优先级不能打断高优先级
            }

            // 2. 优先级相同，检查取消策略
            switch (currentState.CancelPolicy)
            {
                case 0: // NonCancellable
                    return false;

                case 1: // AlwaysCancellable
                    return true;

                case 4: // OnlyByHigherPriority
                    return false; // 已经在优先级检查中处理

                case 3: // CancellableOnEnd
                    return normalizedTime >= FP._0_80; // 80%以上可以取消

                case 2: // CancellableInWindow
                    return IsInCancelWindow(currentState, targetState.StateHash, normalizedTime);

                default:
                    return true;
            }
        }

        // 检查是否在取消窗口内
        private bool IsInCancelWindow(AnimationStateData currentState, int targetHash, FP normalizedTime)
        {
            if (currentState.CancelWindows == null || currentState.CancelWindows.Length == 0)
                return false;

            for (int i = 0; i < currentState.CancelWindows.Length; i++)
            {
                var window = currentState.CancelWindows[i];
                
                // 检查时间窗口
                if (normalizedTime < window.StartTime || normalizedTime > window.EndTime)
                    continue;

                // 检查目标是否允许
                if (window.AllowedTargetHashes == null || window.AllowedTargetHashes.Length == 0)
                    return true; // null表示允许任何目标

                for (int j = 0; j < window.AllowedTargetHashes.Length; j++)
                {
                    if (window.AllowedTargetHashes[j] == targetHash)
                        return true;
                }
            }

            return false;
        }
    }
}
