using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe partial class ChargeAttackAbilityData : AttackAbilityData
    {
        [Header("Charge Settings")] [Tooltip("最小蓄力时间（达到此时间即可释放）")]
        public FP MinChargeTime = FP._0_25;

        [Tooltip("最大蓄力时间（达到此时间为满蓄力）")] public FP MaxChargeTime = FP._1;

        [Tooltip("蓄力时是否可以移动")] public bool CanMoveWhileCharging = false;

        [Header("Charge Damage Scaling")] [Tooltip("最小蓄力伤害倍率")]
        public FP MinChargeDamageMultiplier = FP._1;

        [Tooltip("最大蓄力伤害倍率")] public FP MaxChargeDamageMultiplier = 2;

        [Header("Charge Knockback Scaling")] [Tooltip("是否根据蓄力时间缩放击退力度")]
        public bool ScaleKnockbackWithCharge = true;

        [Tooltip("最小蓄力击退力度倍率")] public FP MinChargeKnockbackMultiplier = FP._1;

        [Tooltip("最大蓄力击退力度倍率")] public FP MaxChargeKnockbackMultiplier = FP._1_50;

        [Header("Charge Visual Settings")] [Tooltip("是否根据蓄力时间缩放攻击范围")]
        public bool ScaleAttackRangeWithCharge = false;

        [Tooltip("最大蓄力时的攻击范围倍率")] public FP MaxChargeRangeMultiplier = FP._1_50;

        public override Shape2DConfig GetCurrentAttackShape(Frame frame, EntityRef entityRef)
        {
            if (!ScaleAttackRangeWithCharge)
                return base.GetCurrentAttackShape(frame, entityRef);

            if (!frame.Unsafe.TryGetPointer<AttackComponent>(entityRef, out var attackComponent))
                return base.GetCurrentAttackShape(frame, entityRef);

            FP chargeTime = attackComponent->HeavyChargeTime;
            FP chargeRatio = GetChargeRatio(chargeTime);
            FP rangeMultiplier = FPMath.Lerp(FP._1, MaxChargeRangeMultiplier, chargeRatio);

            return ScaleAttackShape(AttackShape, rangeMultiplier);
        }

        public override AssetRef<KnockbackStatusEffectData> GetCurrentKnockbackStatusEffectData(Frame frame,
            EntityRef entityRef)
        {
            if (!ScaleKnockbackWithCharge)
                return base.GetCurrentKnockbackStatusEffectData(frame, entityRef);

            AssetRef<KnockbackStatusEffectData>
                baseDataRef = base.GetCurrentKnockbackStatusEffectData(frame, entityRef);

            if (!baseDataRef.Id.IsValid)
                return baseDataRef;

            if (!frame.Unsafe.TryGetPointer<AttackComponent>(entityRef, out var attackComponent))
                return baseDataRef;

            if (!frame.Unsafe.TryGetPointer<ChargeAttackRuntimeComponent>(entityRef, out var chargeRuntime))
            {
                frame.Add<ChargeAttackRuntimeComponent>(entityRef);
                chargeRuntime = frame.Unsafe.GetPointer<ChargeAttackRuntimeComponent>(entityRef);
            }

            FP chargeTime = attackComponent->HeavyChargeTime;
            FP chargeRatio = GetChargeRatio(chargeTime);
            FP knockbackMultiplier =
                FPMath.Lerp(MinChargeKnockbackMultiplier, MaxChargeKnockbackMultiplier, chargeRatio);

            chargeRuntime->CurrentKnockbackMultiplier = knockbackMultiplier;
            chargeRuntime->BaseKnockbackData = baseDataRef;

            return baseDataRef;
        }

        public override void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability,
            SimpleInput2D input)
        {
            if (!ability->AbilityData.Id.IsValid || ability->AbilityData.Id != Guid)
            {
                return;
            }

            // 修改：检查是否有技能正在执行，如果有则不处理蓄力输入
            if (frame.Unsafe.TryGetPointer<SkillComponent>(entityRef, out var skillComponent))
            {
                if (skillComponent->CurrentSkill.Id.IsValid)
                {
                    // 有技能正在执行，不处理蓄力输入
                    return;
                }
            }

            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);

            if (attackComponent->IsChargingHeavy)
            {
                bool wasButtonReleased = input.GetAbilityInputWasPressed(abilityType) == false &&
                                         (abilityType == AbilityType.AttackHeavy
                                             ? input.HP.WasReleased
                                             : input.LP.WasReleased);

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

                        if (!CanMoveWhileCharging &&
                            frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
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
                        AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);

                        if (!IsIncapacitated(frame, entityRef) && !abilityInventory->HasActiveAbility)
                        {
                            attackComponent->IsChargingHeavy = true;
                            attackComponent->ChargeTimer = FrameTimer.FromSeconds(frame, MaxChargeTime);

                            if (!CanMoveWhileCharging &&
                                frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                            {
                                abilityEnable->MovementEnabled = false;
                            }

                            frame.Events.ChargingStarted(entityRef, MaxChargeTime);
                        }
                    }
                }
            }
        }


        public override bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink,
            AbilityType abilityType, ref Ability ability)
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

                if (!CanMoveWhileCharging &&
                    frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                {
                    abilityEnable->MovementEnabled = true;
                }

                frame.Events.ChargeAttackReleased(entityRef, currentChargeTime, currentChargeTime >= MinChargeTime);
            }

            return activated;
        }

        protected override void OnAbilityCancelled(Frame frame, EntityRef entityRef, AbilityType cancelledAbilityType)
        {
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);

            if (attackComponent->IsChargingHeavy)
            {
                attackComponent->IsChargingHeavy = false;
                attackComponent->ChargeTimer = FrameTimer.None;

                if (!CanMoveWhileCharging &&
                    frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                {
                    abilityEnable->MovementEnabled = true;
                }

                frame.Events.ChargingCancelled(entityRef);
            }

            base.OnAbilityCancelled(frame, entityRef, cancelledAbilityType);
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

        public virtual FP GetChargeDamageMultiplier(FP chargeTime)
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

        public override void OnCommandInputDetected(Frame frame, EntityRef entityRef)
        {
            // 当检测到指令输入时，取消蓄力状态
            if (!frame.Unsafe.TryGetPointer<AttackComponent>(entityRef, out var attackComponent))
                return;

            if (!attackComponent->IsChargingHeavy)
                return;

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[ChargeAttack] Cancelling charge due to command input");
#endif

            attackComponent->IsChargingHeavy = false;
            attackComponent->ChargeTimer = FrameTimer.None;

            // 恢复移动能力
            if (frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
            {
                abilityEnable->MovementEnabled = true;
            }

            // 触发取消事件
            frame.Events.ChargingCancelled(entityRef);
        }
        
        private bool IsKnockedBack(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer<KnockbackComponent>(entity, out var knockback))
            {
                return knockback->IsKnockedBack;
            }
            return false;
        }
        
        private bool IsIncapacitated(Frame frame, EntityRef entity)
        {
            return IsKnockedBack(frame, entity);
        }
    }
}