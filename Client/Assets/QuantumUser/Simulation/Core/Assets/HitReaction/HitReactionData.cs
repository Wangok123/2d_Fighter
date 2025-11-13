using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class HitReactionData : AssetObject
    {
        [Header("Core Flags")] [Tooltip("是否可以被击退")]
        public bool CanBeKnockedBack = true;
    }
}