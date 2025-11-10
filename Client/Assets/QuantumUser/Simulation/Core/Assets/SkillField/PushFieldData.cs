using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class PushFieldData : SkillFieldData
    {
        [Header("力场设置")]
        [Tooltip("力场类型")]
        public ForceFieldType FieldType = ForceFieldType.Push;
        
        [Tooltip("力场强度")]
        public FP ForceStrength = 5;
        
        [Tooltip("力场方向")]
        public ForceDirection Direction = ForceDirection.FromCenter;
        
        [Tooltip("自定义方向（使用CustomDirection时）")]
        public FPVector2 CustomDirection = FPVector2.Up;

        [Header("高级设置")]
        [Tooltip("是否受距离衰减")]
        public bool FalloffWithDistance = true;
        
        [Tooltip("最大影响距离")]
        public FP MaxEffectRange = 5;
        
        [Tooltip("是否持续施加力")]
        public bool ContinuousForce = true;

        public override void ApplyEffect(Frame frame, EntityRef skillFieldEntity, EntityRef target, FPVector2 hitPoint)
        {
            SkillFieldComponent* skillField = frame.Unsafe.GetPointer<SkillFieldComponent>(skillFieldEntity);
            ApplyForceToTarget(frame, skillField->Owner, target, hitPoint);
        }

        private void ApplyForceToTarget(Frame frame, EntityRef owner, EntityRef target, FPVector2 hitPoint)
        {
            Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(owner);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 forceDirection = CalculateForceDirection(ownerTransform->Position, targetTransform->Position, hitPoint);
            FP forceMagnitude = CalculateForceMagnitude(ownerTransform->Position, targetTransform->Position);

            if (frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
            {
                FPVector2 force = forceDirection * forceMagnitude;
                hitReaction->ApplyKnockback(frame, target, force, FP._0_10);
            }
        }

        private FPVector2 CalculateForceDirection(FPVector2 center, FPVector2 targetPos, FPVector2 hitPoint)
        {
            FPVector2 baseDirection = Direction switch
            {
                ForceDirection.FromCenter => (targetPos - center).Normalized,
                ForceDirection.ToCenter => (center - targetPos).Normalized,
                ForceDirection.CustomDirection => CustomDirection.Normalized,
                _ => FPVector2.Zero
            };

            return FieldType == ForceFieldType.Push ? baseDirection : -baseDirection;
        }

        private FP CalculateForceMagnitude(FPVector2 center, FPVector2 targetPos)
        {
            if (!FalloffWithDistance)
                return ForceStrength;

            FP distance = FPVector2.Distance(center, targetPos);
            if (distance >= MaxEffectRange)
                return FP._0;

            FP falloff = FP._1 - (distance / MaxEffectRange);
            return ForceStrength * falloff;
        }
    }

    public enum ForceFieldType
    {
        Push,
        Pull
    }

    public enum ForceDirection
    {
        FromCenter,
        ToCenter,
        CustomDirection
    }
}
