using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public class ComboStepConfig
    {
        [Tooltip("打击框激活时间")]
        public FP HitboxActiveTime = FP._0;

        [Tooltip("打击框持续时间")]
        public FP HitboxActiveDuration = FP._0_10;

        [Tooltip("持续时间")]
        public FP Duration = FP._1;

        [Tooltip("击退力度")]
        public FP KnockbackForce = FP._5;

        [Tooltip("击退方向")]
        public FPVector2 KnockbackDirection = new FPVector2(FP._1, FP._0_50);

        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_25;

        [Tooltip("受击类型")]
        public HitType HitType = HitType.Light;

        [Tooltip("攻击形状")]
        public Shape2DConfig AttackShape;
    }

    [Serializable]
    public unsafe partial class ComboAttackAbilityData : AttackAbilityData
    {
        [Header("Combo Settings")]
        [Tooltip("最大连击数")]
        public int MaxComboCount = 3;

        [Tooltip("连击时间窗口")]
        public FP ComboWindow = FP._0_50;

        [Header("Combo Chain Configuration")]
        [Tooltip("每段的配置")]
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

        public override FP GetCurrentKnockbackForce(Frame frame, EntityRef entityRef)
        {
            if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                return base.GetCurrentKnockbackForce(frame, entityRef);

            ComboAttackRuntimeComponent* comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
            return comboRuntime->CurrentKnockbackForce;
        }

        public override FP GetCurrentHitstunDuration(Frame frame, EntityRef entityRef)
        {
            if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                return base.GetCurrentHitstunDuration(frame, entityRef);

            ComboAttackRuntimeComponent* comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
            return comboRuntime->CurrentHitstunDuration;
        }

        public override FPVector2 GetCurrentKnockbackDirection(Frame frame, EntityRef entityRef, FPVector2 attackerPos, FPVector2 targetPos)
        {
            if (!frame.Has<ComboAttackRuntimeComponent>(entityRef))
                return base.GetCurrentKnockbackDirection(frame, entityRef, attackerPos, targetPos);

            ComboAttackRuntimeComponent* comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
    
            switch (KnockbackType)
            {
                case AttackKnockbackType.AwayFromAttacker:
                    FPVector2 awayDirection = targetPos - attackerPos;
                    return awayDirection.Normalized;

                case AttackKnockbackType.AttackerFacingDirection:
                    bool isFacingRight = GetIsFacingRight(frame, entityRef);
                    return new FPVector2(
                        comboRuntime->CurrentKnockbackDirection.X * (isFacingRight ? FP._1 : -FP._1),
                        comboRuntime->CurrentKnockbackDirection.Y
                    ).Normalized;

                case AttackKnockbackType.Up:
                    return FPVector2.Up;

                case AttackKnockbackType.Fixed:
                    isFacingRight = GetIsFacingRight(frame, entityRef);
                    return new FPVector2(
                        comboRuntime->CurrentKnockbackDirection.X * (isFacingRight ? FP._1 : -FP._1),
                        comboRuntime->CurrentKnockbackDirection.Y
                    ).Normalized;
            }
    
            bool defaultIsFacingRight = GetIsFacingRight(frame, entityRef);
            return new FPVector2(
                comboRuntime->CurrentKnockbackDirection.X * (defaultIsFacingRight ? FP._1 : -FP._1),
                comboRuntime->CurrentKnockbackDirection.Y
            ).Normalized;
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

                ComboAttackRuntimeComponent* runtimeComponent = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(entityRef);
                runtimeComponent->CurrentComboStep = nextComboCounter;
                runtimeComponent->CurrentHitboxActiveTime = stepConfig.HitboxActiveTime;
                runtimeComponent->CurrentHitboxActiveDuration = stepConfig.HitboxActiveDuration;
                runtimeComponent->CurrentKnockbackForce = stepConfig.KnockbackForce;
                runtimeComponent->CurrentHitstunDuration = stepConfig.HitstunDuration;
                runtimeComponent->CurrentKnockbackDirection = stepConfig.KnockbackDirection;

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
