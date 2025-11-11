#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Combat;

/// <summary>
/// AttackDefinition 的自定义编辑器
/// Custom Editor for AttackDefinition - Visualizes attack parameters with color coding and progress bars
/// </summary>
[CustomEditor(typeof(AttackDefinition), true)]
public class AttackDefinitionEditor : Editor
{
    private SerializedProperty coolDownProp;
    private SerializedProperty rangeProp;
    private SerializedProperty minDamageProp;
    private SerializedProperty maxDamageProp;
    private SerializedProperty criticalMultiplierProp;
    private SerializedProperty criticalChanceProp;

    private void OnEnable()
    {
        // 获取所有属性
        coolDownProp = serializedObject.FindProperty("CoolDown");
        rangeProp = serializedObject.FindProperty("Range");
        minDamageProp = serializedObject.FindProperty("MinDamage");
        maxDamageProp = serializedObject.FindProperty("MaxDamage");
        criticalMultiplierProp = serializedObject.FindProperty("CriticalMultiplier");
        criticalChanceProp = serializedObject.FindProperty("CriticalChance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 标题
        CustomEditorStyles.DrawHeader("攻击定义配置 Attack Definition", CustomEditorStyles.Icons.Attack);

        EditorGUILayout.Space(10);

        // ======= 时序设置 =======
        CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);
        CustomEditorStyles.DrawSubHeader("时序设置 Timing", CustomEditorStyles.Icons.Timing);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(coolDownProp, new GUIContent("冷却时间 Cool Down (s)"));
        float coolDown = coolDownProp.floatValue;
        Color coolDownColor = CustomEditorStyles.GetTimingColor(coolDown);
        CustomEditorStyles.DrawColoredValue($"≈ {coolDown:F2}s", coolDownColor);
        EditorGUILayout.EndHorizontal();

        // 冷却时间可视化进度条
        if (coolDown > 0)
        {
            Rect cdRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            float cdProgress = Mathf.Clamp01(coolDown / 10f); // 假设10秒为最大值
            CustomEditorStyles.DrawProgressBar(cdRect, cdProgress, coolDownColor, $"{coolDown:F2}s");
        }

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= 范围设置 =======
        CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);
        CustomEditorStyles.DrawSubHeader("攻击范围 Attack Range", CustomEditorStyles.Icons.Area);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(rangeProp, new GUIContent("攻击距离 Range"));
        float range = rangeProp.floatValue;
        Color rangeColor = range < 2f ? CustomEditorStyles.Colors.TypeHitbox : 
                          range < 5f ? CustomEditorStyles.Colors.TypeProjectile : 
                          CustomEditorStyles.Colors.TypeSkillField;
        CustomEditorStyles.DrawColoredValue($"{range:F2}m", rangeColor);
        EditorGUILayout.EndHorizontal();

