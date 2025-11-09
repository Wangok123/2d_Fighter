using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct HitReactionComponent
    {
        public bool CanAct => !IsHitstunned;

        public bool CanMove => !IsHitstunned;

        public void ApplyKnockback(Frame frame, EntityRef self, FPVector2 velocity, FP duration)
        {
            frame.Signals.OnKnockbackApplied(self, duration, velocity);
        }

        public void ApplyHitstun(Frame frame, EntityRef self, FP duration)
        {
            frame.Signals.OnHitstunApplied(self, duration);
        }
    }
}