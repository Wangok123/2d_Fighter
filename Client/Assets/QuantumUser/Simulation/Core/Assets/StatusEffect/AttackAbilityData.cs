using Photon.Deterministic;
using System;
using System.Collections.Generic;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe partial class AttackAbilityData : AbilityData
    {
        [Header("Attack Properties")]
        [Tooltip("基础伤害")]
        public FP BaseDamage = 10;
        
        [Tooltip("每级伤害加成")]
        public FP DamagePerLevel = FP._0_50;
        
        [Header("Attack Timing")]
        [Tooltip("攻击启动时间")]
        public FP StartupTime = FP._0_10;
        
        [Tooltip("攻击判定活跃时间")]
        public FP ActiveTime = FP._0_20;
        
        [Tooltip("攻击恢复时间")]
        public FP RecoveryTime = FP._0_33;
        
        [Header("Attack Range")]
        [Tooltip("攻击判定形状")]
        public Shape2DConfig AttackShape;
        
        [Header("Knockback")]
        [Tooltip("击退力度")]
        public FP KnockbackForce = 5;
        
        [Tooltip("击退方向（水平）")]
        public FP KnockbackDirectionX = FP._1;
        
        [Tooltip("击退方向（垂直）")]
        public FP KnockbackDirectionY = FP._0_50;
        
        [Header("Hitstun")]
        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_25;
        
        [Header("Status Effects")]
        [Tooltip("命中时施加的状态效果")]
        public StatusEffectConfig[] HitStatusEffects;

        private static HashSet<EntityRef> _hitEntities = new HashSet<EntityRef>();

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (abilityState.IsActiveStartTick)
            {
                OnAttackActivate(frame, entityRef, ability);
            }

            return abilityState;
        }

        protected virtual void OnAttackActivate(Frame frame, EntityRef entityRef, Ability* ability)
        {
            CharacterStatus* playerStatus = frame.Unsafe.GetPointer<CharacterStatus>(entityRef);
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            GameSettingsData gameSettingsData = frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            var shape = AttackShape.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettingsData.PlayerLayerMask , QueryOptions.HitKinematics);

            if (hits.Count > 0)
            {
                _hitEntities.Add(entityRef);

                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (_hitEntities.Contains(hit.Entity))
                    {
                        continue;
                    }

                    _hitEntities.Add(hit.Entity);

                    CharacterStatus* hitPlayerStatus = frame.Unsafe.GetPointer<CharacterStatus>(hit.Entity);

                    // if (playerStatus->PlayerTeam == hitPlayerStatus->PlayerTeam)
                    // {
                    //     continue;
                    // }

                    Transform2D* hitPlayerTransform = frame.Unsafe.GetPointer<Transform2D>(hit.Entity);
                    AbilityInventory* hitPlayerAbilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(hit.Entity);

                    FPVector2 hitLateralDirection = hitPlayerTransform->Position - transform->Position;
                    hitLateralDirection = hitLateralDirection.Normalized;

                    // if (hitPlayerAbilityInventory->IsBlocking)
                    // {
                    //    // frame.Events.OnPlayerBlockHit(hit.Entity, hitLateralDirection);
                    // }
                    // else
                    // {
                    //     ApplyDamage(frame, entityRef, hit.Entity);
                    //     ApplyKnockback(frame, entityRef, hit.Entity, hitLateralDirection);
                    //     ApplyStatusEffects(frame, hit.Entity, hitLateralDirection);
                    //     
                    //    // frame.Events.OnPlayerHit(hit.Entity);
                    // }
                }

                _hitEntities.Clear();
            }
        }

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                //frame.Events.OnPlayerAttacked(entityRef);
            }

            return activated;
        }

        protected virtual void ApplyDamage(Frame frame, EntityRef attacker, EntityRef target)
        {
            FP damage = CalculateDamage(frame, attacker);
            
            if (frame.Unsafe.TryGetPointer<CharacterStatus>(target, out var health))
            {
                health->CurrentHealth -= damage;
                
                if (health->CurrentHealth <= 0)
                {
                    //frame.Events.EntityDied(target, attacker);
                }
            }
        }

        protected virtual FP CalculateDamage(Frame frame, EntityRef entityRef)
        {
            FP damage = BaseDamage;
            
            if (frame.TryGet<CharacterLevel>(entityRef, out var level))
            {
                damage += DamagePerLevel * level.CurrentLevel;
            }
            
            return damage;
        }

        protected virtual void ApplyKnockback(Frame frame, EntityRef attacker, EntityRef target, FPVector2 hitDirection)
        {
            if (KnockbackForce <= 0)
                return;

            FPVector2 knockbackDirection = hitDirection * KnockbackDirectionX;
            knockbackDirection.Y = KnockbackDirectionY;
            knockbackDirection = knockbackDirection.Normalized;
            
            FPVector3 knockbackDirection3D = new FPVector3(knockbackDirection.X, knockbackDirection.Y, FP._0);
            
            frame.Signals.OnKnockbackApplied(target, HitstunDuration, knockbackDirection3D * KnockbackForce);
        }

        protected virtual void ApplyStatusEffects(Frame frame, EntityRef target, FPVector2 hitDirection)
        {
            if (HitStatusEffects == null || HitStatusEffects.Length == 0)
                return;

            foreach (var statusEffectConfig in HitStatusEffects)
            {
                switch (statusEffectConfig.Type)
                {
                    case StatusEffectType.Stun:
                        frame.Signals.OnStunApplied(target, statusEffectConfig.Duration);
                        break;

                    case StatusEffectType.Knockback:
                        FPVector3 direction3D = new FPVector3(hitDirection.X, hitDirection.Y, FP._0);
                        frame.Signals.OnKnockbackApplied(target, statusEffectConfig.Duration, direction3D);
                        break;

                    default:
                        throw new System.ArgumentException($"Unknown {nameof(StatusEffectType)}: {statusEffectConfig.Type}", nameof(statusEffectConfig.Type));
                }
            }
        }
    }
}