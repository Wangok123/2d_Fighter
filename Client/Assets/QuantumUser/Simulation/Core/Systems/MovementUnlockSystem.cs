namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Signal-based system that handles movement ability unlocks
    /// Listens to OnMovementInput signal and filters based on unlocked abilities
    /// </summary>
    public unsafe class MovementUnlockSystem : SystemSignalsOnly, ISignalOnMovementInput
    {
        public void OnMovementInput(Frame frame, EntityRef entity, SimpleInput2D input)
        {
            // Get required components
            if (!frame.Unsafe.TryGetPointer(entity, out AttackData* attackData))
            {
                return;
            }

            if (!frame.Unsafe.TryGetPointer(entity, out CharacterLevel* level))
            {
                return;
            }

            // Get modular config
            if (!attackData->ModularConfig.Id.IsValid)
            {
                return; // No config, allow all inputs
            }

            var modularConfig = frame.FindAsset(attackData->ModularConfig);
            if (modularConfig == null)
            {
                return;
            }

            // Check dash unlock
            bool dashUnlocked = IsAbilityUnlocked(level, modularConfig, AbilityId.MovementDash);
            
            // Filter dash input if not unlocked
            if (!dashUnlocked && input.Dash.WasPressed)
            {
                // Note: We would need to modify the input, but signals are read-only
                // This system validates but the actual filtering happens in MovementExecutionSystem
                Log.Debug($"Dash not unlocked for entity {entity}");
            }
        }

        private bool IsAbilityUnlocked(CharacterLevel* level, ModularCharacterConfig config, AbilityId abilityId)
        {
            if (config.AbilityUnlocks == null || config.AbilityUnlocks.Length == 0)
            {
                return true; // No unlock system, all abilities available
            }
            
            foreach (var unlock in config.AbilityUnlocks)
            {
                if (unlock.AbilityId == abilityId)
                {
                    return level->CurrentLevel >= unlock.UnlockLevel;
                }
            }
            
            return true; // Ability not in unlock list, assume unlocked
        }
    }
}
