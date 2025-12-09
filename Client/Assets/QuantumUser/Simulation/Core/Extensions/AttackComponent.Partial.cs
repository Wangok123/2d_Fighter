using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct AttackComponent
    {
        public void ResetToDefault(Frame frame, EntityRef entity)
        {
            ComboCounter = 0;
            IsAttacking = false;
            IsChargingHeavy = false;
            HeavyChargeTime = FP._0;
            HasStartedHitboxWindow = false;
            KnockbackDirection = FPVector2.Zero;
            
            ComboResetTimer = FrameTimer.None;
            AttackCooldown = FrameTimer.None;
            ChargeTimer = FrameTimer.None;
            ComboWindowTimer = FrameTimer.None;
            
            if (frame.TryResolveList(HitEntitiesThisAttack, out var hitList))
            {
                hitList.Clear();
            }
        }
    }
}
