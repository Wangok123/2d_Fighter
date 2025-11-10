using System;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public class CommandSequenceConfig
    {
        [Tooltip("指令序列名称（如：波动拳、升龙拳等）")]
        public string SequenceName = "Special Move";
        
        [Tooltip("指令序列（如236+P代表：下、下右、右、P）")]
        public CommandInput[] InputSequence = new CommandInput[] 
        { 
            CommandInput.Down, 
            CommandInput.DownRight, 
            CommandInput.Right, 
            CommandInput.LP 
        };
        
        [Tooltip("持续时间")]
        public FP Duration = FP._1;
        
        [Tooltip("打击框激活时间")]
        public FP HitboxActiveTime = FP._0_10;
        
        [Tooltip("打击框持续时间")]
        public FP HitboxActiveDuration = FP._0_10;
        
        [Tooltip("击退力度")]
        public FP KnockbackForce = 10;
        
        [Tooltip("击退方向（水平）")]
        public FP KnockbackDirectionX = FP._1;
        
        [Tooltip("击退方向（垂直）")]
        public FP KnockbackDirectionY = FP._0_50;
        
        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_50;
        
        [Tooltip("受击类型")]
        public HitType HitType = HitType.Heavy;
        
        [Tooltip("攻击形状")]
        public Shape2DConfig AttackShape;
    }
    
    public unsafe class CommandAttackAbilityData: AttackAbilityData
    {
        [Header("Command Input Settings")]
        [Tooltip("指令输入窗口时间（完成整个指令序列的最大时间）")]
        public FP CommandInputWindow = FP._0_50;
        
        [Tooltip("指令序列配置列表")]
        public CommandSequenceConfig[] CommandSequences = new CommandSequenceConfig[0];

        private int _matchedSequenceIndex = -1;

        public override void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            if (!ability->AbilityData.Id.IsValid || ability->AbilityData.Id != Guid)
            {
                return;
            }

            if (!frame.Unsafe.TryGetPointer<CommandInputComponent>(entityRef, out var commandInput))
            {
                return;
            }

            if (!frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
            {
                return;
            }

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
            {
                return false;
            }

            CommandSequenceConfig config = CommandSequences[_matchedSequenceIndex];

            FP oldDuration = Duration;
            FP oldHitboxActiveTime = HitboxActiveTime;
            FP oldHitboxActiveDuration = HitboxActiveDuration;
            FP oldKnockbackForce = KnockbackForce;
            FP oldKnockbackDirectionX = KnockbackDirectionX;
            FP oldKnockbackDirectionY = KnockbackDirectionY;
            FP oldHitstunDuration = HitstunDuration;
            HitType oldHitType = HitType;
            Shape2DConfig oldAttackShape = AttackShape;

            ApplySequenceParameters(config);

            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);

            if (activated)
            {
                if (frame.Unsafe.TryGetPointer<CommandInputComponent>(entityRef, out var commandInput))
                {
                    commandInput->ClearBuffer();
                }

                frame.Events.CommandAttackExecuted(entityRef, 666);
            }
            else
            {
                Duration = oldDuration;
                HitboxActiveTime = oldHitboxActiveTime;
                HitboxActiveDuration = oldHitboxActiveDuration;
                KnockbackForce = oldKnockbackForce;
                KnockbackDirectionX = oldKnockbackDirectionX;
                KnockbackDirectionY = oldKnockbackDirectionY;
                HitstunDuration = oldHitstunDuration;
                HitType = oldHitType;
                AttackShape = oldAttackShape;
            }

            _matchedSequenceIndex = -1;
            return activated;
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

        private void ApplySequenceParameters(CommandSequenceConfig config)
        {
            Duration = config.Duration;
            HitboxActiveTime = config.HitboxActiveTime;
            HitboxActiveDuration = config.HitboxActiveDuration;
            KnockbackForce = config.KnockbackForce;
            KnockbackDirectionX = config.KnockbackDirectionX;
            KnockbackDirectionY = config.KnockbackDirectionY;
            HitstunDuration = config.HitstunDuration;
            HitType = config.HitType;
            AttackShape = config.AttackShape;
        }

        protected override void OnAttackActivate(Frame frame, EntityRef entityRef, Ability* ability)
        {
            base.OnAttackActivate(frame, entityRef, ability);
        }
    }
}
