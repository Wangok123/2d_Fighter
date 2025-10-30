using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Configuration for special moves (command inputs like Street Fighter)
    /// </summary>
    public class SpecialMoveConfig : AssetObject
    {
        [Header("Move Identification")]
        [Tooltip("Unique ID for this special move")]
        public int MoveId;
        
        [Tooltip("Name of the special move")]
        public string MoveName;
        
        [Header("Input Sequence")]
        [Tooltip("Input sequence required to trigger this move (e.g., Down, DownRight, Right, LP for hadouken)")]
        public int[] InputSequence;
        
        [Header("Move Properties")]
        [Tooltip("Damage dealt by this special move")]
        public FP Damage = 30;
        
        [Tooltip("Cooldown after using this move")]
        public FP Cooldown = FP._1;
        
        [Tooltip("Minimum level required to use this move")]
        public int RequiredLevel = 0;
    }
}
