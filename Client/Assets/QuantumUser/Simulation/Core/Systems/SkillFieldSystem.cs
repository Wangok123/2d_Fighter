using Photon.Deterministic;
using Quantum.Physics2D;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class SkillFieldSystem : SystemMainThreadFilter<SkillFieldSystem.Filter>, 
        ISignalSpawnSkillField,
        ISignalDestroySkillField,
        ISignalOnSkillFieldApplyEffect
    {
        public struct Filter
        {
            public EntityRef Entity;
            public SkillFieldComponent* SkillField;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.SkillField->IsActive)
                return;

            SkillFieldData skillFieldData = frame.FindAsset<SkillFieldData>(filter.SkillField->SkillFieldData.Id);

            if (!filter.SkillField->LifetimeTimer.IsRunning(frame))
            {
                DestroySkillField(frame, filter.Entity);
                return;
            }

            UpdateSpecialFieldBehavior(frame, filter.Entity, filter.SkillField, skillFieldData);

            if (!filter.SkillField->TickTimer.IsRunning(frame))
            {
                ExecuteTick(frame, filter.Entity, filter.SkillField, skillFieldData);
                filter.SkillField->TickTimer = FrameTimer.FromSeconds(frame, skillFieldData.TickInterval);
            }

            skillFieldData.OnCustomTick(frame, filter.Entity, filter.SkillField);
        }

        public void SpawnSkillField(Frame frame, AssetRef<SkillFieldData> skillFieldDataRef, FPVector2 position, EntityRef owner)
        {
            SkillFieldData skillFieldData = frame.FindAsset<SkillFieldData>(skillFieldDataRef.Id);

            EntityRef skillFieldEntity;
            if (skillFieldData.VisualPrototype == null)
            {
                skillFieldEntity = frame.Create();
                frame.Add<Transform2D>(skillFieldEntity);
                frame.Add<SkillFieldComponent>(skillFieldEntity);
            }
            else
            {
                skillFieldEntity = frame.Create(skillFieldData.VisualPrototype);
            }

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            transform->Position = position;

            SkillFieldComponent* skillField = frame.Unsafe.GetPointer<SkillFieldComponent>(skillFieldEntity);
            skillField->SkillFieldData = skillFieldDataRef;
            skillField->Owner = owner;
            skillField->Center = position;
            skillField->IsActive = true;
            skillField->TickInterval = skillFieldData.TickInterval;
            skillField->LifetimeTimer = FrameTimer.FromSeconds(frame, skillFieldData.Duration);
            skillField->TickTimer = FrameTimer.FromSeconds(frame, skillFieldData.TickInterval);
            skillField->AffectedEntities = frame.AllocateList<EntityRef>();

            InitializeSpecialField(frame, skillFieldEntity, skillField, skillFieldData, owner, position);

            skillFieldData.OnCustomSpawn(frame, skillFieldEntity, skillField);

            frame.Signals.OnSkillFieldSpawned(skillFieldEntity, owner, position);
            frame.Events.OnSkillFieldSpawned(skillFieldEntity, owner, position);
        }

        public void DestroySkillField(Frame frame, EntityRef entityRef)
        {
            if (!frame.Unsafe.TryGetPointer<SkillFieldComponent>(entityRef, out var skillField))
                return;

            if (!skillField->IsActive)
                return;

            skillField->IsActive = false;

            SkillFieldData skillFieldData = frame.FindAsset<SkillFieldData>(skillField->SkillFieldData.Id);
            skillFieldData.OnCustomDestroy(frame, entityRef, skillField);

            frame.FreeList(skillField->AffectedEntities);
            frame.Events.OnSkillFieldDestroyed(entityRef);
            frame.Destroy(entityRef);
        }

        public void OnSkillFieldApplyEffect(Frame frame, EntityRef skillField, EntityRef target, FPVector2 hitPoint)
        {
            if (!frame.Unsafe.TryGetPointer<SkillFieldComponent>(skillField, out var skillFieldComponent))
                return;

            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            SkillFieldData skillFieldData = frame.FindAsset<SkillFieldData>(skillFieldComponent->SkillFieldData.Id);

            ApplyFieldEffect(frame, skillField, skillFieldComponent, skillFieldData, target, hitPoint, hitReaction);
        }

        private void InitializeSpecialField(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, SkillFieldData data, EntityRef owner, FPVector2 position)
        {
            if (data is DelayedExplosionFieldData delayedData)
            {
                if (!frame.Has<DelayedExplosionRuntimeComponent>(skillFieldEntity))
                {
                    frame.Add<DelayedExplosionRuntimeComponent>(skillFieldEntity);
                }

                if (delayedData.WarningEffect != null)
                {
                    EntityRef warning = frame.Create(delayedData.WarningEffect);
                    if (frame.Unsafe.TryGetPointer<Transform2D>(warning, out var warningTransform))
                    {
                        warningTransform->Position = position;
                    }
                }
            }
        }

        private void UpdateSpecialFieldBehavior(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, SkillFieldData data)
        {
            if (data is DelayedExplosionFieldData delayedData)
            {
                if (!frame.Unsafe.TryGetPointer<DelayedExplosionRuntimeComponent>(skillFieldEntity, out var runtime))
                    return;

                if (runtime->HasExploded)
                    return;

                FP elapsed = skillField->LifetimeTimer.ElapsedSeconds(frame);

                if (elapsed >= delayedData.ExplosionDelay)
                {
                    TriggerDelayedExplosion(frame, skillFieldEntity, skillField, delayedData);
                    runtime->HasExploded = true;
                    DestroySkillField(frame, skillFieldEntity);
                }
            }
        }

        private void TriggerDelayedExplosion(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, DelayedExplosionFieldData data)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            FPVector2 explosionCenter = transform->Position;

            if (data.ExplosionEffect != null)
            {
                EntityRef effect = frame.Create(data.ExplosionEffect);
                if (frame.Unsafe.TryGetPointer<Transform2D>(effect, out var effectTransform))
                {
                    effectTransform->Position = explosionCenter;
                }
            }

            var shape = data.EffectArea.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, data.TargetLayer, QueryOptions.HitDynamics);

            if (hits.Count > 0)
            {
                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];
                    
                    if (hit.Entity == skillField->Owner)
                        continue;

                    if (!data.ShouldAffectTarget(frame, skillField->Owner, hit.Entity))
                        continue;

                    if (!frame.Unsafe.TryGetPointer<Transform2D>(hit.Entity, out var targetTransform))
                        continue;

                    if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(hit.Entity, out var hitReaction))
                        continue;

                    FP distance = FPVector2.Distance(explosionCenter, targetTransform->Position);
                    FP damage = CalculateExplosionDamage(data, explosionCenter, targetTransform->Position);

                    if (data.ApplyKnockback)
                    {
                        FPVector2 knockbackVelocity = CalculateExplosionKnockback(data, explosionCenter, targetTransform->Position);
                        hitReaction->ApplyKnockback(frame, hit.Entity, knockbackVelocity, data.HitstunDuration);
                    }
                }
            }
        }

        private FP CalculateExplosionDamage(DelayedExplosionFieldData data, FPVector2 center, FPVector2 targetPos)
        {
            if (!data.DamageFalloff)
                return data.ExplosionDamage;

            FP distance = FPVector2.Distance(center, targetPos);
            FP maxRange = data.EffectArea.CircleRadius;

            if (distance <= FP._0_01)
                return data.ExplosionDamage * data.CenterDamageMultiplier;

            FP ratio = FP._1 - (distance / maxRange);
            FP damageMultiplier = FP._1 + (data.CenterDamageMultiplier - FP._1) * ratio;
            
            return data.ExplosionDamage * damageMultiplier;
        }

        private FPVector2 CalculateExplosionKnockback(DelayedExplosionFieldData data, FPVector2 center, FPVector2 targetPos)
        {
            FPVector2 direction = data.KnockbackType switch
            {
                ExplosionKnockbackType.FromCenter => (targetPos - center).Normalized,
                ExplosionKnockbackType.Up => FPVector2.Up,
                ExplosionKnockbackType.None => FPVector2.Zero,
                _ => FPVector2.Zero
            };

            return direction * data.KnockbackForce;
        }

        private void ApplyFieldEffect(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, SkillFieldData data, EntityRef target, FPVector2 hitPoint, HitReactionComponent* hitReaction)
        {
            if (data is DamageFieldData damageData)
            {
                if (damageData.ApplyKnockback)
                {
                    FPVector2 knockbackVelocity = CalculateDamageFieldKnockback(frame, damageData, skillField->Owner, target, hitPoint);
                    hitReaction->ApplyKnockback(frame, target, knockbackVelocity, damageData.HitstunDuration);
                }
            }
            else if (data is HealFieldData healData)
            {
            }
            else if (data is PushFieldData pushData)
            {
                ApplyPushFieldForce(frame, pushData, skillField->Owner, target, hitPoint, hitReaction);
            }
            else if (data is SlowFieldData slowData)
            {
            }
            else if (data is VortexFieldData vortexData)
            {
                ApplyVortexFieldForce(frame, skillFieldEntity, vortexData, target, hitReaction);
                
                if (vortexData.DealDamage)
                {
                }
            }
            else
            {
                FPVector2 knockbackDirection = data.GetKnockbackDirection(frame, skillFieldEntity, target, hitPoint);
                FP knockbackForce = data.GetKnockbackForce(frame, skillFieldEntity);
                FPVector2 knockbackVelocity = knockbackDirection * knockbackForce;

                hitReaction->ApplyKnockback(frame, target, knockbackVelocity, data.HitstunDuration);
            }
        }

        private FPVector2 CalculateDamageFieldKnockback(Frame frame, DamageFieldData data, EntityRef owner, EntityRef target, FPVector2 hitPoint)
        {
            Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(owner);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 direction = data.KnockbackDirection switch
            {
                KnockbackDirection.FromCenter => (targetTransform->Position - ownerTransform->Position).Normalized,
                KnockbackDirection.FromHitPoint => (targetTransform->Position - hitPoint).Normalized,
                KnockbackDirection.Up => FPVector2.Up,
                _ => FPVector2.Zero
            };

            return direction * data.KnockbackForce;
        }

        private void ApplyPushFieldForce(Frame frame, PushFieldData data, EntityRef owner, EntityRef target, FPVector2 hitPoint, HitReactionComponent* hitReaction)
        {
            Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(owner);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 forceDirection = CalculatePushFieldDirection(data, ownerTransform->Position, targetTransform->Position, hitPoint);
            FP forceMagnitude = CalculatePushFieldMagnitude(data, ownerTransform->Position, targetTransform->Position);

            FPVector2 force = forceDirection * forceMagnitude;
            hitReaction->ApplyKnockback(frame, target, force, FP._0_10);
        }

        private FPVector2 CalculatePushFieldDirection(PushFieldData data, FPVector2 center, FPVector2 targetPos, FPVector2 hitPoint)
        {
            FPVector2 baseDirection = data.Direction switch
            {
                ForceDirection.FromCenter => (targetPos - center).Normalized,
                ForceDirection.ToCenter => (center - targetPos).Normalized,
                ForceDirection.CustomDirection => data.CustomDirection.Normalized,
                _ => FPVector2.Zero
            };

            return data.FieldType == ForceFieldType.Push ? baseDirection : -baseDirection;
        }

        private FP CalculatePushFieldMagnitude(PushFieldData data, FPVector2 center, FPVector2 targetPos)
        {
            if (!data.FalloffWithDistance)
                return data.ForceStrength;

            FP distance = FPVector2.Distance(center, targetPos);
            if (distance >= data.MaxEffectRange)
                return FP._0;

            FP falloff = FP._1 - (distance / data.MaxEffectRange);
            return data.ForceStrength * falloff;
        }

        private void ApplyVortexFieldForce(Frame frame, EntityRef skillFieldEntity, VortexFieldData data, EntityRef target, HitReactionComponent* hitReaction)
        {
            Transform2D* fieldTransform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 toTarget = targetTransform->Position - fieldTransform->Position;
            FP distance = toTarget.Magnitude;

            if (distance < FP._0_01)
                return;

            FPVector2 centripetalDir = -toTarget.Normalized;
            
            FPVector2 tangentialDir = data.RotationDirection == VortexRotation.Clockwise
                ? new FPVector2(toTarget.Y, -toTarget.X).Normalized
                : new FPVector2(-toTarget.Y, toTarget.X).Normalized;

            FPVector2 totalForce = centripetalDir * data.CentripetalForce + tangentialDir * data.TangentialForce;

            hitReaction->ApplyKnockback(frame, target, totalForce * frame.DeltaTime, FP._0_10);
        }
        
        private void ExecuteTick(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, SkillFieldData skillFieldData)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);

            var shape = skillFieldData.EffectArea.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, skillFieldData.TargetLayer, QueryOptions.HitDynamics);

            var affectedList = frame.ResolveList(skillField->AffectedEntities);
            affectedList.Clear();

            if (hits.Count > 0)
            {
                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (!skillFieldData.ShouldAffectTarget(frame, skillField->Owner, hit.Entity))
                        continue;

                    OnSkillFieldApplyEffect(frame, skillFieldEntity, hit.Entity, hit.Point);
                    affectedList.Add(hit.Entity);
                }
            }
            
            frame.Events.OnSkillFieldTick(skillFieldEntity, skillField->AffectedEntities);
        }
    }
}
