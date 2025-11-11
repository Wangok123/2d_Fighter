using Photon.Deterministic;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    public unsafe partial class ProjectileData : AssetObject
    {
        [Header("基础设置")]
        
        [Tooltip("生命周期（秒）")]
        public FP Lifetime = 3;
        
        [Tooltip("视觉Prototype")]
        public EntityPrototype VisualPrototype;

        [Header("伤害设置")]
        [Tooltip("基础伤害")]
        public FP BaseDamage = 10;
        
        [Tooltip("击退力度")]
        public FP KnockbackForce = 5;
        
        [Tooltip("击退类型")]
        public KnockbackDirectionType KnockbackType = KnockbackDirectionType.AwayFromSource;
        
        [Tooltip("固定击退方向（仅当类型为Fixed时使用）")]
        public FPVector2 FixedKnockbackDirection = new FPVector2(FP._1, FP._0_50);
        
        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_25;

        [Header("碰撞设置")]
        [Tooltip("碰撞形状")]
        public Shape2DConfig CollisionShape;
        
        [Tooltip("碰撞层")]
        public LayerMask CollisionLayer = 1 << 6;
        
        [Tooltip("是否穿透目标")]
        public bool PierceTargets = false;
        
        [Tooltip("最大穿透数量（-1为无限）")]
        public int MaxPierceCount = 1;
        
        [Tooltip("是否被环境阻挡")]
        public bool BlockedByEnvironment = true;

        public virtual FP GetBaseDamage(Frame frame, EntityRef projectileEntity)
        {
            return BaseDamage;
        }

        public virtual FP GetKnockbackForce(Frame frame, EntityRef projectileEntity)
        {
            return KnockbackForce;
        }

        public virtual FPVector2 GetKnockbackDirection(Frame frame, EntityRef projectileEntity, EntityRef attacker, EntityRef target, FPVector2 hitPoint)
        {
            switch (KnockbackType)
            {
                case KnockbackDirectionType.AwayFromSource:
                    if (frame.Unsafe.TryGetPointer<Transform2D>(projectileEntity, out var projectileTransform) &&
                        frame.Unsafe.TryGetPointer<Transform2D>(target, out var targetTransform))
                    {
                        FPVector2 direction = targetTransform->Position - projectileTransform->Position;
                        return direction.Normalized;
                    }
                    break;

                case KnockbackDirectionType.AwayFromAttacker:
                    if (frame.Unsafe.TryGetPointer<Transform2D>(attacker, out var attackerTransform) &&
                        frame.Unsafe.TryGetPointer<Transform2D>(target, out var targetTransform2))
                    {
                        FPVector2 direction = targetTransform2->Position - attackerTransform->Position;
                        return direction.Normalized;
                    }
                    break;

                case KnockbackDirectionType.ProjectileDirection:
                    if (frame.Unsafe.TryGetPointer<ProjectileComponent>(projectileEntity, out var projectileComp))
                    {
                        return projectileComp->Direction.Normalized;
                    }
                    break;

                case KnockbackDirectionType.Up:
                    return FPVector2.Up;

                case KnockbackDirectionType.Fixed:
                    return FixedKnockbackDirection.Normalized;
            }

            return FixedKnockbackDirection.Normalized;
        }

        public virtual bool ShouldDestroyOnHit(Frame frame, EntityRef projectileEntity, EntityRef target)
        {
            if (!frame.Unsafe.TryGetPointer<ProjectileComponent>(projectileEntity, out var projectile))
                return true;

            if (!PierceTargets)
                return true;

            if (MaxPierceCount >= 0 && projectile->CurrentPierceCount >= MaxPierceCount)
                return true;

            return false;
        }

        public virtual void OnCustomUpdate(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
        }
    }
}
