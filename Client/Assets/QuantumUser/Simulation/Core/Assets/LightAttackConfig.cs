using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Configuration for light attack with combo system.
    /// Light attacks are fast, low-damage attacks that can chain into combos.
    /// Inherits common attack properties from AttackConfig base class.
    /// </summary>
    public class LightAttackConfig : AttackConfig
    {
        [Header("Combo System")]
        [Tooltip("Time window for combo continuation")]
        public FP ComboWindow = FP._1;
        
        [Tooltip("Maximum combo count")]
        public int MaxComboCount = 3;
        
        [Tooltip("Damage multiplier per combo level")]
        public FP[] ComboDamageMultipliers = new FP[] { FP._1, FP._1_25, FP._1_50 };

        public LightAttackConfig()
        {
            // Set default values for light attacks
            Priority = 10;
            Damage = 10;
            Cooldown = FP._0_25;
        }
    }
}
