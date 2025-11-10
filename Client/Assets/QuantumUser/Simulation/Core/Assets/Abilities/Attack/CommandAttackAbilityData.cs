using Photon.Deterministic;
using System;
using System.Collections.Generic;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    public enum CommandAttackExecutionType
    {
        Hitbox,
        Projectile,
        SkillField,
    }

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

    [Serializable]
    public unsafe partial class CommandAttackAbilityData : AbilityData
    {
        [Header("指令输入设置")]
        [Tooltip("指令输入窗口时间")]
        public FP CommandInputWindow = FP._0_50;
        
        [Tooltip("指令序列配置")]
        public CommandSequenceConfig[] CommandSequences = new CommandSequenceConfig[0];

        private int _matchedSequenceIndex = -1;
        private CommandSequenceConfig _currentSequence;
        private bool _hasStartedHitboxWindow;
        private HashSet<EntityRef> _hitEntitiesThisAttack = new HashSet<EntityRef>();

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
                
                _matchedSequenceIndex = CheckForMatchingSequence(frame, entityRef, commandInput);
                
                if (_matchedSequenceIndex >= 0)
                {
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
            if (_matchedSequenceIndex < 0 || _matchedSequenceIndex >= CommandSequences.Length)
                return false;

            _currentSequence = CommandSequences[_matchedSequenceIndex];

            Duration = _currentSequence.Duration;
            Cooldown = _currentSequence.Cooldown;

            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                if (frame.Unsafe.TryGetPointer<CommandInputComponent>(entityRef, out var commandInput))
                {
                    commandInput->ClearBuffer();
                }

                _hasStartedHitboxWindow = false;
                _hitEntitiesThisAttack.Clear();

                ExecuteCommandAttack(frame, entityRef, ref ability);
                
                frame.Events.CommandAttackExecuted(entityRef, _matchedSequenceIndex);
            }

            _matchedSequenceIndex = -1;
            return activated;
        }

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (_currentSequence != null && _currentSequence.ExecutionType == CommandAttackExecutionType.Hitbox)
            {
                UpdateHitboxAttack(frame, entityRef, ability, abilityState);
            }

            return abilityState;
        }

        private void ExecuteCommandAttack(Frame frame, EntityRef entityRef, ref Ability ability)
        {
            switch (_currentSequence.ExecutionType)
            {
                case CommandAttackExecutionType.Hitbox:
                    break;
                    
                case CommandAttackExecutionType.Projectile:
                    SpawnProjectile(frame, entityRef);
                    break;
                    
                case CommandAttackExecutionType.SkillField:
                    SpawnSkillField(frame, entityRef);
                    break;
            }
        }

        private void UpdateHitboxAttack(Frame frame, EntityRef entityRef, Ability* ability, Ability.AbilityState abilityState)
        {
            FP elapsedTime = ability->DurationTimer.ElapsedTime;
            FP hitboxStartTime = _currentSequence.HitboxActiveTime;
            FP hitboxEndTime = hitboxStartTime + _currentSequence.HitboxActiveDuration;

            if (elapsedTime >= hitboxStartTime && elapsedTime < hitboxEndTime)
            {
                if (!_hasStartedHitboxWindow)
                {
                    _hasStartedHitboxWindow = true;
                    OnHitboxWindowStart(frame, entityRef, ability);
                }

                ExecuteAttackHitbox(frame, entityRef, ability);
            }
        }

        private void OnHitboxWindowStart(Frame frame, EntityRef entityRef, Ability* ability)
        {
#if UNITY_EDITOR
            frame.Events.AttackHitboxActivated(entityRef, _matchedSequenceIndex);
#endif
        }

        private void ExecuteAttackHitbox(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);
            GameSettingsData gameSettings = frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            var shape = CreateAttackShapeWithDirection(frame, _currentSequence.AttackShape, movement->IsFacingRight);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettings.PlayerLayerMask, QueryOptions.HitDynamics);

            if (hits.Count > 0)
            {
                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == entityRef)
                        continue;

                    if (_hitEntitiesThisAttack.Contains(hit.Entity))
                        continue;

                    if (!frame.Has<HitReactionComponent>(hit.Entity))
                        continue;

                    _hitEntitiesThisAttack.Add(hit.Entity);

                    Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(hit.Entity);
                    FPVector2 hitDirection = (targetTransform->Position - transform->Position).Normalized;

                    ApplyHitboxDamage(frame, entityRef, hit.Entity, hitDirection);
                }
            }
        }

        private void ApplyHitboxDamage(Frame frame, EntityRef attacker, EntityRef target, FPVector2 hitDirection)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            FPVector2 knockbackDirection = new FPVector2(
                _currentSequence.KnockbackDirectionX, 
                _currentSequence.KnockbackDirectionY
            ).Normalized;
            
            FPVector2 knockbackVelocity = knockbackDirection * _currentSequence.KnockbackForce;

            if (frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReactionData))
            {
                var data = frame.FindAsset(hitReactionData->HitReactionData);
                data.OnKnockbackApplied(frame, target, hitReaction, _currentSequence.HitstunDuration, knockbackVelocity);
            }
        }

        private void SpawnProjectile(Frame frame, EntityRef entityRef)
        {
            if (!_currentSequence.ProjectileData.Id.IsValid)
                return;

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);

            FPVector2 direction = movement->IsFacingRight ? FPVector2.Right : FPVector2.Left;
            FPVector2 spawnOffset = new FPVector2(
                _currentSequence.SpawnOffset.X * (movement->IsFacingRight ? FP._1 : -FP._1),
                _currentSequence.SpawnOffset.Y
            );
            FPVector2 spawnPosition = transform->Position + spawnOffset;

            frame.SpawnProjectile(_currentSequence.ProjectileData, spawnPosition, direction, entityRef);
        }

        private void SpawnSkillField(Frame frame, EntityRef entityRef)
        {
            if (!_currentSequence.SkillFieldData.Id.IsValid)
                return;

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);

            FPVector2 spawnOffset = new FPVector2(
                _currentSequence.FieldSpawnOffset.X * (movement->IsFacingRight ? FP._1 : -FP._1),
                _currentSequence.FieldSpawnOffset.Y
            );
            FPVector2 spawnPosition = transform->Position + spawnOffset;

            frame.SpawnSkillField(_currentSequence.SkillFieldData, spawnPosition, entityRef);
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
            Shape2D shape = shapeConfig.CreateShape(frame);
            
            if (!isFacingRight && shape.Type == Shape2DType.Box)
            {
                FPVector2 center = shape.Box.Extents;
                center.X = -center.X;
                shape.Box.Extents = center;
            }

            return shape;
        }
    }
}
