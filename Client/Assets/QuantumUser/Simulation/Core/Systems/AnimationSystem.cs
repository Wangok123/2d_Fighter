namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// 动画系统 - 处理动画状态切换和优先级逻辑
    /// Animation System - Handles animation state transitions and priority logic
    /// 
    /// 这是确定性系统，运行在Quantum模拟层
    /// This is a deterministic system running in Quantum simulation layer
    /// </summary>
    public unsafe class AnimationSystem : SystemMainThreadFilter<AnimationSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public AnimationState* AnimationState;
            public AnimationConfig* AnimationConfig;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // 更新动画时间和过渡
            UpdateAnimationTime(frame, ref filter);
        }

        /// <summary>
        /// 尝试播放动画
        /// Try to play animation
        /// </summary>
        public static bool TryPlayAnimation(Frame frame, EntityRef entity, int targetStateHash, bool ignoreCancelRules = false)
        {
            if (!frame.Unsafe.TryGetPointer(entity, out AnimationState* animState))
                return false;

            if (!frame.Unsafe.TryGetPointer(entity, out AnimationConfig* animConfig))
                return false;

            var configAsset = frame.FindAsset(animConfig->ConfigAsset);
            if (configAsset == null)
                return false;

            // 如果已经在播放相同的动画，忽略
            if (animState->CurrentStateHash == targetStateHash)
                return false;

            // 获取目标状态数据
            if (!configAsset.TryGetState(targetStateHash, out var targetState))
            {
                Log.Error($"Animation state not found: {targetStateHash}");
                return false;
            }

            // 检查是否可以取消当前动画
            if (!ignoreCancelRules && animState->CurrentStateHash != 0)
            {
                if (!configAsset.TryGetState(animState->CurrentStateHash, out var currentState))
                    return false;

                // 计算当前动画的归一化时间（这里简化处理，实际应该从View层获取）
                FP normalizedTime = FP._0_50; // 简化：假设在50%

                if (!configAsset.CanCancel(currentState, targetState, normalizedTime))
                {
                    return false; // 不能取消当前动画
                }
            }

            // 播放新动画
            animState->CurrentStateHash = targetStateHash;
            animState->Priority = targetState.Priority;
            animState->StartTime = frame.Number;
            animState->IsTransitioning = true;
            animState->TransitionDuration = targetState.CrossfadeDuration;

            return true;
        }

        /// <summary>
        /// 强制播放动画（忽略所有规则）
        /// Force play animation (ignore all rules)
        /// </summary>
        public static void ForcePlayAnimation(Frame frame, EntityRef entity, int stateHash)
        {
            TryPlayAnimation(frame, entity, stateHash, ignoreCancelRules: true);
        }

        /// <summary>
        /// 检查是否可以播放动画
        /// Check if animation can be played
        /// </summary>
        public static bool CanPlayAnimation(Frame frame, EntityRef entity, int targetStateHash)
        {
            if (!frame.Unsafe.TryGetPointer(entity, out AnimationState* animState))
                return false;

            if (!frame.Unsafe.TryGetPointer(entity, out AnimationConfig* animConfig))
                return false;

            var configAsset = frame.FindAsset(animConfig->ConfigAsset);
            if (configAsset == null)
                return false;

            if (animState->CurrentStateHash == 0)
                return true;

            if (!configAsset.TryGetState(targetStateHash, out var targetState))
                return false;

            if (!configAsset.TryGetState(animState->CurrentStateHash, out var currentState))
                return false;

            FP normalizedTime = FP._0_50; // 简化处理
            return configAsset.CanCancel(currentState, targetState, normalizedTime);
        }

        private void UpdateAnimationTime(Frame frame, ref Filter filter)
        {
            // 更新过渡状态
            if (filter.AnimationState->IsTransitioning)
            {
                FP elapsed = (frame.Number - filter.AnimationState->StartTime) * frame.DeltaTime;
                if (elapsed >= filter.AnimationState->TransitionDuration)
                {
                    filter.AnimationState->IsTransitioning = false;
                }
            }
        }
    }
}
