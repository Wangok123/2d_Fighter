using Photon.Deterministic;
using System;
using System.Collections.Generic;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public class ComboStepConfig
    {
        [Tooltip("打击框激活时间（从动画开始到判定触发的延迟）")]
        public FP HitboxActiveTime = FP._0;

        [Tooltip("打击框持续时间（判定生效的时间窗口，期间每个敌人只会被击中一次）")]
        public FP HitboxActiveDuration = FP._0_10;

        [Tooltip("持续时间")]
        public FP Duration = FP._1;

        [Tooltip("击退力度")]
        public FP KnockbackForce = FP._5;

        [Tooltip("击退方向")]
        public FPVector2 KnockbackDirection = new FPVector2(FP._1, FP._0_50);

        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_25;

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
            
            if (comboIndex < 0 || comboIndex >= ComboSteps.Length)
                return false;

            ComboStepConfig stepConfig = ComboSteps[comboIndex];
            
            AttackAbilityCache cache = new AttackAbilityCache
            {
                Duration = Duration,
                HitboxActiveTime = HitboxActiveTime,
                HitboxActiveDuration = HitboxActiveDuration,
                KnockbackForce = KnockbackForce,
                HitstunDuration = HitstunDuration,
            };
            Shape2DConfig oldAttackShape = AttackShape;
            
            Duration = stepConfig.Duration;
            HitboxActiveTime = stepConfig.HitboxActiveTime;
            HitboxActiveDuration = stepConfig.HitboxActiveDuration;
            AttackShape = stepConfig.AttackShape;
            KnockbackForce = stepConfig.KnockbackForce;
            HitstunDuration = stepConfig.HitstunDuration;

            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                attackData->ComboCounter = nextComboCounter;
                attackData->ComboWindowTimer = FrameTimer.FromSeconds(frame, ComboWindow);
            }
            else
            {
                Duration = cache.Duration;
                HitboxActiveTime = cache.HitboxActiveTime;
                HitboxActiveDuration = cache.HitboxActiveDuration;
                KnockbackForce = cache.KnockbackForce;
                HitstunDuration = cache.HitstunDuration;
                AttackShape = oldAttackShape;
            }

            return activated;
        }
        
        protected override void ExecuteAttackHitbox(Frame frame, EntityRef entityRef, Ability* ability)
        {
            AttackComponent* attackData = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
            int comboIndex = attackData->ComboCounter - 1;
            
            if (comboIndex < 0 || comboIndex >= ComboSteps.Length)
            {
                base.ExecuteAttackHitbox(frame, entityRef, ability);
                return;
            }

            ComboStepConfig stepConfig = ComboSteps[comboIndex];
            
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
            GameSettingsData gameSettingsData = frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            bool isFacingRight = GetIsFacingRight(frame, entityRef);
            var shape = CreateAttackShapeWithDirection(frame, stepConfig.AttackShape, isFacingRight);
            
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettingsData.PlayerLayerMask, QueryOptions.HitDynamics);

            if (hits.Count > 0)
            {
                var hitList = frame.ResolveList(attackComponent->HitEntitiesThisAttack);
                
                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == entityRef)
                        continue;
                    
                    if (hitList.Contains(hit.Entity))
                        continue;

                    if (!frame.Unsafe.TryGetPointer<Transform2D>(hit.Entity, out var hitPlayerTransform))
                        continue;

                    hitList.Add(hit.Entity);

                    OnHitTargetWithComboConfig(frame, entityRef, hit.Entity, transform->Position, hitPlayerTransform->Position, stepConfig);
                }
            }
        }
        
        protected virtual void OnHitTargetWithComboConfig(Frame frame, EntityRef attacker, EntityRef target, FPVector2 attackerPos, FPVector2 targetPos, ComboStepConfig stepConfig)
        {
            if (frame.Has<HitReactionComponent>(target))
            {
                ApplyComboKnockback(frame, attacker, target, attackerPos, targetPos, stepConfig);
            }
        }
        
        protected virtual void ApplyComboKnockback(Frame frame, EntityRef attacker, EntityRef target, FPVector2 attackerPos, FPVector2 targetPos, ComboStepConfig stepConfig)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            bool isFacingRight = GetIsFacingRight(frame, attacker);
            FPVector2 knockbackDirection = new FPVector2(
                stepConfig.KnockbackDirection.X * (isFacingRight ? FP._1 : -FP._1),
                stepConfig.KnockbackDirection.Y
            ).Normalized;
            
            FPVector2 knockbackVelocity = knockbackDirection * stepConfig.KnockbackForce;
    
            hitReaction->ApplyKnockback(frame, target, knockbackVelocity, stepConfig.HitstunDuration);
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
