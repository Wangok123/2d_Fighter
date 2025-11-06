using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe partial class ComboAttackAbilityData : AttackAbilityData
    {
        [Header("Combo Settings")]
        [Tooltip("最大连击数")]
        public int MaxComboCount = 3;
        
        [Tooltip("连击时间窗口")]
        public FP ComboWindow = FP._0_50;
        
        [Tooltip("是否需要在特定时机输入")]
        public bool RequireTimedInput = false;
        
        [Tooltip("目押时间窗口")]
        public FP TimedInputWindow = FP._0_10;
        
        [Header("Combo Chain Configuration")]
        [Tooltip("每段连击的伤害倍率")]
        public FP[] ComboDamageMultipliers = new FP[] { FP._1, FP._1_20, FP._1_50 };
        
        [Tooltip("每段连击的持续时间")]
        public FP[] ComboDurations = new FP[] { FP._0_33, FP._0_33, FP._0_50 };
        
        [Tooltip("每段连击的启动时间")]
        public FP[] ComboStartupTimes = new FP[] { FP._0_10, FP._0_10, FP._0_10 };
        
        [Tooltip("每段连击的活跃时间")]
        public FP[] ComboActiveTimes = new FP[] { FP._0_20, FP._0_20, FP._0_25 };
        
        [Tooltip("每段连击的击退力度")]
        public FP[] ComboKnockbackForces = new FP[] { 3, 4, 8 };
        
        [Tooltip("每段连击的攻击形状")]
        public Shape2DConfig[] ComboAttackShapes;
        
        [Tooltip("每段连击的状态效果")]
        public StatusEffectConfig[][] ComboStatusEffects;
        
        [Tooltip("最后一击是否有特殊效果")]
        public bool LastHitLaunches = true;
        
        [Tooltip("最后一击的垂直击退方向")]
        public FP LastHitVerticalKnockback = FP._2;

        private FP _lastComboInputTime;

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            var attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            
            bool withinComboWindow = attackData->ComboWindowTimer.IsRunning(frame);
            
            if (withinComboWindow && attackData->ComboCounter < MaxComboCount)
            {
                attackData->ComboCounter++;
            }
            else
            {
                attackData->ComboCounter = 1;
            }
            
            attackData->ComboWindowTimer = FrameTimer.FromSeconds(frame, ComboWindow);
            
            int comboIndex = attackData->ComboCounter - 1;
            UpdateComboParameters(comboIndex);

            return base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);
        }

        private void UpdateComboParameters(int comboIndex)
        {
            if (comboIndex >= 0 && comboIndex < ComboDurations.Length)
            {
                Duration = ComboDurations[comboIndex];
            }
            
            if (comboIndex >= 0 && comboIndex < ComboStartupTimes.Length)
            {
                StartupTime = ComboStartupTimes[comboIndex];
            }
            
            if (comboIndex >= 0 && comboIndex < ComboActiveTimes.Length)
            {
                ActiveTime = ComboActiveTimes[comboIndex];
            }
            
            if (comboIndex >= 0 && comboIndex < ComboKnockbackForces.Length)
            {
                KnockbackForce = ComboKnockbackForces[comboIndex];
            }
            
            if (comboIndex >= 0 && comboIndex < ComboAttackShapes.Length)
            {
                AttackShape = ComboAttackShapes[comboIndex];
            }
            
            if (comboIndex >= 0 && comboIndex < ComboStatusEffects.Length)
            {
                HitStatusEffects = ComboStatusEffects[comboIndex];
            }
        }

        protected override void OnAttackActivate(Frame frame, EntityRef entityRef, Ability* ability)
        {
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            int comboStep = attackData->ComboCounter;
            
            //frame.Events.ComboAttackStarted(entityRef, comboStep, MaxComboCount);
            
            base.OnAttackActivate(frame, entityRef, ability);
            
            if (attackData->ComboCounter >= MaxComboCount)
            {
                attackData->ComboCounter = 0;
            }
        }

        protected override FP CalculateDamage(Frame frame, EntityRef entityRef)
        {
            FP baseDamage = base.CalculateDamage(frame, entityRef);
            
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            int comboIndex = attackData->ComboCounter - 1;
            
            if (comboIndex >= 0 && comboIndex < ComboDamageMultipliers.Length)
            {
                baseDamage *= ComboDamageMultipliers[comboIndex];
            }
            
            return baseDamage;
        }

        protected override void ApplyKnockback(Frame frame, EntityRef attacker, EntityRef target, FPVector2 hitDirection)
        {
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(attacker);
            bool isFinalHit = attackData->ComboCounter >= MaxComboCount;
            
            if (isFinalHit && LastHitLaunches)
            {
                FPVector2 knockbackDirection = hitDirection * KnockbackDirectionX;
                knockbackDirection.Y = LastHitVerticalKnockback;
                knockbackDirection = knockbackDirection.Normalized;
                
                FPVector2 knockbackDirection2D = new FPVector2(knockbackDirection.X, knockbackDirection.Y);
                
                frame.Signals.OnKnockbackApplied(target, HitstunDuration, knockbackDirection2D * KnockbackForce);
            }
            else
            {
                base.ApplyKnockback(frame, attacker, target, hitDirection);
            }
        }
    }
}
