using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    [CreateAssetMenu(menuName = "Quantum/Assets/Projectile/BulletProjectile", order = 1)]
    public unsafe partial class BulletProjectileData : ProjectileData
    {
        [Header("子弹移动设置")]
        [Tooltip("移动速度")]
        public FP MoveSpeed = 10;
        
        [Tooltip("移动模式")]
        public ProjectileMovePattern MovePattern = ProjectileMovePattern.Straight;
        
        [Header("追踪设置（Homing模式）")]
        [Tooltip("自动追踪最近目标")]
        public bool AutoTargetNearest = false;
        
        [Tooltip("追踪转向速度")]
        public FP HomingTurnSpeed = 5;
        
        [Header("弧形弹道设置（Arc模式）")]
        [Tooltip("重力")]
        public FP Gravity = 10;
        
        [Header("回旋镖设置（Boomerang模式）")]
        [Tooltip("返回速度")]
        public FP ReturnSpeed = 12;
        
        [Tooltip("返回延迟")]
        public FP ReturnDelay = FP._0_50;

        public override void OnProjectileSpawned(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef owner)
        {
            base.OnProjectileSpawned(frame, projectileEntity, projectile, owner);
            projectile->Speed = MoveSpeed;
        }

        public override void OnProjectileUpdate(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
            base.OnProjectileUpdate(frame, projectileEntity, projectile);
            UpdateMovement(frame, projectileEntity, projectile);
        }

        private void UpdateMovement(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);

            switch (MovePattern)
            {
                case ProjectileMovePattern.Straight:
                    UpdateStraightMovement(frame, transform, projectile);
                    break;
                    
                case ProjectileMovePattern.Homing:
                    UpdateHomingMovement(frame, transform, projectile);
                    break;
                    
                case ProjectileMovePattern.Arc:
                    UpdateArcMovement(frame, transform, projectile);
                    break;
                    
                case ProjectileMovePattern.Boomerang:
                    UpdateBoomerangMovement(frame, projectileEntity, transform, projectile);
                    break;
            }
        }

        private void UpdateStraightMovement(Frame frame, Transform2D* transform, ProjectileComponent* projectile)
        {
            transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
        }

        private void UpdateHomingMovement(Frame frame, Transform2D* transform, ProjectileComponent* projectile)
        {
            if (AutoTargetNearest)
            {
                EntityRef target = FindNearestTarget(frame, transform->Position, projectile->Owner);
                if (target != EntityRef.None)
                {
                    Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);
                    FPVector2 directionToTarget = (targetTransform->Position - transform->Position).Normalized;
                    
                    projectile->Direction = FPVector2.Lerp(projectile->Direction, directionToTarget, HomingTurnSpeed * frame.DeltaTime);
                    projectile->Direction = projectile->Direction.Normalized;
                }
            }

            transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
            transform->Rotation = FPMath.Atan2(projectile->Direction.Y, projectile->Direction.X) * FP.Rad2Deg;
        }

        private void UpdateArcMovement(Frame frame, Transform2D* transform, ProjectileComponent* projectile)
        {
            projectile->Direction += FPVector2.Down * Gravity * frame.DeltaTime;
            transform->Position += projectile->Direction * frame.DeltaTime;
            transform->Rotation = FPMath.Atan2(projectile->Direction.Y, projectile->Direction.X) * FP.Rad2Deg;
        }

        private void UpdateBoomerangMovement(Frame frame, EntityRef projectileEntity, Transform2D* transform, ProjectileComponent* projectile)
        {
            FP elapsed = projectile->LifetimeTimer.ElapsedSeconds(frame);
            
            if (elapsed < ReturnDelay)
            {
                transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
            }
            else
            {
                if (!frame.Exists(projectile->Owner))
                {
                    frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.Manual);
                    return;
                }

                Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(projectile->Owner);
                FPVector2 directionToOwner = (ownerTransform->Position - transform->Position).Normalized;
                
                projectile->Direction = directionToOwner;
                transform->Position += directionToOwner * ReturnSpeed * frame.DeltaTime;
                transform->Rotation = FPMath.Atan2(directionToOwner.Y, directionToOwner.X) * FP.Rad2Deg;

                FP distance = FPVector2.Distance(transform->Position, ownerTransform->Position);
                if (distance < FP._0_50)
                {
                    frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.Manual);
                }
            }
        }

        private EntityRef FindNearestTarget(Frame frame, FPVector2 position, EntityRef owner)
        {
            EntityRef nearest = EntityRef.None;
            FP nearestDistance = FP.MaxValue;

            var filter = frame.Filter<Transform2D, HitReactionComponent>();
            
            while (filter.NextUnsafe(out var entity, out var transform, out var hitReaction))
            {
                if (entity == owner)
                    continue;

                FP distance = FPVector2.Distance(position, transform->Position);
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
