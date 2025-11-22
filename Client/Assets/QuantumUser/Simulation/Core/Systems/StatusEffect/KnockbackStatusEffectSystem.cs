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
            
            FPVector2 lastRelativePosition = GetKnockbackRelativePosition(frame, filter.CharacterStatus);
            filter.CharacterStatus->KnockbackStatusEffect.DurationTimer.Tick(frame.DeltaTime);
            FPVector2 newRelativePosition = GetKnockbackRelativePosition(frame, filter.CharacterStatus);

            FPVector2 movement = newRelativePosition - lastRelativePosition;
            filter.Transform->Position += movement;

            if (filter.CharacterStatus->KnockbackStatusEffect.DurationTimer.IsRunning)
            {
                filter.CharacterStatus->KnockbackStatusEffect.KnockbackVelocity = movement / frame.DeltaTime;
            }
            else
            {
                filter.KCC->Velocity = filter.CharacterStatus->KnockbackStatusEffect.KnockbackVelocity;
            }
        }

        public void OnKnockbackApplied(Frame frame, EntityRef target, FP duration, FPVector2 dir, AssetRef<KnockbackStatusEffectData> statusEffectData)
        {
            CharacterStatusComponent* characterStatus = frame.Unsafe.GetPointer<CharacterStatusComponent>(target);
            
            characterStatus->KnockbackStatusEffect.DurationTimer.Start(duration);
            characterStatus->KnockbackStatusEffect.KnockbackDirection = dir.Normalized;
            characterStatus->KnockbackStatusEffect.StatusEffectData = statusEffectData;
        }
        
        private FPVector2 GetKnockbackRelativePosition(Frame frame, CharacterStatusComponent* characterStatus)
        {
            FP normalizedTime;
            
            KnockbackStatusEffectData data = frame.FindAsset<KnockbackStatusEffectData>(characterStatus->KnockbackStatusEffect.StatusEffectData.Id);

            normalizedTime = characterStatus->KnockbackStatusEffect.DurationTimer.NormalizedTime;
            FP normalizedPositionX = data.KnockbackCurveX.Evaluate(normalizedTime);
            FP normalizedPositionY = data.KnockbackCurveY.Evaluate(normalizedTime);
            
            FPVector2 relativePosition = new FPVector2(
                characterStatus->KnockbackStatusEffect.KnockbackDirection.X * data.KnockbackDistanceX * normalizedPositionX,
                characterStatus->KnockbackStatusEffect.KnockbackDirection.Y * data.KnockbackDistanceY * normalizedPositionY
            );

            return relativePosition;
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