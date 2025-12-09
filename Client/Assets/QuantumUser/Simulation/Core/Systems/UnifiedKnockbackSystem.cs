using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class UnifiedKnockbackSystem : SystemMainThreadFilter<UnifiedKnockbackSystem.Filter>, ISignalOnKnockbackApplied
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public Transform2D* Transform;
            public KnockbackComponent* Knockback;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.Knockback->StatusEffect.DurationTimer.IsRunning)
            {
                return;
            }

            FPVector2 lastRelativePosition = GetKnockbackRelativePosition(frame, filter.Knockback);
            filter.Knockback->StatusEffect.DurationTimer.Tick(frame.DeltaTime);
            FPVector2 newRelativePosition = GetKnockbackRelativePosition(frame, filter.Knockback);

            FPVector2 knockbackMovement = newRelativePosition - lastRelativePosition;
            FPVector2 knockbackVelocity = knockbackMovement / frame.DeltaTime;

            filter.Knockback->StatusEffect.KnockbackVelocity = knockbackVelocity;

            ApplyKnockbackByControllerType(frame, filter.EntityRef, filter.Transform, knockbackMovement, knockbackVelocity);

            if (!filter.Knockback->StatusEffect.DurationTimer.IsRunning)
            {
                OnKnockbackEnd(frame, filter.EntityRef);
            }
        }

        public void OnKnockbackApplied(Frame frame, EntityRef target, FP duration, FPVector2 direction, AssetRef<KnockbackStatusEffectData> statusEffectData)
        {
            if (!frame.Has<KnockbackComponent>(target))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"Entity {target} doesn't have KnockbackComponent. Adding it automatically.");
#endif
                frame.Add<KnockbackComponent>(target);
            }

            KnockbackComponent* knockback = frame.Unsafe.GetPointer<KnockbackComponent>(target);
            
            knockback->StatusEffect.DurationTimer.Start(duration);
            knockback->StatusEffect.KnockbackDirection = direction.Normalized;
            knockback->StatusEffect.StatusEffectData = statusEffectData;
            knockback->StatusEffect.KnockbackVelocity = FPVector2.Zero;

            DetermineApplicationMode(frame, target, knockback);
        }

        private void DetermineApplicationMode(Frame frame, EntityRef target, KnockbackComponent* knockback)
        {
            if (frame.Has<KCC2D>(target))
            {
                knockback->ApplicationMode = KnockbackApplicationMode.KCC2D;
            }
            else if (frame.Has<CharacterController2D>(target))
            {
                knockback->ApplicationMode = KnockbackApplicationMode.CharacterController;
            }
            else if (frame.Has<PhysicsBody2D>(target))
            {
                knockback->ApplicationMode = KnockbackApplicationMode.Physics2D;
            }
            else
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogError($"Entity {target} has no supported movement controller!");
#endif
            }
        }

        private void ApplyKnockbackByControllerType(Frame frame, EntityRef entity, Transform2D* transform, 
            FPVector2 movement, FPVector2 velocity)
        {
            KnockbackComponent* knockback = frame.Unsafe.GetPointer<KnockbackComponent>(entity);

            switch (knockback->ApplicationMode)
            {
                case KnockbackApplicationMode.KCC2D:
                    ApplyToKCC2D(frame, entity, velocity);
                    break;

                case KnockbackApplicationMode.CharacterController:
                    ApplyToCharacterController(frame, entity, transform, movement, velocity);
                    break;

                case KnockbackApplicationMode.Physics2D:
                    ApplyToPhysics2D(frame, entity, velocity);
                    break;
            }
        }

        private void ApplyToKCC2D(Frame frame, EntityRef entity, FPVector2 velocity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                kcc->DynamicVelocity = velocity;
            }
        }

        private void ApplyToCharacterController(Frame frame, EntityRef entity, Transform2D* transform, 
            FPVector2 movement, FPVector2 velocity)
        {
            transform->Position += movement;
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc))
            {
                cc->Velocity = velocity;
            }
        }

        private void ApplyToPhysics2D(Frame frame, EntityRef entity, FPVector2 velocity)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
            {
                physicsBody->Velocity = velocity;
            }
        }

        private void OnKnockbackEnd(Frame frame, EntityRef entity)
        {
            KnockbackComponent* knockback = frame.Unsafe.GetPointer<KnockbackComponent>(entity);

            switch (knockback->ApplicationMode)
            {
                case KnockbackApplicationMode.KCC2D:
                    if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
                    {
                        kcc->DynamicVelocity = FPVector2.Zero;
                    }
                    break;

                case KnockbackApplicationMode.CharacterController:
                    if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc))
                    {
                        cc->Velocity = FPVector2.Zero;
                    }
                    break;
                    
                case KnockbackApplicationMode.Physics2D:
                    break;
            }
        }

        private FPVector2 GetKnockbackRelativePosition(Frame frame, KnockbackComponent* knockback)
        {
            KnockbackStatusEffectData data = frame.FindAsset<KnockbackStatusEffectData>(knockback->StatusEffect.StatusEffectData.Id);

            FP normalizedTime = knockback->StatusEffect.DurationTimer.NormalizedTime;
            FP normalizedPositionX = data.KnockbackCurveX.Evaluate(normalizedTime);
            FP normalizedPositionY = data.KnockbackCurveY.Evaluate(normalizedTime);

            FPVector2 relativePosition = new FPVector2(
                knockback->StatusEffect.KnockbackDirection.X * data.KnockbackDistanceX * normalizedPositionX,
                knockback->StatusEffect.KnockbackDirection.Y * data.KnockbackDistanceY * normalizedPositionY
            );

            return relativePosition;
        }
    }
}
