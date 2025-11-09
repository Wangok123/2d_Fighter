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
        [Header("Attack Range")]
        [Tooltip("攻击判定形状")]
        public Shape2DConfig AttackShape;
        
        [Header("Attack Timing")]
        [Tooltip("打击框激活时间（从动画开始到判定触发的延迟，即启动帧）")]
        public FP HitboxActiveTime = FP._0;
        
        [Tooltip("打击框持续时间（判定生效的时间窗口，即判定帧，期间每个敌人只会被击中一次）")]
        public FP HitboxActiveDuration = FP._0_10;
        
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
        
        [Tooltip("受击类型")]
        public HitType HitType = HitType.Light;

        protected bool _hasStartedHitboxWindow;
        protected HashSet<EntityRef> _hitEntitiesThisAttack = new HashSet<EntityRef>();

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (abilityState.IsActiveStartTick)
            {
                _hasStartedHitboxWindow = false;
                _hitEntitiesThisAttack.Clear();
                OnAttackActivate(frame, entityRef, ability);
            }

            if (abilityState.IsActive)
            {
                FP elapsedTime = ability->DurationTimer.ElapsedTime;
                FP hitboxStartTime = HitboxActiveTime;
                FP hitboxEndTime = HitboxActiveTime + HitboxActiveDuration;

                if (elapsedTime >= hitboxStartTime && elapsedTime < hitboxEndTime)
                {
                    if (!_hasStartedHitboxWindow)
                    {
                        OnHitboxWindowStart(frame, entityRef, ability);
                        _hasStartedHitboxWindow = true;
                    }
                    
                    ExecuteAttackHitbox(frame, entityRef, ability);
                }
            }
            
            return abilityState;
        }

        protected virtual void OnAttackActivate(Frame frame, EntityRef entityRef, Ability* ability)
        {
        }

        protected virtual void OnHitboxWindowStart(Frame frame, EntityRef entityRef, Ability* ability)
        {
#if UNITY_EDITOR
            if (frame.Unsafe.TryGetPointer<AttackComponent>(entityRef, out var attackData))
            {
                frame.Events.AttackHitboxActivated(entityRef, attackData->ComboCounter);
            }
#endif
        }

        protected virtual void ExecuteAttackHitbox(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            GameSettingsData gameSettingsData = frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            bool isFacingRight = GetIsFacingRight(frame, entityRef);
            var shape = CreateAttackShapeWithDirection(frame, AttackShape, isFacingRight);
            
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettingsData.PlayerLayerMask, QueryOptions.HitDynamics);

            if (hits.Count > 0)
            {
                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == entityRef)
                        continue;
                    
                    if (_hitEntitiesThisAttack.Contains(hit.Entity))
                        continue;

                    if (!frame.Unsafe.TryGetPointer<Transform2D>(hit.Entity, out var hitPlayerTransform))
                        continue;

                    _hitEntitiesThisAttack.Add(hit.Entity);

                    FPVector2 hitLateralDirection = hitPlayerTransform->Position - transform->Position;
                    hitLateralDirection = hitLateralDirection.Normalized;

                    OnHitTarget(frame, entityRef, hit, hitLateralDirection);
                }
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
        
        protected virtual void OnHitTarget(Frame frame, EntityRef attacker, Hit hit, FPVector2 hitLateralDirection)
        {
            EntityRef target = hit.Entity;
    
            if (frame.Has<HitReactionComponent>(target))
            {
                ApplyKnockback(frame, attacker, target, hitLateralDirection);
            }
    
            frame.Events.OnPlayerHit(target, attacker, FP._0, hitLateralDirection, HitType, false);
        }

        protected virtual void ApplyKnockback(Frame frame, EntityRef attacker, EntityRef target, FPVector2 hitDirection)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            FPVector2 knockbackDirection2D = new FPVector2(
                KnockbackDirectionX * (hitDirection.X > 0 ? FP._1 : -FP._1),
                KnockbackDirectionY
            ).Normalized;
    
            FPVector2 knockbackVelocity = knockbackDirection2D * KnockbackForce;
    
            hitReaction->ApplyKnockback(frame, target, knockbackVelocity, HitstunDuration);
        }
        
        protected virtual bool GetIsFacingRight(Frame frame, EntityRef entityRef)
        {
            if (frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
            {
                return movement->IsFacingRight;
            }
    
            return true;
        }
        
        protected virtual Shape2D CreateAttackShapeWithDirection(Frame frame, Shape2DConfig shapeConfig, bool isFacingRight)
        {
            Shape2DConfig adjustedConfig = new Shape2DConfig
            {
                ShapeType = shapeConfig.ShapeType,
                PolygonCollider = shapeConfig.PolygonCollider,
                CircleRadius = shapeConfig.CircleRadius,
                CapsuleSize = shapeConfig.CapsuleSize,
                EdgeExtent = shapeConfig.EdgeExtent,
                BoxExtents = shapeConfig.BoxExtents,
                PositionOffset = shapeConfig.PositionOffset,
                RotationOffset = shapeConfig.RotationOffset,
                UserTag = shapeConfig.UserTag,
                IsPersistent = shapeConfig.IsPersistent,
                CompoundShapes = shapeConfig.CompoundShapes
            };
    
            if (!isFacingRight)
            {
                adjustedConfig.PositionOffset.X = -adjustedConfig.PositionOffset.X;
            }
    
            return adjustedConfig.CreateShape(frame);
        }
    }
}