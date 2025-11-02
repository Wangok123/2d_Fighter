namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Example system demonstrating how to use KCC2D + AbilityEnable integration.
    /// This system shows runtime ability control based on game conditions.
    /// </summary>
    public unsafe class AbilityControlExampleSystem : SystemMainThreadFilter<AbilityControlExampleSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public AbilityEnable* AbilityEnable;
            public CharacterLevel* Level;
            public CharacterStatus* Status;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Example 1: Disable all abilities when stunned
            if (filter.Status->IsStunned)
            {
                // Temporarily disable all abilities
                KCCAbilityIntegration.DisableAllAbilities(filter.AbilityEnable);
                return;
            }

            // Example 2: Level-based ability unlocking
            UnlockAbilitiesByLevel(frame, ref filter);

            // Example 3: Conditional ability enabling (e.g., power-up system)
            // This would typically check for power-up components, buffs, etc.
            HandleConditionalAbilities(frame, ref filter);
        }

        private void UnlockAbilitiesByLevel(Frame frame, ref Filter filter)
        {
            // Enable basic movement from level 1
            if (filter.Level->CurrentLevel >= 1)
            {
                KCCAbilityIntegration.SetAbilityEnabled(
                    filter.AbilityEnable, 
                    AbilityId.AttackLight, 
                    true
                );
            }

            // Enable dash at level 2
            if (filter.Level->CurrentLevel >= 2)
            {
                KCCAbilityIntegration.SetAbilityEnabled(
                    filter.AbilityEnable, 
                    AbilityId.MovementDash, 
                    true
                );
            }

            // Enable double jump at level 3
            if (filter.Level->CurrentLevel >= 3)
            {
                KCCAbilityIntegration.SetAbilityEnabled(
                    filter.AbilityEnable, 
                    AbilityId.MovementDoubleJump, 
                    true
                );
            }

            // Enable wall jump at level 4
            if (filter.Level->CurrentLevel >= 4)
            {
                KCCAbilityIntegration.SetAbilityEnabled(
                    filter.AbilityEnable, 
                    AbilityId.MovementWallJump, 
                    true
                );
            }

            // Enable air dash at level 5
            if (filter.Level->CurrentLevel >= 5)
            {
                KCCAbilityIntegration.SetAbilityEnabled(
                    filter.AbilityEnable, 
                    AbilityId.MovementAirDash, 
                    true
                );
            }

            // Enable heavy attacks at level 6
            if (filter.Level->CurrentLevel >= 6)
            {
                KCCAbilityIntegration.SetAbilityEnabled(
                    filter.AbilityEnable, 
                    AbilityId.AttackHeavy, 
                    true
                );
            }

            // Enable special abilities at level 10
            if (filter.Level->CurrentLevel >= 10)
            {
                KCCAbilityIntegration.SetAbilityEnabled(
                    filter.AbilityEnable, 
                    AbilityId.SpecialUltimate, 
                    true
                );
            }
        }

        private void HandleConditionalAbilities(Frame frame, ref Filter filter)
        {
            // Example: You could check for power-up components here
            // For example, a "Flight" power-up could temporarily enable glide
            
            // if (frame.Unsafe.TryGetPointer<PowerUpData>(filter.Entity, out var powerUp))
            // {
            //     if (powerUp->HasFlightPowerUp)
            //     {
            //         KCCAbilityIntegration.SetAbilityEnabled(
            //             filter.AbilityEnable, 
            //             AbilityId.MovementGlide, 
            //             true
            //         );
            //     }
            // }
        }

        /// <summary>
        /// Example of checking ability state before allowing an action.
        /// This could be used in other systems.
        /// </summary>
        public static bool CanUseAbility(Frame frame, EntityRef entity, AbilityId abilityId)
        {
            return KCCAbilityIntegration.IsAbilityEnabled(frame, entity, abilityId);
        }
    }

    /// <summary>
    /// Example signal handler for ability state changes.
    /// This demonstrates the Sports Arena Brawler style of signal-based architecture.
    /// </summary>
    public unsafe class AbilitySignalExampleSystem : SystemSignalsOnly, ISignalOnKCC2DAfterState
    {
        public void OnKCC2DAfterState(Frame frame, EntityRef entity, KCC2D* kcc, ref KCC2DSettings settings)
        {
            // Example: Automatically enable certain abilities based on KCC state
            
            if (!frame.Unsafe.TryGetPointer<AbilityEnable>(entity, out var abilityEnable))
            {
                return;
            }

            // Example: Disable air dash when grounded (force one air dash per jump)
            if (kcc->IsGrounded)
            {
                // Reset air dash usage - this would be tracked in a custom extended config
                // See ExtendedKCC2DConfig for implementation example
            }

            // Example: Enable/disable abilities based on state
            switch (kcc->State)
            {
                case KCCState.GROUNDED:
                    // On ground, all abilities available
                    break;

                case KCCState.WALLED:
                    // On wall, only wall jump available
                    // You could disable other abilities here if needed
                    break;

                case KCCState.DASHING:
                    // During dash, disable certain abilities
                    // This prevents ability spam during dash
                    break;
            }
        }
    }
}
