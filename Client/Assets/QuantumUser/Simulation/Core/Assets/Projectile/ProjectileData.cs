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
        
        [Tooltip("击退配置数据")]
        public AssetRef<KnockbackStatusEffectData> KnockbackStatusEffectData;

        [Header("碰撞设置")]
        [Tooltip("碰撞形状")]
        public Shape2DConfig CollisionShape;
        
        [Tooltip("碰撞层")]
        public LayerMask CollisionLayer = 1 << 6;
        
        [Tooltip("是否穿透目标")]
        public bool PierceTargets = false;
        
        [Tooltip("最大穿透数量（-1为无限）")]
        public int MaxPierceCount = 1;

        public virtual FP GetBaseDamage(Frame frame, EntityRef projectileEntity)
        {
            return BaseDamage;
        }

        public virtual AssetRef<KnockbackStatusEffectData> GetKnockbackStatusEffectData(Frame frame, EntityRef projectileEntity)
        {
            return KnockbackStatusEffectData;
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