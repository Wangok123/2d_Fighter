// CommandAttackAbilityDataEditor.cs - 增强版本
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 增强的指令攻击编辑器，提供可视化的输入序列显示
/// </summary>
[CustomEditor(typeof(CommandAttackAbilityData), true)]
public class CommandAttackAbilityDataEditor : UnityEditor.Editor
{
    private bool showGeneralInfo = true;
    private bool[] sequenceFoldouts;

    private void OnEnable()
    {
        CommandAttackAbilityData data = (CommandAttackAbilityData)target;
        if (data.CommandSequences != null)
        {
            sequenceFoldouts = new bool[data.CommandSequences.Length];
            for (int i = 0; i < sequenceFoldouts.Length; i++)
            {
                sequenceFoldouts[i] = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CommandAttackAbilityData data = (CommandAttackAbilityData)target;

        // 标题
        EditorGUILayout.Space(5);
        DrawHeaderWithIcon("🎮 指令攻击配置 (Command Attack Configuration)");
        EditorGUILayout.Space(10);

        // 系统说明
        showGeneralInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showGeneralInfo, "📖 系统说明 (System Info)");
        if (showGeneralInfo)
        {
            DrawSystemInfo();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // 绘制默认属性（基础技能属性）
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // 指令序列可视化
        DrawCommandSequencesHeader();

        if (data.CommandSequences != null && data.CommandSequences.Length > 0)
        {
            for (int i = 0; i < data.CommandSequences.Length; i++)
            {
                DrawCommandSequence(data.CommandSequences[i], i);
                EditorGUILayout.Space(5);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ 尚未配置任何指令序列", MessageType.Warning);
        }

        // 添加快速参考
        DrawQuickReference();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeaderWithIcon(string text)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);
        EditorGUILayout.LabelField(text, headerStyle);
    }

    private void DrawSystemInfo()
    {
        Rect infoRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle textStyle = new GUIStyle(EditorStyles.label);
        textStyle.wordWrap = true;
        textStyle.richText = true;
        
        EditorGUILayout.LabelField(
            "<b>指令攻击系统说明：</b>\n\n" +
            "<b>• 支持三种执行类型：</b>\n" +
            "  <color=#88DDFF>- Hitbox:</color> 碰撞盒攻击（瞬时判定，类似普通攻击）\n" +
            "  <color=#FFDD88>- Projectile:</color> 飞行道具（波动拳、火球等）\n" +
            "  <color=#88FFDD>- SkillField:</color> 技能场（AOE持续伤害区域）\n\n" +
            "<b>• 常见指令示例：</b>\n" +
            "  <color=#FF8888>波动拳 (236+P):</color> 下 → 下右 → 右 → LP\n" +
            "  <color=#88FF88>升龙拳 (623+P):</color> 右 → 下 → 下右 → HP\n" +
            "  <color=#8888FF>旋风腿 (214+K):</color> 下 → 下左 → 左 → LK",
            textStyle,
            GUILayout.Height(150));
        
        EditorGUILayout.EndVertical();
    }

    private void DrawCommandSequencesHeader()
    {
        Rect headerRect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = new Color(1f, 0.8f, 0.4f);
        
        EditorGUILayout.LabelField("🎯 指令序列配置 (Command Sequences)", headerStyle);
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
    }

    private void DrawCommandSequence(CommandSequenceConfig sequence, int index)
    {
        if (sequence.InputSequence == null || sequence.InputSequence.Length == 0)
            return;

        // 确保 foldout 数组有效
        if (sequenceFoldouts == null || index >= sequenceFoldouts.Length)
        {
            CommandAttackAbilityData data = (CommandAttackAbilityData)target;
            sequenceFoldouts = new bool[data.CommandSequences.Length];
            for (int i = 0; i < sequenceFoldouts.Length; i++)
                sequenceFoldouts[i] = true;
        }

        // 序列容器
        Rect containerRect = EditorGUILayout.BeginVertical(GUI.skin.box);
        
        // 序列头部
        EditorGUILayout.BeginHorizontal();
        
        sequenceFoldouts[index] = EditorGUILayout.Foldout(sequenceFoldouts[index], "", true);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        titleStyle.normal.textColor = GetSequenceColor(index);
        
        EditorGUILayout.LabelField($"#{index + 1}: {sequence.SequenceName}", titleStyle);
        
        // 执行类型标签
        DrawExecutionTypeBadge(sequence.ExecutionType);
        
        EditorGUILayout.EndHorizontal();

        if (sequenceFoldouts[index])
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.Space(5);
            
            // 输入序列可视化
            DrawInputSequenceVisual(sequence.InputSequence);
            
            EditorGUILayout.Space(5);
            
            // 输入序列文本
            DrawInputSequenceText(sequence.InputSequence);
            
            EditorGUILayout.Space(5);
            
            // 执行类型详情
            DrawExecutionDetails(sequence);
            
            EditorGUILayout.Space(5);
            
            // 配置状态检查
            DrawConfigurationStatus(sequence);
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawInputSequenceVisual(CommandInput[] inputs)
    {
        EditorGUILayout.LabelField("输入序列可视化:", EditorStyles.miniBoldLabel);
        
        Rect rect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
        
        // 背景
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.2f));
        
        float inputWidth = 50f;
        float arrowWidth = 20f;
        float totalWidth = inputs.Length * (inputWidth + arrowWidth) - arrowWidth;
        float startX = rect.x + (rect.width - totalWidth) / 2f;
        
        for (int i = 0; i < inputs.Length && i < 10; i++)
        {
            float x = startX + i * (inputWidth + arrowWidth);
            
            // 输入框
            Rect inputRect = new Rect(x, rect.y + 10, inputWidth, 40);
            Color inputColor = GetInputColor(inputs[i]);
            EditorGUI.DrawRect(inputRect, inputColor);
            
            // 输入图标/文字
            GUIStyle iconStyle = new GUIStyle(EditorStyles.boldLabel);
            iconStyle.alignment = TextAnchor.MiddleCenter;
            iconStyle.fontSize = 18;
            iconStyle.normal.textColor = Color.white;
            
            GUI.Label(inputRect, GetInputSymbol(inputs[i]), iconStyle);
            
            // 连接箭头
            if (i < inputs.Length - 1 && i < 9)
            {
                Rect arrowRect = new Rect(x + inputWidth, rect.y + 25, arrowWidth, 10);
                GUI.Label(arrowRect, "→", iconStyle);
            }
        }
        
        if (inputs.Length > 10)
        {
            GUIStyle moreStyle = new GUIStyle(EditorStyles.boldLabel);
            moreStyle.alignment = TextAnchor.MiddleRight;
            Rect moreRect = new Rect(rect.x + rect.width - 50, rect.y + 25, 40, 20);
            GUI.Label(moreRect, "...", moreStyle);
        }
    }

    private void DrawInputSequenceText(CommandInput[] inputs)
    {
        Rect textRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("指令文本:", GUILayout.Width(70));
        
        string sequenceStr = "";
        for (int j = 0; j < inputs.Length; j++)
        {
            sequenceStr += GetInputSymbol(inputs[j]);
            if (j < inputs.Length - 1)
                sequenceStr += " → ";
        }
        
        GUIStyle seqStyle = new GUIStyle(EditorStyles.label);
        seqStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        EditorGUILayout.LabelField(sequenceStr, seqStyle);
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawExecutionTypeBadge(CommandAttackExecutionType type)
    {
        string badgeText = type switch
        {
            CommandAttackExecutionType.Hitbox => "📦 碰撞盒",
            CommandAttackExecutionType.Projectile => "🚀 飞行道具",
            CommandAttackExecutionType.SkillField => "✨ 技能场",
            _ => type.ToString()
        };
        
        Color badgeColor = type switch
        {
            CommandAttackExecutionType.Hitbox => new Color(0.5f, 0.7f, 1f, 0.3f),
            CommandAttackExecutionType.Projectile => new Color(1f, 0.7f, 0.3f, 0.3f),
            CommandAttackExecutionType.SkillField => new Color(0.5f, 1f, 0.7f, 0.3f),
            _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
        };
        
        Rect badgeRect = GUILayoutUtility.GetRect(100, 20);
        EditorGUI.DrawRect(badgeRect, badgeColor);
        
        GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniLabel);
        badgeStyle.alignment = TextAnchor.MiddleCenter;
        badgeStyle.fontStyle = FontStyle.Bold;
        GUI.Label(badgeRect, badgeText, badgeStyle);
    }

    private void DrawExecutionDetails(CommandSequenceConfig sequence)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("执行类型详情:", EditorStyles.miniBoldLabel);
        
        EditorGUI.indentLevel++;
        
        switch (sequence.ExecutionType)
        {
            case CommandAttackExecutionType.Hitbox:
                EditorGUILayout.LabelField($"伤害类型: {sequence.HitType}");
                DrawHitboxPreview();
                break;
                
            case CommandAttackExecutionType.Projectile:
                bool projectileValid = sequence.ProjectileData.Id.IsValid;
                GUIStyle projectileStyle = new GUIStyle(EditorStyles.label);
                projectileStyle.normal.textColor = projectileValid ? Color.green : Color.yellow;
                EditorGUILayout.LabelField("飞行道具配置:", projectileValid ? "✓ 已配置" : "⚠ 未配置", projectileStyle);
                if (projectileValid)
                    DrawProjectilePreview();
                break;
                
            case CommandAttackExecutionType.SkillField:
                bool skillFieldValid = sequence.SkillFieldData.Id.IsValid;
                GUIStyle fieldStyle = new GUIStyle(EditorStyles.label);
                fieldStyle.normal.textColor = skillFieldValid ? Color.green : Color.yellow;
                EditorGUILayout.LabelField("技能场配置:", skillFieldValid ? "✓ 已配置" : "⚠ 未配置", fieldStyle);
                if (skillFieldValid)
                    DrawSkillFieldPreview();
                break;
        }
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
    }

