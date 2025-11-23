using Photon.Deterministic;
using Quantum.Physics2D;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class CommandInputSystem : SystemMainThreadFilter<CommandInputSystem.Filter>,
        ISignalOnCommandAttackActivated,
        ISignalOnCommandAttackHitboxActivate,
        ISignalOnCommandAttackExecute
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public CommandInputComponent* CommandInput;
            public MovementComponent* Movement;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.CommandInput->InputExpiryTimer.IsRunning(frame))
            {
                if (filter.CommandInput->InputExpiryTimer.IsExpired(frame))
                {
                    filter.CommandInput->ClearBuffer();
                }
            }
        }

        public void OnCommandAttackActivated(Frame frame, EntityRef entityRef, int sequenceIndex)
        {
#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log(
                $"[CommandInputSystem] OnCommandAttackActivated called - Entity: {entityRef}, SequenceIndex: {sequenceIndex}");
#endif

            if (!frame.Unsafe.TryGetPointer<AbilityInventory>(entityRef, out var abilityInventory))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[CommandInputSystem] No AbilityInventory found!");
#endif
                return;
            }

            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            if (!dic.TryGetValue(AbilityType.SpecialUltimate, out var ability))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[CommandInputSystem] No SpecialUltimate ability found in inventory!");
#endif
                return;
            }

            AbilityData abilityDataBase = frame.FindAsset<AbilityData>(ability.AbilityData.Id);
            if (!(abilityDataBase is CommandAttackAbilityData commandData))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[CommandInputSystem] Ability is not CommandAttackAbilityData!");
#endif
                return;
            }

            CommandSequenceConfig sequence = commandData.GetCommandSequence(sequenceIndex);
            if (sequence == null)
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[CommandInputSystem] Sequence {sequenceIndex} is null!");
#endif
                return;
            }

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[CommandInputSystem] Sequence found: {sequence.SequenceName}");
#endif

            // 修改：通过虚方法检查是否应该使用技能系统
            if (commandData.ShouldUseSkillSystem(sequenceIndex))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.Log($"[CommandInputSystem] Using Skill System for sequence {sequenceIndex}");
