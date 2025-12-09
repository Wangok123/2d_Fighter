using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct ChargeAttackRuntimeComponent
    {
        public void ResetToDefault()
        {
            CurrentKnockbackMultiplier = FP._1;
        }
    }
}
