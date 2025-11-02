namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Lightweight system that handles player input and movement execution
    /// Replaces the monolithic MovementSystem with better separation of concerns
    /// </summary>
    public unsafe class MovementInputSystem : SystemMainThreadFilter<MovementInputSystem.Filter>
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

            // Get input
            SimpleInput2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);
            
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
            
            UpdateIsFacingRight(input, filter.MovementData);
        }
        
        private KCC2DSettings? GetModifiedSettingsByUnlocks(Frame frame, ref Filter filter)
        {
            // If no level or attack data, use default settings
            if (filter.Level == null || filter.AttackData == null)
            {
                return null;
            }
            
            // Get modular config
            if (!filter.AttackData->ModularConfig.Id.IsValid)
            {
                return null;
            }

            var modularConfig = frame.FindAsset(filter.AttackData->ModularConfig);
            if (modularConfig == null)
            {
                return null;
            }
            
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
            if (!filter.AttackData->ModularConfig.Id.IsValid)
            {
                return input;
            }

            var modularConfig = frame.FindAsset(filter.AttackData->ModularConfig);
            if (modularConfig == null)
            {
                return input;
            }
            
            // Check dash unlock
            bool dashUnlocked = IsAbilityUnlocked(filter.Level, modularConfig, AbilityId.MovementDash);
            
            // Filter dash input if not unlocked
            if (!dashUnlocked && input.Dash.WasPressed)
            {
                input.Dash = default;
            }
            
            return input;
        }
        
        private void UpdateIsFacingRight(SimpleInput2D input, MovementData* movementData)
        {
            bool noInput = !input.Left.IsDown && !input.Right.IsDown;
            if (noInput)
                return;
            
            // When both directions are pressed, maintain current facing direction
            if (input.Left.IsDown && input.Right.IsDown)
                return;
            
            movementData->IsFacingRight = input.Right.IsDown;
        }
    }
}
