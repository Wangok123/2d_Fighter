using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Modular character configuration using ECS-style ability composition.
    /// Inspired by Overwatch's GDC presentation on hero creation.
    /// 
    /// This approach allows:
    /// 1. Creating characters by composing reusable ability components
    /// 2. Easy addition of new characters by mixing existing abilities
    /// 3. Minimal code changes when adding new heroes
    /// 4. Clear separation of concerns (each ability is independent)
    /// 
    /// Example workflow:
    /// - First hero: Create multiple ability components (movement, attacks, defense, specials)
    /// - Second hero: Reuse some abilities, create a few new ones, compose character
    /// - Third+ heroes: Mostly just compose from existing abilities
    /// 
    /// This maintains compatibility with Quantum's deterministic execution model.
    /// </summary>
    public class ModularCharacterConfig : AssetObject
    {
        [Header("Character Identity")]
        [Tooltip("Unique character ID")]
        public int CharacterId;
        
        [Tooltip("Character display name")]
        public string CharacterName;
        
        [Tooltip("Character description")]
        [TextArea(3, 5)]
        public string Description;
        
        [Header("Movement Abilities")]
        [Tooltip("Movement abilities this character has (walk, jump, dash, etc.)")]
        public AssetRef<MovementAbilityComponent>[] MovementAbilities;
        
        [Header("Attack Abilities")]
        [Tooltip("Attack abilities this character has (light attack, heavy attack, etc.)")]
        public AssetRef<AttackAbilityComponent>[] AttackAbilities;
        
        [Header("Defense Abilities")]
        [Tooltip("Defensive abilities this character has (block, parry, dodge, etc.)")]
        public AssetRef<DefenseAbilityComponent>[] DefenseAbilities;
        
        [Header("Special Abilities")]
        [Tooltip("Special/unique abilities this character has (ultimates, special moves, etc.)")]
        public AssetRef<SpecialAbilityComponent>[] SpecialAbilities;
        
        [Header("Passive Traits")]
        [Tooltip("Passive stat modifiers and traits")]
        public PassiveTraits PassiveTraits;
        
        [Header("Progression")]
        [Tooltip("Level-based ability unlocks")]
        public AbilityUnlock[] AbilityUnlocks;
        
        [Header("Legacy Support")]
        [Tooltip("Legacy attack config for backward compatibility (optional)")]
        public AssetRef<CharacterAttackConfig> LegacyAttackConfig;
    }
    
    /// <summary>
    /// Passive traits and stat modifiers for a character
    /// </summary>
    [System.Serializable]
    public struct PassiveTraits
    {
        [Tooltip("Base health multiplier")]
        public FP HealthMultiplier;
        
        [Tooltip("Base movement speed multiplier")]
        public FP SpeedMultiplier;
        
        [Tooltip("Base damage multiplier")]
        public FP DamageMultiplier;
        
        [Tooltip("Base defense multiplier")]
        public FP DefenseMultiplier;
        
        [Tooltip("Health regeneration per second")]
        public FP HealthRegeneration;
        
        [Tooltip("Energy regeneration per second")]
        public FP EnergyRegeneration;
    }
    
    /// <summary>
    /// Defines when abilities unlock as character levels up
    /// </summary>
    [System.Serializable]
    public struct AbilityUnlock
    {
        [Tooltip("Level at which this unlock occurs")]
        public int UnlockLevel;
        
        [Tooltip("Ability ID that gets unlocked")]
        public string AbilityId;
        
        [Tooltip("Description of what unlocks")]
        public string UnlockDescription;
    }
}
