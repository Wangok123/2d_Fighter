using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Main attack configuration that composes different attack type configs.
    /// Each attack type has its own dedicated configuration for better modularity.
    /// </summary>
    public class AttackConfig : AssetObject
    {
        [Header("Attack Type Configurations")]
        [Tooltip("Configuration for light attacks")]
        public AssetRef<LightAttackConfig> LightAttackConfig;
        
        [Tooltip("Configuration for heavy attacks")]
        public AssetRef<HeavyAttackConfig> HeavyAttackConfig;
        
        [Tooltip("Configuration for command input system")]
        public AssetRef<CommandInputConfig> CommandInputConfig;
        
        [Header("Unlock Settings")]
        [Tooltip("Level required to unlock double jump")]
        public int DoubleJumpUnlockLevel = 5;
        
        [Tooltip("Level required to unlock dash")]
        public int DashUnlockLevel = 5;
    }
}
