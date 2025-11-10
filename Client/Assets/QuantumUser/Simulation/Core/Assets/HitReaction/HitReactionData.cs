using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class HitReactionData : AssetObject
    {
        [Header("Core Flags")]
        [Tooltip("是否可以被击退")]
        public bool CanBeKnockedBack = true;
        
        [Tooltip("是否可以被硬直")]
        public bool CanBeHitstunned = true;

        public virtual void UpdateHitReaction(Frame frame, EntityRef entity, HitReactionComponent* hitReaction)
        {
            UpdateHitstun(frame, hitReaction);
            UpdateKnockback(frame, entity, hitReaction);
        }

        public virtual void OnHitstunApplied(Frame frame, EntityRef target, HitReactionComponent* hitReaction, FP duration, HitType hitType)
        {
            if (!CanBeHitstunned)
                return;

            FP finalDuration = CalculateHitstunDuration(duration, hitType);

            hitReaction->IsHitstunned = true;
            hitReaction->HitstunTimer = FrameTimer.FromSeconds(frame, finalDuration);
            
            OnHitstunStarted(frame, target, hitReaction, hitType);
        }

        public virtual void OnKnockbackApplied(Frame frame, EntityRef target, HitReactionComponent* hitReaction, FP duration, FPVector2 knockbackVelocity)
        {
            if (!CanBeKnockedBack)
                return;

            KnockbackConfig config = GetKnockbackConfig();

            hitReaction->IsKnockedBack = true;
            hitReaction->KnockbackVelocity = knockbackVelocity;
            hitReaction->InitialKnockbackVelocity = knockbackVelocity;
            hitReaction->KnockbackStartTime = frame.Number * frame.DeltaTime;
            hitReaction->CurrentMode = config.Mode;
            
            if (config.Mode == KnockbackMode.CustomCurve)
            {
                hitReaction->KnockbackDuration = config.CurveDuration;
            }
            else
            {
                hitReaction->KnockbackDecay = config.Mode == KnockbackMode.Physics 
                    ? config.HorizontalDecayRate 
                    : config.LinearDecayRate;
            }
            
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(target, out var physicsBody))
            {
                physicsBody->Velocity = knockbackVelocity;
            }
            
            OnHitstunApplied(frame, target, hitReaction, duration, HitType.Heavy);
            OnKnockbackStarted(frame, target, hitReaction, knockbackVelocity);
        }

        protected virtual FP CalculateHitstunDuration(FP baseDuration, HitType hitType)
        {
            return baseDuration;
        }

        protected virtual KnockbackConfig GetKnockbackConfig()
        {
            KnockbackConfig config = default;
            config.Mode = KnockbackMode.LinearDecay;
            config.HorizontalDecayRate = FP._1 + FP._0_50;
            config.UseGravity = true;
            config.CurveDuration = FP._1;
            config.LinearDecayRate = FP._8;
            config.MinThreshold = FP._0_50;
            return config;
        }

        protected virtual FP EvaluateHorizontalCurve(FP normalizedTime)
        {
            return FP._1 - normalizedTime;
        }

        protected virtual FP EvaluateVerticalCurve(FP normalizedTime)
        {
            return FP._1 - normalizedTime;
        }

        protected virtual void OnHitstunStarted(Frame frame, EntityRef target, HitReactionComponent* hitReaction, HitType hitType)
        {
            if (frame.Unsafe.TryGetPointer<AbilityEnable>(target, out var abilityEnable))
            {
                abilityEnable->MovementEnabled = false;
            }
    
            if (frame.Unsafe.TryGetPointer<AbilityInventory>(target, out var abilityInventory))
            {
                if (abilityInventory->HasActiveAbility)
                {
                    frame.Signals.OnActiveAbilityStopped(target);
                }
            }
        }

        protected virtual void OnKnockbackStarted(Frame frame, EntityRef target, HitReactionComponent* hitReaction, FPVector2 velocity)
        {
            if (frame.Unsafe.TryGetPointer<AbilityEnable>(target, out var abilityEnable))
            {
                abilityEnable->MovementEnabled = false;
            }
    
            if (frame.Unsafe.TryGetPointer<MovementComponent>(target, out var movementData))
            {
                FP horizontalDirection = velocity.X;
                if (FPMath.Abs(horizontalDirection) > FP._0_10)
                {
                    movementData->IsFacingRight = horizontalDirection < 0;
                }
            }

            frame.Events.OnPlayerKnockedBack(target, velocity.Normalized, velocity.Magnitude);
        }

        protected virtual void UpdateHitstun(Frame frame, HitReactionComponent* hitReaction)
        {
            if (hitReaction->IsHitstunned && !hitReaction->HitstunTimer.IsRunning(frame))
            {
                hitReaction->IsHitstunned = false;
        
                OnHitstunEnded(frame, hitReaction);
            }
        }
        
        protected virtual void OnHitstunEnded(Frame frame, HitReactionComponent* hitReaction)
        {
        }


        protected virtual void UpdateKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction)
        {
            if (!hitReaction->IsKnockedBack)
                return;

            KnockbackConfig config = GetKnockbackConfig();

            switch (hitReaction->CurrentMode)
            {
                case KnockbackMode.Physics:
                    UpdatePhysicsKnockback(frame, entity, hitReaction, config);
                    break;
                
                case KnockbackMode.CustomCurve:
                    UpdateCurveKnockback(frame, entity, hitReaction, config);
                    break;
                
                case KnockbackMode.LinearDecay:
                default:
                    UpdateLinearKnockback(frame, entity, hitReaction, config);
                    break;
            }
        }

        protected virtual void UpdatePhysicsKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, KnockbackConfig config)
        {
            FP horizontalSpeed = FPMath.Abs(hitReaction->KnockbackVelocity.X);

            if (horizontalSpeed > config.MinThreshold)
            {
                FP decay = hitReaction->KnockbackDecay * frame.DeltaTime;
                hitReaction->KnockbackVelocity.X *= (FP._1 - decay);
                
                ApplyPhysicsKnockback(frame, entity, hitReaction, config.UseGravity);
            }
            else
            {
                EndKnockback(frame, entity, hitReaction, config.UseGravity);
            }
        }

        protected virtual void UpdateCurveKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, KnockbackConfig config)
        {
            FP currentTime = frame.Number * frame.DeltaTime;
            FP elapsedTime = currentTime - hitReaction->KnockbackStartTime;
            FP normalizedTime = elapsedTime / hitReaction->KnockbackDuration;

            if (normalizedTime >= FP._1)
            {
                EndKnockback(frame, entity, hitReaction, config.UseGravity);
                return;
            }

            FP horizontalMultiplier = EvaluateHorizontalCurve(normalizedTime);
            FP verticalMultiplier = EvaluateVerticalCurve(normalizedTime);

            hitReaction->KnockbackVelocity = new FPVector2(
                hitReaction->InitialKnockbackVelocity.X * horizontalMultiplier,
                hitReaction->InitialKnockbackVelocity.Y * verticalMultiplier
            );

            ApplyCurveKnockback(frame, entity, hitReaction);
        }

        protected virtual void UpdateLinearKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, KnockbackConfig config)
        {
            FP velocityMagnitude = hitReaction->KnockbackVelocity.Magnitude;

            if (velocityMagnitude > config.MinThreshold)
            {
                FP decay = hitReaction->KnockbackDecay * frame.DeltaTime;
                hitReaction->KnockbackVelocity *= (FP._1 - decay);
                
                ApplyLinearKnockback(frame, entity, hitReaction);
            }
            else
            {
                EndKnockback(frame, entity, hitReaction, false);
            }
        }

        protected virtual void ApplyPhysicsKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, bool useGravity)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
            {
                if (physicsBody->IsKinematic)
                {
                    physicsBody->Velocity = hitReaction->KnockbackVelocity;

                    if (frame.Unsafe.TryGetPointer<Transform2D>(entity, out var trans))
                    {
                        trans->Position += hitReaction->KnockbackVelocity * frame.DeltaTime;
                    }
                }
                else
                {
                    physicsBody->Velocity = new FPVector2(
                        hitReaction->KnockbackVelocity.X,
                        physicsBody->Velocity.Y
                    );
                }

                return;
            }

            if (frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform))
            {
                transform->Position += hitReaction->KnockbackVelocity * frame.DeltaTime;
            }
        }

        protected virtual void ApplyCurveKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
            {
                physicsBody->Velocity = hitReaction->KnockbackVelocity;
                
                if (physicsBody->IsKinematic && frame.Unsafe.TryGetPointer<Transform2D>(entity, out var trans))
                {
                    trans->Position += hitReaction->KnockbackVelocity * frame.DeltaTime;
                }
                return;
            }

            if (frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform))
            {
                transform->Position += hitReaction->KnockbackVelocity * frame.DeltaTime;
            }
        }

        protected virtual void ApplyLinearKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
            {
                if (physicsBody->IsKinematic)
                {
                    physicsBody->Velocity = hitReaction->KnockbackVelocity;
                    
                    if (frame.Unsafe.TryGetPointer<Transform2D>(entity, out var trans))
                    {
                    trans->Position += hitReaction->KnockbackVelocity * frame.DeltaTime;
                    }
                }
                else
                {
                    physicsBody->Velocity = new FPVector2(
                        hitReaction->KnockbackVelocity.X,
                        physicsBody->Velocity.Y
                    );
                }
                return;
            }

            if (frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform))
            {
                transform->Position += hitReaction->KnockbackVelocity * frame.DeltaTime;
            }
        }

        protected virtual void EndKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, bool preserveVerticalVelocity)
        {
            hitReaction->IsKnockedBack = false;
            hitReaction->KnockbackVelocity = FPVector2.Zero;

            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
            {
                if (physicsBody->IsKinematic)
                {
                    // Kinematic物体必须手动清零
                    physicsBody->Velocity = FPVector2.Zero;
                }
                else if (preserveVerticalVelocity)
                {
                    // Dynamic物体空中击退，只清零水平速度
                    physicsBody->Velocity = new FPVector2(FP._0, physicsBody->Velocity.Y);
                }
                // Dynamic物体地面击退：不清零，让物理引擎自然减速
            }
    
            if (frame.Unsafe.TryGetPointer<AbilityEnable>(entity, out var abilityEnable))
            {
                if (frame.Unsafe.TryGetPointer<AbilityInventory>(entity, out var abilityInventory))
                {
                    if (!abilityInventory->HasActiveAbility)
                    {
                        abilityEnable->MovementEnabled = true;
                    }
                }
                else
                {
                    abilityEnable->MovementEnabled = true;
                }
            }
        }
    }
}
