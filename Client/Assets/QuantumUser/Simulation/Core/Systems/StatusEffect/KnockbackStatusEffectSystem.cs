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
    
            // 直接设置 KCC 速度，让 KCC 处理碰撞
            filter.KCC->Velocity = knockbackVelocity;
            filter.KCC->Move(frame, filter.EntityRef, FPVector2.Up);
    
            // 更新计时器
            filter.CharacterStatus->KnockbackStatusEffect.DurationTimer.Tick(frame.DeltaTime);
    
            // 击退结束
            if (!filter.CharacterStatus->KnockbackStatusEffect.DurationTimer.IsRunning)
            {
                // 恢复设置
                // CharacterMovementData playerMovementData = frame.FindAsset<CharacterMovementData>(filter.CharacterStatus->CharacterMovementData.Id);
                // playerMovementData.UpdateKCCSettings(frame, filter.EntityRef);
            }
        }

        public void OnKnockbackApplied(Frame frame, EntityRef target, FPVector2 knockbackVelocity, FP duration)
        {
            CharacterStatusComponent* playerStatus = frame.Unsafe.GetPointer<CharacterStatusComponent>(target);
            CharacterMovementData playerMovementData = frame.FindAsset<CharacterMovementData>(playerStatus->CharacterMovementData.Id);

            playerStatus->KnockbackStatusEffect.DurationTimer.Start(1);
            playerStatus->KnockbackStatusEffect.KnockbackDirection = knockbackVelocity.Normalized;
            playerStatus->KnockbackStatusEffect.KnockbackVelocity = knockbackVelocity;
        }
        
        private FPVector2 GetKnockbackVelocity(Frame frame, CharacterStatusComponent* hitReaction)
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

        public void OnKnockbackPhysic2DApplied(Frame frame, EntityRef target, FPVector2 knockbackVelocity)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(target, out var physicsBody))
            {
                physicsBody->Velocity = knockbackVelocity;
            }
        }

        
    }
}