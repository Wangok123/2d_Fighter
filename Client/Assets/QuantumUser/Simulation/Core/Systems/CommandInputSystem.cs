using Photon.Deterministic;
using Quantum.Physics2D;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class CommandInputSystem: SystemMainThreadFilter<CommandInputSystem.Filter>,
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
            if (!frame.Unsafe.TryGetPointer<AbilityInventory>(entityRef, out var abilityInventory))
                return;

            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            if (!dic.TryGetValue(AbilityType.SpecialUltimate, out var ability))
                return;

            AbilityData abilityDataBase = frame.FindAsset<AbilityData>(ability.AbilityData.Id);
            if (!(abilityDataBase is CommandAttackAbilityData commandData))
                return;

            CommandSequenceConfig sequence = commandData.GetCommandSequence(sequenceIndex);
            if (sequence == null)
                return;

            switch (sequence.ExecutionType)
            {
                case CommandAttackExecutionType.Projectile:
                    SpawnProjectile(frame, entityRef, sequence);
                    break;

                case CommandAttackExecutionType.SkillField:
                    SpawnSkillField(frame, entityRef, sequence);
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

            ExecuteHitboxAttack(frame, entityRef, sequence, runtime);
        }

        private void ExecuteHitboxAttack(Frame frame, EntityRef entityRef, CommandSequenceConfig sequence, CommandAttackRuntimeComponent* runtime)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);
            GameSettingsData gameSettings = frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            var shape = CreateAttackShapeWithDirection(frame, sequence.AttackShape, movement->IsFacingRight);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettings.PlayerLayerMask, QueryOptions.HitDynamics);

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

                    ApplyHitboxDamage(frame, hit.Entity, sequence, movement->IsFacingRight);
                }
            }
        }

        private void ApplyHitboxDamage(Frame frame, EntityRef target, CommandSequenceConfig sequence, bool isFacingRight)
        {
            if (!frame.Unsafe.TryGetPointer<CharacterStatusComponent>(target, out var hitReaction))
                return;

            FPVector2 knockbackDirection = new FPVector2(
                sequence.KnockbackDirection.X * (isFacingRight ? FP._1 : -FP._1),
                sequence.KnockbackDirection.Y
            ).Normalized;
            
            FPVector2 knockbackVelocity = knockbackDirection * sequence.KnockbackForce;

            frame.Signals.OnKnockbackApplied(target, knockbackVelocity, 0);
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