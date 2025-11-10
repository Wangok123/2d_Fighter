using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class ArcProjectileData : ProjectileData
    {
        [Header("抛物线弹道设置")]
        [Tooltip("初始速度")]
        public FP InitialSpeed = 10;
        
        [Tooltip("重力加速度")]
        public FP Gravity = 10;
        
        [Tooltip("发射角度（度）")]
        public FP LaunchAngle = 45;
        
        [Tooltip("是否使用自定义角度（false则根据方向自动计算）")]
        public bool UseCustomAngle = false;

        public override void OnProjectileSpawned(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef owner)
        {
            base.OnProjectileSpawned(frame, projectileEntity, projectile, owner);
            
            if (UseCustomAngle)
            {
                FP angleRad = LaunchAngle * FP.Deg2Rad;
                FP velocityX = FPMath.Cos(angleRad) * InitialSpeed;
                FP velocityY = FPMath.Sin(angleRad) * InitialSpeed;
                
                projectile->Direction = new FPVector2(velocityX, velocityY);
            }
            else
            {
                projectile->Direction = projectile->Direction.Normalized * InitialSpeed;
            }
            
            projectile->Speed = InitialSpeed;
        }

        public override void OnProjectileUpdate(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
            base.OnProjectileUpdate(frame, projectileEntity, projectile);
            
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
            
            projectile->Direction += FPVector2.Down * Gravity * frame.DeltaTime;
            transform->Position += projectile->Direction * frame.DeltaTime;
            transform->Rotation = FPMath.Atan2(projectile->Direction.Y, projectile->Direction.X) * FP.Rad2Deg;
        }
    }
}