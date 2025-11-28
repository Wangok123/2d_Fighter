using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class PushFieldData : SkillFieldData
    {
        [Header("力场设置")] [Tooltip("力场类型")] public ForceFieldType FieldType = ForceFieldType.Push;

        [Tooltip("力场强度")] public FP ForceStrength = 5;

        [Tooltip("力场方向")] public ForceDirection Direction = ForceDirection.FromCenter;

        [Tooltip("自定义方向（使用CustomDirection时）")] public FPVector2 CustomDirection = FPVector2.Up;

        [Header("高级设置")] [Tooltip("是否受距离衰减")] public bool FalloffWithDistance = true;

        [Tooltip("最大影响距离")] public FP MaxEffectRange = 5;

        [Tooltip("是否持续施加力")] public bool ContinuousForce = true;

        public override void ApplyEffect(Frame frame, EntityRef skillFieldEntity,
            SkillFieldComponent* skillField, EntityRef target, FPVector2 hitPoint)
        {
            AssetRef<KnockbackStatusEffectData> knockbackDataRef =
                GetKnockbackStatusEffectData(frame, skillFieldEntity);

            if (!knockbackDataRef.Id.IsValid)
                return;

            KnockbackStatusEffectData knockbackData =
                frame.FindAsset<KnockbackStatusEffectData>(knockbackDataRef.Id);

            Transform2D* fieldTransform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            // 计算Push力的方向（保留原有逻辑）
            FPVector2 forceDirection = CalculatePushFieldDirection(
                fieldTransform->Position, targetTransform->Position, hitPoint);
            FP forceMagnitude = CalculatePushFieldMagnitude(
                fieldTransform->Position, targetTransform->Position);

            ApplyKnockbackToTarget(frame, target, knockbackData, forceDirection, forceMagnitude, knockbackDataRef);
        }

        private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData, 
            FPVector2 forceDirection, FP forceMagnitude, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
        {
            if (frame.Has<PhysicsBody2D>(target))
            {
                FPVector2 force = forceDirection * forceMagnitude;
                frame.Signals.OnKnockbackPhysic2DApplied(target, force);
                return;
            }
    
            if (frame.Has<CharacterController2D>(target))
            {
                frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, forceDirection, knockbackDataRef);
                return;
            }
    
#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.LogWarning($"[PushFieldData] Target entity {target} has neither PhysicsBody2D nor CharacterController2D");
#endif
        }

        private FPVector2 CalculatePushFieldDirection(FPVector2 center, FPVector2 targetPos, FPVector2 hitPoint)
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

        private FP CalculatePushFieldMagnitude(FPVector2 center, FPVector2 targetPos)
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