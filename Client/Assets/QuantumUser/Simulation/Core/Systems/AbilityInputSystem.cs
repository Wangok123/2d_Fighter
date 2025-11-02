namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Signal-based system that processes ability inputs
    /// Fires ability execution signals based on player input
    /// </summary>
    public unsafe class AbilityInputSystem : SystemMainThreadFilter<AbilityInputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterStatus* Status;
            public AttackData* AttackData;
            public PlayerLink* PlayerLink;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Skip if dead
            if (filter.Status->IsDead)
            {
                return;
            }

            // Get modular config
            if (!filter.AttackData->ModularConfig.Id.IsValid)
            {
                return; // No modular config, skip
            }

            // Get input
            SimpleInput2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);

            // Update timers
            UpdateAbilityTimers(frame, ref filter);

            // Early return if on cooldown
            if (filter.AttackData->AttackCooldown.IsRunning(frame))
            {
                filter.AttackData->IsAttacking = false;
                return;
            }

            // Fire ability execute signal for other systems to handle
            // The signal will be processed by specialized ability execution systems
            frame.Signals.OnAbilityExecute(filter.Entity, AbilityId.None, input);
        }

        private void UpdateAbilityTimers(Frame frame, ref Filter filter)
        {
            // Reset combo if timer expired
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }
        }
    }
}
