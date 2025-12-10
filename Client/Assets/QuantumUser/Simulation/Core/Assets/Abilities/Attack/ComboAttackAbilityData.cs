using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public class ComboStepConfig
    {
        [Tooltip("打击框激活时间")] public FP HitboxActiveTime = FP._0;

        [Tooltip("打击框持续时间")] public FP HitboxActiveDuration = FP._0_10;

        [Tooltip("持续时间")] public FP Duration = FP._1;

        [Tooltip("击退配置数据")] public AssetRef<KnockbackStatusEffectData> KnockbackStatusEffectData;

        [Tooltip("攻击形状")] public Shape2DConfig AttackShape;
    }

    [Serializable]
    public unsafe partial class ComboAttackAbilityData : AttackAbilityData
    {
        [Header("Combo Settings")] [Tooltip("最大连击数")]
        public int MaxComboCount = 3;

        [Tooltip("连击时间窗口")] public FP ComboWindow = FP._0_50;

        [Header("Combo Chain Configuration")] [Tooltip("每段的配置")]
        public ComboStepConfig[] ComboSteps;

        public override Shape2DConfig GetCurrentAttackShape(Frame frame, EntityRef entityRef)
        {
            if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                return base.GetCurrentAttackShape(frame, entityRef);

            ComboAttackRuntimeComponent* comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
            int stepIndex = comboRuntime->CurrentComboStep - 1;

            if (stepIndex >= 0 && stepIndex < ComboSteps.Length)
            {
                return ComboSteps[stepIndex].AttackShape;
            }

            return base.GetCurrentAttackShape(frame, entityRef);
        }

        public override FP GetCurrentHitboxActiveTime(Frame frame, EntityRef entityRef)
        {
            if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                return base.GetCurrentHitboxActiveTime(frame, entityRef);

            ComboAttackRuntimeComponent* comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
            return comboRuntime->CurrentHitboxActiveTime;
        }

        public override FP GetCurrentHitboxActiveDuration(Frame frame, EntityRef entityRef)
        {
            if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                return base.GetCurrentHitboxActiveDuration(frame, entityRef);

            ComboAttackRuntimeComponent* comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
            return comboRuntime->CurrentHitboxActiveDuration;
        }

        public override AssetRef<KnockbackStatusEffectData> GetCurrentKnockbackStatusEffectData(Frame frame,
            EntityRef entityRef)
        {
            if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                return base.GetCurrentKnockbackStatusEffectData(frame, entityRef);

            ComboAttackRuntimeComponent* comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
            return comboRuntime->CurrentKnockbackStatusEffectData;
        }

        public override bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink,
            AbilityType abilityType, ref Ability ability)
        {
            if (!frame.Has<AttackComponent>(entityRef))
                return base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            var attackData = frame.Unsafe.GetPointer<AttackComponent>(entityRef);

            bool withinComboWindow = attackData->ComboWindowTimer.IsRunning(frame);
            int nextComboCounter;

            if (withinComboWindow && attackData->ComboCounter < MaxComboCount)
            {
                nextComboCounter = attackData->ComboCounter + 1;
            }
            else
            {
                nextComboCounter = 1;
            }

            int comboIndex = nextComboCounter - 1;

            if (comboIndex < 0 || comboIndex >= ComboSteps.Length)
                return false;

            ComboStepConfig stepConfig = ComboSteps[comboIndex];

            Duration = stepConfig.Duration;

            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                {
                    frame.Add<ComboAttackRuntimeComponent>(entityRef);
                }

                ComboAttackRuntimeComponent* runtimeComponent =
                    frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
                runtimeComponent->CurrentComboStep = nextComboCounter;
                runtimeComponent->CurrentHitboxActiveTime = stepConfig.HitboxActiveTime;
                runtimeComponent->CurrentHitboxActiveDuration = stepConfig.HitboxActiveDuration;
                runtimeComponent->CurrentKnockbackStatusEffectData = stepConfig.KnockbackStatusEffectData;

                attackData->ComboCounter = nextComboCounter;
                attackData->ComboWindowTimer = FrameTimer.FromSeconds(frame, ComboWindow);

                frame.Events.ComboAttackStarted(entityRef, nextComboCounter, MaxComboCount);
            }

            return activated;
        }

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (!frame.Has<AttackComponent>(entityRef))
                return abilityState;

            AttackComponent* attackData = frame.Unsafe.GetPointer<AttackComponent>(entityRef);

            if (attackData->ComboCounter > 0 && !attackData->ComboWindowTimer.IsRunning(frame))
            {
                ResetComboState(frame, entityRef);
            }

            if (abilityState.IsActiveEndTick)
            {
                if (attackData->ComboCounter >= MaxComboCount)
                {
                    ResetComboState(frame, entityRef);
                }
            }

            return abilityState;
        }

        protected override void OnAbilityCancelled(Frame frame, EntityRef entityRef, AbilityType cancelledAbilityType)
        {
            if (cancelledAbilityType == AbilityType.AttackLight)
            {
                ResetComboState(frame, entityRef);
            }

            base.OnAbilityCancelled(frame, entityRef, cancelledAbilityType);
        }

        // 修改：添加打断连击的方法
        public override void OnCommandInputDetected(Frame frame, EntityRef entityRef)
        {
            if (!frame.Unsafe.TryGetPointer<AttackComponent>(entityRef, out var attackComponent))
                return;

            bool hasCombo = attackComponent->ComboCounter > 0;
            bool hasComboWindow = attackComponent->ComboWindowTimer.IsRunning(frame);

            if (!hasCombo && !hasComboWindow)
                return;

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log(
                $"[ComboAttack] ✓ Interrupting combo due to command input. ComboCounter: {attackComponent->ComboCounter}");
#endif
            
            if (attackComponent->HitEntitiesThisAttack != default)
            {
                frame.FreeList(attackComponent->HitEntitiesThisAttack);
                attackComponent->HitEntitiesThisAttack = default;
            }

            attackComponent->HasStartedHitboxWindow = false;

            // 重置连击状态
            ResetComboState(frame, entityRef);
            
            if (frame.Unsafe.TryGetPointer<AbilityInventory>(entityRef, out var abilityInventory))
            {
                var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);

                if (dic.TryGetValuePointer(AbilityType.AttackLight, out Ability* ability))
                {
                    if (ability->IsActive || ability->IsDelayed)
                    {
                        ability->StopAbility(frame, entityRef);
                        frame.Events.AbilityCancelled(entityRef, AbilityType.AttackLight);
                    }
                }
            }
        }

        private void ResetComboState(Frame frame, EntityRef entityRef)
        {
            if (frame.Unsafe.TryGetPointer<AttackComponent>(entityRef, out var attackData))
            {
                attackData->ComboCounter = 0;
                attackData->ComboWindowTimer = FrameTimer.None;
            }
        }
    }
}