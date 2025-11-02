// DEPRECATED: This system has been replaced by signal-based systems:
// - MovementInputSystem: Handles player input processing
// - MovementExecutionSystem: Executes movement with KCC
// - MovementUnlockSystem: Validates ability unlocks
// This file is kept for backward compatibility but should not be used in new code.

namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PlayerLink* PlayerLink;
            public CharacterStatus* Status;
            public MovementData* MovementData;
            public KCC2D* KCC;
            public CharacterLevel* Level;
            public AttackData* AttackData;
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Status->IsDead == true)
            {
                return;
            }

            SimpleInput2D input = default;
            if(frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = *frame.GetPlayerInput(playerLink->Player);
            }
            
            // Apply ability unlock filtering
            input = FilterInputByUnlocks(frame, filter, input);
            
            var config = frame.FindAsset(filter.KCC->Config);
            filter.KCC->Input = input;
            
            // Get modified settings based on unlock status
            KCC2DSettings? modifiedSettings = GetModifiedSettingsByUnlocks(frame, ref filter);
            
            // Move with modified settings if needed
            if (modifiedSettings.HasValue)
            {
                config.Move(frame, filter.Entity, filter.Transform, filter.KCC, modifiedSettings.Value);
            }
            else
            {
                config.Move(frame, filter.Entity, filter.Transform, filter.KCC);
            }
            
            UpdateIsFacingRight(frame, ref filter, input);
        }
        
        /// <summary>
        /// Try to get modular character config from entity
        /// </summary>
        private ModularCharacterConfig TryGetModularConfig(Frame frame, ref Filter filter)
        {
            // Note: AttackData is part of the filter, so it's guaranteed to exist
            if (filter.AttackData->ModularConfig.Id.IsValid)
            {
                return frame.FindAsset(filter.AttackData->ModularConfig);
            }
            return null;
        }
        
        private KCC2DSettings? GetModifiedSettingsByUnlocks(Frame frame, ref Filter filter)
        {
            // If no level or attack data, use default settings
            if (filter.Level == null || filter.AttackData == null)
            {
                return null;
            }
            
            // Get modular config
            var modularConfig = TryGetModularConfig(frame, ref filter);
            if (modularConfig != null)
            {
                return GetModifiedSettingsFromModularConfig(frame, ref filter, modularConfig);
            }
            
            return null;
        }
        
        private KCC2DSettings? GetModifiedSettingsFromModularConfig(Frame frame, ref Filter filter, ModularCharacterConfig modularConfig)
        {
            // Get base KCC config
            var kccConfig = frame.FindAsset(filter.KCC->Config);
            if (kccConfig == null)
            {
                return null;
            }
            
            // Create modified settings
            KCC2DSettings settings = default;
            kccConfig.BaseSettings.Materialize(frame, ref settings);
            
            // Check if double jump ability is unlocked based on modular config 
            bool doubleJumpUnlocked = IsAbilityUnlocked(filter.Level, modularConfig, AbilityId.MovementDoubleJump);
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
        
        private SimpleInput2D FilterInputByUnlocks(Frame frame, Filter filter, SimpleInput2D input)
        {
            // If no level or attack data, allow all inputs
            if (filter.Level == null || filter.AttackData == null)
            {
                return input;
            }
            
            // Get modular config
            var modularConfig = TryGetModularConfig(frame, ref filter);
            if (modularConfig != null)
            {
                // Check dash unlock
                bool dashUnlocked = IsAbilityUnlocked(filter.Level, modularConfig, AbilityId.MovementDash);
                
                // Filter dash input if not unlocked
                if (!dashUnlocked && input.Dash.WasPressed)
                {
                    input.Dash = default;
                }
                
                return input;
            }
            
            return input;
        }
        
        private void UpdateIsFacingRight(Frame frame, ref Filter filter, SimpleInput2D input)
        {
            bool noInput = !input.Left.IsDown && !input.Right.IsDown;
            if (noInput)
                return;
            
            filter.MovementData->IsFacingRight = input.Right.IsDown;
        }
    }
}
