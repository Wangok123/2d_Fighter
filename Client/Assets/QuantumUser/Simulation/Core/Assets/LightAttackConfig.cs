using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Configuration for light attack with combo system.
    /// Light attacks are fast, low-damage attacks that can chain into combos.
    /// </summary>
    public class LightAttackConfig : AssetObject
    {
        [Header("Priority")]
        [Tooltip("Priority for light attack (higher value = higher priority)")]
        public int Priority = 10;
        
        [Header("Basic Settings")]
        [Tooltip("Base damage for light attack")]
        public FP Damage = 10;
        
        [Tooltip("Cooldown between light attacks")]
        public FP Cooldown = FP._0_25;
        
        [Header("Combo System")]
        [Tooltip("Time window for combo continuation")]
        public FP ComboWindow = FP._1;
        
        [Tooltip("Maximum combo count")]
        public int MaxComboCount = 3;
        
        [Tooltip("Damage multiplier per combo level")]
        public FP[] ComboDamageMultipliers = new FP[] { FP._1, FP._1_25, FP._1_50 };
    }
}
