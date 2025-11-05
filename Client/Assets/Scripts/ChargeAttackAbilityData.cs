using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe partial class ChargeAttackAbilityData : AttackAbilityData
    {
        [Header("Charge Settings")]
        [Tooltip("最小蓄力时间")]
        public FP MinChargeTime = FP._0_50;
        
        [Tooltip("最大蓄力时间")]
        public FP MaxChargeTime = FP._2;
        
        [Tooltip("满蓄力伤害倍率")]
        public FP FullChargeDamageMultiplier = FP._2_50;
        
        [Tooltip("最小蓄力伤害倍率")]
        public FP MinChargeDamageMultiplier = FP._1_20;
        
        [Header("Charge Visual/Audio")]
        [Tooltip("蓄力过程中是否播放特效")]
        public bool ShowChargingEffect = true;
        
        [Tooltip("满蓄力时是否有特殊提示")]
        public bool ShowFullChargeIndicator = true;
        
        [Header("Charge Attack Properties")]
        [Tooltip("满蓄力时的攻击形状")]
        public Shape2DConfig FullChargeAttackShape;
        
        [Tooltip("满蓄力时的击退力度")]
        public FP FullChargeKnockbackForce = 10;
        
        [Tooltip("满蓄力时的状态效果")]
        public StatusEffectConfig[] FullChargeStatusEffects;
        
        [Tooltip("是否可以在蓄力过程中移动")]
        public bool CanMoveWhileCharging = false;
        
        [Tooltip("蓄力时的移动速度倍率")]
        public FP ChargingMoveSpeedMultiplier = FP._0_50;

        private FP _chargeStartTime;
        private FP _chargeRatio;

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerStatus* playerStatus, ref Ability ability)
        {
            _chargeStartTime = frame.Global->Time;
            
            if (ShowChargingEffect)
            {
                frame.Events.ChargingStarted(entityRef, MaxChargeTime);
            }
            
            return false;
        }

        protected virtual void ReleaseCharge(Frame frame, EntityRef entityRef)
        {
            FP chargeTime = frame.Global->Time - _chargeStartTime;
            _chargeRatio = FPMath.Clamp(chargeTime / MaxChargeTime, FP._0, FP._1);
            
            bool isFullyCharged = chargeTime >= MaxChargeTime;
            bool isMinCharged = chargeTime >= MinChargeTime;
            
            if (isMinCharged)
            {
                if (isFullyCharged)
                {
                    AttackShape = FullChargeAttackShape;
                    KnockbackForce = FullChargeKnockbackForce;
                    HitStatusEffects = FullChargeStatusEffects;
                }
                
                Ability tempAbility = new Ability();
                OnAttackActivate(frame, entityRef, ref tempAbility);
            }
        }

        protected override FP CalculateDamage(Frame frame, EntityRef entityRef)
        {
            FP baseDamage = base.CalculateDamage(frame, entityRef);
            
            FP chargeMultiplier = FPMath.Lerp(MinChargeDamageMultiplier, FullChargeDamageMultiplier, _chargeRatio);
            
            return baseDamage * chargeMultiplier;
        }
    }
}
