using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class GrenadeProjectileData : ArcProjectileData
    {
        [Header("手榴弹设置")]
        [Tooltip("爆炸区域数据")]
        public AssetRef<SkillFieldData> ExplosionFieldData;
        
        [Tooltip("最小落地高度（低于此高度视为落地）")]
        public FP GroundHeight = FP._0;
        
        [Tooltip("落地触发还是碰撞触发")]
        public GrenadeDetonateType DetonateType = GrenadeDetonateType.GroundContact;
        
        [Tooltip("是否允许空中碰撞触发")]
        public bool CanDetonateInAir = false;

        private bool _hasDetonated = false;

        public override void OnProjectileSpawned(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef owner)
        {
            base.OnProjectileSpawned(frame, projectileEntity, projectile, owner);
            _hasDetonated = false;
        }

        public override void OnProjectileUpdate(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
            base.OnProjectileUpdate(frame, projectileEntity, projectile);

            if (_hasDetonated) return;

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);

            if (DetonateType == GrenadeDetonateType.GroundContact)
            {
                if (transform->Position.Y <= GroundHeight && projectile->Direction.Y <= FP._0)
                {
                    SpawnExplosionField(frame, projectile, transform->Position);
                    _hasDetonated = true;
                    frame.Destroy(projectileEntity);
                }
            }
        }

        public override bool OnHitTarget(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef target, FPVector2 hitPoint)
        {
            if (_hasDetonated) return true;

            if (CanDetonateInAir || DetonateType == GrenadeDetonateType.FirstContact)
            {
                SpawnExplosionField(frame, projectile, hitPoint);
                _hasDetonated = true;
                return true;
            }

            return false;
        }

        private void SpawnExplosionField(Frame frame, ProjectileComponent* projectile, FPVector2 position)
        {
            if (ExplosionFieldData.Id.IsValid)
            {
                frame.SpawnSkillField(ExplosionFieldData, position, projectile->Owner);
            }
        }
    }

    public enum GrenadeDetonateType
    {
        GroundContact,
        FirstContact
    }
}
