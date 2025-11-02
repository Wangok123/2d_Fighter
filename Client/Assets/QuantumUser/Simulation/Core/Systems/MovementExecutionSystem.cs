namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Signal-based system that executes movement based on input
    /// Listens to OnMovementInput signal and applies KCC movement
    /// </summary>
    public unsafe class MovementExecutionSystem : SystemSignalsOnly, ISignalOnMovementInput
    {
        public void OnMovementInput(Frame frame, EntityRef entity, SimpleInput2D input)
        {
            // Get required components
            if (!frame.Unsafe.TryGetPointer(entity, out Transform2D* transform))
            {
                return;
            }

            if (!frame.Unsafe.TryGetPointer(entity, out KCC2D* kcc))
            {
                return;
            }

            if (!frame.Unsafe.TryGetPointer(entity, out MovementData* movementData))
            {
                return;
            }

            // Apply input filtering based on unlocks
            input = FilterInputByUnlocks(frame, entity, input);
            
            // Set KCC input
            kcc->Input = input;
            
            // Get modified settings based on unlock status
            KCC2DSettings? modifiedSettings = GetModifiedSettingsByUnlocks(frame, entity);
            
            // Get KCC config
            var config = frame.FindAsset(kcc->Config);
            if (config == null)
            {
                return;
            }
            
            // Move with modified settings if needed
            if (modifiedSettings.HasValue)
            {
                config.Move(frame, entity, transform, kcc, modifiedSettings.Value);
            }
            else
            {
                config.Move(frame, entity, transform, kcc);
            }
            
            // Update facing direction
            UpdateIsFacingRight(input, movementData);
        }

        private SimpleInput2D FilterInputByUnlocks(Frame frame, EntityRef entity, SimpleInput2D input)
        {
            // Get level and attack data
            if (!frame.Unsafe.TryGetPointer(entity, out CharacterLevel* level))
            {
                return input;
            }

            if (!frame.Unsafe.TryGetPointer(entity, out AttackData* attackData))
            {
                return input;
            }

            // Get modular config
            if (!attackData->ModularConfig.Id.IsValid)
            {
                return input; // No config, allow all inputs
            }

            var modularConfig = frame.FindAsset(attackData->ModularConfig);
            if (modularConfig == null)
            {
                return input;
            }

            // Check dash unlock
            bool dashUnlocked = IsAbilityUnlocked(level, modularConfig, AbilityId.MovementDash);
            
            // Filter dash input if not unlocked
            if (!dashUnlocked && input.Dash.WasPressed)
            {
                input.Dash = default;
            }
            
            return input;
        }

        private KCC2DSettings? GetModifiedSettingsByUnlocks(Frame frame, EntityRef entity)
        {
            // Get level and attack data
            if (!frame.Unsafe.TryGetPointer(entity, out CharacterLevel* level))
            {
                return null;
            }

            if (!frame.Unsafe.TryGetPointer(entity, out AttackData* attackData))
            {
                return null;
            }

            if (!frame.Unsafe.TryGetPointer(entity, out KCC2D* kcc))
            {
                return null;
            }

            // Get modular config
            if (!attackData->ModularConfig.Id.IsValid)
            {
                return null;
            }

            var modularConfig = frame.FindAsset(attackData->ModularConfig);
            if (modularConfig == null)
            {
                return null;
            }

            // Get base KCC config
            var kccConfig = frame.FindAsset(kcc->Config);
            if (kccConfig == null)
            {
                return null;
            }
            
            // Create modified settings
            KCC2DSettings settings = default;
            kccConfig.BaseSettings.Materialize(frame, ref settings);
            
            // Check if double jump ability is unlocked based on modular config 
            bool doubleJumpUnlocked = IsAbilityUnlocked(level, modularConfig, AbilityId.MovementDoubleJump);
            if (!doubleJumpUnlocked)
            {
                settings.DoubleJumpEnabled = false;
            }
            
            return settings;
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

        private void UpdateIsFacingRight(SimpleInput2D input, MovementData* movementData)
        {
            bool noInput = !input.Left.IsDown && !input.Right.IsDown;
            if (noInput)
            {
                return;
            }
            
            movementData->IsFacingRight = input.Right.IsDown;
        }
    }
}
