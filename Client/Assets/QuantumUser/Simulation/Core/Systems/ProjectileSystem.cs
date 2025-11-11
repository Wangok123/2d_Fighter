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

            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            ProjectileData projectileData = frame.FindAsset<ProjectileData>(projectileComponent->ProjectileData.Id);

            if (HandleSpecialHitBehavior(frame, projectile, projectileComponent, projectileData, target, hitPoint))
                return;

            FPVector2 knockbackDirection =
                projectileData.GetKnockbackDirection(frame, projectile, projectileComponent->Owner, target, hitPoint);
            FP knockbackForce = projectileData.GetKnockbackForce(frame, projectile);
            FPVector2 knockbackVelocity = knockbackDirection * knockbackForce;

            hitReaction->ApplyKnockback(frame, target, knockbackVelocity, projectileData.HitstunDuration);
        }

        private void InitializeProjectile(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData data, EntityRef owner)
        {
            if (data is StraightProjectileData straightData)
            {
                projectile->Speed = straightData.MoveSpeed;
            }
            else if (data is HomingProjectileData homingData)
            {
                projectile->Speed = homingData.MoveSpeed;
            }
            else if (data is ArcProjectileData arcData)
            {
                projectile->Speed = arcData.InitialSpeed;
            }
            else if (data is BoomerangProjectileData boomerangData)
            {
                projectile->Speed = boomerangData.ForwardSpeed;

                if (frame.Unsafe.TryGetPointer<BoomerangRuntimeComponent>(projectileEntity, out var boomerangRuntime))
                {
                    boomerangRuntime->CurrentPhase = BoomerangPhase.Forward;
                    boomerangRuntime->DistanceTraveled = FP._0;

                    if (boomerangData.UseKCC &&
                        frame.Unsafe.TryGetPointer<CharacterController2D>(projectileEntity, out var kcc))
                    {
                        InitializeKCC(frame, kcc, boomerangData.KCCConfig);
                        kcc->Velocity = projectile->Direction * boomerangData.ForwardSpeed;
                    }
                }
                else
                {
                    Debug.LogError("BoomerangRuntimeComponent not found on projectile entity!");
                }
            }
            else if (data is GrenadeProjectileData grenadeData)
            {
                projectile->Speed = grenadeData.ThrowSpeed;

                if (!frame.Unsafe.TryGetPointer<GrenadeRuntimeComponent>(projectileEntity, out var grenadeRuntime))
                {
                    Log.Error(
                        "[Grenade Init] GrenadeRuntimeComponent not found! Add QPrototypeGrenadeRuntimeComponent to prefab");
                    return;
                }

                grenadeRuntime->TimeAlive = FP._0;
                grenadeRuntime->HasDetonated = false;

                if (!frame.Unsafe.TryGetPointer<PhysicsBody2D>(projectileEntity, out var body))
                {
                    Log.Error("[Grenade Init] PhysicsBody2D not found! Add QPrototypePhysicsBody2D to prefab");
                    return;
                }

                // 修改：计算投掷方向
                FP horizontalSign = projectile->Direction.X >= FP._0 ? FP._1 : -FP._1;

                if (FPMath.Abs(projectile->Direction.X) < FP._0_01)
                {
                    if (frame.Unsafe.TryGetPointer<Transform2D>(owner, out var ownerTransform))
                    {
                        FP ownerRotation = ownerTransform->Rotation;
                        FP rotationRad = ownerRotation * FP.Deg2Rad;
                        FP cosRot = FPMath.Cos(rotationRad);
                        horizontalSign = cosRot >= FP._0 ? FP._1 : -FP._1;
                    }
                    else
                    {
                        horizontalSign = FP._1;
                    }
                }

                // 修改：直接设置PhysicsBody的速度（物理引擎会自动应用重力和碰撞）
                body->Velocity = new FPVector2(
                    horizontalSign * grenadeData.ThrowSpeed,
                    grenadeData.ThrowArc
                );
            }
        }

        private void InitializeKCC(Frame frame, CharacterController2D* kcc,
            AssetRef<CharacterController2DConfig> configRef)
        {
            if (configRef.Id.IsValid)
            {
                var config = frame.FindAsset<CharacterController2DConfig>(configRef.Id);
                kcc->Init(frame, config);
            }
            else
            {
                kcc->Init(frame);
            }
        }

        private bool UpdateProjectileMovement(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData data)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);

            if (data is StraightProjectileData)
            {
                transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
            }
            else if (data is HomingProjectileData homingData)
            {
                if (homingData.AutoTargetNearest)
                {
                    EntityRef target = FindNearestTarget(frame, transform->Position, projectile->Owner,
                        homingData.TrackingRange);
                    if (target != EntityRef.None)
                    {
                        Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);
                        FPVector2 directionToTarget = (targetTransform->Position - transform->Position).Normalized;

                        projectile->Direction = FPVector2.Lerp(projectile->Direction, directionToTarget,
                            homingData.TurnSpeed * frame.DeltaTime);
                        projectile->Direction = projectile->Direction.Normalized;
                    }
                }


                transform->Position += projectile->Direction * projectile->Speed * frame.DeltaTime;
                transform->Rotation = FPMath.Atan2(projectile->Direction.Y, projectile->Direction.X) * FP.Rad2Deg;
            }
            else if (data is BoomerangProjectileData boomerangData)
            {
                if (frame.Unsafe.TryGetPointer<BoomerangRuntimeComponent>(projectileEntity, out var boomerangRuntime))
                {
                    if (boomerangRuntime->CurrentPhase == BoomerangPhase.Forward)
                    {
                        FP speed = boomerangData.ForwardSpeed;
                        FP moveDistance = speed * frame.DeltaTime;
                        transform->Position += projectile->Direction * moveDistance;
                        boomerangRuntime->DistanceTraveled += moveDistance;

                        if (boomerangRuntime->DistanceTraveled >= boomerangData.MaxDistance)
                        {
                            boomerangRuntime->CurrentPhase = BoomerangPhase.Return;
                        }
                    }
                    else if (boomerangRuntime->CurrentPhase == BoomerangPhase.Return)
                    {
                        if (frame.Unsafe.TryGetPointer<Transform2D>(projectile->Owner, out var ownerTransform))
                        {
                            FPVector2 directionToOwner = (ownerTransform->Position - transform->Position).Normalized;
                            FP speed = boomerangData.ReturnSpeed;
                            transform->Position += directionToOwner * speed * frame.DeltaTime;
                            if (FPVector2.Distance(transform->Position, ownerTransform->Position) <
                                boomerangData.CatchDistance)
                            {
                                frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.HitTarget);
                                return false;
                            }
                        }
                    }
                }
            }
            else if (data is GrenadeProjectileData grenadeData)
            {
                if (frame.Unsafe.TryGetPointer<GrenadeRuntimeComponent>(projectileEntity, out var grenadeRuntime))
                {
                    if (!grenadeRuntime->HasDetonated)
                    {
                        grenadeRuntime->TimeAlive += frame.DeltaTime;
                        if (grenadeData.DetonateType == GrenadeDetonateType.Timer &&
                            grenadeRuntime->TimeAlive >= grenadeData.DetonationDelay)
                        {
                            TriggerGrenadeExplosion(frame, projectileEntity, grenadeData, grenadeRuntime);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool HandleSpecialHitBehavior(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData data, EntityRef target, FPVector2 hitPoint)
        {
            if (data is GrenadeProjectileData grenadeData)
            {
                if (!frame.Unsafe.TryGetPointer<GrenadeRuntimeComponent>(projectileEntity, out var grenadeRuntime))
                    return false;

                if (grenadeRuntime->HasDetonated)
                    return true;

                if (grenadeData.CanDetonateInAir || grenadeData.DetonateType == GrenadeDetonateType.FirstContact)
                {
                    TriggerGrenadeExplosion(frame, projectileEntity, grenadeData, grenadeRuntime);
                    return true;
                }
            }

            return false;
        }

        private void TriggerGrenadeExplosion(Frame frame, EntityRef projectileEntity, GrenadeProjectileData grenadeData,
            GrenadeRuntimeComponent* grenadeRuntime)
        {
            grenadeRuntime->HasDetonated = true;

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
            FPVector2 explosionPosition = transform->Position;

            if (grenadeData.SpawnSkillFieldOnExplosion && grenadeData.ExplosionFieldData.Id.IsValid)
            {
                frame.Signals.SpawnSkillField(grenadeData.ExplosionFieldData, explosionPosition,
                    frame.Unsafe.GetPointer<ProjectileComponent>(projectileEntity)->Owner);
            }

            frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.HitTarget);
        }

        private EntityRef FindNearestTarget(Frame frame, FPVector2 position, EntityRef owner, FP trackingRange)
        {
            EntityRef nearest = EntityRef.None;
            FP nearestDistance = FP.MaxValue;

            var filter = frame.Filter<Transform2D, HitReactionComponent>();

            while (filter.NextUnsafe(out var entity, out var transform, out var hitReaction))
            {
                if (entity == owner)
                    continue;

                FP distance = FPVector2.Distance(position, transform->Position);

                if (trackingRange > 0 && distance > trackingRange)
                    continue;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = entity;
                }
            }

            return nearest;
        }

        private void CheckCollisions(Frame frame, EntityRef projectileEntity, ProjectileComponent* projectile,
            ProjectileData projectileData)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);

            var shape = projectileData.CollisionShape.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, projectileData.CollisionLayer,
                QueryOptions.HitDynamics | QueryOptions.HitStatics);

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

                    if (!frame.Has<HitReactionComponent>(hit.Entity))
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