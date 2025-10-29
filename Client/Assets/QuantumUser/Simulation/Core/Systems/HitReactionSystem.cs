namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// 受击反应系统 - 处理受击逻辑
    /// Hit Reaction System - Handles hit reaction logic
    /// </summary>
    public unsafe class HitReactionSystem : SystemMainThreadFilter<HitReactionSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public HitReaction* HitReaction;
            public AnimationState* AnimationState;
            public Transform2D* Transform;
            public KCC2D* KCC;
        }

        // 受击动画哈希值常量
        private const int HURT_GROUND_HASH = -2234567;
        private const int HURT_AIR_HASH = -2234568;
        private const int KNOCKDOWN_HASH = -2234569;

        public override void Update(Frame frame, ref Filter filter)
        {
            UpdateHitStun(frame, ref filter);
            ApplyKnockback(frame, ref filter);
        }

        /// <summary>
        /// 应用受击（智能选择地面/空中）
        /// Apply hit (smart ground/air selection)
        /// </summary>
        public static void ApplyHit(Frame frame, EntityRef target, FP damage, FPVector2 knockback, FP hitStunDuration, bool isGrounded)
        {
            // 造成伤害
            if (frame.Unsafe.TryGetPointer(target, out Status* status))
            {
                status->CurrentHealth -= damage;
                if (status->CurrentHealth <= FP._0)
                {
                    status->CurrentHealth = FP._0;
                    status->IsDead = true;
                    
                    // 播放死亡动画
                    ApplyDeath(frame, target);
                    return;
                }
            }

            // 设置受击状态
            if (frame.Unsafe.TryGetPointer(target, out HitReaction* hitReaction))
            {
                hitReaction->IsGrounded = isGrounded;
                hitReaction->KnockbackVelocity = knockback;
                hitReaction->HitStunTimer = FrameTimer.CreateFromSeconds(frame, hitStunDuration);
            }

            // 播放对应的受击动画
            int hitAnimHash = isGrounded ? HURT_GROUND_HASH : HURT_AIR_HASH;
            AnimationSystem.ForcePlayAnimation(frame, target, hitAnimHash);
        }

        /// <summary>
        /// 应用击倒
        /// Apply knockdown
        /// </summary>
        public static void ApplyKnockdown(Frame frame, EntityRef target, FP damage, FPVector2 knockback)
        {
            // 造成伤害
            if (frame.Unsafe.TryGetPointer(target, out Status* status))
            {
                status->CurrentHealth -= damage;
                if (status->CurrentHealth <= FP._0)
                {
                    status->CurrentHealth = FP._0;
                    status->IsDead = true;
                    ApplyDeath(frame, target);
                    return;
                }
            }

            // 设置击倒状态
            if (frame.Unsafe.TryGetPointer(target, out HitReaction* hitReaction))
            {
                hitReaction->KnockbackVelocity = knockback;
                hitReaction->HitStunTimer = FrameTimer.CreateFromSeconds(frame, FP._1); // 1秒硬直
            }

            // 播放击倒动画
            AnimationSystem.ForcePlayAnimation(frame, target, KNOCKDOWN_HASH);
        }

        /// <summary>
        /// 应用死亡
        /// Apply death
        /// </summary>
        private static void ApplyDeath(Frame frame, EntityRef target)
        {
            // 死亡动画哈希（优先级最高）
            const int DEATH_HASH = -3234567;
            AnimationSystem.ForcePlayAnimation(frame, target, DEATH_HASH);

            // 可以在这里添加其他死亡逻辑
        }

        /// <summary>
        /// 更新硬直时间
        /// Update hit stun
        /// </summary>
        private void UpdateHitStun(Frame frame, ref Filter filter)
        {
            if (filter.HitReaction->HitStunTimer.IsRunning)
            {
                if (filter.HitReaction->HitStunTimer.Expired(frame))
                {
                    // 硬直结束，返回Idle
                    if (frame.Unsafe.TryGetPointer(filter.Entity, out AnimationConfig* animConfig))
                    {
                        var configAsset = frame.FindAsset(animConfig->ConfigAsset);
                        if (configAsset != null)
                        {
                            AnimationSystem.ForcePlayAnimation(frame, filter.Entity, configAsset.DefaultStateHash);
                        }
                    }

                    // 清除击退速度
                    filter.HitReaction->KnockbackVelocity = FPVector2.Zero;
                }
            }
        }

        /// <summary>
        /// 应用击退力
        /// Apply knockback force
        /// </summary>
        private void ApplyKnockback(Frame frame, ref Filter filter)
        {
            if (filter.HitReaction->KnockbackVelocity != FPVector2.Zero)
            {
                // 应用击退到角色位置
                // 注意：这里简化处理，实际应该通过KCC或物理系统应用
                filter.Transform->Position += filter.HitReaction->KnockbackVelocity * frame.DeltaTime;

                // 逐渐减小击退速度（摩擦力）
                FP friction = FP._0_90; // 90%保留
                filter.HitReaction->KnockbackVelocity *= friction;

                // 如果速度很小，停止
                if (filter.HitReaction->KnockbackVelocity.SqrMagnitude < FP._0_01)
                {
                    filter.HitReaction->KnockbackVelocity = FPVector2.Zero;
                }
            }
        }
    }
}
