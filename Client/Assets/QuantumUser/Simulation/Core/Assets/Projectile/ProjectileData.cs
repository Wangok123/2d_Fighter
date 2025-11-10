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
        
        [Tooltip("击退方向X")]
        public FP KnockbackDirectionX = FP._1;
        
        [Tooltip("击退方向Y")]
        public FP KnockbackDirectionY = FP._0_50;
        
        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_25;
        
        [Tooltip("受击类型")]
        public HitType HitType = HitType.Light;

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

        public virtual void OnProjectileSpawned(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef owner)
        {
        }

        public virtual void OnProjectileUpdate(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile)
        {
        }

        public virtual void OnProjectileDestroyed(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, ProjectileDestroyReason reason)
        {
        }

        public virtual bool OnHitTarget(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, EntityRef target, FPVector2 hitPoint)
        {
            if (frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
            {
                ApplyDamageAndKnockback(frame, projectile->Owner, target, hitReaction);
                return true;
            }

            return false;
        }

        protected virtual void ApplyDamageAndKnockback(Frame frame, EntityRef attacker, EntityRef target, HitReactionComponent* hitReaction)
        {
            FPVector2 knockbackDirection = new FPVector2(KnockbackDirectionX, KnockbackDirectionY).Normalized;
            FPVector2 knockbackVelocity = knockbackDirection * KnockbackForce;

            hitReaction->ApplyKnockback(frame, target, knockbackVelocity, HitstunDuration);
        }
    }
}
