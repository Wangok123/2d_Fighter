using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Configuration for heavy attack with charging mechanics.
    /// Heavy attacks deal high damage and can be charged for additional power.
    /// Inherits common attack properties from AttackConfig base class.
    /// </summary>
    public class HeavyAttackConfig : AttackConfig
    {
        [Header("Charge System")]
        [Tooltip("Maximum charge time")]
        public FP MaxChargeTime = FP._2;
        
        [Tooltip("Minimum charge time to get bonus damage")]
        public FP MinChargeTime = FP._0_50;
        
        [Tooltip("Damage multiplier at full charge")]
        public FP FullChargeDamageMultiplier = FP._2;

        public HeavyAttackConfig()
        {
            // Set default values for heavy attacks
            Priority = 50;
            Damage = 25;
            Cooldown = FP._0_50;
        }
    }
}
