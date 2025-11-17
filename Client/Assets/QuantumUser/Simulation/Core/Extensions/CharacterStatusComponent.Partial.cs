namespace Quantum
{
    public partial struct CharacterStatusComponent
    {
        public bool IsRespawning => false; // Placeholder for respawn logic
        
        public bool IsKnockedBack => KnockbackStatusEffect.DurationTimer.IsRunning;
        public bool IsIncapacitated => IsKnockedBack;
    }
}