using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct ComboAttackRuntimeComponent
    {
        public void ResetToDefault()
        {
            CurrentComboStep = 0;
            CurrentHitboxActiveTime = FP._0;
            CurrentHitboxActiveDuration = FP._0;
            CurrentKnockbackStatusEffectData = default;
        }
    }
}
