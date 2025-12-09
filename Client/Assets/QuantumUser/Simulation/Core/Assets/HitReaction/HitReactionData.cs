using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class HitReactionData : AssetObject
    {
        [Header("Knockback Settings")]
        [Tooltip("是否可以被击退")]
        public bool CanBeKnockedBack = true;

        [Tooltip("击退强度倍率")]
        public FP KnockbackStrengthMultiplier = FP._1;

        [Tooltip("击退持续时间倍率")]
        public FP KnockbackDurationMultiplier = FP._1;

        [Header("Immunity Settings")]
        [Tooltip("击退免疫时间（在上一次击退结束后）")]
        public FP KnockbackImmunityDuration = FP._0;

        [Tooltip("最小击退间隔（防止连续击退）")]
        public FP MinKnockbackInterval = FP._0_10;
    }
}