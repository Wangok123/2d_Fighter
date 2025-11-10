using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class StraightProjectileData : ProjectileData
    {
        [Header("直线弹道设置")]
        [Tooltip("移动速度")]
        public FP MoveSpeed = 10;

        public override void OnProjectileSpawned(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef owner)
        {
            base.OnProjectileSpawned(frame, projectileEntity, projectile, owner);
            projectile->Speed = MoveSpeed;
        }

        public override void OnProjectileUpdate(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
            base.OnProjectileUpdate(frame, projectileEntity, projectile);
            
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
            transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
        }
    }
}