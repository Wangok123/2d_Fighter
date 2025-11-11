using Photon.Deterministic;
using System;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe partial class ChargeAttackAbilityData : AttackAbilityData
    {
        [Header("Charge Settings")]
        [Tooltip("最小蓄力时间（达到此时间即可释放）")]
        public FP MinChargeTime = FP._0_25;
        
        [Tooltip("最大蓄力时间（达到此时间为满蓄力）")]
        public FP MaxChargeTime = FP._1;
        
        [Tooltip("蓄力时是否可以移动")]
        public bool CanMoveWhileCharging = false;
        
        [Header("Charge Damage Scaling")]
        [Tooltip("最小蓄力伤害倍率")]
        public FP MinChargeDamageMultiplier = FP._1;
        
        [Tooltip("最大蓄力伤害倍率")]
        public FP MaxChargeDamageMultiplier = 2;
        
        [Header("Charge Knockback Scaling")]
        [Tooltip("是否根据蓄力时间缩放击退力度")]
        public bool ScaleKnockbackWithCharge = true;
        
        [Tooltip("最小蓄力击退倍率")]
        public FP MinChargeKnockbackMultiplier = FP._1;
        
        [Tooltip("最大蓄力击退倍率")]
        public FP MaxChargeKnockbackMultiplier = FP._1_50;
        
        [Header("Charge Visual Settings")]
        [Tooltip("是否根据蓄力时间缩放攻击范围")]
        public bool ScaleAttackRangeWithCharge = false;
        
        [Tooltip("最大蓄力时的攻击范围倍率")]
        public FP MaxChargeRangeMultiplier = FP._1_50;

        public override void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            if (!ability->AbilityData.Id.IsValid || ability->AbilityData.Id != Guid)
            {
                return;
            }

            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);

            if (attackComponent->IsChargingHeavy)
            {
                bool wasButtonReleased = input.GetAbilityInputWasPressed(abilityType) == false && 
                                         (abilityType == AbilityType.AttackHeavy ? input.HP.WasReleased : input.LP.WasReleased);

                FP currentChargeTime = attackComponent->ChargeTimer.ElapsedSeconds(frame);
                
                if (currentChargeTime >= MaxChargeTime)
                {
                    currentChargeTime = MaxChargeTime;
                }

                if (wasButtonReleased)
                {
                    if (currentChargeTime >= MinChargeTime)
                    {
                        ability->BufferInput(frame);
                    }
                    else
                    {
                        attackComponent->IsChargingHeavy = false;
                        attackComponent->ChargeTimer = FrameTimer.None;
                        
                        if (!CanMoveWhileCharging && frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                        {
                            abilityEnable->MovementEnabled = true;
                        }

                        frame.Events.ChargingCancelled(entityRef);
                    }
                }
            }
            else
            {
                if (input.GetAbilityInputWasPressed(abilityType))
                {
                    if (!ability->IsOnCooldown)
                    {
                        CharacterStatusComponent* playerStatus = frame.Unsafe.GetPointer<CharacterStatusComponent>(entityRef);
                        AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);
                        
                        if (!playerStatus->IsIncapacitated && !abilityInventory->HasActiveAbility)
                        {
                            attackComponent->IsChargingHeavy = true;
                            attackComponent->ChargeTimer = FrameTimer.FromSeconds(frame, MaxChargeTime);
                            
                            if (!CanMoveWhileCharging && frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                            {
                                abilityEnable->MovementEnabled = false;
                            }

                            frame.Events.ChargingStarted(entityRef, MaxChargeTime);
                        }
                    }
                }
            }
        }

        public override bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
            
            if (!attackComponent->IsChargingHeavy)
            {
                return false;
            }
            
            FP currentChargeTime = attackComponent->ChargeTimer.ElapsedSeconds(frame);

            if (currentChargeTime < MinChargeTime)
            {
                return false;
            }

            attackComponent->HeavyChargeTime = currentChargeTime;

            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                attackComponent->IsChargingHeavy = false;
                attackComponent->ChargeTimer = FrameTimer.None;
                
                if (!CanMoveWhileCharging && frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                {
                    abilityEnable->MovementEnabled = true;
                }
                
                frame.Events.ChargeAttackReleased(entityRef, currentChargeTime, currentChargeTime >= MinChargeTime);
            }

            return activated;
        }

        protected override void OnHitTarget(Frame frame, EntityRef attacker, EntityRef target, FPVector2 attackerPos, FPVector2 targetPos)
        {
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(attacker);
            FP chargeTime = attackComponent->HeavyChargeTime;
            FP chargeRatio = (chargeTime - MinChargeTime) / FPMath.Max(MaxChargeTime - MinChargeTime, FP.EN4);
            chargeRatio = FPMath.Clamp01(chargeRatio);
            
            FP oldKnockbackForce = KnockbackForce;
            
            if (ScaleKnockbackWithCharge)
            {
                FP knockbackMultiplier = FPMath.Lerp(MinChargeKnockbackMultiplier, MaxChargeKnockbackMultiplier, chargeRatio);
                KnockbackForce *= knockbackMultiplier;
            }

            base.OnHitTarget(frame, attacker, target, attackerPos, targetPos);
            
            KnockbackForce = oldKnockbackForce;
        }

        protected override void ExecuteAttackHitbox(Frame frame, EntityRef entityRef, Ability* ability)
        {
            if (ScaleAttackRangeWithCharge)
            {
                AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
                FP chargeTime = attackComponent->HeavyChargeTime;
                FP chargeRatio = (chargeTime - MinChargeTime) / FPMath.Max(MaxChargeTime - MinChargeTime, FP.EN4);
                chargeRatio = FPMath.Clamp01(chargeRatio);
                
                FP rangeMultiplier = FPMath.Lerp(FP._1, MaxChargeRangeMultiplier, chargeRatio);
                
                Shape2DConfig originalShape = AttackShape;
                AttackShape = ScaleAttackShape(originalShape, rangeMultiplier);
                
                base.ExecuteAttackHitbox(frame, entityRef, ability);
                
                AttackShape = originalShape;
            }
            else
            {
                base.ExecuteAttackHitbox(frame, entityRef, ability);
            }
        }
        
        protected virtual FP GetChargeRatio(FP chargeTime)
        {
            if (MaxChargeTime <= MinChargeTime)
            {
                return FP._1;
            }

            FP ratio = (chargeTime - MinChargeTime) / (MaxChargeTime - MinChargeTime);
            return FPMath.Clamp01(ratio);
        }

        protected virtual FP GetChargeDamageMultiplier(FP chargeTime)
        {
            FP chargeRatio = GetChargeRatio(chargeTime);
            return FPMath.Lerp(MinChargeDamageMultiplier, MaxChargeDamageMultiplier, chargeRatio);
        }

        protected virtual Shape2DConfig ScaleAttackShape(Shape2DConfig originalShape, FP scale)
        {
            Shape2DConfig scaledShape = new Shape2DConfig
            {
                ShapeType = originalShape.ShapeType,
                PolygonCollider = originalShape.PolygonCollider,
                CircleRadius = originalShape.CircleRadius * scale,
                CapsuleSize = originalShape.CapsuleSize * scale,
                EdgeExtent = originalShape.EdgeExtent * scale,
                BoxExtents = originalShape.BoxExtents * scale,
                PositionOffset = originalShape.PositionOffset,
                RotationOffset = originalShape.RotationOffset,
                UserTag = originalShape.UserTag,
                IsPersistent = originalShape.IsPersistent,
                CompoundShapes = originalShape.CompoundShapes
            };

            return scaledShape;
        }

        protected override void OnAbilityCancelled(Frame frame, EntityRef entityRef, AbilityType cancelledAbilityType)
        {
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
            
            if (attackComponent->IsChargingHeavy)
            {
                attackComponent->IsChargingHeavy = false;
                attackComponent->ChargeTimer = FrameTimer.None;
                
                if (!CanMoveWhileCharging && frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                {
                    abilityEnable->MovementEnabled = true;
                }

                frame.Events.ChargingCancelled(entityRef);
            }

            base.OnAbilityCancelled(frame, entityRef, cancelledAbilityType);
        }
    }
}
