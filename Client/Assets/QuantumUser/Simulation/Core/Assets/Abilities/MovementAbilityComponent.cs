using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Movement ability component that defines how a character moves.
    /// Examples: Standard walk/run, double jump, dash, air dash, wall jump, etc.
    /// 
    /// Multiple movement abilities can be combined on a single character.
    /// </summary>
    public class MovementAbilityComponent : AbilityComponentBase
    {
        [Header("Movement Type")]
        [Tooltip("Type of movement this ability provides")]
        public MovementAbilityType MovementType;
        
        [Header("Movement Parameters")]
        [Tooltip("Movement speed multiplier")]
        public FP SpeedMultiplier = FP._1;
        
        [Tooltip("Can be used in air?")]
        public bool CanUseInAir = false;
        
        [Tooltip("Number of uses allowed (0 = unlimited, >0 = limited uses before landing)")]
        public int MaxAirUses = 0;
        
        [Tooltip("Movement distance (for dashes, teleports, etc.)")]
        public FP MovementDistance = 0;
        
        [Tooltip("Movement duration (for dashes, rolls, etc.)")]
        public FP MovementDuration = FP._0_25;
        
        [Tooltip("Does this ability grant invincibility during execution?")]
        public bool GrantsInvincibility = false;
        
        public MovementAbilityComponent()
        {
            AbilityId = "movement_basic";
            AbilityName = "Basic Movement";
            Priority = 10;
        }
    }
    
    /// <summary>
    /// Types of movement abilities that can be assigned to characters
    /// </summary>
    public enum MovementAbilityType
    {
        Walk,           // Basic walking/running
        Dash,           // Quick dash in a direction
        DoubleJump,     // Additional jump in air
        AirDash,        // Dash while airborne
        WallJump,       // Jump off walls
        Teleport,       // Instant position change
        Roll,           // Rolling movement with invincibility frames
        Slide,          // Sliding along ground
        Glide,          // Slow fall/gliding
        Sprint          // Faster running with stamina cost
    }
}