        // 范围类型提示
        string rangeType = range < 2f ? "近战 Melee" : 
                          range < 5f ? "中距离 Mid-Range" : 
                          "远程 Long-Range";
        EditorGUILayout.HelpBox($"类型: {rangeType}", MessageType.Info);

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= 伤害设置 =======
        CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);
        CustomEditorStyles.DrawSubHeader("伤害设置 Damage", CustomEditorStyles.Icons.Damage);
        
        // 最小伤害
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(minDamageProp, new GUIContent("最小伤害 Min Damage"));
        float minDamage = minDamageProp.floatValue;
        Color minDmgColor = CustomEditorStyles.GetDamageColor((int)minDamage);
        CustomEditorStyles.DrawColoredValue($"{minDamage:F0}", minDmgColor);
        EditorGUILayout.EndHorizontal();

        // 最大伤害
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(maxDamageProp, new GUIContent("最大伤害 Max Damage"));
        float maxDamage = maxDamageProp.floatValue;
        Color maxDmgColor = CustomEditorStyles.GetDamageColor((int)maxDamage);
        CustomEditorStyles.DrawColoredValue($"{maxDamage:F0}", maxDmgColor);
        EditorGUILayout.EndHorizontal();

        // 平均伤害显示
        float avgDamage = (minDamage + maxDamage) / 2f;
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"{CustomEditorStyles.Icons.Info} 平均伤害 Average Damage: {avgDamage:F1}");

        // 伤害范围可视化
        Rect dmgRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
        DrawDamageRangeBar(dmgRect, minDamage, maxDamage);

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= 暴击设置 =======
        CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);
        CustomEditorStyles.DrawSubHeader("暴击设置 Critical Hit", CustomEditorStyles.Icons.Buff);
        
        // 暴击倍率
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(criticalMultiplierProp, new GUIContent("暴击倍率 Critical Multiplier"));
        float critMult = criticalMultiplierProp.floatValue;
        Color critMultColor = critMult >= 3f ? CustomEditorStyles.Colors.DamageVeryHigh :
                             critMult >= 2f ? CustomEditorStyles.Colors.DamageHigh :
                             CustomEditorStyles.Colors.DamageMedium;
        CustomEditorStyles.DrawColoredValue($"×{critMult:F2}", critMultColor);
        EditorGUILayout.EndHorizontal();

        // 暴击概率
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(criticalChanceProp, new GUIContent("暴击概率 Critical Chance"));
        float critChance = criticalChanceProp.floatValue;
        Color critChanceColor = critChance >= 0.5f ? CustomEditorStyles.Colors.StatusValid :
                                critChance >= 0.25f ? CustomEditorStyles.Colors.StatusWarning :
                                CustomEditorStyles.Colors.StatusDisabled;
        CustomEditorStyles.DrawColoredValue($"{(critChance * 100):F1}%", critChanceColor);
        EditorGUILayout.EndHorizontal();

        // 暴击概率进度条
        Rect critRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
        CustomEditorStyles.DrawProgressBar(critRect, critChance, critChanceColor, $"{(critChance * 100):F1}%");

        // 暴击伤害计算
        float critDamage = avgDamage * critMult;
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"{CustomEditorStyles.Icons.Attack} 暴击平均伤害 Crit Avg Damage: {critDamage:F1}");

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= 总结面板 =======
        CustomEditorStyles.BeginColoredBox(new Color(0.2f, 0.3f, 0.4f, 0.3f));
        CustomEditorStyles.DrawSubHeader("攻击总结 Attack Summary", CustomEditorStyles.Icons.Info);
        
        float expectedDamage = avgDamage * (1f - critChance) + critDamage * critChance;
        float dps = coolDown > 0 ? expectedDamage / coolDown : 0;
        
        EditorGUILayout.LabelField($"• 期望伤害 Expected Damage: {expectedDamage:F1}");
        if (coolDown > 0)
        {
            EditorGUILayout.LabelField($"• DPS (Damage Per Second): {dps:F2}");
        }
        EditorGUILayout.LabelField($"• 攻击类型 Attack Type: {rangeType}");
        
        // 性能评估
        string performance = dps >= 50 ? "🔥 高输出 High" :
                           dps >= 20 ? "✓ 中等 Medium" :
                           dps >= 10 ? "⚠ 低输出 Low" :
                           "⚠ 极低 Very Low";
        EditorGUILayout.LabelField($"• 输出评估 DPS Rating: {performance}");

        CustomEditorStyles.EndColoredBox();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制伤害范围条
    /// </summary>
    private void DrawDamageRangeBar(Rect rect, float minDmg, float maxDmg)
    {
        // 背景
        EditorGUI.DrawRect(rect, CustomEditorStyles.Colors.BackgroundDark);

        // 计算最大值用于归一化
        float maxValue = Mathf.Max(maxDmg, 100f);
        float minProgress = minDmg / maxValue;
        float maxProgress = maxDmg / maxValue;

        // 绘制最小伤害到最大伤害的渐变区域
        Rect minRect = new Rect(rect.x, rect.y, rect.width * minProgress, rect.height);
        Rect rangeRect = new Rect(rect.x + rect.width * minProgress, rect.y, 
                                  rect.width * (maxProgress - minProgress), rect.height);

        // 最小伤害区域 - 深色
        EditorGUI.DrawRect(minRect, new Color(0.3f, 0.3f, 0.3f, 0.5f));

        // 伤害范围区域 - 渐变色
        Color minColor = CustomEditorStyles.GetDamageColor((int)minDmg);
        Color maxColor = CustomEditorStyles.GetDamageColor((int)maxDmg);
        
        // 绘制渐变
        int steps = 20;
        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)steps;
            Color color = Color.Lerp(minColor, maxColor, t);
            Rect stepRect = new Rect(
                rangeRect.x + rangeRect.width * t,
                rangeRect.y,
                rangeRect.width / steps,
                rangeRect.height
            );
            EditorGUI.DrawRect(stepRect, color);
        }

        // 边框
        Handles.color = new Color(0.5f, 0.5f, 0.5f);
        Handles.DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin));
        Handles.DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax));
        Handles.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax));
        Handles.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin, rect.yMin));

        // 标签
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;
        
        // 最小值标签
        Rect minLabelRect = new Rect(rect.x, rect.y, rect.width * minProgress, rect.height);
        if (minLabelRect.width > 30)
        {
            GUI.Label(minLabelRect, $"Min: {minDmg:F0}", labelStyle);
        }

        // 最大值标签
        Rect maxLabelRect = new Rect(rect.x + rect.width * minProgress, rect.y, 
                                     rect.width * (maxProgress - minProgress), rect.height);
        if (maxLabelRect.width > 30)
        {
            GUI.Label(maxLabelRect, $"Max: {maxDmg:F0}", labelStyle);
        }
    }
}
#endif
