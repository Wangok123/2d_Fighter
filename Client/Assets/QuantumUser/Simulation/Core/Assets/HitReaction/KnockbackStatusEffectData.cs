using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial class KnockbackStatusEffectData : AssetObject
    {
        [Header("Common Knockback Settings")]
        [Tooltip("击退类型")]
        public AttackKnockbackType KnockbackType = AttackKnockbackType.AwayFromAttacker;
        
        [Header("Physics Knockback Settings")]
        [Tooltip("击退力度（仅Physics2D模式使用）")]
        public FP KnockbackForce = 5;
        
        [Space]
        [Header("Character Controller Knockback Settings")]
        
        [Tooltip("击退持续时间")]
        public FP KnockBackDuration = FP._0_25;
        
        [Tooltip("固定击退方向（仅当类型为Fixed时使用）")]
        public FPVector2 FixedKnockbackDirection = new FPVector2(FP._1, FP._0_50);
        
        [Tooltip("X轴击退曲线")]
        public FPAnimationCurve KnockbackCurveX;
        
        [Tooltip("Y轴击退曲线")]
        public FPAnimationCurve KnockbackCurveY;
        
        [Tooltip("X轴击退距离")]
        public FP KnockbackDistanceX = 3;
        
        [Tooltip("Y轴击退距离")]
        public FP KnockbackDistanceY = 2;

        public FPVector2 GetKnockbackDirection(Frame frame, EntityRef attacker, FPVector2 attackerPos, FPVector2 targetPos)
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
                    // 修改：根据攻击者朝向调整固定方向
                    bool attackerFacingRight = GetIsFacingRight(frame, attacker);
                    FPVector2 directionWithFacing = new FPVector2(
                        attackerFacingRight ? FixedKnockbackDirection.X : -FixedKnockbackDirection.X,
                        FixedKnockbackDirection.Y
                    );
                    return directionWithFacing.Normalized;
            }

            return FixedKnockbackDirection.Normalized;
        }
        
        public FPVector2 GetKnockbackDirection(Frame frame, bool isFacingRight, FPVector2 attackerPos, FPVector2 targetPos)
        {
            switch (KnockbackType)
            {
                case AttackKnockbackType.AwayFromAttacker:
                    FPVector2 awayDirection = targetPos - attackerPos;
                    return awayDirection.Normalized;

                case AttackKnockbackType.AttackerFacingDirection:
                    return new FPVector2(isFacingRight ? FP._1 : -FP._1, FixedKnockbackDirection.Y).Normalized;

                case AttackKnockbackType.Up:
                    return FPVector2.Up;

                case AttackKnockbackType.Fixed:
                    // 修改：根据攻击者朝向调整固定方向
                    FPVector2 directionWithFacing = new FPVector2(
                        isFacingRight ? FixedKnockbackDirection.X : -FixedKnockbackDirection.X,
                        FixedKnockbackDirection.Y
                    );
                    return directionWithFacing.Normalized;
            }

            return FixedKnockbackDirection.Normalized;
        }

        private bool GetIsFacingRight(Frame frame, EntityRef entityRef)
        {
            if (frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
            {
                return movement->IsFacingRight;
            }
            return true;
        }
    }
}
