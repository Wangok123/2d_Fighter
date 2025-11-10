// CommandAttackAbilityDataEditor.cs
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
            "• 玩家需要在规定时间内完成特定的输入序列\n" +
            "• 方向输入会根据角色朝向自动镜像（面向右时：右=前，左=后）\n" +
            "• 常见指令示例：\n" +
            "  - 波动拳 (236+P): 下 → 下右 → 右 → LP/HP\n" +
            "  - 升龙拳 (623+P): 右 → 下 → 下右 → LP/HP\n" +
            "  - 后前拳 (41236+P): 左 → 下左 → 下 → 下右 → 右 → LP/HP",
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
                        sequenceStr += " → ";
                }
                
                EditorGUILayout.LabelField("输入序列:", sequenceStr);
                EditorGUILayout.LabelField("伤害类型:", sequence.HitType.ToString());
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
            CommandInput.Down => "↓(下)",
            CommandInput.DownRight => "↘(下右)",
            CommandInput.Right => "→(右/前)",
            CommandInput.UpRight => "↗(上右)",
            CommandInput.Up => "↑(上)",
            CommandInput.UpLeft => "↖(上左)",
            CommandInput.Left => "←(左/后)",
            CommandInput.DownLeft => "↙(下左)",
            CommandInput.LP => "LP(轻拳)",
            CommandInput.HP => "HP(重拳)",
            CommandInput.Dash => "Dash(冲刺)",
            CommandInput.Jump => "Jump(跳跃)",
            _ => input.ToString()
        };
    }
}
#endif
