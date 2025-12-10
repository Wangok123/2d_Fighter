using Photon.Deterministic;
using Quantum.Physics2D;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class ProjectileSystem : SystemMainThreadFilter<ProjectileSystem.Filter>,
        ISignalSpawnProjectile,
        ISignalDestroyProjectile,
        ISignalOnProjectileHitTarget
    {
        public struct Filter
        {
            public EntityRef Entity;
            public ProjectileComponent* Projectile;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.Projectile->IsActive)
                return;

            ProjectileData projectileData = frame.FindAsset<ProjectileData>(filter.Projectile->ProjectileData.Id);

            if (!filter.Projectile->LifetimeTimer.IsRunning(frame))
            {
                frame.Signals.DestroyProjectile(filter.Entity, ProjectileDestroyReason.Lifetime);
                return;
            }

            bool shouldContinue = UpdateProjectileMovement(frame, filter.Entity, filter.Projectile, projectileData);

            if (!shouldContinue)
                return;

            CheckCollisions(frame, filter.Entity, filter.Projectile, projectileData);
        }

        public void SpawnProjectile(Frame frame, AssetRef<ProjectileData> projectileDataRef, FPVector2 position,
            FPVector2 direction, EntityRef owner)
        {
            ProjectileData data = frame.FindAsset<ProjectileData>(projectileDataRef.Id);

            if (data.VisualPrototype == null)
            {
                Debug.LogError("ProjectileData VisualPrototype is null! Cannot spawn projectile entity.");
                return;
            }

            EntityRef projectileEntity = frame.Create(data.VisualPrototype);

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
            transform->Position = position;
            transform->Rotation = FPMath.Atan2(direction.Y, direction.X) * FP.Rad2Deg;

            ProjectileComponent* projectile = frame.Unsafe.GetPointer<ProjectileComponent>(projectileEntity);
            projectile->ProjectileData = projectileDataRef;
            projectile->Owner = owner;
            projectile->Direction = direction.Normalized;
            projectile->IsActive = true;
            projectile->PierceTargets = data.PierceTargets;
            projectile->MaxPierceCount = data.MaxPierceCount;
            projectile->CurrentPierceCount = 0;
            projectile->LifetimeTimer = FrameTimer.FromSeconds(frame, data.Lifetime);
            projectile->HitEntities = frame.AllocateList<EntityRef>();

            InitializeProjectile(frame, projectileEntity, projectile, data, owner);

            frame.Events.OnProjectileSpawned(projectileEntity, owner);
        }

        public void DestroyProjectile(Frame frame, EntityRef projectile, ProjectileDestroyReason reason)
        {
            if (!frame.Unsafe.TryGetPointer<ProjectileComponent>(projectile, out var projectileComponent))
                return;

            if (!projectileComponent->IsActive)
                return;

            projectileComponent->IsActive = false;

            frame.FreeList(projectileComponent->HitEntities);
            frame.Events.OnProjectileDestroyed(projectile, reason);
            frame.Destroy(projectile);
        }

        public void OnProjectileHitTarget(Frame frame, EntityRef projectile, EntityRef target, FPVector2 hitPoint)
        {
            if (!frame.Unsafe.TryGetPointer<ProjectileComponent>(projectile, out var projectileComponent))
                return;

            ProjectileData projectileData = frame.FindAsset<ProjectileData>(projectileComponent->ProjectileData.Id);

            if (HandleSpecialHitBehavior(frame, projectile, projectileComponent, projectileData, target, hitPoint))
                return;
            
            AssetRef<KnockbackStatusEffectData> knockbackDataRef = projectileData.GetKnockbackStatusEffectData(frame, projectile);
    
            if (!knockbackDataRef.Id.IsValid)
            {
                return;
            }

            KnockbackStatusEffectData knockbackData = frame.FindAsset<KnockbackStatusEffectData>(knockbackDataRef.Id);
    
            Transform2D* projectileTransform = frame.Unsafe.GetPointer<Transform2D>(projectile);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);
    
            FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(
                frame, 
                projectile, 
                projectileTransform->Position, 
                targetTransform->Position
            );

            ApplyKnockbackToTarget(frame, target, knockbackData, knockbackDirection, knockbackDataRef);
        }

        private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData, 
            FPVector2 knockbackDirection, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
        {
            frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection, knockbackDataRef);
        }


        private void InitializeProjectile(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData data, EntityRef owner)
        {
            data.OnInitialize(frame, projectileEntity, projectile, owner);
        }

        private bool UpdateProjectileMovement(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData data)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
    
            // 直接调用多态方法
            return data.OnUpdateMovement(frame, projectileEntity, projectile, transform);
        }

        private bool HandleSpecialHitBehavior(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData data, EntityRef target, FPVector2 hitPoint)
        {
            return data.OnHandleHit(frame, projectileEntity, projectile, target, hitPoint);
        }

        private void CheckCollisions(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData projectileData)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);

            var shape = projectileData.CollisionShape.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, projectileData.CollisionLayer);

            if (hits.Count > 0)
            {
                var hitList = frame.ResolveList(projectile->HitEntities);

                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == projectile->Owner)
                        continue;

                    if (hitList.Contains(hit.Entity))
                        continue;

                    hitList.Add(hit.Entity);
                    projectile->CurrentPierceCount++;

                    frame.Signals.OnProjectileHitTarget(projectileEntity, hit.Entity, hit.Point);
                    frame.Events.OnProjectileHitTarget(projectileEntity, hit.Entity, hit.Point);

                    if (projectileData.ShouldDestroyOnHit(frame, projectileEntity, hit.Entity))
                    {
                        frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.HitTarget);
                        return;
                    }
                }
            }
        }
    }
}