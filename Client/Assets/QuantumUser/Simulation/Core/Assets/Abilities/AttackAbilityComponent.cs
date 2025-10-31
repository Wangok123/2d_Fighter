using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Attack ability component that defines offensive capabilities.
    /// Examples: Melee combo, projectile attack, area attack, charge attack, etc.
    /// 
    /// This replaces the individual attack configs and allows for more flexible composition.
    /// </summary>
    public class AttackAbilityComponent : AbilityComponentBase
    {
        [Header("Attack Type")]
        [Tooltip("Type of attack this ability provides")]
        public AttackAbilityType AttackType;
        
        [Header("Damage Properties")]
        [Tooltip("Base damage dealt by this attack")]
        public FP BaseDamage = 10;
        
        [Tooltip("Damage scaling per character level")]
        public FP DamagePerLevel = FP._0_50;
        
        [Header("Attack Mechanics")]
        [Tooltip("Range of the attack")]
        public FP AttackRange = FP._1;
        
        [Tooltip("Width/radius of the attack")]
        public FP AttackWidth = FP._0_50;
        
        [Tooltip("Attack startup time (frames before damage is dealt)")]
        public FP StartupTime = FP._0_10;
        
        [Tooltip("Attack active time (frames where attack can hit)")]
        public FP ActiveTime = FP._0_20;
        
        [Tooltip("Attack recovery time (frames before character can act again)")]
        public FP RecoveryTime = FP._0_30;
        
        [Header("Combo System")]
        [Tooltip("Can this attack chain into a combo?")]
        public bool CanCombo = false;
        
        [Tooltip("Maximum combo count")]
        public int MaxComboCount = 1;
        
        [Tooltip("Time window to continue combo")]
        public FP ComboWindow = FP._0_50;
        
        [Tooltip("Damage multiplier per combo stage")]
        public FP[] ComboDamageMultipliers = new FP[] { FP._1 };
        
        [Header("Charge System")]
        [Tooltip("Can this attack be charged?")]
        public bool CanCharge = false;
        
        [Tooltip("Minimum charge time for bonus damage")]
        public FP MinChargeTime = FP._0_50;
        
        [Tooltip("Maximum charge time")]
        public FP MaxChargeTime = FP._2;
        
        [Tooltip("Damage multiplier at full charge")]
        public FP FullChargeDamageMultiplier = FP._2;
        
        [Header("Projectile Settings (if applicable)")]
        [Tooltip("Is this a projectile attack?")]
        public bool IsProjectile = false;
        
        [Tooltip("Projectile speed")]
        public FP ProjectileSpeed = 10;
        
        [Tooltip("Projectile lifetime")]
        public FP ProjectileLifetime = FP._2;
        
        [Tooltip("Number of projectiles spawned")]
        public int ProjectileCount = 1;
        
        [Header("Status Effects")]
        [Tooltip("Does this attack apply a status effect?")]
        public bool AppliesStatusEffect = false;
        
        [Tooltip("Type of status effect (if any)")]
        public string StatusEffectType = "";
        
        [Tooltip("Duration of status effect")]
        public FP StatusEffectDuration = FP._1;
        
        public AttackAbilityComponent()
        {
            AbilityId = "attack_basic";
            AbilityName = "Basic Attack";
            Priority = 20;
            BaseDamage = 10;
        }
    }
    
    /// <summary>
    /// Types of attack abilities
    /// </summary>
    public enum AttackAbilityType
    {
        LightMelee,     // Fast, low damage melee attack
        HeavyMelee,     // Slow, high damage melee attack
        Projectile,     // Ranged projectile attack
        AreaOfEffect,   // Attack that hits an area
        Grab,           // Grab/throw attack
        ChargedShot,    // Charged projectile
        RapidFire,      // Multiple quick shots
        Uppercut,       // Launching attack
        GroundPound,    // Downward attack
        Counter         // Counterattack/parry
    }
}
