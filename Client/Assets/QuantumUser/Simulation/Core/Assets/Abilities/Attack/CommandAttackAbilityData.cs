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

        public override void UpdateInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability,
            SimpleInput2D input)
        {
            // 检查实体是否有指令输入组件
            if (!frame.Unsafe.TryGetPointer<CommandInputComponent>(entityRef, out var commandInput))
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning(
                    $"[CommandAttack] Entity {entityRef} does not have CommandInputComponent!");
#endif
                return;
            }

            // 检查实体是否有移动组件（用于获取朝向）
            if (!frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
                return;

            // 记录方向输入
            CommandInput directionInput = commandInput->GetDirectionInput(input, movement->IsFacingRight);
            if (directionInput != CommandInput.None)
            {
                commandInput->RecordInput(frame, directionInput);
            }

            // 记录按钮输入并检测指令序列
            CommandInput buttonInput = commandInput->GetButtonInput(input);
            if (buttonInput != CommandInput.None)
            {
                commandInput->RecordInput(frame, buttonInput);

                // 遍历所有指令序列，检查是否匹配
                for (int i = 0; i < CommandSequences.Length; i++)
                {
                    if (commandInput->CheckCommandSequence(frame, CommandSequences[i].InputSequence))
                    {
#if DEBUG || UNITY_EDITOR
                        UnityEngine.Debug.Log(
                            $"[CommandAttack] ✓ Matched sequence {i}: {CommandSequences[i].SequenceName}");
#endif
                        // 清空缓冲区
                        commandInput->ClearBuffer();

                        // 修改：通知所有 Ability 指令输入被检测到
                        NotifyAllAbilitiesCommandDetected(frame, entityRef);

                        // 触发指令攻击激活信号
                        frame.Signals.OnCommandAttackActivated(entityRef, i);

                        return;
                    }
                }
            }
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