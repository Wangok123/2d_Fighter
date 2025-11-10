using UnityEngine;
using Photon.Deterministic;
using Quantum.Physics2D;

namespace Quantum
{
    [CreateAssetMenu(menuName = "Quantum/Assets/Projectile/SkillField", order = 2)]
    public unsafe partial class SkillFieldData : AssetObject
    {
        [Header("基础设置")]
        [Tooltip("持续时间")]
        public FP Duration = 5;
        
        [Tooltip("Tick间隔")]
        public FP TickInterval = FP._0_50;
        
        [Tooltip("视觉Prototype")]
        public EntityPrototype VisualPrototype;

        [Header("效果范围")]
        [Tooltip("范围形状")]
        public Shape2DConfig EffectArea;
        
        [Tooltip("影响层")]
        public int TargetLayer = 1 << 6;
        
        [Tooltip("是否影响友军")]
        public bool AffectAllies = false;
        
        [Tooltip("是否影响敌人")]
        public bool AffectEnemies = true;

        [Header("效果设置")]
        [Tooltip("效果类型")]
        public SkillFieldEffectType EffectType = SkillFieldEffectType.Damage;
        
        [Tooltip("每Tick伤害/治疗量")]
        public FP EffectValue = 5;
        
        [Tooltip("是否应用击退")]
        public bool ApplyKnockback = false;
        
        [Tooltip("击退力度")]
        public FP KnockbackForce = 3;

        public virtual void OnSkillFieldSpawned(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, EntityRef owner, FPVector2 position)
        {
        }

        public virtual void OnSkillFieldTick(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual void OnSkillFieldDestroyed(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual bool ShouldAffectTarget(Frame frame, EntityRef owner, EntityRef target)
        {
            if (target == owner)
                return false;

            bool isSameTeam = CheckSameTeam(frame, owner, target);

            if (isSameTeam && !AffectAllies)
                return false;

            if (!isSameTeam && !AffectEnemies)
                return false;

            return true;
        }

        protected virtual bool CheckSameTeam(Frame frame, EntityRef entity1, EntityRef entity2)
        {
            return false;
        }

        public virtual void ApplyEffect(Frame frame, EntityRef owner, EntityRef target, FPVector2 hitPoint)
        {
            switch (EffectType)
            {
                case SkillFieldEffectType.Damage:
                    ApplyDamage(frame, owner, target, hitPoint);
                    break;
                    
                case SkillFieldEffectType.Heal:
                    ApplyHeal(frame, target);
                    break;
                    
                case SkillFieldEffectType.Buff:
                    ApplyBuff(frame, target);
                    break;
                    
                case SkillFieldEffectType.Debuff:
                    ApplyDebuff(frame, target);
                    break;
                    
                case SkillFieldEffectType.Control:
                    ApplyControl(frame, target);
                    break;
            }
        }

        protected virtual void ApplyDamage(Frame frame, EntityRef owner, EntityRef target, FPVector2 hitPoint)
        {
            if (frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
            {
                if (ApplyKnockback)
                {
                    Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(owner);
                    Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);
                    FPVector2 knockbackDirection = (targetTransform->Position - ownerTransform->Position).Normalized;
                    FPVector2 knockbackVelocity = knockbackDirection * KnockbackForce;

                    hitReaction->ApplyKnockback(frame, target, knockbackVelocity, FP._0_25);
                }
            }
        }

        protected virtual void ApplyHeal(Frame frame, EntityRef target)
        {
        }

        protected virtual void ApplyBuff(Frame frame, EntityRef target)
        {
        }

        protected virtual void ApplyDebuff(Frame frame, EntityRef target)
        {
        }

        protected virtual void ApplyControl(Frame frame, EntityRef target)
        {
        }
    }
}
