using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Base class for all attack configurations.
    /// Defines common properties shared by all attack types.
    /// Inherit from this class to create specific attack configurations (Light, Heavy, Special, etc.)
    /// </summary>
    public abstract class AttackConfig : AssetObject
    {
        [Header("Common Attack Properties")]
        [Tooltip("Priority for this attack type (higher value = higher priority)")]
        public int Priority = 50;
        
        [Tooltip("Base damage for this attack")]
        public FP Damage = 10;
        
        [Tooltip("Cooldown after using this attack")]
        public FP Cooldown = FP._0_50;
    }
}
