using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Configuration for command input system (for special moves).
    /// Defines how input buffering and sequence matching works.
    /// </summary>
    public class CommandInputConfig : AssetObject
    {
        [Header("Input Buffer Settings")]
        [Tooltip("Time window for command input sequence")]
        public FP InputWindow = FP._0_50;
        
        [Tooltip("Maximum number of inputs to track")]
        public int MaxInputBufferSize = 8;
        
        [Header("Special Move Priority")]
        [Tooltip("Priority for special moves (higher value = higher priority)")]
        public int Priority = 100;
    }
}
