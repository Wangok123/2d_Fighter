namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Simple system to handle character leveling
    /// In a real game, this would be based on XP, kills, etc.
    /// For testing, we can trigger level-ups manually or via conditions
    /// </summary>
    public unsafe class LevelUpSystem : SystemMainThreadFilter<LevelUpSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterLevel* Level;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // This system can be extended to handle XP tracking and automatic level-ups
            // For now, it's a placeholder that can be called from other systems
        }

        /// <summary>
        /// Manually increase character level
        /// </summary>
        public static void LevelUpCharacter(Frame frame, EntityRef entity, int levelsToAdd = 1)
        {
            if (!frame.Unsafe.TryGetPointer(entity, out CharacterLevel* level))
            {
                return;
            }

            int oldLevel = level->CurrentLevel;
            level->CurrentLevel += levelsToAdd;
            
            // Fire level up event
            frame.Events.LevelUp(entity, level->CurrentLevel, oldLevel);
            
            Log.Info($"Character leveled up! Old Level: {oldLevel}, New Level: {level->CurrentLevel}");
        }

        /// <summary>
        /// Set character level directly
        /// </summary>
        public static void SetCharacterLevel(Frame frame, EntityRef entity, int newLevel)
        {
            if (!frame.Unsafe.TryGetPointer(entity, out CharacterLevel* level))
            {
                return;
            }

            if (newLevel == level->CurrentLevel)
            {
                return;
            }

            int oldLevel = level->CurrentLevel;
            level->CurrentLevel = newLevel;
            
            // Fire level up event
            frame.Events.LevelUp(entity, level->CurrentLevel, oldLevel);
            
            Log.Info($"Character level changed! Old Level: {oldLevel}, New Level: {level->CurrentLevel}");
        }
    }
}
