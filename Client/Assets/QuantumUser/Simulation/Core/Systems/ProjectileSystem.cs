using Photon.Deterministic;
using Quantum.Physics2D;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class ProjectileSystem: SystemMainThreadFilter<ProjectileSystem.Filter>, ISignalSpawnProjectile,
        ISignalDestroyProjectile
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

            projectileData.OnProjectileUpdate(frame, filter.Entity, filter.Projectile);
            
            CheckCollisions(frame, filter.Entity, filter.Projectile, projectileData);
        }

        public void SpawnProjectile(Frame frame, AssetRef<ProjectileData> projectileData, FPVector2 position, FPVector2 direction, EntityRef owner)
        {
            ProjectileData data = frame.FindAsset<ProjectileData>(projectileData.Id);

            EntityRef projectileEntity = default;
            if (data.VisualPrototype == null)
            {
                Debug.LogError("ProjectileData VisualPrototype is null! Cannot spawn projectile entity.");
                return;
            }

            projectileEntity = frame.Create(data.VisualPrototype);

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
            transform->Position = position;
            transform->Rotation = FPMath.Atan2(direction.Y, direction.X) * FP.Rad2Deg;

            ProjectileComponent* projectile = frame.Unsafe.GetPointer<ProjectileComponent>(projectileEntity);
            projectile->ProjectileData = data;
            projectile->Owner = owner;
            projectile->Direction = direction.Normalized;
            projectile->IsActive = true;
            projectile->PierceTargets = data.PierceTargets;
            projectile->MaxPierceCount = data.MaxPierceCount;
            projectile->CurrentPierceCount = 0;
            projectile->LifetimeTimer = FrameTimer.FromSeconds(frame, data.Lifetime);
            projectile->HitEntities = frame.AllocateList<EntityRef>();

            data.OnProjectileSpawned(frame, projectileEntity, projectile, owner);

            frame.Events.OnProjectileSpawned(projectileEntity, owner);
        }

        public void DestroyProjectile(Frame frame, EntityRef projectile, ProjectileDestroyReason reason)
        {
            if (!frame.Unsafe.TryGetPointer<ProjectileComponent>(projectile, out var projectileComponent))
                return;
            
            if (!projectileComponent->IsActive)
                return;

            projectileComponent->IsActive = false;

            ProjectileData projectileData = frame.FindAsset<ProjectileData>(projectileComponent->ProjectileData.Id);
            projectileData.OnProjectileDestroyed(frame, projectile, projectileComponent, reason);

            frame.FreeList(projectileComponent->HitEntities);
            frame.Events.OnProjectileDestroyed(projectile, reason);
            frame.Destroy(projectile);
        }
        
        private void CheckCollisions(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile, ProjectileData projectileData)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);

            var shape = projectileData.CollisionShape.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, projectileData.CollisionLayer, QueryOptions.HitDynamics | QueryOptions.HitStatics);

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

                    if (projectileData.OnHitTarget(frame, projectileEntity, projectile, hit.Entity, hit.Point))
                    {
                        hitList.Add(hit.Entity);
                        projectile->CurrentPierceCount++;

                        frame.Events.OnProjectileHitTarget(projectileEntity, hit.Entity, hit.Point);

                        if (!projectileData.PierceTargets || 
                            (projectileData.MaxPierceCount >= 0 && projectile->CurrentPierceCount >= projectileData.MaxPierceCount))
                        {
                            frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.HitTarget);
                            return;
                        }
                    }
                }
            }
        }
    }
}