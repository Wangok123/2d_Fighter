using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class DelayedExplosionFieldData : SkillFieldData
    {
        [Header("爆炸延迟")]
        [Tooltip("引爆延迟时间")]
        public FP ExplosionDelay = FP._1;

        [Header("爆炸伤害")]
        [Tooltip("爆炸伤害")]
        public FP ExplosionDamage = 50;

        [Tooltip("是否有伤害衰减")]
        public bool DamageFalloff = true;

        [Tooltip("中心伤害倍率")]
        public FP CenterDamageMultiplier = FP._1_50;

        [Header("击退效果")]
        [Tooltip("是否应用击退")]
        public bool ApplyKnockback = true;

        [Header("视觉效果")]
        [Tooltip("爆炸特效Prototype")]
        public EntityPrototype ExplosionEffect;

        [Tooltip("预警特效Prototype")]
        public EntityPrototype WarningEffect;
    }

    public enum ExplosionKnockbackType
    {
        None,
        FromCenter,
        Up
    }
}