    private void DrawHitboxPreview()
    {
        Rect previewRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.3f));
        
        // 绘制简单的碰撞盒示意
        Rect hitboxRect = new Rect(previewRect.x + 20, previewRect.y + 5, 60, 20);
        EditorGUI.DrawRect(hitboxRect, new Color(1f, 0.5f, 0.5f, 0.5f));
        
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(hitboxRect, "碰撞盒", labelStyle);
    }

    private void DrawProjectilePreview()
    {
        Rect previewRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.3f));
        
        // 绘制飞行轨迹
        Handles.color = new Color(1f, 0.8f, 0.3f);
        Vector2 start = new Vector2(previewRect.x + 20, previewRect.y + 15);
        Vector2 end = new Vector2(previewRect.x + previewRect.width - 20, previewRect.y + 15);
        Handles.DrawLine(start, end);
        
        // 绘制弹道
        Handles.DrawSolidDisc(end - new Vector2(10, 0), Vector3.forward, 5f);
        
        // 箭头
        Handles.DrawLine(end, end - new Vector2(8, 4));
        Handles.DrawLine(end, end - new Vector2(8, -4));
    }

    private void DrawSkillFieldPreview()
    {
        Rect previewRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.3f));
        
        // 绘制技能场区域
        Vector2 center = new Vector2(previewRect.x + previewRect.width / 2, previewRect.y + 20);
        Handles.color = new Color(0.3f, 1f, 0.7f, 0.5f);
        Handles.DrawSolidDisc(center, Vector3.forward, 25f);
        
        Handles.color = new Color(0.3f, 1f, 0.7f);
        Handles.DrawWireDisc(center, Vector3.forward, 25f);
    }

    private void DrawConfigurationStatus(CommandSequenceConfig sequence)
    {
        bool isConfigured = sequence.ExecutionType switch
        {
            CommandAttackExecutionType.Hitbox => true,
            CommandAttackExecutionType.Projectile => sequence.ProjectileData.Id.IsValid,
            CommandAttackExecutionType.SkillField => sequence.SkillFieldData.Id.IsValid,
            _ => false
        };
        
        if (isConfigured)
        {
            EditorGUILayout.HelpBox("✓ 配置完整", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ 配置不完整，请配置相应的数据资产", MessageType.Warning);
        }
    }

    private void DrawQuickReference()
    {
        EditorGUILayout.Space(10);
        
        bool showReference = EditorGUILayout.Foldout(false, "📚 指令输入快速参考", true);
        if (showReference)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("方向输入:", EditorStyles.miniBoldLabel);
            DrawReferenceRow("↓ 下", "↘ 下右", "→ 右", "↗ 上右");
            DrawReferenceRow("↑ 上", "↖ 上左", "← 左", "↙ 下左");
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("按钮输入:", EditorStyles.miniBoldLabel);
            DrawReferenceRow("LP 轻拳", "HP 重拳", "Dash 冲刺", "Jump 跳跃");
            
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawReferenceRow(params string[] items)
    {
        EditorGUILayout.BeginHorizontal();
        foreach (var item in items)
        {
            EditorGUILayout.LabelField(item, EditorStyles.miniLabel, GUILayout.Width(80));
        }
        EditorGUILayout.EndHorizontal();
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
            CommandInput.Dash => "⚡",
            CommandInput.Jump => "⬆",
            _ => input.ToString()
        };
    }

    private Color GetInputColor(CommandInput input)
    {
        if (input == CommandInput.LP || input == CommandInput.HP)
            return new Color(1f, 0.3f, 0.3f, 0.8f); // 红色 - 攻击键
        else if (input == CommandInput.Dash || input == CommandInput.Jump)
            return new Color(0.3f, 1f, 0.3f, 0.8f); // 绿色 - 移动键
        else
            return new Color(0.3f, 0.7f, 1f, 0.8f); // 蓝色 - 方向键
    }

    private Color GetSequenceColor(int index)
    {
        Color[] colors = new Color[]
        {
            new Color(1f, 0.6f, 0.6f),
            new Color(0.6f, 1f, 0.6f),
            new Color(0.6f, 0.6f, 1f),
            new Color(1f, 1f, 0.6f),
            new Color(1f, 0.6f, 1f),
            new Color(0.6f, 1f, 1f)
        };
        return colors[index % colors.Length];
    }
}
#endif
