using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class DamageFieldData : SkillFieldData
    {
        [Header("伤害设置")]
        [Tooltip("每Tick伤害")]
        public FP DamagePerTick = 5;
        
        [Tooltip("伤害类型")]
        public DamageType DamageType = DamageType.Fire;
        
        [Tooltip("是否造成DOT（持续伤害效果）")]
        public bool ApplyDOT = false;
        
        [Tooltip("DOT持续时间")]
        public FP DOTDuration = 3;
        
        [Tooltip("DOT每秒伤害")]
        public FP DOTDamagePerSecond = 2;

        [Header("击退设置")]
        [Tooltip("是否应用击退")]
        public bool ApplyKnockback = false;
        
        [Tooltip("击退力度")]
        public FP KnockbackForce = 3;
        
        [Tooltip("击退方向")]
        public KnockbackDirection KnockbackDirection = KnockbackDirection.FromCenter;
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
        Dark,
        Holy
    }

    public enum KnockbackDirection
    {
        FromCenter,
        FromHitPoint,
        Up
    }
}