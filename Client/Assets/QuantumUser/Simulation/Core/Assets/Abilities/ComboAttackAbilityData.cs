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
    public unsafe partial class ComboAttackAbilityData : AttackAbilityData
    {
        [Header("Combo Settings")] [Tooltip("最大连击数")]
        public int MaxComboCount = 3;

        [Tooltip("连击时间窗口")] public FP ComboWindow = FP._0_50;

        [Header("Combo Chain Configuration")] [Tooltip("每段连击的伤害倍率")]
        public FP[] ComboDamageMultipliers;

        [Tooltip("每段连击的打击框激活时间（从动画开始到判定触发的延迟）")]
        public FP[] ComboHitboxActiveTimes;

        [Tooltip("每段连击的持续时间")] public FP[] ComboDurations;

        [Tooltip("每段连击的击退力度")] public FP[] ComboKnockbackForces;

        [Tooltip("每段连击的攻击形状")] public Shape2DConfig[] ComboAttackShapes;

        [Tooltip("每段连击的状态效果")] public ComboStatusEffectArray[] ComboStatusEffects;

        [Tooltip("最后一击是否有特殊效果")] public bool LastHitLaunches = true;

        [Tooltip("最后一击的垂直击退方向")] public FP LastHitVerticalKnockback = FP._2;

        // 存储当前段的打击框激活时间
        private FP _currentHitboxActiveTime;

        // 标记是否已经触发过打击判定
        private bool _hasTriggeredHitbox;

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink,
            AbilityType abilityType, ref Ability ability)
        {
            var attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);

            // 计算下一段连击数
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
    
            // 保存当前参数（以防激活失败需要恢复）
            FP oldDuration = Duration;
            FP oldHitboxActiveTime = _currentHitboxActiveTime;
            FP oldKnockbackForce = KnockbackForce;
            Shape2DConfig oldAttackShape = AttackShape;
            StatusEffectConfig[] oldHitStatusEffects = HitStatusEffects;
    
            // 更新参数（需要在激活前更新，因为 base 会使用 Duration）
            UpdateComboParameters(comboIndex);

            // 尝试激活
            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);
    
            if (activated)
            {
                // 激活成功，提交连击状态
                attackData->ComboCounter = nextComboCounter;
                attackData->ComboWindowTimer = FrameTimer.FromSeconds(frame, ComboWindow);
                _hasTriggeredHitbox = false;
            }
            else
            {
                // 激活失败，恢复所有参数
                Duration = oldDuration;
                _currentHitboxActiveTime = oldHitboxActiveTime;
                KnockbackForce = oldKnockbackForce;
                AttackShape = oldAttackShape;
                HitStatusEffects = oldHitStatusEffects;
            }

            return activated;
        }

        private void UpdateComboParameters(int comboIndex)
        {
            if (comboIndex < 0)
                return;

            if (comboIndex < ComboDurations.Length)
            {
                Duration = ComboDurations[comboIndex];
            }

            //设置打击框激活时间
            if (ComboHitboxActiveTimes != null && comboIndex < ComboHitboxActiveTimes.Length)
            {
                _currentHitboxActiveTime = ComboHitboxActiveTimes[comboIndex];
            }
            else
            {
                _currentHitboxActiveTime = FP._0; // 默认立即触发
            }

            // 设置击退力度
            if (comboIndex < ComboKnockbackForces.Length)
            {
                KnockbackForce = ComboKnockbackForces[comboIndex];
            }

            // 设置击退力度
            if (ComboAttackShapes != null && comboIndex < ComboAttackShapes.Length)
            {
                AttackShape = ComboAttackShapes[comboIndex];
            }

            // 设置状态效果
            if (ComboStatusEffects != null && comboIndex < ComboStatusEffects.Length)
            {
                HitStatusEffects = ComboStatusEffects[comboIndex].StatusEffects;
            }
        }

        protected override void OnAttackActivate(Frame frame, EntityRef entityRef, Ability* ability)
        {
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            int comboStep = attackData->ComboCounter;

            frame.Events.ComboAttackStarted(entityRef, comboStep, MaxComboCount);
        }

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);

            if (abilityState.IsActive && !_hasTriggeredHitbox)
            {
                FP elapsedTime = ability->DurationTimer.ElapsedTime;

                if (elapsedTime >= _currentHitboxActiveTime)
                {
                    // ✅ 触发攻击判定
                    ExecuteAttackHitbox(frame, entityRef, ability);
                    _hasTriggeredHitbox = true;
                }
            }

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

            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            int comboIndex = attackData->ComboCounter - 1;

            if (comboIndex >= 0 && comboIndex < ComboDamageMultipliers.Length)
            {
                baseDamage *= ComboDamageMultipliers[comboIndex];
            }

            return baseDamage;
        }

        protected override void ApplyKnockback(Frame frame, EntityRef attacker, EntityRef target,
            FPVector2 hitDirection)
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

        private void ResetComboState(Frame frame, EntityRef entityRef)
        {
            if (frame.Unsafe.TryGetPointer<AttackData>(entityRef, out var attackData))
            {
                attackData->ComboCounter = 0;
                attackData->ComboWindowTimer = FrameTimer.None;
            }
        }

        private void ExecuteAttackHitbox(Frame frame, EntityRef entityRef, Ability* ability)
        {
#if UNITY_EDITOR
            AttackData* attackData = frame.Unsafe.GetPointer<AttackData>(entityRef);
            // 发送打击框激活事件
            frame.Events.AttackHitboxActivated(entityRef, attackData->ComboCounter);
#endif
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            GameSettingsData gameSettingsData =
                frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            var shape = AttackShape.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettingsData.PlayerLayerMask,
                QueryOptions.HitKinematics);

            if (hits.Count > 0)
            {
                HashSet<EntityRef> hitEntities = new HashSet<EntityRef>();
                hitEntities.Add(entityRef);

                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hitEntities.Contains(hit.Entity))
                    {
                        continue;
                    }

                    hitEntities.Add(hit.Entity);

                    CharacterStatus* hitPlayerStatus = frame.Unsafe.GetPointer<CharacterStatus>(hit.Entity);
                    Transform2D* hitPlayerTransform = frame.Unsafe.GetPointer<Transform2D>(hit.Entity);

                    FPVector2 hitLateralDirection = hitPlayerTransform->Position - transform->Position;
                    hitLateralDirection = hitLateralDirection.Normalized;

                    // 执行伤害、击退等效果
                    ApplyDamage(frame, entityRef, hit.Entity);
                    ApplyKnockback(frame, entityRef, hit.Entity, hitLateralDirection);
                    ApplyStatusEffects(frame, hit.Entity, hitLateralDirection);
                }
            }
        }
    }
}