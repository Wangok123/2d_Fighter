using Photon.Deterministic;
using System;
using System.Collections.Generic;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public class CommandSequenceConfig
    {
        [Tooltip("指令序列名称")]
        public string SequenceName = "Special Move";
        
        [Tooltip("指令序列")]
        public CommandInput[] InputSequence = new CommandInput[] 
        { 
            CommandInput.Down, 
            CommandInput.DownRight, 
            CommandInput.Right, 
            CommandInput.LP 
        };
        
        [Header("执行类型")]
        [Tooltip("攻击执行方式")]
        public CommandAttackExecutionType ExecutionType = CommandAttackExecutionType.Projectile;
        
        [Header("通用设置")]
        [Tooltip("持续时间")]
        public FP Duration = FP._1;
        
        [Tooltip("冷却时间")]
        public FP Cooldown = 3;

        [Header("碰撞盒攻击设置（ExecutionType = Hitbox）")]
        [Tooltip("打击框激活时间")]
        public FP HitboxActiveTime = FP._0_10;
        
        [Tooltip("打击框持续时间")]
        public FP HitboxActiveDuration = FP._0_10;
        
        [Tooltip("攻击形状")]
        public Shape2DConfig AttackShape;
        
        [Tooltip("击退力度")]
        public FP KnockbackForce = 10;
        
        [Tooltip("击退方向X")]
        public FP KnockbackDirectionX = FP._1;
        
        [Tooltip("击退方向Y")]
        public FP KnockbackDirectionY = FP._0_50;
        
        [Tooltip("受击硬直")]
        public FP HitstunDuration = FP._0_50;
        
        [Tooltip("受击类型")]
        public HitType HitType = HitType.Heavy;

        [Header("飞行道具设置（ExecutionType = Projectile）")]
        [Tooltip("飞行道具数据")]
        public AssetRef<ProjectileData> ProjectileData;
        
        [Tooltip("生成偏移")]
        public FPVector2 SpawnOffset = FPVector2.Right;

        [Header("技能场设置（ExecutionType = SkillField）")]
        [Tooltip("技能场数据")]
        public AssetRef<SkillFieldData> SkillFieldData;
        
        [Tooltip("生成位置偏移")]
        public FPVector2 FieldSpawnOffset = FPVector2.Zero;
    }
    
    public unsafe partial class CommandAttackAbilityData : AbilityData
    {
        [Header("指令输入设置")]
        [Tooltip("指令输入窗口时间")]
        public FP CommandInputWindow = FP._0_50;
        
        [Tooltip("指令序列配置")]
        public CommandSequenceConfig[] CommandSequences = new CommandSequenceConfig[0];

        public override void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            if (!ability->AbilityData.Id.IsValid || ability->AbilityData.Id != Guid)
                return;

            if (!frame.Unsafe.TryGetPointer<CommandInputComponent>(entityRef, out var commandInput))
                return;

            if (!frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
                return;

            CommandInput directionInput = commandInput->GetDirectionInput(input, movement->IsFacingRight);
            if (directionInput != CommandInput.None)
            {
                commandInput->RecordInput(frame, directionInput);
            }

            CommandInput buttonInput = commandInput->GetButtonInput(input);
            if (buttonInput != CommandInput.None)
            {
                commandInput->RecordInput(frame, buttonInput);
                
                int matchedSequenceIndex = CheckForMatchingSequence(frame, entityRef, commandInput);
                
                if (matchedSequenceIndex >= 0)
                {
                    CommandAttackRuntimeComponent* runtime = GetOrCreateRuntimeComponent(frame, entityRef);
                    runtime->MatchedSequenceIndex = matchedSequenceIndex;
                    ability->BufferInput(frame);
                }
            }

            if (!commandInput->InputExpiryTimer.IsRunning(frame))
            {
                commandInput->ClearBuffer();
            }
        }

        public override bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            if (!frame.Unsafe.TryGetPointer<CommandAttackRuntimeComponent>(entityRef, out var runtime))
                return false;

            if (runtime->MatchedSequenceIndex < 0 || runtime->MatchedSequenceIndex >= CommandSequences.Length)
                return false;

            CommandSequenceConfig currentSequence = CommandSequences[runtime->MatchedSequenceIndex];

            Duration = currentSequence.Duration;
            Cooldown = currentSequence.Cooldown;

            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                if (frame.Unsafe.TryGetPointer<CommandInputComponent>(entityRef, out var commandInput))
                {
                    commandInput->ClearBuffer();
                }

                runtime->HasStartedHitboxWindow = false;
                
                if (runtime->HitEntitiesThisAttack.Ptr != default)
                {
                    frame.FreeList(runtime->HitEntitiesThisAttack);
                }
                runtime->HitEntitiesThisAttack = frame.AllocateList<EntityRef>();

                ExecuteCommandAttack(frame, entityRef, currentSequence, runtime->MatchedSequenceIndex);
                
                frame.Events.CommandAttackExecuted(entityRef, runtime->MatchedSequenceIndex);
            }

            return activated;
        }

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (!frame.Unsafe.TryGetPointer<CommandAttackRuntimeComponent>(entityRef, out var runtime))
                return abilityState;

            if (runtime->MatchedSequenceIndex >= 0 && runtime->MatchedSequenceIndex < CommandSequences.Length)
            {
                CommandSequenceConfig currentSequence = CommandSequences[runtime->MatchedSequenceIndex];
                
                if (currentSequence.ExecutionType == CommandAttackExecutionType.Hitbox)
                {
                    UpdateHitboxAttack(frame, entityRef, ability, currentSequence, runtime);
                }
            }

            if (abilityState.IsActiveEndTick)
            {
                if (runtime->HitEntitiesThisAttack.Ptr != default)
                {
                    frame.FreeList(runtime->HitEntitiesThisAttack);
                    runtime->HitEntitiesThisAttack = default;
                }
                runtime->MatchedSequenceIndex = -1;
            }

            return abilityState;
        }
        
        private CommandAttackRuntimeComponent* GetOrCreateRuntimeComponent(Frame frame, EntityRef entityRef)
        {
            frame.AddOrGet<CommandAttackRuntimeComponent>(entityRef, out var result);
            return result;
        }

        private void ExecuteCommandAttack(Frame frame, EntityRef entityRef, CommandSequenceConfig sequence, int sequenceIndex)
        {
            switch (sequence.ExecutionType)
            {
                case CommandAttackExecutionType.Hitbox:
                    break;
                    
                case CommandAttackExecutionType.Projectile:
                    SpawnProjectile(frame, entityRef, sequence);
                    break;
                    
                case CommandAttackExecutionType.SkillField:
                    SpawnSkillField(frame, entityRef, sequence);
                    break;
            }
        }

        private void UpdateHitboxAttack(Frame frame, EntityRef entityRef, Ability* ability, CommandSequenceConfig sequence, CommandAttackRuntimeComponent* runtime)
        {
            FP elapsedTime = ability->DurationTimer.ElapsedTime;
            FP hitboxStartTime = sequence.HitboxActiveTime;
            FP hitboxEndTime = hitboxStartTime + sequence.HitboxActiveDuration;

            if (elapsedTime >= hitboxStartTime && elapsedTime < hitboxEndTime)
            {
                if (!runtime->HasStartedHitboxWindow)
                {
                    runtime->HasStartedHitboxWindow = true;
                    OnHitboxWindowStart(frame, entityRef, ability, runtime->MatchedSequenceIndex);
                }

                ExecuteAttackHitbox(frame, entityRef, sequence, runtime);
            }
        }

        private void OnHitboxWindowStart(Frame frame, EntityRef entityRef, Ability* ability, int sequenceIndex)
        {
            frame.Events.AttackHitboxActivated(entityRef, sequenceIndex);
        }

        private void ExecuteAttackHitbox(Frame frame, EntityRef entityRef, CommandSequenceConfig sequence, CommandAttackRuntimeComponent* runtime)
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

                    if (!frame.Has<HitReactionComponent>(hit.Entity))
                        continue;

                    hitList.Add(hit.Entity);

                    Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(hit.Entity);
                    FPVector2 hitDirection = (targetTransform->Position - transform->Position).Normalized;

                    ApplyHitboxDamage(frame, hit.Entity, sequence);
                }
            }
        }

        private void ApplyHitboxDamage(Frame frame, EntityRef target, CommandSequenceConfig sequence)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            FPVector2 knockbackDirection = new FPVector2(
                sequence.KnockbackDirectionX, 
                sequence.KnockbackDirectionY
            ).Normalized;
            
            FPVector2 knockbackVelocity = knockbackDirection * sequence.KnockbackForce;

            if (frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReactionData))
            {
                var data = frame.FindAsset(hitReactionData->HitReactionData);
                data.OnKnockbackApplied(frame, target, hitReaction, sequence.HitstunDuration, knockbackVelocity);
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

            frame.SpawnProjectile(sequence.ProjectileData, spawnPosition, direction, entityRef);
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

            frame.SpawnSkillField(sequence.SkillFieldData, spawnPosition, entityRef);
        }

        private int CheckForMatchingSequence(Frame frame, EntityRef entityRef, CommandInputComponent* commandInput)
        {
            for (int i = 0; i < CommandSequences.Length; i++)
            {
                if (CommandSequences[i].InputSequence == null || CommandSequences[i].InputSequence.Length == 0)
                    continue;

                if (commandInput->CheckCommandSequence(frame, CommandSequences[i].InputSequence))
                {
                    return i;
                }
            }

            return -1;
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
