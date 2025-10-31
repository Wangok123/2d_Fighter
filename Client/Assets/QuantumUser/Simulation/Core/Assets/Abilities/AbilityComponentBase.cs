using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Base class for all ability components in the ECS-style modular system.
    /// Inspired by Overwatch's GDC presentation on hero composition.
    /// 
    /// This allows characters to be built from reusable ability components,
    /// making it easy to create new characters by assembling existing abilities.
    /// </summary>
    public abstract class AbilityComponentBase : AssetObject
    {
        [Header("Ability Identity")]
        [Tooltip("Unique identifier for this ability")]
        public string AbilityId;
        
        [Tooltip("Display name for this ability")]
        public string AbilityName;
        
        [Tooltip("Description of what this ability does")]
        [TextArea(3, 5)]
        public string Description;
        
        [Header("Ability Requirements")]
        [Tooltip("Minimum level required to use this ability")]
        public int RequiredLevel = 0;
        
        [Tooltip("Is this ability unlocked by default?")]
        public bool UnlockedByDefault = true;
        
        [Header("Ability Properties")]
        [Tooltip("Priority for this ability (higher value = higher priority)")]
        public int Priority = 50;
        
        [Tooltip("Cooldown after using this ability")]
        public FP Cooldown = FP._0_50;
        
        [Tooltip("Energy/resource cost to use this ability")]
        public FP EnergyCost = 0;
    }
}
