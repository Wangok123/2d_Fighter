using Photon.Deterministic;

namespace Quantum
{
    public partial struct KnockbackComponent
    {
        public bool IsKnockedBack => StatusEffect.DurationTimer.IsRunning;
        
        public FP KnockbackProgress => StatusEffect.DurationTimer.NormalizedTime;

        public FP RemainingKnockbackTime => StatusEffect.DurationTimer.TimeLeft;
    }
}