#endif

                // 修改：通过虚方法获取技能数据
                if (commandData.TryGetSkillDataForSequence(frame, entityRef, sequenceIndex, out var skillDataRef))
                {
#if DEBUG || UNITY_EDITOR
                    UnityEngine.Debug.Log($"[CommandInputSystem] SkillData found: {skillDataRef.Id.Value}");
#endif

                    // 确保实体有SkillComponent
                    if (!frame.Has<SkillComponent>(entityRef))
                    {
                        frame.Add<SkillComponent>(entityRef);
#if DEBUG || UNITY_EDITOR
                        UnityEngine.Debug.Log($"[CommandInputSystem] Added SkillComponent to entity");
#endif
                    }

                    // 修改：通过Signal激活技能
                    frame.Signals.OnSkillActivationRequested(entityRef, skillDataRef);
#if DEBUG || UNITY_EDITOR
                    UnityEngine.Debug.Log($"[CommandInputSystem] ✓ OnSkillActivationRequested signal sent!");
#endif
                }
                else
                {
#if DEBUG || UNITY_EDITOR
                    UnityEngine.Debug.LogWarning(
                        $"[CommandInputSystem] Failed to get SkillData for sequence {sequenceIndex}");
#endif
                }

                return;
            }

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[CommandInputSystem] Using traditional execution for sequence {sequenceIndex}");
#endif

            // 修改：处理传统的Command Attack执行类型
            switch (sequence.ExecutionType)
            {
                case CommandAttackExecutionType.Projectile:
                    SpawnProjectile(frame, entityRef, sequence);
                    break;

                case CommandAttackExecutionType.SkillField:
                    SpawnSkillField(frame, entityRef, sequence);
                    break;

                case CommandAttackExecutionType.Hitbox:
                    // Hitbox类型在OnCommandAttackExecute中处理
                    break;
            }
        }


        public void OnCommandAttackHitboxActivate(Frame frame, EntityRef entityRef, int sequenceIndex)
        {
#if UNITY_EDITOR
            frame.Events.AttackHitboxActivated(entityRef, sequenceIndex);
#endif
        }

        public void OnCommandAttackExecute(Frame frame, EntityRef entityRef, int sequenceIndex)
        {
            if (!frame.Unsafe.TryGetPointer<CommandAttackRuntimeComponent>(entityRef, out var runtime))
                return;

            if (!frame.Unsafe.TryGetPointer<AbilityInventory>(entityRef, out var abilityInventory))
                return;

            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            if (!dic.TryGetValue(AbilityType.SpecialUltimate, out var ability))
                return;

            AbilityData abilityDataBase = frame.FindAsset<AbilityData>(ability.AbilityData.Id);
            if (!(abilityDataBase is CommandAttackAbilityData commandData))
                return;

            CommandSequenceConfig sequence = commandData.GetCommandSequence(sequenceIndex);
            if (sequence == null || sequence.ExecutionType != CommandAttackExecutionType.Hitbox)
                return;

            // 修改：技能类型的攻击（如PlungeAttack）由SkillSystem处理，这里跳过
            if (commandData.ShouldUseSkillSystem(sequenceIndex))
            {
                return;
            }

            // 修改：执行普通Hitbox攻击
            ExecuteHitboxAttack(frame, entityRef, sequence, runtime);
        }

        private void ExecuteHitboxAttack(Frame frame, EntityRef entityRef, CommandSequenceConfig sequence,
            CommandAttackRuntimeComponent* runtime)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);
            GameSettingsData gameSettings = frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            var shape = CreateAttackShapeWithDirection(frame, sequence.AttackShape, movement->IsFacingRight);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettings.PlayerLayerMask,
                QueryOptions.HitDynamics);

            if (hits.Count > 0)
            {
                var hitList = frame.ResolveList(runtime->HitEntitiesThisAttack);

                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == entityRef)
                        continue;

                    if (hitList.Contains(hit.Entity))
                        continue;

                    if (!frame.Has<CharacterStatusComponent>(hit.Entity))
                        continue;

                    hitList.Add(hit.Entity);

                    ApplyHitboxDamage(frame, entityRef, hit.Entity, sequence, movement->IsFacingRight);
                }
            }
        }

        private void ApplyHitboxDamage(Frame frame, EntityRef source, EntityRef target, CommandSequenceConfig sequence,
            bool isFacingRight)
        {
            if (!frame.Unsafe.TryGetPointer<CharacterStatusComponent>(target, out var hitReaction))
                return;

            if (!sequence.KnockbackStatusEffectData.Id.IsValid)
            {
                return;
            }

            KnockbackStatusEffectData knockbackData =
                frame.FindAsset<KnockbackStatusEffectData>(sequence.KnockbackStatusEffectData.Id);

            Transform2D* attackerTransform = frame.Unsafe.GetPointer<Transform2D>(source);
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(
                frame,
                isFacingRight,
                attackerTransform->Position,
                targetTransform->Position
            );

            KnockbackApplicationMode knockbackMode = knockbackData.KnockbackApplicationMode;

            switch (knockbackMode)
            {
                case KnockbackApplicationMode.CharacterController:
                    frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection,
                        sequence.KnockbackStatusEffectData);
                    break;

                case KnockbackApplicationMode.Physics2D:
                    FPVector2 knockbackVelocity = knockbackDirection * knockbackData.KnockbackForce;
                    frame.Signals.OnKnockbackPhysic2DApplied(target, knockbackVelocity);
                    break;
            }
        }

        private void SpawnProjectile(Frame frame, EntityRef entityRef, CommandSequenceConfig sequence)
        {
            if (!sequence.ProjectileData.Id.IsValid)
                return;

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);

            FPVector2 direction = movement->IsFacingRight ? FPVector2.Right : FPVector2.Left;
            FPVector2 spawnOffset = new FPVector2(
                sequence.SpawnOffset.X * (movement->IsFacingRight ? FP._1 : -FP._1),
                sequence.SpawnOffset.Y
            );
            FPVector2 spawnPosition = transform->Position + spawnOffset;

            frame.Signals.SpawnProjectile(sequence.ProjectileData, spawnPosition, direction, entityRef);
        }

        private void SpawnSkillField(Frame frame, EntityRef entityRef, CommandSequenceConfig sequence)
        {
            if (!sequence.SkillFieldData.Id.IsValid)
                return;

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);

            FPVector2 spawnOffset = new FPVector2(
                sequence.FieldSpawnOffset.X * (movement->IsFacingRight ? FP._1 : -FP._1),
                sequence.FieldSpawnOffset.Y
            );
            FPVector2 spawnPosition = transform->Position + spawnOffset;

            frame.Signals.SpawnSkillField(sequence.SkillFieldData, spawnPosition, entityRef);
        }

        private Shape2D CreateAttackShapeWithDirection(Frame frame, Shape2DConfig shapeConfig, bool isFacingRight)
        {
            Shape2DConfig adjustedConfig = new Shape2DConfig
            {
                ShapeType = shapeConfig.ShapeType,
                PolygonCollider = shapeConfig.PolygonCollider,
                CircleRadius = shapeConfig.CircleRadius,
                CapsuleSize = shapeConfig.CapsuleSize,
                EdgeExtent = shapeConfig.EdgeExtent,
                BoxExtents = shapeConfig.BoxExtents,
                PositionOffset = shapeConfig.PositionOffset,
                RotationOffset = shapeConfig.RotationOffset,
                UserTag = shapeConfig.UserTag,
                IsPersistent = shapeConfig.IsPersistent,
                CompoundShapes = shapeConfig.CompoundShapes
            };

            if (!isFacingRight)
            {
                adjustedConfig.PositionOffset.X = -adjustedConfig.PositionOffset.X;
            }

            return adjustedConfig.CreateShape(frame);
        }
    }
}