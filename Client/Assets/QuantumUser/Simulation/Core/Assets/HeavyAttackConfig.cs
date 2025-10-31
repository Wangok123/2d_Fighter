using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Configuration for heavy attack with charging mechanics.
    /// Heavy attacks deal high damage and can be charged for additional power.
    /// </summary>
    public class HeavyAttackConfig : AssetObject
    {
        [Header("Priority")]
        [Tooltip("Priority for heavy attack (higher value = higher priority)")]
        public int Priority = 50;
        
        [Header("Basic Settings")]
        [Tooltip("Base damage for heavy attack")]
        public FP Damage = 25;
        
        [Tooltip("Cooldown for heavy attack")]
        public FP Cooldown = FP._0_50;
        
        [Header("Charge System")]
        [Tooltip("Maximum charge time")]
        public FP MaxChargeTime = FP._2;
        
        [Tooltip("Minimum charge time to get bonus damage")]
        public FP MinChargeTime = FP._0_50;
        
        [Tooltip("Damage multiplier at full charge")]
        public FP FullChargeDamageMultiplier = FP._2;
    }
}
