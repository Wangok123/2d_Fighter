using Photon.Deterministic;
using System;
using Quantum.Collections;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
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
        
        [Tooltip("击退类型")]
        public AttackKnockbackType KnockbackType = AttackKnockbackType.AwayFromAttacker;
        
        [Tooltip("固定击退方向（仅当类型为Fixed时使用）")]
        public FPVector2 FixedKnockbackDirection = new FPVector2(FP._1, FP._0_50);
        
        [Header("Hitstun")]
        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_25;

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);
            
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);

            if (abilityState.IsActiveStartTick)
            {
                attackComponent->HasStartedHitboxWindow = false;
                
                if (attackComponent->HitEntitiesThisAttack.Ptr != default)
                {
                    frame.FreeList(attackComponent->HitEntitiesThisAttack);
                }
                attackComponent->HitEntitiesThisAttack = frame.AllocateList<EntityRef>();
                
                OnAttackActivate(frame, entityRef, ability);
            }

            if (abilityState.IsActive)
            {
                FP elapsedTime = ability->DurationTimer.ElapsedTime;
                FP hitboxStartTime = HitboxActiveTime;
                FP hitboxEndTime = HitboxActiveTime + HitboxActiveDuration;

                if (elapsedTime >= hitboxStartTime && elapsedTime < hitboxEndTime)
                {
                    if (!attackComponent->HasStartedHitboxWindow)
                    {
                        OnHitboxWindowStart(frame, entityRef, ability);
                        attackComponent->HasStartedHitboxWindow = true;
                    }
                    
                    ExecuteAttackHitbox(frame, entityRef, ability);
                }
            }
            
            if (abilityState.IsActiveEndTick)
            {
                if (attackComponent->HitEntitiesThisAttack.Ptr != default)
                {
                    frame.FreeList(attackComponent->HitEntitiesThisAttack);
                    attackComponent->HitEntitiesThisAttack = default;
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
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);
            GameSettingsData gameSettingsData = frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            bool isFacingRight = GetIsFacingRight(frame, entityRef);
            var shape = CreateAttackShapeWithDirection(frame, AttackShape, isFacingRight);
            
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

                    OnHitTarget(frame, entityRef, hit.Entity, transform->Position, hitPlayerTransform->Position);
                }
            }
        }
        
        protected virtual void OnHitTarget(Frame frame, EntityRef attacker, EntityRef target, FPVector2 attackerPos, FPVector2 targetPos)
        {
            if (frame.Has<HitReactionComponent>(target))
            {
                ApplyKnockback(frame, attacker, target, attackerPos, targetPos);
            }
        }

        protected virtual void ApplyKnockback(Frame frame, EntityRef attacker, EntityRef target, FPVector2 attackerPos, FPVector2 targetPos)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            FPVector2 knockbackDirection = CalculateKnockbackDirection(frame, attacker, attackerPos, targetPos);
            FPVector2 knockbackVelocity = knockbackDirection * KnockbackForce;
    
            hitReaction->ApplyKnockback(frame, target, knockbackVelocity, HitstunDuration);
        }

        protected virtual FPVector2 CalculateKnockbackDirection(Frame frame, EntityRef attacker, FPVector2 attackerPos, FPVector2 targetPos)
        {
            switch (KnockbackType)
            {
                case AttackKnockbackType.AwayFromAttacker:
                    FPVector2 awayDirection = targetPos - attackerPos;
                    return awayDirection.Normalized;

                case AttackKnockbackType.AttackerFacingDirection:
                    bool isFacingRight = GetIsFacingRight(frame, attacker);
                    return new FPVector2(isFacingRight ? FP._1 : -FP._1, FixedKnockbackDirection.Y).Normalized;

                case AttackKnockbackType.Up:
                    return FPVector2.Up;

                case AttackKnockbackType.Fixed:
                    return FixedKnockbackDirection.Normalized;
            }

            return FixedKnockbackDirection.Normalized;
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
