using Photon.Deterministic;
using System;
using System.Collections.Generic;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    // 添加这个包装类
    [Serializable]
    public class ComboStatusEffectArray
    {
        [Tooltip("该连击段的状态效果列表")] public StatusEffectConfig[] StatusEffects = new StatusEffectConfig[0];
    }

    [Serializable]
    public class ComboStepConfig
    {
        [Tooltip("伤害倍率")] public FP DamageMultiplier = FP._1;

        [Tooltip("打击框激活时间（从动画开始到判定触发的延迟）")] public FP HitboxActiveTime = FP._0;

        [Tooltip("打击框持续时间（判定生效的时间窗口，期间每个敌人只会被击中一次）")]
        public FP HitboxActiveDuration = FP._0_10;

        [Tooltip("持续时间")] public FP Duration = FP._1;

        [Tooltip("击退力度")] public FP KnockbackForce = FP._5;

        [Tooltip("击退方向（水平）")] public FP KnockbackDirectionX = FP._1;

        [Tooltip("击退方向（垂直）")] public FP KnockbackDirectionY = FP._0_50;

        [Tooltip("攻击形状")] public Shape2DConfig AttackShape;

        [Tooltip("状态效果")] public StatusEffectConfig[] StatusEffects = new StatusEffectConfig[0];
    }

    [Serializable]
    public unsafe partial class ComboAttackAbilityData : AttackAbilityData
    {
        [Header("Combo Settings")] [Tooltip("最大连击数")]
        public int MaxComboCount = 3;

        [Tooltip("连击时间窗口")] public FP ComboWindow = FP._0_50;

        [Header("Combo Chain Configuration")] [Tooltip("每段连击的伤害倍率")]
        public FP[] ComboDamageMultipliers;

        [Tooltip("每段的配置")] public ComboStepConfig[] ComboSteps;

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink,
            AbilityType abilityType, ref Ability ability)
        {
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

            FP oldDuration = Duration;
            FP oldHitboxActiveTime = HitboxActiveTime;
            FP oldHitboxActiveDuration = HitboxActiveDuration;
            FP oldKnockbackForce = KnockbackForce;
            FP oldKnockbackDirectionX = KnockbackDirectionX;
            FP oldKnockbackDirectionY = KnockbackDirectionY;
            Shape2DConfig oldAttackShape = AttackShape;
            StatusEffectConfig[] oldHitStatusEffects = HitStatusEffects;

            UpdateComboParameters(comboIndex);

            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                attackData->ComboCounter = nextComboCounter;
                attackData->ComboWindowTimer = FrameTimer.FromSeconds(frame, ComboWindow);
            }
            else
            {
                Duration = oldDuration;
                HitboxActiveTime = oldHitboxActiveTime;
                HitboxActiveDuration = oldHitboxActiveDuration;
                KnockbackForce = oldKnockbackForce;
                KnockbackDirectionX = oldKnockbackDirectionX;
                KnockbackDirectionY = oldKnockbackDirectionY;
                AttackShape = oldAttackShape;
                HitStatusEffects = oldHitStatusEffects;
            }

            return activated;
        }

        private void UpdateComboParameters(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= ComboSteps.Length)
                return;

            Duration = ComboSteps[comboIndex].Duration;
            HitboxActiveTime = ComboSteps[comboIndex].HitboxActiveTime;
            HitboxActiveDuration = ComboSteps[comboIndex].HitboxActiveDuration;
            KnockbackForce = ComboSteps[comboIndex].KnockbackForce;
            KnockbackDirectionX = ComboSteps[comboIndex].KnockbackDirectionX;
            KnockbackDirectionY = ComboSteps[comboIndex].KnockbackDirectionY;
            AttackShape = ComboSteps[comboIndex].AttackShape;
            HitStatusEffects = ComboSteps[comboIndex].StatusEffects;
        }

        protected override void OnAttackActivate(Frame frame, EntityRef entityRef, Ability* ability)
        {
            AttackComponent* attackData = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
            int comboStep = attackData->ComboCounter;

            frame.Events.ComboAttackStarted(entityRef, comboStep, MaxComboCount);
        }

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);
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
            // 只处理连击攻击被取消的情况
            if (cancelledAbilityType == AbilityType.AttackLight)
            {
                ResetComboState(frame, entityRef);
            }

            base.OnAbilityCancelled(frame, entityRef, cancelledAbilityType);
        }
        
        protected override FP CalculateDamage(Frame frame, EntityRef entityRef)
        {
            FP baseDamage = base.CalculateDamage(frame, entityRef);

            AttackComponent* attackData = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
            int comboIndex = attackData->ComboCounter - 1;

            if (comboIndex >= 0 && comboIndex < ComboDamageMultipliers.Length)
            {
                baseDamage *= ComboDamageMultipliers[comboIndex];
            }

            return baseDamage;
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