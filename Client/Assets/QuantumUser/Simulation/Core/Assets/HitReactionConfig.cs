using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class HitReactionConfig : AssetObject
    {
        [Header("Health Settings")]
        [Tooltip("最大生命值")]
        public FP MaxHealth = 100;
        
        [Tooltip("最小伤害阈值")]
        public FP MinimumDamage = FP._0_10;
        
        [Tooltip("是否可以重生")]
        public bool CanRespawn = true;
        
        [Tooltip("重生时间")]
        public FP RespawnTime = FP._3;
        
        [Header("Hitstun Settings")]
        [Tooltip("默认硬直时间")]
        public FP DefaultHitstunDuration = FP._0_25;
        
        [Tooltip("轻击硬直倍率")]
        public FP LightHitStunMultiplier = FP._0_50;
        
        [Tooltip("中击硬直倍率")]
        public FP MediumHitStunMultiplier = FP._1;
        
        [Tooltip("重击硬直倍率")]
        public FP HeavyHitStunMultiplier = FP._1_50;
        
        [Header("Knockback Settings")]
        [Tooltip("击退速度衰减率")]
        public FP KnockbackDecayRate = FP._8;
        
        [Tooltip("最小击退速度阈值")]
        public FP MinKnockbackThreshold = FP._0_50;
        
        [Tooltip("击退时是否可以空中控制")]
        public bool AllowAirControlDuringKnockback = false;
        
        [Tooltip("空中控制强度")]
        public FP AirControlStrength = FP._0_25;
        
        [Tooltip("地面摩擦力")]
        public FP GroundFriction = FP._10;
        
        [Header("Super Armor Settings")]
        [Tooltip("是否有超级护甲")]
        public bool HasSuperArmor = false;
        
        [Tooltip("超级护甲初始值")]
        public FP InitialSuperArmor = 100;
        
        [Tooltip("超级护甲是否免疫击退")]
        public bool SuperArmorPreventKnockback = true;
        
        [Tooltip("超级护甲是否免疫硬直")]
        public bool SuperArmorPreventHitstun = false;
        
        [Tooltip("超级护甲自动恢复速度")]
        public FP SuperArmorRegenRate = FP._10;
        
        [Tooltip("超级护甲恢复延迟")]
        public FP SuperArmorRegenDelay = FP._2;
        
        [Header("Damage Reduction")]
        [Tooltip("基础伤害减免百分比(0-1)")]
        public FP BaseDamageReduction = FP._0;
        
        [Tooltip("连续受击伤害衰减百分比")]
        public FP ConsecutiveHitDamageReduction = FP._0_10;
        
        [Tooltip("连续受击计数重置时间")]
        public FP ConsecutiveHitResetTime = FP._1;
        
        [Tooltip("最大连续受击衰减次数")]
        public int MaxConsecutiveHitReduction = 5;
        
        [Header("Health Regeneration")]
        [Tooltip("是否自动回血")]
        public bool EnableHealthRegen = false;
        
        [Tooltip("回血速率(每秒)")]
        public FP HealthRegenRate = FP._5;
        
        [Tooltip("受击后多久开始回血")]
        public FP RegenDelayAfterHit = FP._3;
        
        [Header("Invincibility")]
        [Tooltip("重生后无敌时间")]
        public FP RespawnInvincibilityDuration = FP._2;
        
        [Tooltip("是否在无敌时显示特效")]
        public bool ShowInvincibilityEffect = true;
        
        [Header("Hit Reactions")]
        [Tooltip("是否受到击退影响")]
        public bool CanBeKnockedBack = true;
        
        [Tooltip("是否可以被眩晕")]
        public bool CanBeStunned = true;
        
        [Tooltip("是否可以被击飞")]
        public bool CanBeLaunched = true;
        
        [Tooltip("受击时是否打断当前动作")]
        public bool HitInterruptsActions = true;
    }
}