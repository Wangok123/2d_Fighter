namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// 投技系统 - 处理投技攻击逻辑
    /// Throw System - Handles throw attack logic
    /// </summary>
    public unsafe class ThrowSystem : SystemMainThreadFilter<ThrowSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public ThrowState* ThrowState;
            public AnimationState* AnimationState;
            public MovementData* MovementData;
        }

        // 投技配置常量
        private const int THROW_START_HASH = -1234567; // 实际应该用Animator.StringToHash("Throw_Start")
        private const int THROW_HOLD_HASH = -1234568;
        private const int THROW_EXECUTE_HASH = -1234569;
        private const int THROW_RECOVERY_HASH = -1234570;

        public override void Update(Frame frame, ref Filter filter)
        {
            UpdateThrowState(frame, ref filter);
        }

        /// <summary>
        /// 尝试执行投技
        /// Try to execute throw
        /// </summary>
        public static bool TryThrow(Frame frame, EntityRef attacker, FP grabRange, FP grabAngle)
        {
            if (!frame.Unsafe.TryGetPointer(attacker, out ThrowState* throwState))
                return false;

            // 检查是否已经在执行投技
            if (throwState->IsExecuting)
                return false;

            if (!frame.Unsafe.TryGetPointer(attacker, out Transform2D* attackerTransform))
                return false;

            if (!frame.Unsafe.TryGetPointer(attacker, out MovementData* attackerMovement))
                return false;

            // 检测范围内的目标
            EntityRef target = DetectTarget(frame, attackerTransform, attackerMovement, grabRange, grabAngle);
            
            if (target == EntityRef.None)
                return false;

            // 检查目标是否可被抓取
            if (!CanGrabTarget(frame, target))
                return false;

            // 开始投技
            throwState->State = 1; // GrabAttempt
            throwState->GrabbedTarget = target;
            throwState->ThrowStartTime = frame.Number;
            throwState->IsExecuting = true;

            // 播放投技起手动画
            AnimationSystem.ForcePlayAnimation(frame, attacker, THROW_START_HASH);

            // 通知目标被抓取
            if (frame.Unsafe.TryGetPointer(target, out AnimationState* targetAnimState))
            {
                // 播放被抓取动画（可选）
                // AnimationSystem.ForcePlayAnimation(frame, target, GRABBED_HASH);
            }

            return true;
        }

        /// <summary>
        /// 执行投技（造成伤害）
        /// Execute throw (deal damage)
        /// </summary>
        public static void ExecuteThrow(Frame frame, EntityRef attacker, FP damage, FPVector2 knockbackVelocity)
        {
            if (!frame.Unsafe.TryGetPointer(attacker, out ThrowState* throwState))
                return;

            if (!throwState->IsExecuting || throwState->GrabbedTarget == EntityRef.None)
                return;

            throwState->State = 3; // Executing

            // 播放投技执行动画
            AnimationSystem.ForcePlayAnimation(frame, attacker, THROW_EXECUTE_HASH);

            // 对目标造成伤害
            var target = throwState->GrabbedTarget;
            if (frame.Unsafe.TryGetPointer(target, out Status* targetStatus))
            {
                targetStatus->CurrentHealth -= damage;
                if (targetStatus->CurrentHealth <= FP._0)
                {
                    targetStatus->CurrentHealth = FP._0;
                    targetStatus->IsDead = true;
                }
            }

            // 应用击退
            if (frame.Unsafe.TryGetPointer(target, out HitReaction* hitReaction))
            {
                hitReaction->KnockbackVelocity = knockbackVelocity;
            }
        }

        /// <summary>
        /// 完成投技
        /// Complete throw
        /// </summary>
        public static void CompleteThrow(Frame frame, EntityRef attacker)
        {
            if (!frame.Unsafe.TryGetPointer(attacker, out ThrowState* throwState))
                return;

            throwState->State = 4; // Recovery
            
            // 播放恢复动画
            AnimationSystem.ForcePlayAnimation(frame, attacker, THROW_RECOVERY_HASH);

            // 释放目标
            throwState->GrabbedTarget = EntityRef.None;
            throwState->IsExecuting = false;
        }

        /// <summary>
        /// 破解投技
        /// Break throw
        /// </summary>
        public static bool TryBreakThrow(Frame frame, EntityRef attacker, FP breakWindow)
        {
            if (!frame.Unsafe.TryGetPointer(attacker, out ThrowState* throwState))
                return false;

            if (!throwState->IsExecuting)
                return false;

            // 检查是否在破解窗口内
            FP timeSinceStart = (frame.Number - throwState->ThrowStartTime) * frame.DeltaTime;
            if (timeSinceStart > breakWindow)
                return false;

            // 破解成功
            throwState->State = 0; // Idle
            throwState->IsExecuting = false;
            var target = throwState->GrabbedTarget;
            throwState->GrabbedTarget = EntityRef.None;

            // 双方返回Idle
            if (frame.Unsafe.TryGetPointer(attacker, out AnimationConfig* animConfig))
            {
                var configAsset = frame.FindAsset(animConfig->ConfigAsset);
                if (configAsset != null)
                {
                    AnimationSystem.ForcePlayAnimation(frame, attacker, configAsset.DefaultStateHash);
                }
            }

            if (target != EntityRef.None && frame.Unsafe.TryGetPointer(target, out AnimationConfig* targetConfig))
            {
                var configAsset = frame.FindAsset(targetConfig->ConfigAsset);
                if (configAsset != null)
                {
                    AnimationSystem.ForcePlayAnimation(frame, target, configAsset.DefaultStateHash);
                }
            }

            return true;
        }

        private void UpdateThrowState(Frame frame, ref Filter filter)
        {
            // 更新投技状态机（可根据需要添加自动进入下一阶段的逻辑）
            if (filter.ThrowState->IsExecuting)
            {
                // 可以根据动画进度自动推进投技阶段
                // 这里简化处理，由外部调用ExecuteThrow和CompleteThrow
            }
        }

        private static EntityRef DetectTarget(Frame frame, Transform2D* attackerTransform, MovementData* attackerMovement, FP grabRange, FP grabAngle)
        {
            // 简化实现：使用物理查询检测范围内的目标
            // 实际应该使用Physics2D.OverlapCircle等API
            
            FPVector2 attackerPos = attackerTransform->Position;
            FPVector2 direction = attackerMovement->IsFacingRight ? FPVector2.Right : FPVector2.Left;

            // 这里需要遍历所有潜在目标实体
            // 简化：返回None，实际应该实现物理检测
            return EntityRef.None;
        }

        private static bool CanGrabTarget(Frame frame, EntityRef target)
        {
            // 检查目标是否可被抓取
            if (frame.Unsafe.TryGetPointer(target, out Status* status))
            {
                if (status->IsDead)
                    return false;

                if (status->InvincibleTimer.IsRunning)
                    return false;
            }

            // 检查目标是否已经被抓取
            if (frame.Unsafe.TryGetPointer(target, out ThrowState* throwState))
            {
                if (throwState->IsExecuting)
                    return false;
            }

            return true;
        }
    }
}
