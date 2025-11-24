using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public class CommandSequenceConfig
    {
        [Tooltip("指令序列名称")] public string SequenceName = "Special Move";

        [Tooltip("指令序列")] public CommandInput[] InputSequence = new CommandInput[]
        {
            CommandInput.Down,
            CommandInput.DownRight,
            CommandInput.Right,
            CommandInput.LP
        };

        [Header("执行类型")] [Tooltip("攻击执行方式")]
        public CommandAttackExecutionType ExecutionType = CommandAttackExecutionType.Projectile;

        [Header("通用设置")] [Tooltip("持续时间")] public FP Duration = FP._1;

        [Tooltip("冷却时间")] public FP Cooldown = 3;

        [Header("碰撞盒攻击设置（ExecutionType = Hitbox）")] [Tooltip("打击框激活时间")]
        public FP HitboxActiveTime = FP._0_10;

        [Tooltip("打击框持续时间")] public FP HitboxActiveDuration = FP._0_10;

        [Tooltip("攻击形状")] public Shape2DConfig AttackShape;

        [Tooltip("击退配置数据")] public AssetRef<KnockbackStatusEffectData> KnockbackStatusEffectData;

        [Header("飞行道具设置（ExecutionType = Projectile）")] [Tooltip("飞行道具数据")]
        public AssetRef<ProjectileData> ProjectileData;

        [Tooltip("生成偏移")] public FPVector2 SpawnOffset = FPVector2.Right;

        [Header("技能场设置（ExecutionType = SkillField）")] [Tooltip("技能场数据")]
        public AssetRef<SkillFieldData> SkillFieldData;

        [Tooltip("生成位置偏移")] public FPVector2 FieldSpawnOffset = FPVector2.Zero;
    }

    [Serializable]
    public class SkillSequenceMapping
    {
        [Tooltip("技能名称（用于标识）")] public string SkillName = "New Skill";

        [Tooltip("对应的指令序列索引")] public int SequenceIndex = -1;

        [Tooltip("技能数据资源引用")] public AssetRef<SkillData> SkillDataRef;

        [Tooltip("是否启用（可以临时禁用某个技能）")] public bool IsEnabled = true;
    }

    public unsafe class CommandAttackAbilityData : AbilityData
    {
        [Header("指令输入设置")] [Tooltip("指令输入窗口时间")]
        public FP CommandInputWindow = FP._0_50;

        [Tooltip("指令序列配置")] public CommandSequenceConfig[] CommandSequences = new CommandSequenceConfig[0];

        [Header("技能系统集成")] [Tooltip("序列到技能的映射（将指令序列映射到 Skill 系统）")]
        public SkillSequenceMapping[] SkillMappings = new SkillSequenceMapping[0];

        public CommandSequenceConfig GetCommandSequence(int sequenceIndex)
        {
            if (sequenceIndex < 0 || sequenceIndex >= CommandSequences.Length)
                return null;

            return CommandSequences[sequenceIndex];
        }
        
        public bool ActivateForSequence(Frame frame, EntityRef entityRef, PlayerLink* playerLink,
            AbilityType abilityType, ref Ability ability, int sequenceIndex)
        {
            CommandSequenceConfig sequence = GetCommandSequence(sequenceIndex);
            if (sequence == null)
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogError($"[CommandAttack] Invalid sequence index: {sequenceIndex}");
#endif
                return false;
            }

            FP originalDuration = Duration;
            FP originalCooldown = Cooldown;
    
            Duration = sequence.Duration;
            Cooldown = sequence.Cooldown;

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log(
                $"[CommandAttack] ActivateForSequence - Duration={Duration}, Cooldown={Cooldown} for sequence {sequenceIndex}");
#endif
            ability.BufferInput(frame);

            bool activated = TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);
            
            if (!activated)
            {
                Duration = originalDuration;
                Cooldown = originalCooldown;
            }

            return activated;
        }


        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (!abilityState.IsActive)
                return abilityState;

            if (!frame.Unsafe.TryGetPointer<CommandAttackRuntimeComponent>(entityRef, out var runtime))
                return abilityState;

            CommandSequenceConfig sequence = GetCommandSequence(runtime->MatchedSequenceIndex);
            if (sequence == null || sequence.ExecutionType != CommandAttackExecutionType.Hitbox)
                return abilityState;

            // 修改：处理Hitbox类型的执行时机
            FP elapsedTime = ability->DurationTimer.ElapsedTime;
            FP hitboxStartTime = sequence.HitboxActiveTime;
            FP hitboxEndTime = sequence.HitboxActiveTime + sequence.HitboxActiveDuration;

            if (elapsedTime >= hitboxStartTime && elapsedTime < hitboxEndTime)
            {
                if (!runtime->HasStartedHitboxWindow)
                {
                    frame.Signals.OnCommandAttackHitboxActivate(entityRef, runtime->MatchedSequenceIndex);
                    runtime->HasStartedHitboxWindow = true;
                }

                frame.Signals.OnCommandAttackExecute(entityRef, runtime->MatchedSequenceIndex);
            }

            // 修改：在Ability结束时清理
            if (abilityState.IsActiveEndTick)
            {
                if (runtime->HitEntitiesThisAttack.Ptr != default)
                {
                    frame.FreeList(runtime->HitEntitiesThisAttack);
                    runtime->HitEntitiesThisAttack = default;
                }

                runtime->HasStartedHitboxWindow = false;
            }

            return abilityState;
        }

        public bool ShouldUseSkillSystem(int sequenceIndex)
        {
            for (int i = 0; i < SkillMappings.Length; i++)
            {
                var mapping = SkillMappings[i];

                if (mapping.SequenceIndex == sequenceIndex &&
                    mapping.SkillDataRef.Id.IsValid &&
                    mapping.IsEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetSkillDataForSequence(Frame frame, EntityRef entityRef, int sequenceIndex,
            out AssetRef<SkillData> skillDataRef)
        {
            for (int i = 0; i < SkillMappings.Length; i++)
            {
                var mapping = SkillMappings[i];

                if (mapping.SequenceIndex == sequenceIndex &&
                    mapping.SkillDataRef.Id.IsValid &&
                    mapping.IsEnabled)
                {
                    skillDataRef = mapping.SkillDataRef;
                    return true;
                }
            }

            skillDataRef = default;
            return false;
        }

        // 修改：UpdateInput 只负责记录输入和检测序列，不设置 HasBufferedInput
        // CommandAttack 的激活完全由 CommandInputSystem 通过 Signal 触发
        public override void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability,
            SimpleInput2D input)
        {
            if (!frame.Unsafe.TryGetPointer<CommandInputComponent>(entityRef, out var commandInput))
            {
                return;
            }

            if (!frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
                return;

            // 修改：只记录方向输入，不激活任何 Ability
            CommandInput directionInput = commandInput->GetDirectionInput(input, movement->IsFacingRight);
            if (directionInput != CommandInput.None)
            {
                commandInput->RecordInput(frame, directionInput);
            }

            // 修改：记录按钮输入并检测序列
            CommandInput buttonInput = commandInput->GetButtonInput(input);
            if (buttonInput != CommandInput.None)
            {
                commandInput->RecordInput(frame, buttonInput);

                // 修改：检测指令序列
                for (int i = 0; i < CommandSequences.Length; i++)
                {
                    if (commandInput->CheckCommandSequence(frame, CommandSequences[i].InputSequence))
                    {
#if DEBUG || UNITY_EDITOR
                        UnityEngine.Debug.Log(
                            $"[CommandAttack] ✓ Matched sequence {i}: {CommandSequences[i].SequenceName}");
#endif
                        commandInput->ClearBuffer();

                        // 修改：通知所有 Ability 指令输入被检测到
                        NotifyAllAbilitiesCommandDetected(frame, entityRef);

                        // 修改：发送信号让 CommandInputSystem 处理激活
                        frame.Signals.OnCommandAttackActivated(entityRef, i);

                        return;
                    }
                }
            }

            // 修改：注意这里不设置 ability->HasBufferedInput，所以 AbilitySystem 不会自动激活 CommandAttack
        }

        
        private void NotifyAllAbilitiesCommandDetected(Frame frame, EntityRef entityRef)
        {
            if (!frame.Unsafe.TryGetPointer<AbilityInventory>(entityRef, out var abilityInventory))
                return;

            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);

            foreach (var abilityPair in dic)
            {
                if (!dic.TryGetValuePointer(abilityPair.Key, out var abilityPtr))
                    continue;

                if (!abilityPtr->AbilityData.Id.IsValid)
                    continue;

                AbilityData abilityData = frame.FindAsset<AbilityData>(abilityPtr->AbilityData.Id);
                abilityData.OnCommandInputDetected(frame, entityRef);
            }
        }
    }
}
