namespace Quantum
{
    public partial struct CharacterStatusComponent
    {
        public bool IsKnockedBack => KnockbackStatusEffect.DurationTimer.IsRunning;
        public bool IsIncapacitated => IsKnockedBack;
    }
}