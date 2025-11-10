#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 自定义编辑器，用于优化 ChargeAttackAbilityData 的显示
/// 将蓄力攻击相关字段进行专门的组织和可视化
/// </summary>
[CustomEditor(typeof(ChargeAttackAbilityData), true)]
public class ChargeAttackAbilityDataEditor : AttackAbilityDataEditor
{
    private SerializedProperty minChargeTimeProp;
    private SerializedProperty maxChargeTimeProp;
    private SerializedProperty canMoveWhileChargingProp;
    private SerializedProperty minChargeDamageMultiplierProp;
    private SerializedProperty maxChargeDamageMultiplierProp;
    private SerializedProperty scaleKnockbackWithChargeProp;
    private SerializedProperty minChargeKnockbackMultiplierProp;
    private SerializedProperty maxChargeKnockbackMultiplierProp;
    private SerializedProperty scaleAttackRangeWithChargeProp;
    private SerializedProperty maxChargeRangeMultiplierProp;

    private bool showChargeTimingSettings = true;
    private bool showChargeDamageSettings = true;
    private bool showChargeKnockbackSettings = true;
    private bool showChargeVisualSettings = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        minChargeTimeProp = serializedObject.FindProperty("MinChargeTime");
        maxChargeTimeProp = serializedObject.FindProperty("MaxChargeTime");
        canMoveWhileChargingProp = serializedObject.FindProperty("CanMoveWhileCharging");
        minChargeDamageMultiplierProp = serializedObject.FindProperty("MinChargeDamageMultiplier");
        maxChargeDamageMultiplierProp = serializedObject.FindProperty("MaxChargeDamageMultiplier");
        scaleKnockbackWithChargeProp = serializedObject.FindProperty("ScaleKnockbackWithCharge");
        minChargeKnockbackMultiplierProp = serializedObject.FindProperty("MinChargeKnockbackMultiplier");
        maxChargeKnockbackMultiplierProp = serializedObject.FindProperty("MaxChargeKnockbackMultiplier");
        scaleAttackRangeWithChargeProp = serializedObject.FindProperty("ScaleAttackRangeWithCharge");
        maxChargeRangeMultiplierProp = serializedObject.FindProperty("MaxChargeRangeMultiplier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制基类内容（AbilityData + AttackAbilityData）
        base.OnInspectorGUI();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("蓄力攻击特定设置 (Charge Attack Settings)", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 蓄力时序设置
        showChargeTimingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showChargeTimingSettings, "⏱ 蓄力时序 (Charge Timing)");
        if (showChargeTimingSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithSeconds(minChargeTimeProp, "最小蓄力时间", "达到此时间即可释放攻击");
            DrawFPPropertyWithSeconds(maxChargeTimeProp, "最大蓄力时间", "达到此时间为满蓄力状态");
            EditorGUILayout.PropertyField(canMoveWhileChargingProp, new GUIContent("蓄力时可移动", "蓄力过程中是否允许角色移动"));
            
            // 绘制蓄力时间可视化
            DrawChargeTimingVisualization();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 蓄力伤害缩放
        showChargeDamageSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showChargeDamageSettings, "⚔️ 伤害缩放 (Damage Scaling)");
        if (showChargeDamageSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithMultiplier(minChargeDamageMultiplierProp, "最小伤害倍率", "最短蓄力时的伤害倍率");
            DrawFPPropertyWithMultiplier(maxChargeDamageMultiplierProp, "最大伤害倍率", "满蓄力时的伤害倍率");
            
            // 绘制伤害曲线可视化
            DrawScalingVisualization("伤害", minChargeDamageMultiplierProp, maxChargeDamageMultiplierProp);
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 蓄力击退缩放
        showChargeKnockbackSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showChargeKnockbackSettings, "💥 击退缩放 (Knockback Scaling)");
        if (showChargeKnockbackSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(scaleKnockbackWithChargeProp, new GUIContent("启用击退缩放", "是否根据蓄力时间缩放击退力度"));
            
            EditorGUI.BeginDisabledGroup(!scaleKnockbackWithChargeProp.boolValue);
            DrawFPPropertyWithMultiplier(minChargeKnockbackMultiplierProp, "最小击退倍率", "最短蓄力时的击退倍率");
            DrawFPPropertyWithMultiplier(maxChargeKnockbackMultiplierProp, "最大击退倍率", "满蓄力时的击退倍率");
            
            if (scaleKnockbackWithChargeProp.boolValue)
            {
                DrawScalingVisualization("击退", minChargeKnockbackMultiplierProp, maxChargeKnockbackMultiplierProp);
            }
            EditorGUI.EndDisabledGroup();
            
            if (!scaleKnockbackWithChargeProp.boolValue)
            {
                EditorGUILayout.HelpBox("击退缩放已禁用，击退力度不受蓄力时间影响", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 蓄力视觉设置
        showChargeVisualSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showChargeVisualSettings, "🎨 视觉效果 (Visual Effects)");
        if (showChargeVisualSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(scaleAttackRangeWithChargeProp, new GUIContent("启用范围缩放", "是否根据蓄力时间缩放攻击范围"));
            
            EditorGUI.BeginDisabledGroup(!scaleAttackRangeWithChargeProp.boolValue);
            DrawFPPropertyWithMultiplier(maxChargeRangeMultiplierProp, "最大范围倍率", "满蓄力时的攻击范围倍率");
            
            if (scaleAttackRangeWithChargeProp.boolValue)
            {
                EditorGUILayout.Space(5);
                var rawValue = maxChargeRangeMultiplierProp.FindPropertyRelative("RawValue");
                if (rawValue != null)
                {
                    float maxMultiplier = rawValue.longValue / 65536f;
                    EditorGUILayout.LabelField($"范围变化: 1.0 → {maxMultiplier:F2} (增加 {(maxMultiplier - 1f) * 100f:F0}%)", EditorStyles.miniLabel);
                }
            }
            EditorGUI.EndDisabledGroup();
            
            if (!scaleAttackRangeWithChargeProp.boolValue)
            {
                EditorGUILayout.HelpBox("范围缩放已禁用，攻击范围不受蓄力时间影响", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFPPropertyWithMultiplier(SerializedProperty property, string label, string tooltip)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
        
        var rawValueProp = property.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float multiplier = rawValue / 65536f;
            EditorGUILayout.LabelField($"×{multiplier:F2}", GUILayout.Width(70));
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawChargeTimingVisualization()
    {
        var minTimeRaw = minChargeTimeProp.FindPropertyRelative("RawValue");
        var maxTimeRaw = maxChargeTimeProp.FindPropertyRelative("RawValue");
        
        if (minTimeRaw != null && maxTimeRaw != null)
        {
            float minTime = minTimeRaw.longValue / 65536f;
            float maxTime = maxTimeRaw.longValue / 65536f;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"蓄力区间: {minTime:F2}s (可释放) → {maxTime:F2}s (满蓄力)", EditorStyles.miniLabel);
            
            // 绘制时间轴
            Rect rect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            
            // 背景 - 无效区域
            float totalTime = maxTime + 0.2f;
            float scale = rect.width / totalTime;
            float minX = rect.x + minTime * scale;
            float maxX = rect.x + maxTime * scale;
            
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 10, minX - rect.x, 10), new Color(0.5f, 0f, 0f, 0.3f)); // 红色 - 太短
            EditorGUI.DrawRect(new Rect(minX, rect.y + 10, maxX - minX, 10), new Color(0f, 1f, 0f, 0.3f)); // 绿色 - 有效
            EditorGUI.DrawRect(new Rect(maxX, rect.y + 10, rect.width - (maxX - rect.x), 10), new Color(1f, 1f, 0f, 0.3f)); // 黄色 - 满蓄力
            
            // 标记
            Handles.color = Color.yellow;
            Handles.DrawLine(new Vector2(minX, rect.y + 5), new Vector2(minX, rect.y + 25));
            Handles.DrawLine(new Vector2(maxX, rect.y + 5), new Vector2(maxX, rect.y + 25));
            
            // 文字标签
            GUI.Label(new Rect(rect.x, rect.y, minX - rect.x, 10), "太短", EditorStyles.miniLabel);
            GUI.Label(new Rect(minX + 5, rect.y, maxX - minX - 10, 10), "可释放", EditorStyles.miniLabel);
            GUI.Label(new Rect(maxX + 5, rect.y, rect.width - (maxX - rect.x) - 10, 10), "满蓄力", EditorStyles.miniLabel);
        }
    }

    private void DrawScalingVisualization(string typeName, SerializedProperty minProp, SerializedProperty maxProp)
    {
        var minRaw = minProp.FindPropertyRelative("RawValue");
        var maxRaw = maxProp.FindPropertyRelative("RawValue");
        
        if (minRaw != null && maxRaw != null)
        {
            float minValue = minRaw.longValue / 65536f;
            float maxValue = maxRaw.longValue / 65536f;
            
            EditorGUILayout.Space(5);
            
            Rect rect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
            
            // 背景
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
            
            // 绘制曲线（线性插值）
            Handles.color = Color.cyan;
            
            int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float t1 = i / (float)segments;
                float t2 = (i + 1) / (float)segments;
                
                float value1 = Mathf.Lerp(minValue, maxValue, t1);
                float value2 = Mathf.Lerp(minValue, maxValue, t2);
                
                float x1 = rect.x + rect.width * t1;
                float x2 = rect.x + rect.width * t2;
                
                float maxDisplayValue = Mathf.Max(maxValue, 2f);
                float y1 = rect.y + rect.height - (value1 / maxDisplayValue) * rect.height;
                float y2 = rect.y + rect.height - (value2 / maxDisplayValue) * rect.height;
                
                Handles.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2));
            }
            
            // 绘制基准线
            Handles.color = new Color(0.5f, 0.5f, 0.5f);
            float baselineY = rect.y + rect.height - (1f / Mathf.Max(maxValue, 2f)) * rect.height;
            Handles.DrawLine(new Vector2(rect.x, baselineY), new Vector2(rect.x + rect.width, baselineY));
            
            EditorGUILayout.LabelField($"{typeName}缩放: ×{minValue:F2} → ×{maxValue:F2}", EditorStyles.miniLabel);
        }
    }
}
#endif
