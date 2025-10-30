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
            input = FilterInputByUnlocks(frame, ref filter, input);
            
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
        
        private KCC2DSettings? GetModifiedSettingsByUnlocks(Frame frame, ref Filter filter)
        {
            // If no level or attack data, use default settings
            if (filter.Level == null || filter.AttackData == null)
            {
                return null;
            }
            
            // Get attack config to check unlock levels
            var attackConfig = frame.FindAsset(filter.AttackData->AttackConfig);
            if (attackConfig == null)
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
            
            // Check double jump unlock
            bool doubleJumpUnlocked = filter.Level->CurrentLevel >= attackConfig.DoubleJumpUnlockLevel;
            if (!doubleJumpUnlocked)
            {
                settings.DoubleJumpEnabled = false;
            }
            
            return settings;
        }
        
        private SimpleInput2D FilterInputByUnlocks(Frame frame, ref Filter filter, SimpleInput2D input)
        {
            // If no level or attack data, allow all inputs
            if (filter.Level == null || filter.AttackData == null)
            {
                return input;
            }
            
            // Get attack config to check unlock levels
            var attackConfig = frame.FindAsset(filter.AttackData->AttackConfig);
            if (attackConfig == null)
            {
                return input;
            }
            
            // Check dash unlock
            bool dashUnlocked = filter.Level->CurrentLevel >= attackConfig.DashUnlockLevel;
            
            // Filter dash input if not unlocked
            if (!dashUnlocked && input.Dash.WasPressed)
            {
                // Clear dash button state
                input.Dash = default;
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
