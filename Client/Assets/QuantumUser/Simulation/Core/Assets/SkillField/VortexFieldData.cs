using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class VortexFieldData : SkillFieldData
    {
        [Header("旋涡设置")]
        [Tooltip("旋转方向")]
        public VortexRotation RotationDirection = VortexRotation.Clockwise;
        
        [Tooltip("向心力强度")]
        public FP CentripetalForce = 3;
        
        [Tooltip("切向力强度（旋转速度）")]
        public FP TangentialForce = 5;
        
        [Tooltip("是否造成伤害")]
        public bool DealDamage = true;
        
        [Tooltip("每Tick伤害")]
        public FP DamagePerTick = 2;

        [Header("高级设置")]
        [Tooltip("旋涡核心半径")]
        public FP CoreRadius = FP._0_50;
        
        [Tooltip("在核心区域是否眩晕")]
        public bool StunInCore = false;
        
        [Tooltip("眩晕持续时间")]
        public FP StunDuration = FP._1;
    
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

            FPVector2 toTarget = targetTransform->Position - fieldTransform->Position;
            FP distance = toTarget.Magnitude;

            if (distance < FP._0_01)
                return;

            // 计算旋涡力方向
            FPVector2 centripetalDir = -toTarget.Normalized;
            FPVector2 tangentialDir = RotationDirection == VortexRotation.Clockwise
                ? new FPVector2(toTarget.Y, -toTarget.X).Normalized
                : new FPVector2(-toTarget.Y, toTarget.X).Normalized;

            FPVector2 forceDirection = (centripetalDir * CentripetalForce + 
                                        tangentialDir * TangentialForce).Normalized;

            switch (knockbackData.KnockbackApplicationMode)
            {
                case KnockbackApplicationMode.CharacterController:
                    frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, 
                        forceDirection, knockbackDataRef);
                    break;

                case KnockbackApplicationMode.Physics2D:
                    FPVector2 totalForce = centripetalDir * CentripetalForce + 
                                           tangentialDir * TangentialForce;
                    frame.Signals.OnKnockbackPhysic2DApplied(target, totalForce);
                    break;
            }

            // 如果造成伤害
            if (DealDamage)
            {
                // TODO: 实现伤害系统后添加
            }
        }
    }

    public enum VortexRotation
    {
        Clockwise,
        CounterClockwise
    }
}