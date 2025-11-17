using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class KnockbackStatusEffectSystem : SystemMainThreadFilter<KnockbackStatusEffectSystem.Filter>, ISignalOnKnockbackApplied, ISignalOnKnockbackPhysic2DApplied
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public CharacterStatusComponent* CharacterStatus;
            public Transform2D* Transform;
            public CharacterController2D* KCC;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.CharacterStatus->IsKnockedBack)
            {
                return;
            }
            
            // 计算当前应有的击退速度
            FPVector2 knockbackVelocity = GetKnockbackVelocity(frame, filter.CharacterStatus);
    
            // 【重构】只设置速度，不调用 Move，统一由 Character2DSystem 处理移动
            filter.KCC->Velocity = knockbackVelocity;
            filter.KCC->Move(frame, filter.EntityRef, filter.CharacterStatus->KnockbackStatusEffect.KnockbackDirection);
    
            // 更新计时器
            filter.CharacterStatus->KnockbackStatusEffect.DurationTimer.Tick(frame.DeltaTime);
    
            // 击退结束
            if (!filter.CharacterStatus->KnockbackStatusEffect.DurationTimer.IsRunning)
            {
                // 恢复设置
                filter.CharacterStatus->KnockbackStatusEffect.KnockbackVelocity = FPVector2.Zero;
            }
        }

        public void OnKnockbackApplied(Frame frame, EntityRef target, FPVector2 knockbackVelocity, FP duration)
        {
            CharacterStatusComponent* playerStatus = frame.Unsafe.GetPointer<CharacterStatusComponent>(target);

            if (duration <= FP._0)
            {
                duration = FP._0_50;
            }
            
            playerStatus->KnockbackStatusEffect.DurationTimer.Start(duration);
            playerStatus->KnockbackStatusEffect.KnockbackDirection = knockbackVelocity.Normalized;
            playerStatus->KnockbackStatusEffect.KnockbackVelocity = knockbackVelocity;
        }
        
        private FPVector2 GetKnockbackVelocity(Frame frame, CharacterStatusComponent* hitReaction)
        {
            if (hitReaction->KnockbackStatusEffect.StatusEffectData.Id.IsValid)
            {
                KnockbackStatusEffectData data = frame.FindAsset<KnockbackStatusEffectData>(hitReaction->KnockbackStatusEffect.StatusEffectData.Id);
    
                FP normalizedTime = hitReaction->KnockbackStatusEffect.DurationTimer.NormalizedTime;
                FP velocityScaleX = data.KnockbackCurveX.Evaluate(normalizedTime);
                FP velocityScaleY = data.KnockbackCurveY.Evaluate(normalizedTime);
    
                FPVector2 knockbackVelocity = new FPVector2(
                    hitReaction->KnockbackStatusEffect.KnockbackDirection.X * hitReaction->KnockbackStatusEffect.KnockbackVelocity.X * velocityScaleX,
                    hitReaction->KnockbackStatusEffect.KnockbackDirection.Y * hitReaction->KnockbackStatusEffect.KnockbackVelocity.Y * velocityScaleY
                );
                
                return knockbackVelocity;
            }
            else
            {
                FP normalizedTime = hitReaction->KnockbackStatusEffect.DurationTimer.NormalizedTime;
                FP falloffScale = FP._1 - normalizedTime;
                
                return hitReaction->KnockbackStatusEffect.KnockbackVelocity * falloffScale;
            }
        }

        public void OnKnockbackPhysic2DApplied(Frame frame, EntityRef target, FPVector2 knockbackVelocity)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(target, out var physicsBody))
            {
                physicsBody->Velocity = knockbackVelocity;
            }
        }
        
    }
}