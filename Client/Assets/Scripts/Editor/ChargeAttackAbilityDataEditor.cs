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
            
            Rect rect = GUILayoutUtility.GetRect(0, 80, GUILayout.ExpandWidth(true));
            
            // 背景渐变
            DrawGradientBackground(rect);
            
            // 绘制网格
            DrawGrid(rect);
            
            // 绘制曲线（线性插值）
            Handles.color = new Color(0.3f, 1f, 1f);
            Handles.DrawAAPolyLine(3f, GetCurvePoints(rect, minValue, maxValue));
            
            // 绘制起点和终点标记
            DrawValueMarkers(rect, minValue, maxValue);
            
            // 绘制基准线 (1.0倍率)
            DrawBaselineLine(rect, maxValue);
            
            // 显示示例值
            DrawSampleValues(rect, minValue, maxValue);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"📊 {typeName}缩放曲线: ×{minValue:F2} → ×{maxValue:F2}", EditorStyles.miniLabel);
        }
    }

    private void DrawGradientBackground(Rect rect)
    {
        // 绘制渐变背景
        Texture2D gradientTex = new Texture2D(1, 2);
        gradientTex.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.2f));
        gradientTex.SetPixel(0, 1, new Color(0.25f, 0.25f, 0.3f));
        gradientTex.Apply();
        GUI.DrawTexture(rect, gradientTex);
    }

    private void DrawGrid(Rect rect)
    {
        Handles.color = new Color(0.4f, 0.4f, 0.4f, 0.3f);
        
        // 横向网格线
        for (int i = 1; i < 4; i++)
        {
            float y = rect.y + rect.height * i / 4f;
            Handles.DrawLine(new Vector2(rect.x, y), new Vector2(rect.x + rect.width, y));
        }
        
        // 纵向网格线
        for (int i = 1; i < 4; i++)
        {
            float x = rect.x + rect.width * i / 4f;
            Handles.DrawLine(new Vector2(x, rect.y), new Vector2(x, rect.y + rect.height));
        }
    }

    private Vector3[] GetCurvePoints(Rect rect, float minValue, float maxValue)
    {
        int segments = 30;
        Vector3[] points = new Vector3[segments + 1];
        float maxDisplayValue = Mathf.Max(maxValue, 2f);
        
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float value = Mathf.Lerp(minValue, maxValue, t);
            
            float x = rect.x + rect.width * t;
            float y = rect.y + rect.height - (value / maxDisplayValue) * (rect.height - 10);
            
            points[i] = new Vector2(x, y);
        }
        
        return points;
    }

    private void DrawValueMarkers(Rect rect, float minValue, float maxValue)
    {
        float maxDisplayValue = Mathf.Max(maxValue, 2f);
        
        // 起点标记
        Vector2 startPos = new Vector2(rect.x, rect.y + rect.height - (minValue / maxDisplayValue) * (rect.height - 10));
        Handles.color = new Color(0.3f, 1f, 0.3f);
        Handles.DrawSolidDisc(startPos, Vector3.forward, 4f);
        
        GUIStyle startStyle = new GUIStyle(EditorStyles.miniLabel);
        startStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);
        GUI.Label(new Rect(startPos.x - 20, startPos.y - 20, 40, 15), $"×{minValue:F2}", startStyle);
        
        // 终点标记
        Vector2 endPos = new Vector2(rect.x + rect.width, rect.y + rect.height - (maxValue / maxDisplayValue) * (rect.height - 10));
        Handles.color = new Color(1f, 0.3f, 0.3f);
        Handles.DrawSolidDisc(endPos, Vector3.forward, 4f);
        
        GUIStyle endStyle = new GUIStyle(EditorStyles.miniLabel);
        endStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
        GUI.Label(new Rect(endPos.x - 20, endPos.y - 20, 40, 15), $"×{maxValue:F2}", endStyle);
    }

    private void DrawBaselineLine(Rect rect, float maxValue)
    {
        float maxDisplayValue = Mathf.Max(maxValue, 2f);
        float baselineY = rect.y + rect.height - (1f / maxDisplayValue) * (rect.height - 10);
        
        Handles.color = new Color(1f, 1f, 0.5f, 0.5f);
        Handles.DrawDottedLine(
            new Vector2(rect.x, baselineY), 
            new Vector2(rect.x + rect.width, baselineY), 
            2f
        );
        
        GUIStyle baselineStyle = new GUIStyle(EditorStyles.miniLabel);
        baselineStyle.normal.textColor = new Color(1f, 1f, 0.5f);
        GUI.Label(new Rect(rect.x + 5, baselineY - 15, 50, 15), "×1.0", baselineStyle);
    }

    private void DrawSampleValues(Rect rect, float minValue, float maxValue)
    {
        // 在25%, 50%, 75%位置显示示例值
        float[] samplePositions = { 0.25f, 0.5f, 0.75f };
        
        foreach (float t in samplePositions)
        {
            float value = Mathf.Lerp(minValue, maxValue, t);
            float x = rect.x + rect.width * t;
            
            // 绘制竖线
            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            Handles.DrawLine(
                new Vector2(x, rect.y), 
                new Vector2(x, rect.y + rect.height)
            );
            
            // 显示百分比和值
            GUIStyle sampleStyle = new GUIStyle(EditorStyles.miniLabel);
            sampleStyle.alignment = TextAnchor.UpperCenter;
            sampleStyle.normal.textColor = new Color(0.8f, 0.8f, 1f);
            GUI.Label(new Rect(x - 25, rect.y + rect.height - 10, 50, 15), $"{t * 100:F0}%\n×{value:F2}", sampleStyle);
        }
    }
}
#endif
