using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Container configuration that holds references to all attack type configurations for a character.
    /// This allows each character to have different combinations of attack configs.
    /// </summary>
    public class CharacterAttackConfig : AssetObject
    {
        [Header("Attack Configurations")]
        [Tooltip("Configuration for light attacks")]
        public AssetRef<LightAttackConfig> LightAttack;
        
        [Tooltip("Configuration for heavy attacks")]
        public AssetRef<HeavyAttackConfig> HeavyAttack;
        
        [Tooltip("Special move configurations")]
        public AssetRef<SpecialMoveConfig>[] SpecialMoves;
        
        [Header("Command Input Settings")]
        [Tooltip("Time window for command input sequence")]
        public FP CommandInputWindow = FP._0_50;
        
        [Tooltip("Maximum number of inputs to track")]
        public int MaxInputBufferSize = 8;
        
        [Tooltip("Priority for special moves (higher value = higher priority)")]
        public int SpecialMovePriority = 100;
        
        [Header("Unlock Settings")]
        [Tooltip("Level required to unlock double jump")]
        public int DoubleJumpUnlockLevel = 5;
        
        [Tooltip("Level required to unlock dash")]
        public int DashUnlockLevel = 5;
    }
}
