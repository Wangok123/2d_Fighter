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
        
        [Tooltip("受击类型")]
        public HitType HitType = HitType.Light;

        public virtual Shape2DConfig GetCurrentAttackShape(Frame frame, EntityRef entityRef)
        {
            return AttackShape;
        }

        public virtual FP GetCurrentHitboxActiveTime(Frame frame, EntityRef entityRef)
        {
            return HitboxActiveTime;
        }

        public virtual FP GetCurrentHitboxActiveDuration(Frame frame, EntityRef entityRef)
        {
            return HitboxActiveDuration;
        }

        public virtual FP GetCurrentKnockbackForce(Frame frame, EntityRef entityRef)
        {
            return KnockbackForce;
        }

        public virtual FP GetCurrentHitstunDuration(Frame frame, EntityRef entityRef)
        {
            return HitstunDuration;
        }

        public virtual FPVector2 GetCurrentKnockbackDirection(Frame frame, EntityRef entityRef, FPVector2 attackerPos, FPVector2 targetPos)
        {
            switch (KnockbackType)
            {
                case AttackKnockbackType.AwayFromAttacker:
                    FPVector2 awayDirection = targetPos - attackerPos;
                    return awayDirection.Normalized;

                case AttackKnockbackType.AttackerFacingDirection:
                    bool isFacingRight = GetIsFacingRight(frame, entityRef);
                    return new FPVector2(isFacingRight ? FP._1 : -FP._1, FixedKnockbackDirection.Y).Normalized;

                case AttackKnockbackType.Up:
                    return FPVector2.Up;

                case AttackKnockbackType.Fixed:
                    return FixedKnockbackDirection.Normalized;
            }

            return FixedKnockbackDirection.Normalized;
        }

        protected bool GetIsFacingRight(Frame frame, EntityRef entityRef)
        {
            if (frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
            {
                return movement->IsFacingRight;
            }
            return true;
        }

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);
            
            if (!frame.Has<AttackComponent>(entityRef))
                return abilityState;
            
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(entityRef);

            if (abilityState.IsActiveStartTick)
            {
                attackComponent->HasStartedHitboxWindow = false;
                
                if (attackComponent->HitEntitiesThisAttack.Ptr != default)
                {
                    frame.FreeList(attackComponent->HitEntitiesThisAttack);
                }
                attackComponent->HitEntitiesThisAttack = frame.AllocateList<EntityRef>();
            }

            if (abilityState.IsActive)
            {
                FP hitboxActiveTime = GetCurrentHitboxActiveTime(frame, entityRef);
                FP hitboxActiveDuration = GetCurrentHitboxActiveDuration(frame, entityRef);

                FP elapsedTime = ability->DurationTimer.ElapsedTime;
                FP hitboxStartTime = hitboxActiveTime;
                FP hitboxEndTime = hitboxActiveTime + hitboxActiveDuration;

                if (elapsedTime >= hitboxStartTime && elapsedTime < hitboxEndTime)
                {
                    if (!attackComponent->HasStartedHitboxWindow)
                    {
                        frame.Signals.OnAttackHitboxActivate(entityRef);
                        attackComponent->HasStartedHitboxWindow = true;
                    }
                    
                    frame.Signals.OnAttackExecute(entityRef);
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
    }
}
