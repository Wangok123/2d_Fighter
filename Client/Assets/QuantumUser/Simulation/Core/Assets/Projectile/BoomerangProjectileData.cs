using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class BoomerangProjectileData : ProjectileData
    {
        [Header("回旋镖弹道设置")]
        [Tooltip("前进速度")]
        public FP ForwardSpeed = 10;
        
        [Tooltip("返回速度")]
        public FP ReturnSpeed = 12;
        
        [Tooltip("返回延迟")]
        public FP ReturnDelay = FP._0_50;
        
        [Tooltip("回收距离")]
        public FP CatchDistance = FP._0_50;
        
        [Tooltip("失去主人后的行为")]
        public BoomerangBehaviorOnOwnerLost BehaviorOnOwnerLost = BoomerangBehaviorOnOwnerLost.Destroy;
        
        [Tooltip("前进阶段是否旋转")]
        public bool RotateWhileForward = true;
        
        [Tooltip("旋转速度（度/秒）")]
        public FP RotationSpeed = 360;

        public override void OnProjectileSpawned(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef owner)
        {
            base.OnProjectileSpawned(frame, projectileEntity, projectile, owner);
            projectile->Speed = ForwardSpeed;
        }

        public override void OnProjectileUpdate(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
            base.OnProjectileUpdate(frame, projectileEntity, projectile);
            
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
            FP elapsed = projectile->LifetimeTimer.ElapsedSeconds(frame);
            
            if (elapsed < ReturnDelay)
            {
                UpdateForwardPhase(frame, transform, projectile);
            }
            else
            {
                UpdateReturnPhase(frame, projectileEntity, transform, projectile);
            }
        }

        private void UpdateForwardPhase(Frame frame, Transform2D* transform, ProjectileComponent* projectile)
        {
            transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
            
            if (RotateWhileForward)
            {
                transform->Rotation += RotationSpeed * frame.DeltaTime;
            }
            else
            {
                transform->Rotation = FPMath.Atan2(projectile->Direction.Y, projectile->Direction.X) * FP.Rad2Deg;
            }
        }

        private void UpdateReturnPhase(Frame frame, EntityRef projectileEntity, Transform2D* transform, ProjectileComponent* projectile)
        {
            if (!frame.Exists(projectile->Owner))
            {
                HandleOwnerLost(frame, projectileEntity, transform, projectile);
                return;
            }

            Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(projectile->Owner);
            FPVector2 directionToOwner = (ownerTransform->Position - transform->Position).Normalized;
            
            projectile->Direction = directionToOwner;
            transform->Position += directionToOwner * ReturnSpeed * frame.DeltaTime;
            
            if (RotateWhileForward)
            {
                transform->Rotation += RotationSpeed * frame.DeltaTime;
            }
            else
            {
                transform->Rotation = FPMath.Atan2(directionToOwner.Y, directionToOwner.X) * FP.Rad2Deg;
            }

            FP distance = FPVector2.Distance(transform->Position, ownerTransform->Position);
            if (distance < CatchDistance)
            {
                frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.Caught);
            }
        }

        private void HandleOwnerLost(Frame frame, EntityRef projectileEntity, Transform2D* transform, ProjectileComponent* projectile)
        {
            switch (BehaviorOnOwnerLost)
            {
                case BoomerangBehaviorOnOwnerLost.Destroy:
                    frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.Manual);
                    break;
                    
                case BoomerangBehaviorOnOwnerLost.ContinueStraight:
                    transform->Position += projectile->Direction * ReturnSpeed * frame.DeltaTime;
                    break;
                    
                case BoomerangBehaviorOnOwnerLost.FallDown:
                    projectile->Direction += FPVector2.Down * 10 * frame.DeltaTime;
                    transform->Position += projectile->Direction * frame.DeltaTime;
                    break;
            }
        }
    }

    public enum BoomerangBehaviorOnOwnerLost
    {
        Destroy,
        ContinueStraight,
        FallDown
    }
}
