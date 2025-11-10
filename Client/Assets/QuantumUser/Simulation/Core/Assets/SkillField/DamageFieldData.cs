using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class DamageFieldData : SkillFieldData
    {
        [Header("伤害设置")]
        [Tooltip("每Tick伤害")]
        public FP DamagePerTick = 5;
        
        [Tooltip("伤害类型")]
        public DamageType DamageType = DamageType.Fire;
        
        [Tooltip("是否造成DOT（持续伤害效果）")]
        public bool ApplyDOT = false;
        
        [Tooltip("DOT持续时间")]
        public FP DOTDuration = 3;
        
        [Tooltip("DOT每秒伤害")]
        public FP DOTDamagePerSecond = 2;

        [Header("击退设置")]
        [Tooltip("是否应用击退")]
        public bool ApplyKnockback = false;
        
        [Tooltip("击退力度")]
        public FP KnockbackForce = 3;
        
        [Tooltip("击退方向")]
        public KnockbackDirection KnockbackDirection = KnockbackDirection.FromCenter;
        
        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_25;

        public override void ApplyEffect(Frame frame, EntityRef skillFieldEntity, EntityRef target, FPVector2 hitPoint)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            SkillFieldComponent* skillField = frame.Unsafe.GetPointer<SkillFieldComponent>(skillFieldEntity);
            ApplyDamageToTarget(frame, skillField->Owner, target, hitPoint, hitReaction);
            
            if (ApplyDOT)
            {
                ApplyDOTEffect(frame, target);
            }
        }

        private void ApplyDamageToTarget(Frame frame, EntityRef owner, EntityRef target, FPVector2 hitPoint, HitReactionComponent* hitReaction)
        {
            if (ApplyKnockback)
            {
                FPVector2 knockbackVelocity = CalculateKnockback(frame, owner, target, hitPoint);
                hitReaction->ApplyKnockback(frame, target, knockbackVelocity, HitstunDuration);
            }
        }

        private FPVector2 CalculateKnockback(Frame frame, EntityRef owner, EntityRef target, FPVector2 hitPoint)
        {
            Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(owner);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 direction = KnockbackDirection switch
            {
                Quantum.KnockbackDirection.FromCenter => (targetTransform->Position - ownerTransform->Position).Normalized,
                Quantum.KnockbackDirection.FromHitPoint => (targetTransform->Position - hitPoint).Normalized,
                Quantum.KnockbackDirection.Up => FPVector2.Up,
                _ => FPVector2.Zero
            };

            return direction * KnockbackForce;
        }

        private void ApplyDOTEffect(Frame frame, EntityRef target)
        {
        }
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
        Dark,
        Holy
    }

    public enum KnockbackDirection
    {
        FromCenter,
        FromHitPoint,
        Up
    }
}
