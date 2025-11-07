using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct HitReactionComponent
    {
        public bool CanAct => !IsDead && !IsHitstunned && !IsStunned;

        public bool CanMove => !IsDead && !IsStunned;

        public bool CanBeHit(Frame frame)
        {
            return !IsDead && !InvincibilityTimer.IsRunning(frame);
        }

        public FP GetHealthPercent()
        {
            if (IsDead || MaxHealth <= 0) return FP._0;
            return CurrentHealth / MaxHealth;
        }

        public void TakeDamage(Frame frame, EntityRef self, EntityRef attacker, FP damage, HitType hitType = HitType.Medium)
        {
            frame.Signals.OnDamageTaken(self, attacker, damage, hitType);
        }

        public void ApplyKnockback(Frame frame, EntityRef self, FPVector2 velocity, FP duration)
        {
            frame.Signals.OnKnockbackApplied(self, duration, velocity);
        }

        public void ApplyStun(Frame frame, EntityRef self, FP duration)
        {
            frame.Signals.OnStunApplied(self, duration);
        }

        public void Kill(Frame frame, EntityRef self, EntityRef killer)
        {
            frame.Signals.OnEntityDied(self, killer);
        }

        public void SetInvincibility(Frame frame, FP duration)
        {
            InvincibilityTimer = FrameTimer.FromSeconds(frame, duration);
        }
    }
}