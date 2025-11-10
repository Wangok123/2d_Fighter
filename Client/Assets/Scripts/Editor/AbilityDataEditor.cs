#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 自定义编辑器，用于优化 AbilityData 及其子类的显示
/// 将相关字段分组，并提供更直观的编辑界面
/// </summary>
[CustomEditor(typeof(AbilityData), true)]
public class AbilityDataEditor : Editor
{
    private SerializedProperty identifierProp;
    private SerializedProperty inputBufferProp;
    private SerializedProperty delayProp;
    private SerializedProperty durationProp;
    private SerializedProperty cooldownProp;
    private SerializedProperty castDirectionTypeProp;
    private SerializedProperty faceCastDirectionProp;
    private SerializedProperty keepVelocityProp;
    private SerializedProperty startCooldownAfterDelayProp;
    private SerializedProperty disableMovementProp;
    private SerializedProperty priorityProp;
    private SerializedProperty canBeCancelledProp;
    private SerializedProperty canCancelLowerProp;
    private SerializedProperty uiAbilityPrefabProp;

    private bool showTimingSettings = true;
    private bool showMovementSettings = true;
    private bool showCancelSettings = true;
    private bool showUISettings = true;

    protected virtual void OnEnable()
    {
        identifierProp = serializedObject.FindProperty("Identifier");
        inputBufferProp = serializedObject.FindProperty("InputBuffer");
        delayProp = serializedObject.FindProperty("Delay");
        durationProp = serializedObject.FindProperty("Duration");
        cooldownProp = serializedObject.FindProperty("Cooldown");
        castDirectionTypeProp = serializedObject.FindProperty("CastDirectionType");
        faceCastDirectionProp = serializedObject.FindProperty("FaceCastDirection");
        keepVelocityProp = serializedObject.FindProperty("KeepVelocity");
        startCooldownAfterDelayProp = serializedObject.FindProperty("StartCooldownAfterDelay");
        disableMovementProp = serializedObject.FindProperty("DisableMovementDuringAbility");
        priorityProp = serializedObject.FindProperty("Priority");
        canBeCancelledProp = serializedObject.FindProperty("CanBeCancelledByHigherPriority");
        canCancelLowerProp = serializedObject.FindProperty("CanCancelLowerPriority");
        uiAbilityPrefabProp = serializedObject.FindProperty("_uiAbilityPrefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Asset Identifier (不可折叠，始终显示)
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("资源标识", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(identifierProp);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // 时序设置
        showTimingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showTimingSettings, "⏱ 时序设置 (Timing Settings)");
        if (showTimingSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithSeconds(inputBufferProp, "输入缓冲时间", "允许提前输入的时间窗口");
            DrawFPPropertyWithSeconds(delayProp, "延迟时间", "从触发到技能开始的延迟");
            DrawFPPropertyWithSeconds(durationProp, "持续时间", "技能激活状态的持续时间");
            DrawFPPropertyWithSeconds(cooldownProp, "冷却时间", "技能冷却时间");
            EditorGUILayout.PropertyField(startCooldownAfterDelayProp, new GUIContent("延迟后开始冷却", "是否在延迟时间后开始计算冷却"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 移动设置
        showMovementSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMovementSettings, "🎯 移动与方向设置 (Movement & Direction)");
        if (showMovementSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(castDirectionTypeProp, new GUIContent("施放方向类型", "技能施放的方向判定方式"));
            EditorGUILayout.PropertyField(faceCastDirectionProp, new GUIContent("面向施放方向", "是否在施放时转向施放方向"));
            EditorGUILayout.PropertyField(keepVelocityProp, new GUIContent("保持速度", "激活时是否保持当前速度"));
            EditorGUILayout.PropertyField(disableMovementProp, new GUIContent("禁用移动", "技能期间是否禁用角色移动"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 取消设置
        showCancelSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCancelSettings, "🔄 优先级与取消设置 (Priority & Cancel)");
        if (showCancelSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(priorityProp, new GUIContent("优先级", "技能优先级，影响能否被打断或打断其他技能"));
            EditorGUILayout.PropertyField(canBeCancelledProp, new GUIContent("可被高优先级打断", "是否允许被更高优先级的技能打断"));
            EditorGUILayout.PropertyField(canCancelLowerProp, new GUIContent("可打断低优先级", "是否可以打断更低优先级的技能"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // UI设置
        showUISettings = EditorGUILayout.BeginFoldoutHeaderGroup(showUISettings, "🎨 UI 设置 (UI Settings)");
        if (showUISettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(uiAbilityPrefabProp, new GUIContent("UI 预制体", "技能的UI显示预制体"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // 绘制子类特定的字段
        DrawPropertiesExcluding(serializedObject, 
            "m_Script", 
            "Identifier",
            "InputBuffer", 
            "Delay", 
            "Duration", 
            "Cooldown",
            "CastDirectionType",
            "FaceCastDirection",
            "KeepVelocity",
            "StartCooldownAfterDelay",
            "DisableMovementDuringAbility",
            "Priority",
            "CanBeCancelledByHigherPriority",
            "CanCancelLowerPriority",
            "_uiAbilityPrefab"
        );

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制 FP 属性，并显示对应的秒数值
    /// </summary>
    protected void DrawFPPropertyWithSeconds(SerializedProperty property, string label, string tooltip)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
        
        // 显示实际的秒数值（只读）
        var rawValueProp = property.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float seconds = rawValue / 65536f; // FP to float conversion
            EditorGUILayout.LabelField($"≈ {seconds:F2}s", GUILayout.Width(70));
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
#endif
