using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Special ability component for unique, character-defining abilities.
    /// Examples: Ultimate abilities, transformation, summons, special moves, etc.
    /// 
    /// These are typically the abilities that make characters unique and interesting.
    /// </summary>
    public class SpecialAbilityComponent : AbilityComponentBase
    {
        [Header("Special Ability Type")]
        [Tooltip("Type of special ability")]
        public SpecialAbilityType SpecialType;
        
        [Header("Activation")]
        [Tooltip("Input sequence required (for command inputs like fighting games)")]
        public int[] InputSequence;
        
        [Tooltip("Can be activated at any time?")]
        public bool CanActivateAnytime = false;
        
        [Tooltip("Can only be used on ground?")]
        public bool GroundOnly = true;
        
        [Header("Special Properties")]
        [Tooltip("Damage dealt (if applicable)")]
        public FP Damage = 30;
        
        [Tooltip("Healing provided (if applicable)")]
        public FP HealingAmount = 0;
        
        [Tooltip("Duration of the special ability effect")]
        public FP EffectDuration = FP._2;
        
        [Tooltip("Range/radius of effect")]
        public FP EffectRange = FP._3;
        
        [Header("Ultimate Charge System")]
        [Tooltip("Is this an ultimate ability that requires charging?")]
        public bool IsUltimate = false;
        
        [Tooltip("Charge required to activate (0-100)")]
        public FP RequiredCharge = 100;
        
        [Tooltip("Charge gained per damage dealt")]
        public FP ChargePerDamage = FP._0_50;
        
        [Tooltip("Charge gained per damage taken")]
        public FP ChargePerDamageTaken = FP._1;
        
        [Header("Transformation")]
        [Tooltip("Does this ability transform the character?")]
        public bool IsTransformation = false;
        
        [Tooltip("Transform duration")]
        public FP TransformDuration = 10;
        
        [Tooltip("Stat multiplier during transformation")]
        public FP TransformStatMultiplier = FP._1_50;
        
        [Header("Summon/Deployable")]
        [Tooltip("Does this ability summon or deploy something?")]
        public bool IsSummon = false;
        
        [Tooltip("Number of summons/deployables")]
        public int SummonCount = 1;
        
        [Tooltip("Summon lifetime")]
        public FP SummonLifetime = 10;
        
        public SpecialAbilityComponent()
        {
            AbilityId = "special_basic";
            AbilityName = "Special Move";
            Priority = 100;
            Damage = 30;
            Cooldown = FP._3;
        }
    }
    
    /// <summary>
    /// Types of special abilities
    /// </summary>
    public enum SpecialAbilityType
    {
        Ultimate,           // Ultimate/super ability
        Transformation,     // Transform into different state
        Summon,            // Summon helper/minion
        AreaDamage,        // Large area damage ability
        Healing,           // Self or area healing
        Buff,              // Temporary stat boost
        Debuff,            // Enemy stat reduction
        Teleport,          // Long-range teleport
        TimeManipulation,  // Slow time, rewind, etc.
        Deployable,        // Place a turret/trap/etc.
        CommandGrab,       // Special grab/throw
        Projectile,        // Special projectile attack
        ScreenClear,       // Clear all enemy projectiles
        Resurrection,      // Revive/second chance
        Combo              // Special combo sequence
    }
}
