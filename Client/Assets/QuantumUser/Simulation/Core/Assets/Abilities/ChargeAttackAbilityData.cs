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
        public FP FullChargeDamageMultiplier = FP._2 + FP._0_50;
        
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

        public override void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            
            if (attackData->IsChargingHeavy)
            {
                if (input.HP.WasReleased || !attackData->ChargeTimer.IsRunning(frame))
                {
                    ReleaseCharge(frame, entityRef, abilityType);
                }
                else if (attackData->ChargeTimer.ElapsedSeconds(frame) >= MaxChargeTime)
                {
                    if (ShowFullChargeIndicator)
                    {
                        frame.Events.ChargeMaxReached(entityRef);
                    }
                }
            }
            else
            {
                base.UpdateInput(frame, entityRef, abilityType, ability, input);
            }
        }
        
        protected override bool ShouldBufferInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            
            if (attackData->IsChargingHeavy)
            {
                return false;
            }
            
            return input.HP.WasPressed;
        }
        
        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            
            if (attackData->IsChargingHeavy)
            {
                return false;
            }
            
            SimpleInput2D input = *frame.GetPlayerInput(playerLink->Player);
            
            if (input.HP.WasPressed)
            {
                attackData->ChargeTimer = FrameTimer.FromSeconds(frame, MaxChargeTime);
                attackData->IsChargingHeavy = true;
                
                if (ShowChargingEffect)
                {
                    frame.Events.ChargingStarted(entityRef, MaxChargeTime);
                }
                
                return false;
            }
            
            return false;
        }

        protected virtual void ReleaseCharge(Frame frame, EntityRef entityRef, AbilityType abilityType)
        {
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            
            FP chargeTime = attackData->ChargeTimer.ElapsedSeconds(frame);
            bool isMinCharged = chargeTime >= MinChargeTime;
            bool isFullyCharged = chargeTime >= MaxChargeTime;
            
            attackData->IsChargingHeavy = false;
            attackData->HeavyChargeTime = chargeTime;
            
            if (!isMinCharged)
            {
                // frame.Events.ChargeReleaseEarly(entityRef, chargeTime, MinChargeTime);
                return;
            }
            
            if (isFullyCharged && FullChargeAttackShape != null)
            {
                AttackShape = FullChargeAttackShape;
                KnockbackForce = FullChargeKnockbackForce;
                HitStatusEffects = FullChargeStatusEffects;
            }
            
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);
            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            
            if (dic.TryGetValuePointer(abilityType, out Ability* ability))
            {
                PlayerLink* playerLink = frame.Unsafe.GetPointer<PlayerLink>(entityRef);
                
                bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref *ability);
                
                if (activated)
                {
                    //frame.Events.ChargeAttackReleased(entityRef, chargeTime, isFullyCharged);
                }
            }
        }

        protected override FP CalculateDamage(Frame frame, EntityRef entityRef)
        {
            FP baseDamage = base.CalculateDamage(frame, entityRef);
            
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            FP chargeRatio = FPMath.Clamp(attackData->HeavyChargeTime / MaxChargeTime, FP._0, FP._1);
            FP chargeMultiplier = FPMath.Lerp(MinChargeDamageMultiplier, FullChargeDamageMultiplier, chargeRatio);
            
            return baseDamage * chargeMultiplier;
        }
    }
}
