// CommandAttackAbilityDataEditor.cs - 更新版本
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

[CustomEditor(typeof(CommandAttackAbilityData), true)]
public class CommandAttackAbilityDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CommandAttackAbilityData data = (CommandAttackAbilityData)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("指令攻击配置", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "指令攻击系统说明：\n" +
            "• 支持三种执行类型：\n" +
            "  - Hitbox: 碰撞盒攻击（瞬时判定，类似普通攻击）\n" +
            "  - Projectile: 飞行道具（波动拳、火球等）\n" +
            "  - SkillField: 技能场（AOE持续伤害区域）\n\n" +
            "• 常见指令示例：\n" +
            "  - 波动拳 (236+P): 下 → 下右 → 右 → LP\n" +
            "  - 升龙拳 (623+P): 右 → 下 → 下右 → HP",
            MessageType.Info);

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("指令序列预览", EditorStyles.boldLabel);
        
        if (data.CommandSequences != null && data.CommandSequences.Length > 0)
        {
            for (int i = 0; i < data.CommandSequences.Length; i++)
            {
                var sequence = data.CommandSequences[i];
                if (sequence.InputSequence == null || sequence.InputSequence.Length == 0)
                    continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{i + 1}. {sequence.SequenceName}", EditorStyles.boldLabel);
                
                string sequenceStr = "";
                for (int j = 0; j < sequence.InputSequence.Length; j++)
                {
                    sequenceStr += GetInputSymbol(sequence.InputSequence[j]);
                    if (j < sequence.InputSequence.Length - 1)
                        sequenceStr += " , ";
                }
                
                EditorGUILayout.LabelField("输入序列:", sequenceStr);
                EditorGUILayout.LabelField("执行类型:", GetExecutionTypeDescription(sequence.ExecutionType));
                
                switch (sequence.ExecutionType)
                {
                    case CommandAttackExecutionType.Hitbox:
                        EditorGUILayout.LabelField("伤害类型:", sequence.HitType.ToString());
                        break;
                    case CommandAttackExecutionType.Projectile:
                        EditorGUILayout.LabelField("飞行道具:", sequence.ProjectileData.Id.IsValid ? "已配置" : "未配置", sequence.ProjectileData.Id.IsValid ? EditorStyles.label : GetWarningStyle());
                        break;
                    case CommandAttackExecutionType.SkillField:
                        EditorGUILayout.LabelField("技能场:", sequence.SkillFieldData.Id.IsValid ? "已配置" : "未配置", sequence.SkillFieldData.Id.IsValid ? EditorStyles.label : GetWarningStyle());
                        break;
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("尚未配置任何指令序列", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private string GetInputSymbol(CommandInput input)
    {
        return input switch
        {
            CommandInput.Down => "↓",
            CommandInput.DownRight => "↘",
            CommandInput.Right => "→",
            CommandInput.UpRight => "↗",
            CommandInput.Up => "↑",
            CommandInput.UpLeft => "↖",
            CommandInput.Left => "←",
            CommandInput.DownLeft => "↙",
            CommandInput.LP => "LP",
            CommandInput.HP => "HP",
            CommandInput.Dash => "Dash",
            CommandInput.Jump => "Jump",
            _ => input.ToString()
        };
    }

    private string GetExecutionTypeDescription(CommandAttackExecutionType type)
    {
        return type switch
        {
            CommandAttackExecutionType.Hitbox => "碰撞盒攻击",
            CommandAttackExecutionType.Projectile => "飞行道具",
            CommandAttackExecutionType.SkillField => "技能场",
            _ => type.ToString()
        };
    }

    private GUIStyle GetWarningStyle()
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.normal.textColor = Color.yellow;
        return style;
    }
}
#endif
