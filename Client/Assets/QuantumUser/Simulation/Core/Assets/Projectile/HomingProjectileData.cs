using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class HomingProjectileData : ProjectileData
    {
        [Header("追踪弹道设置")] [Tooltip("移动速度")] public FP MoveSpeed = 10;

        [Tooltip("自动追踪最近目标")] public bool AutoTargetNearest = true;

        [Tooltip("追踪转向速度")] public FP TurnSpeed = 5;

        [Tooltip("追踪范围（0为无限制）")] public FP TrackingRange = 0;

        public override void OnInitialize(Frame frame, EntityRef projectileEntity,
            ProjectileComponent* projectile, EntityRef owner)
        {
            projectile->Speed = MoveSpeed;
        }

        public override bool OnUpdateMovement(Frame frame, EntityRef projectileEntity,
            ProjectileComponent* projectile, Transform2D* transform)
        {
            if (AutoTargetNearest)
            {
                EntityRef target = FindNearestTarget(frame, transform->Position,
                    projectile->Owner, TrackingRange);

                if (target != EntityRef.None)
                {
                    Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);
                    FPVector2 directionToTarget = (targetTransform->Position - transform->Position).Normalized;

                    projectile->Direction = FPVector2.Lerp(projectile->Direction, directionToTarget,
                        TurnSpeed * frame.DeltaTime);
                    projectile->Direction = projectile->Direction.Normalized;
                }
            }

            transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
            transform->Rotation = FPMath.Atan2(projectile->Direction.Y, projectile->Direction.X) * FP.Rad2Deg;

            return true;
        }

        private EntityRef FindNearestTarget(Frame frame, FPVector2 position,
            EntityRef owner, FP trackingRange)
        {
            // 从ProjectileSystem移动到这里
            EntityRef nearest = EntityRef.None;
            FP nearestDistance = FP.MaxValue;

            var filter = frame.Filter<Transform2D>();
            while (filter.NextUnsafe(out var entity, out var transform))
            {
                if (entity == owner)
                    continue;

                FP distance = FPVector2.Distance(position, transform->Position);

                if (trackingRange > 0 && distance > trackingRange)
                    continue;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = entity;
                }
            }

            return nearest;
        }
    }
}