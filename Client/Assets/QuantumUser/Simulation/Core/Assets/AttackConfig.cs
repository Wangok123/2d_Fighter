using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    public class AttackConfig : AssetObject
    {
        [Header("Attack Priority Settings")]
        [Tooltip("Priority for special moves (higher value = higher priority)")]
        public int SpecialMovePriority = 100;
        
        [Tooltip("Priority for heavy attack (higher value = higher priority)")]
        public int HeavyAttackPriority = 50;
        
        [Tooltip("Priority for light attack (higher value = higher priority)")]
        public int LightAttackPriority = 10;
        
        [Header("Light Attack Settings")]
        [Tooltip("Damage for light attack")]
        public FP LightAttackDamage = 10;
        
        [Tooltip("Cooldown between light attacks")]
        public FP LightAttackCooldown = FP._0_25;
        
        [Tooltip("Time window for combo continuation")]
        public FP ComboWindow = FP._1;
        
        [Tooltip("Max combo count for light attacks")]
        public int MaxComboCount = 3;
        
        [Tooltip("Damage multiplier per combo level")]
        public FP[] ComboDamageMultipliers = new FP[] { FP._1, FP._1_25, FP._1_50 };
        
        [Header("Heavy Attack Settings")]
        [Tooltip("Damage for heavy attack")]
        public FP HeavyAttackDamage = 25;
        
        [Tooltip("Cooldown for heavy attack")]
        public FP HeavyAttackCooldown = FP._0_50;
        
        [Header("Heavy Charge Settings")]
        [Tooltip("Maximum charge time for heavy attack")]
        public FP MaxChargeTime = FP._2;
        
        [Tooltip("Minimum charge time to get bonus damage")]
        public FP MinChargeTime = FP._0_50;
        
        [Tooltip("Damage multiplier at full charge")]
        public FP FullChargeDamageMultiplier = FP._2;
        
        [Header("Command Input Settings")]
        [Tooltip("Time window for command input sequence")]
        public FP CommandInputWindow = FP._0_50;
        
        [Tooltip("Maximum number of inputs to track")]
        public int MaxInputBufferSize = 8;
        
        [Header("Unlock Settings")]
        [Tooltip("Level required to unlock double jump")]
        public int DoubleJumpUnlockLevel = 5;
        
        [Tooltip("Level required to unlock dash")]
        public int DashUnlockLevel = 5;
    }
}
