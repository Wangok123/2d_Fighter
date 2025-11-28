using UnityEngine;
using Photon.Deterministic;
using Quantum.Physics2D;

namespace Quantum
{
    public unsafe partial class SkillFieldData : AssetObject
    {
        [Header("基础设置")] [Tooltip("持续时间")] public FP Duration = 5;

        [Tooltip("Tick间隔")] public FP TickInterval = FP._0_50;

        [Tooltip("视觉Prototype")] public EntityPrototype VisualPrototype;

        [Header("效果范围")] [Tooltip("范围形状")] public Shape2DConfig EffectArea;

        [Tooltip("影响层")] public LayerMask TargetLayer = 1 << 6;

        [Tooltip("是否影响友军")] public bool AffectAllies = false;

        [Tooltip("是否影响敌人")] public bool AffectEnemies = true;

        [Header("伤害设置")] [Tooltip("每次Tick伤害")] public FP DamagePerTick = 5;

        [Tooltip("击退配置数据")] public AssetRef<KnockbackStatusEffectData> KnockbackStatusEffectData;

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

        public virtual FP GetDamagePerTick(Frame frame, EntityRef skillFieldEntity)
        {
            return DamagePerTick;
        }

        public virtual AssetRef<KnockbackStatusEffectData> GetKnockbackStatusEffectData(Frame frame,
            EntityRef skillFieldEntity)
        {
            return KnockbackStatusEffectData;
        }

        public virtual void OnCustomSpawn(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual void OnCustomTick(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual void OnCustomDestroy(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual void ApplyEffect(Frame frame, EntityRef skillFieldEntity,
            SkillFieldComponent* skillField, EntityRef target, FPVector2 hitPoint)
        {
            if (KnockbackStatusEffectData.Id.IsValid)
            {
                ApplyKnockback(frame, skillFieldEntity, skillField, target, hitPoint);
            }
        }

        // 添加：受保护的辅助方法供子类使用
        protected void ApplyKnockback(Frame frame, EntityRef skillFieldEntity,
            SkillFieldComponent* skillField, EntityRef target, FPVector2 hitPoint)
        {
            KnockbackStatusEffectData knockbackData =
                frame.FindAsset<KnockbackStatusEffectData>(KnockbackStatusEffectData.Id);

            Transform2D* fieldTransform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(
                frame, skillField->Owner, fieldTransform->Position, targetTransform->Position);

            ApplyKnockbackToTarget(frame, target, knockbackData, knockbackDirection, KnockbackStatusEffectData);
        }

        private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData, 
            FPVector2 knockbackDirection, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
        {
            if (frame.Has<PhysicsBody2D>(target))
            {
                FPVector2 knockbackVelocity = knockbackDirection * knockbackData.KnockbackForce;
                frame.Signals.OnKnockbackPhysic2DApplied(target, knockbackVelocity);
                return;
            }
    
            if (frame.Has<CharacterController2D>(target))
            {
                frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection, knockbackDataRef);
                return;
            }
    
#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.LogWarning($"[SkillFieldData] Target entity {target} has neither PhysicsBody2D nor CharacterController2D");
#endif
        }
    }
}