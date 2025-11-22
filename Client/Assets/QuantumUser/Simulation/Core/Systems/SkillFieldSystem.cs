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
                frame.Physics2D.OverlapShape(*transform, shape, data.TargetLayer, QueryOptions.HitDynamics);

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
            if (data is DamageFieldData damageData)
            {
                if (damageData.ApplyKnockback)
                {
                    ApplyKnockbackFromData(frame, skillFieldEntity, skillField, data, target, hitPoint);
                }
            }
            else if (data is HealFieldData healData)
            {
                // 治疗逻辑
            }
            else if (data is PushFieldData pushData)
            {
                // 修改：Push力场使用持续物理力
                ApplyPushFieldForce(frame, pushData, skillField->Owner, target, hitPoint);
            }
            else if (data is SlowFieldData slowData)
            {
                // 减速逻辑
            }
            else if (data is VortexFieldData vortexData)
            {
                // 修改：Vortex力场使用持续物理力
                ApplyVortexFieldForce(frame, skillFieldEntity, vortexData, target);

                if (vortexData.DealDamage)
                {
                    // 伤害逻辑
                }
            }
            else
            {
                ApplyKnockbackFromData(frame, skillFieldEntity, skillField, data, target, hitPoint);
            }
        }

        private void ApplyKnockbackFromData(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField,
            SkillFieldData data, EntityRef target, FPVector2 hitPoint)
        {
            AssetRef<KnockbackStatusEffectData> knockbackDataRef =
                data.GetKnockbackStatusEffectData(frame, skillFieldEntity);

            if (!knockbackDataRef.Id.IsValid)
            {
                return;
            }

            KnockbackStatusEffectData knockbackData = frame.FindAsset<KnockbackStatusEffectData>(knockbackDataRef.Id);

            Transform2D* fieldTransform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(
                frame,
                skillField->Owner,
                fieldTransform->Position,
                targetTransform->Position
            );

            KnockbackApplicationMode knockbackMode = knockbackData.KnockbackApplicationMode;

            switch (knockbackMode)
            {
                case KnockbackApplicationMode.CharacterController:
                    frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection,
                        knockbackDataRef);
                    break;

                case KnockbackApplicationMode.Physics2D:
                    FPVector2 knockbackVelocity = knockbackDirection * knockbackData.KnockbackForce;
                    frame.Signals.OnKnockbackPhysic2DApplied(target, knockbackVelocity);
                    break;
            }
        }
        
        private void ApplyPushFieldForce(Frame frame, PushFieldData data, EntityRef owner, EntityRef target,
            FPVector2 hitPoint)
        {
            // 修改：从 PushFieldData 获取 KnockbackStatusEffectData
            AssetRef<KnockbackStatusEffectData> knockbackDataRef = data.GetKnockbackStatusEffectData(frame, owner);
    
            if (!knockbackDataRef.Id.IsValid)
            {
                return;
            }
    
            KnockbackStatusEffectData knockbackData = frame.FindAsset<KnockbackStatusEffectData>(knockbackDataRef.Id);
    
            Transform2D* ownerTransform = frame.Unsafe.GetPointer<Transform2D>(owner);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            // 修改：计算力的方向（保留原有的 PushField 逻辑）
            FPVector2 forceDirection = CalculatePushFieldDirection(data, ownerTransform->Position, targetTransform->Position, hitPoint);
            FP forceMagnitude = CalculatePushFieldMagnitude(data, ownerTransform->Position, targetTransform->Position);

            // 修改：根据 KnockbackApplicationMode 选择不同的信号
            switch (knockbackData.KnockbackApplicationMode)
            {
                case KnockbackApplicationMode.CharacterController:
                    // 修改：使用 CharacterController 模式，传入方向和持续时间
                    frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, forceDirection, knockbackDataRef);
                    break;

                case KnockbackApplicationMode.Physics2D:
                    // 修改：使用 Physics2D 模式，直接设置速度
                    FPVector2 force = forceDirection * forceMagnitude;
                    frame.Signals.OnKnockbackPhysic2DApplied(target, force);
                    break;
            }
        }

        private FPVector2 CalculatePushFieldDirection(PushFieldData data, FPVector2 center, FPVector2 targetPos,
            FPVector2 hitPoint)
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

        // 修改：ApplyVortexFieldForce 使用 Physics2D 信号
        private void ApplyVortexFieldForce(Frame frame, EntityRef skillFieldEntity, VortexFieldData data, EntityRef target)
        {
            // 修改：从 VortexFieldData 获取 KnockbackStatusEffectData
            AssetRef<KnockbackStatusEffectData> knockbackDataRef = data.GetKnockbackStatusEffectData(frame, skillFieldEntity);
    
            if (!knockbackDataRef.Id.IsValid)
            {
                return;
            }
    
            KnockbackStatusEffectData knockbackData = frame.FindAsset<KnockbackStatusEffectData>(knockbackDataRef.Id);
    
            Transform2D* fieldTransform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 toTarget = targetTransform->Position - fieldTransform->Position;
            FP distance = toTarget.Magnitude;

            if (distance < FP._0_01)
                return;

            // 修改：计算旋涡力的方向（保留原有的 Vortex 逻辑）
            FPVector2 centripetalDir = -toTarget.Normalized;

            FPVector2 tangentialDir = data.RotationDirection == VortexRotation.Clockwise
                ? new FPVector2(toTarget.Y, -toTarget.X).Normalized
                : new FPVector2(-toTarget.Y, toTarget.X).Normalized;

            FPVector2 forceDirection = (centripetalDir * data.CentripetalForce + tangentialDir * data.TangentialForce).Normalized;

            // 修改：根据 KnockbackApplicationMode 选择不同的信号
            switch (knockbackData.KnockbackApplicationMode)
            {
                case KnockbackApplicationMode.CharacterController:
                    // 修改：使用 CharacterController 模式，传入方向和持续时间
                    frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, forceDirection, knockbackDataRef);
                    break;

                case KnockbackApplicationMode.Physics2D:
                    // 修改：使用 Physics2D 模式，直接设置速度
                    FPVector2 totalForce = centripetalDir * data.CentripetalForce + tangentialDir * data.TangentialForce;
                    frame.Signals.OnKnockbackPhysic2DApplied(target, totalForce);
                    break;
            }
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
