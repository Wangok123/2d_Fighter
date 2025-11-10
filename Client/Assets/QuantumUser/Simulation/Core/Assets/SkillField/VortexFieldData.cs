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

        public override void ApplyEffect(Frame frame, EntityRef skillFieldEntity, EntityRef target, FPVector2 hitPoint)
        {
            Transform2D* fieldTransform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 toTarget = targetTransform->Position - fieldTransform->Position;
            FP distance = toTarget.Magnitude;

            bool inCore = distance < CoreRadius;

            if (inCore && StunInCore)
            {
                ApplyStun(frame, target);
            }

            ApplyVortexForce(frame, target, fieldTransform->Position, targetTransform->Position);

            if (DealDamage)
            {
                ApplyVortexDamage(frame, target);
            }
        }

        private void ApplyVortexForce(Frame frame, EntityRef target, FPVector2 vortexCenter, FPVector2 targetPos)
        {
            FPVector2 toTarget = targetPos - vortexCenter;
            FP distance = toTarget.Magnitude;

            if (distance < FP._0_01)
                return;

            FPVector2 centripetalDir = -toTarget.Normalized;
            
            FPVector2 tangentialDir = RotationDirection == VortexRotation.Clockwise
                ? new FPVector2(toTarget.Y, -toTarget.X).Normalized
                : new FPVector2(-toTarget.Y, toTarget.X).Normalized;

            FPVector2 totalForce = centripetalDir * CentripetalForce + tangentialDir * TangentialForce;

            if (frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
            {
                hitReaction->ApplyKnockback(frame, target, totalForce * frame.DeltaTime, FP._0_10);
            }
        }

        private void ApplyStun(Frame frame, EntityRef target)
        {
        }

        private void ApplyVortexDamage(Frame frame, EntityRef target)
        {
        }
    }

    public enum VortexRotation
    {
        Clockwise,
        CounterClockwise
    }
}
