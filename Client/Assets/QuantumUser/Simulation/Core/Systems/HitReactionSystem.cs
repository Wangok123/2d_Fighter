using Photon.Deterministic;

namespace Quantum
{
    public unsafe class HitReactionSystem : SystemMainThreadFilter<HitReactionSystem.Filter>,
        ISignalOnKnockbackApplied,
        ISignalOnHitstunApplied
    {
        public struct Filter
        {
            public EntityRef Entity;
            public HitReactionComponent* HitReaction;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.HitReaction->IsHitstunned && !filter.HitReaction->IsKnockedBack)
                return;

            HitReactionData data = frame.FindAsset<HitReactionData>(filter.HitReaction->HitReactionData);
            data.UpdateHitReaction(frame, filter.Entity, filter.HitReaction);
        }

        public void OnKnockbackApplied(Frame frame, EntityRef target, FP duration, FPVector2 knockbackVelocity)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            HitReactionData data = frame.FindAsset<HitReactionData>(hitReaction->HitReactionData);
            data.OnKnockbackApplied(frame, target, hitReaction, duration, knockbackVelocity);
        }

        public void OnHitstunApplied(Frame frame, EntityRef target, FP duration)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            HitReactionData data = frame.FindAsset<HitReactionData>(hitReaction->HitReactionData);
            data.OnHitstunApplied(frame, target, hitReaction, duration, HitType.Light);
        }
    }
}