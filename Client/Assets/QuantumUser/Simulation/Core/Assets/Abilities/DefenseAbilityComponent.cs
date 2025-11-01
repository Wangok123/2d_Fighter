using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Defensive ability component that defines protective capabilities.
    /// Examples: Block, parry, dodge, shield, counter, etc.
    /// </summary>
    public class DefenseAbilityComponent : AbilityComponentBase
    {
        [Header("Defense Type")]
        [Tooltip("Type of defense this ability provides")]
        public DefenseAbilityType DefenseType;
        
        [Header("Defense Properties")]
        [Tooltip("Damage reduction percentage (0-1, where 1 = 100% reduction)")]
        public FP DamageReduction = FP._0_50;
        
        [Tooltip("Duration of the defensive state")]
        public FP DefenseDuration = FP._1;
        
        [Tooltip("Can move while defending?")]
        public bool CanMoveWhileDefending = false;
        
        [Header("Perfect Defense")]
        [Tooltip("Has perfect defense window (complete damage negation)?")]
        public bool HasPerfectWindow = false;
        
        [Tooltip("Duration of perfect defense window")]
        public FP PerfectWindowDuration = FP._0_10;
        
        [Tooltip("Perfect defense must be timed (starts at ability activation)?")]
        public bool PerfectWindowAtStart = true;
        
        [Header("Counter Mechanics")]
        [Tooltip("Can counter after successful defense?")]
        public bool CanCounter = false;
        
        [Tooltip("Counter damage multiplier")]
        public FP CounterDamageMultiplier = FP._1_50;
        
        [Tooltip("Counter window duration after successful defense")]
        public FP CounterWindowDuration = FP._0_50;
        
        [Header("Resource Cost")]
        [Tooltip("Does defense consume stamina/energy continuously?")]
        public bool ContinuousCost = false;
        
        [Tooltip("Energy cost per second (if continuous)")]
        public FP EnergyCostPerSecond = FP._0_10;
        
        public DefenseAbilityComponent()
        {
            AbilityId = "defense_basic";
            AbilityName = "Basic Block";
            Priority = 30;
            DamageReduction = FP._0_50;
        }
    }
    
    /// <summary>
    /// Types of defensive abilities
    /// </summary>
    public enum DefenseAbilityType
    {
        Block,          // Simple block that reduces damage
        Parry,          // Perfect defense with timing window
        Dodge,          // Evasive maneuver that avoids damage
        Shield,         // Temporary shield that absorbs damage
        Counter,        // Defensive move that counterattacks
        Reflect,        // Reflects projectiles
        Invincibility,  // Brief invincibility frames
        Armor,          // Passive damage reduction
        Barrier,        // Deployable barrier
        Teleport        // Defensive teleport/phase
    }
}
