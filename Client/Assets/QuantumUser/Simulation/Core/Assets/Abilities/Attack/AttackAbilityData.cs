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
        
        [Tooltip("击退配置数据")]
        public AssetRef<KnockbackStatusEffectData> KnockbackStatusEffectData;

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

        public virtual AssetRef<KnockbackStatusEffectData> GetCurrentKnockbackStatusEffectData(Frame frame, EntityRef entityRef)
        {
            return KnockbackStatusEffectData;
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
