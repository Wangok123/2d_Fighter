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

        public void SpawnSkillField(Frame frame, AssetRef<SkillFieldData> skillFieldDataRef, FPVector2 position,
            EntityRef owner)
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

            SkillFieldData skillFieldData = frame.FindAsset<SkillFieldData>(skillFieldComponent->SkillFieldData.Id);

            ApplyFieldEffect(frame, skillField, skillFieldComponent, skillFieldData, target, hitPoint);
        }

        private void InitializeSpecialField(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField,
            SkillFieldData data, EntityRef owner, FPVector2 position)
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

        private void UpdateSpecialFieldBehavior(Frame frame, EntityRef skillFieldEntity,
            SkillFieldComponent* skillField, SkillFieldData data)
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

        private void TriggerDelayedExplosion(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField,
            DelayedExplosionFieldData data)
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
            HitCollection hits =
                frame.Physics2D.OverlapShape(*transform, shape, data.TargetLayer, QueryOptions.HitSolids);

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

                    // 修改：使用 KnockbackStatusEffectData 统一处理击退
                    if (data.ApplyKnockback)
                    {
                        AssetRef<KnockbackStatusEffectData> knockbackDataRef =
                            data.GetKnockbackStatusEffectData(frame, skillFieldEntity);

                        if (!knockbackDataRef.Id.IsValid)
                        {
                            continue;
                        }

                        KnockbackStatusEffectData knockbackData =
                            frame.FindAsset<KnockbackStatusEffectData>(knockbackDataRef.Id);

                        // 修改：使用 KnockbackStatusEffectData 计算击退方向
                        FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(
                            frame,
                            skillField->Owner,
                            explosionCenter,
                            targetTransform->Position
                        );

                        KnockbackApplicationMode knockbackMode = knockbackData.KnockbackApplicationMode;

                        // 修改：根据模式选择不同的信号
                        switch (knockbackMode)
                        {
                            case KnockbackApplicationMode.CharacterController:
                                frame.Signals.OnKnockbackApplied(hit.Entity, knockbackData.KnockBackDuration,
                                    knockbackDirection, knockbackDataRef);
                                break;

                            case KnockbackApplicationMode.Physics2D:
                                FPVector2 knockbackVelocity = knockbackDirection * knockbackData.KnockbackForce;
                                frame.Signals.OnKnockbackPhysic2DApplied(hit.Entity, knockbackVelocity);
                                break;
                        }
                    }
                }
            }
        }

        private void ApplyFieldEffect(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField,
            SkillFieldData data, EntityRef target, FPVector2 hitPoint)
        {
            data.ApplyEffect(frame, skillFieldEntity, skillField, target, hitPoint);
        }

        private void ExecuteTick(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField,
            SkillFieldData skillFieldData)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);

            var shape = skillFieldData.EffectArea.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, skillFieldData.TargetLayer,
                QueryOptions.HitDynamics);

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
