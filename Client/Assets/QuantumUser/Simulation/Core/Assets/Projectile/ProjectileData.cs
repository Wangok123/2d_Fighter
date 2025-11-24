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
        
        public virtual void OnInitialize(Frame frame, EntityRef projectileEntity, 
            ProjectileComponent* projectile, EntityRef owner)
        {
            // 基础实现：设置速度（大多数弹道都需要）
            // 子类可以重写此方法
        }

        public virtual bool OnUpdateMovement(Frame frame, EntityRef projectileEntity, 
            ProjectileComponent* projectile, Transform2D* transform)
        {
            // 基础实现：直线移动
            transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
            return true;
        }
        
        public virtual bool OnHandleHit(Frame frame, EntityRef projectileEntity, 
            ProjectileComponent* projectile, EntityRef target, FPVector2 hitPoint)
        {
            return false; // 默认不处理特殊行为
        }
    }
}