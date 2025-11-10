#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 增强的飞行道具编辑器，提供可视化的配置界面
/// </summary>
[CustomEditor(typeof(ProjectileData), true)]
public class ProjectileDataEditor : UnityEditor.Editor
{
    private SerializedProperty identifierProp;
    private SerializedProperty projectileTypeProp;
    private SerializedProperty speedProp;
    private SerializedProperty lifetimeProp;
    private SerializedProperty maxDistanceProp;
    private SerializedProperty collisionShapeProp;
    private SerializedProperty damageProp;
    private SerializedProperty hitTypeProp;
    private SerializedProperty pierceCountProp;
    private SerializedProperty movementModeProp;
    private SerializedProperty gravityScaleProp;
    private SerializedProperty homingTargetProp;
    private SerializedProperty homingStrengthProp;

    private bool showBasicSettings = true;
    private bool showMovementSettings = true;
    private bool showCollisionSettings = true;
    private bool showDamageSettings = true;

    private void OnEnable()
    {
        identifierProp = serializedObject.FindProperty("Identifier");
        projectileTypeProp = serializedObject.FindProperty("ProjectileType");
        speedProp = serializedObject.FindProperty("Speed");
        lifetimeProp = serializedObject.FindProperty("Lifetime");
        maxDistanceProp = serializedObject.FindProperty("MaxDistance");
        collisionShapeProp = serializedObject.FindProperty("CollisionShape");
        damageProp = serializedObject.FindProperty("Damage");
        hitTypeProp = serializedObject.FindProperty("HitType");
        pierceCountProp = serializedObject.FindProperty("PierceCount");
        movementModeProp = serializedObject.FindProperty("MovementMode");
        gravityScaleProp = serializedObject.FindProperty("GravityScale");
        homingTargetProp = serializedObject.FindProperty("HomingTarget");
        homingStrengthProp = serializedObject.FindProperty("HomingStrength");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 标题和资源标识
        EditorGUILayout.Space(5);
        DrawHeaderWithIcon("🚀 飞行道具配置 (Projectile Configuration)");
        
        EditorGUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(true);
        if (identifierProp != null)
            EditorGUILayout.PropertyField(identifierProp, new GUIContent("资源标识"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // 类型说明
        DrawProjectileTypeInfo();

        EditorGUILayout.Space(10);

        // 基础设置
        showBasicSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBasicSettings, "⚙️ 基础设置 (Basic Settings)");
        if (showBasicSettings)
        {
            EditorGUI.indentLevel++;
            if (projectileTypeProp != null)
                EditorGUILayout.PropertyField(projectileTypeProp, new GUIContent("弹道类型", "Bullet或SkillField"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 移动设置
        showMovementSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMovementSettings, "🎯 移动设置 (Movement Settings)");
        if (showMovementSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithValue(speedProp, "移动速度", "Speed", "单位/秒");
            DrawFPPropertyWithValue(lifetimeProp, "生命时长", "Lifetime", "秒");
            DrawFPPropertyWithValue(maxDistanceProp, "最大距离", "MaxDistance", "单位");
            
            if (movementModeProp != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(movementModeProp, new GUIContent("移动模式", "直线、抛物线或追踪"));
                
                // 根据移动模式显示相关参数
                DrawMovementModeParameters();
            }
            
            // 绘制运动轨迹预览
            DrawTrajectoryPreview();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 碰撞设置
        showCollisionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCollisionSettings, "💥 碰撞设置 (Collision Settings)");
        if (showCollisionSettings)
        {
            EditorGUI.indentLevel++;
            if (collisionShapeProp != null)
            {
                EditorGUILayout.PropertyField(collisionShapeProp, new GUIContent("碰撞形状", "定义弹道的碰撞检测范围"));
            }
            
            if (pierceCountProp != null)
            {
                EditorGUILayout.PropertyField(pierceCountProp, new GUIContent("穿透次数", "可穿透的目标数量，0表示首次碰撞即销毁"));
                DrawPierceVisualization();
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 伤害设置
        showDamageSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showDamageSettings, "⚔️ 伤害设置 (Damage Settings)");
        if (showDamageSettings)
        {
            EditorGUI.indentLevel++;
            if (damageProp != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(damageProp, new GUIContent("伤害值", "造成的伤害数值"));
                DrawDamageBar(damageProp);
                EditorGUILayout.EndHorizontal();
            }
            
            if (hitTypeProp != null)
                EditorGUILayout.PropertyField(hitTypeProp, new GUIContent("受击类型", "Light/Heavy/Launch等"));
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // 绘制其他未处理的字段
        DrawRemainingProperties();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeaderWithIcon(string text)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        EditorGUILayout.LabelField(text, headerStyle);
    }

    private void DrawProjectileTypeInfo()
    {
        EditorGUILayout.HelpBox(
            "🎯 飞行道具系统说明：\n\n" +
            "• Bullet - 持续飞行的弹道类道具\n" +
            "  - 支持直线、抛物线、追踪等移动模式\n" +
            "  - 可设置穿透次数\n" +
            "  - 支持重力和追踪参数\n\n" +
            "• SkillField - 区域效果场\n" +
            "  - 在指定位置创建持续区域\n" +
            "  - 定时Tick触发效果\n" +
            "  - 可配置影响范围",
            MessageType.Info);
    }

    private void DrawFPPropertyWithValue(SerializedProperty property, string label, string propertyName, string unit)
    {
        if (property == null) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property, new GUIContent(label));
        
        var rawValueProp = property.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float value = rawValue / 65536f;
            
            // 根据属性类型使用不同颜色
            GUIStyle valueStyle = new GUIStyle(EditorStyles.label);
            if (value <= 0)
                valueStyle.normal.textColor = Color.red;
            else if (value > 10)
                valueStyle.normal.textColor = new Color(1f, 0.5f, 0f);
            else
                valueStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            
            EditorGUILayout.LabelField($"≈ {value:F2} {unit}", valueStyle, GUILayout.Width(100));
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawMovementModeParameters()
    {
        if (movementModeProp == null) return;
        
        int mode = movementModeProp.enumValueIndex;
        EditorGUI.indentLevel++;
        
        switch (mode)
        {
            case 1: // Parabolic (抛物线)
                EditorGUILayout.HelpBox("抛物线模式: 受重力影响，形成弧线轨迹", MessageType.None);
                if (gravityScaleProp != null)
                    DrawFPPropertyWithValue(gravityScaleProp, "重力缩放", "GravityScale", "");
                break;
                
            case 2: // Homing (追踪)
                EditorGUILayout.HelpBox("追踪模式: 自动追踪目标", MessageType.None);
                if (homingTargetProp != null)
                    EditorGUILayout.PropertyField(homingTargetProp, new GUIContent("追踪目标类型"));
                if (homingStrengthProp != null)
                    DrawFPPropertyWithValue(homingStrengthProp, "追踪强度", "HomingStrength", "");
                break;
                
            default: // Linear (直线)
                EditorGUILayout.HelpBox("直线模式: 匀速直线运动", MessageType.None);
                break;
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawTrajectoryPreview()
    {
        if (speedProp == null || lifetimeProp == null) return;
        
        var speedRaw = speedProp.FindPropertyRelative("RawValue");
        var lifetimeRaw = lifetimeProp.FindPropertyRelative("RawValue");
        
        if (speedRaw == null || lifetimeRaw == null) return;
        
        float speed = speedRaw.longValue / 65536f;
        float lifetime = lifetimeRaw.longValue / 65536f;
        float distance = speed * lifetime;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("运动轨迹预览", EditorStyles.boldLabel);
        
        Rect rect = GUILayoutUtility.GetRect(0, 80, GUILayout.ExpandWidth(true));
        
        // 背景
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
        
        // 绘制网格
        Handles.color = new Color(0.3f, 0.3f, 0.3f);
        for (int i = 1; i < 4; i++)
        {
            float y = rect.y + rect.height * i / 4f;
            Handles.DrawLine(new Vector2(rect.x, y), new Vector2(rect.x + rect.width, y));
        }
        
        // 起点和终点
        Vector2 start = new Vector2(rect.x + 20, rect.y + rect.height / 2);
        float scale = Mathf.Min((rect.width - 40) / Mathf.Max(distance, 1f), 50f);
        Vector2 end = start + new Vector2(distance * scale, 0);
        end.x = Mathf.Min(end.x, rect.x + rect.width - 20);
        
        // 绘制轨迹
        if (movementModeProp != null && movementModeProp.enumValueIndex == 1) // Parabolic
        {
            // 抛物线
            Handles.color = new Color(1f, 0.8f, 0.2f);
            int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float t1 = i / (float)segments;
                float t2 = (i + 1) / (float)segments;
                
                Vector2 p1 = Vector2.Lerp(start, end, t1);
                Vector2 p2 = Vector2.Lerp(start, end, t2);
                
                // 添加抛物线高度
                p1.y -= Mathf.Sin(t1 * Mathf.PI) * 20f;
                p2.y -= Mathf.Sin(t2 * Mathf.PI) * 20f;
                
                Handles.DrawLine(p1, p2);
            }
        }
        else
        {
            // 直线
            Handles.color = new Color(0.2f, 0.8f, 1f);
            Handles.DrawLine(start, end);
        }
        
        // 绘制箭头
        Vector2 arrowDir = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-arrowDir.y, arrowDir.x);
        Handles.DrawLine(end, end - arrowDir * 10f + perpendicular * 5f);
        Handles.DrawLine(end, end - arrowDir * 10f - perpendicular * 5f);
        
        // 起点标记
        Handles.color = Color.green;
        Handles.DrawSolidDisc(start, Vector3.forward, 3f);
        
        // 终点标记
        Handles.color = Color.red;
        Handles.DrawSolidDisc(end, Vector3.forward, 3f);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"预计飞行距离: {distance:F2} 单位 | 飞行时间: {lifetime:F2} 秒", EditorStyles.miniLabel);
    }

    private void DrawPierceVisualization()
    {
        if (pierceCountProp == null) return;
        
        int pierceCount = pierceCountProp.intValue;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("穿透效果:", GUILayout.Width(70));
        
        if (pierceCount == 0)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = Color.yellow;
            EditorGUILayout.LabelField("首次碰撞即销毁", style);
        }
        else
        {
            // 绘制穿透可视化
            for (int i = 0; i <= pierceCount && i < 5; i++)
            {
                Rect iconRect = GUILayoutUtility.GetRect(20, 20);
                EditorGUI.DrawRect(iconRect, i == pierceCount ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 0.8f, 0.3f));
                
                if (i < pierceCount)
                {
                    // 绘制箭头
                    Rect arrowRect = GUILayoutUtility.GetRect(15, 20);
                    GUI.Label(arrowRect, "→", EditorStyles.boldLabel);
                }
            }
            
            if (pierceCount >= 5)
            {
                EditorGUILayout.LabelField("...", EditorStyles.boldLabel);
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDamageBar(SerializedProperty damageProp)
    {
        if (damageProp == null) return;
        
        int damage = damageProp.intValue;
        Rect barRect = GUILayoutUtility.GetRect(100, 18);
        
        // 背景
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));
        
        // 伤害条
        float damagePercent = Mathf.Min(damage / 100f, 1f);
        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * damagePercent, barRect.height);
        
        Color damageColor = Color.Lerp(new Color(0.3f, 1f, 0.3f), new Color(1f, 0.3f, 0.3f), damagePercent);
        EditorGUI.DrawRect(fillRect, damageColor);
        
        // 文字
        GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel);
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;
        GUI.Label(barRect, $"{damage}", textStyle);
    }

    private void DrawRemainingProperties()
    {
        // 绘制所有未在自定义UI中处理的属性
        DrawPropertiesExcluding(serializedObject,
            "m_Script",
            "Identifier",
            "ProjectileType",
            "Speed",
            "Lifetime",
            "MaxDistance",
            "CollisionShape",
            "Damage",
            "HitType",
            "PierceCount",
            "MovementMode",
            "GravityScale",
            "HomingTarget",
            "HomingStrength"
        );
    }
}

/// <summary>
/// 增强的技能场编辑器，提供可视化的配置界面
/// </summary>
[CustomEditor(typeof(SkillFieldData), true)]
public class SkillFieldDataEditor : UnityEditor.Editor
{
    private SerializedProperty identifierProp;
    private SerializedProperty durationProp;
    private SerializedProperty tickIntervalProp;
    private SerializedProperty effectAreaShapeProp;
    private SerializedProperty effectTypeProp;
    private SerializedProperty damagePerTickProp;
    private SerializedProperty affectAlliesProp;
    private SerializedProperty affectEnemiesProp;
    private SerializedProperty maxTargetsProp;

    private bool showBasicSettings = true;
    private bool showEffectSettings = true;
    private bool showTargetingSettings = true;
    private bool showAreaSettings = true;

    private void OnEnable()
    {
        identifierProp = serializedObject.FindProperty("Identifier");
        durationProp = serializedObject.FindProperty("Duration");
        tickIntervalProp = serializedObject.FindProperty("TickInterval");
        effectAreaShapeProp = serializedObject.FindProperty("EffectAreaShape");
        effectTypeProp = serializedObject.FindProperty("EffectType");
        damagePerTickProp = serializedObject.FindProperty("DamagePerTick");
        affectAlliesProp = serializedObject.FindProperty("AffectAllies");
        affectEnemiesProp = serializedObject.FindProperty("AffectEnemies");
        maxTargetsProp = serializedObject.FindProperty("MaxTargets");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 标题和资源标识
        EditorGUILayout.Space(5);
        DrawHeaderWithIcon("✨ 技能场配置 (Skill Field Configuration)");
        
        EditorGUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(true);
        if (identifierProp != null)
            EditorGUILayout.PropertyField(identifierProp, new GUIContent("资源标识"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // 类型说明
        DrawSkillFieldInfo();

        EditorGUILayout.Space(10);

        // 基础设置
        showBasicSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBasicSettings, "⏱ 时序设置 (Timing Settings)");
        if (showBasicSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithValue(durationProp, "持续时间", "Duration", "秒");
            DrawFPPropertyWithValue(tickIntervalProp, "触发间隔", "TickInterval", "秒");
            
            // 绘制tick时间轴
            DrawTickTimeline();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 区域设置
        showAreaSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAreaSettings, "🎯 区域设置 (Area Settings)");
        if (showAreaSettings)
        {
            EditorGUI.indentLevel++;
            if (effectAreaShapeProp != null)
            {
                EditorGUILayout.PropertyField(effectAreaShapeProp, new GUIContent("效果区域形状", "定义技能场的影响范围"));
                DrawAreaPreview();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 效果设置
        showEffectSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showEffectSettings, "💫 效果设置 (Effect Settings)");
        if (showEffectSettings)
        {
            EditorGUI.indentLevel++;
            if (effectTypeProp != null)
            {
                EditorGUILayout.PropertyField(effectTypeProp, new GUIContent("效果类型", "伤害、治疗、Buff、Debuff等"));
                DrawEffectTypeDetails();
            }
            
            if (damagePerTickProp != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(damagePerTickProp, new GUIContent("每跳伤害/治疗", "每次Tick触发的数值"));
                DrawEffectBar(damagePerTickProp);
                EditorGUILayout.EndHorizontal();
            }
            
            // 总效果计算
            DrawTotalEffectCalculation();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 目标设置
        showTargetingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showTargetingSettings, "🎲 目标设置 (Targeting Settings)");
        if (showTargetingSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            if (affectAlliesProp != null)
                EditorGUILayout.PropertyField(affectAlliesProp, new GUIContent("影响友军", "是否对友方目标生效"), GUILayout.Width(200));
            if (affectEnemiesProp != null)
                EditorGUILayout.PropertyField(affectEnemiesProp, new GUIContent("影响敌人", "是否对敌方目标生效"));
            EditorGUILayout.EndHorizontal();
            
            // 目标配置可视化
            DrawTargetingVisualization();
            
            if (maxTargetsProp != null)
            {
                EditorGUILayout.PropertyField(maxTargetsProp, new GUIContent("最大目标数", "同时影响的最大目标数量，0表示无限制"));
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // 绘制其他未处理的字段
        DrawRemainingProperties();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeaderWithIcon(string text)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = new Color(0.9f, 0.8f, 1f);
        EditorGUILayout.LabelField(text, headerStyle);
    }

    private void DrawSkillFieldInfo()
    {
        EditorGUILayout.HelpBox(
            "✨ 技能场系统说明：\n\n" +
            "• 在指定位置创建持续性区域效果\n" +
            "• 支持定时Tick触发效果\n" +
            "• 可配置影响友军/敌人\n" +
            "• 支持多种效果类型：\n" +
            "  - 伤害 (Damage)\n" +
            "  - 治疗 (Heal)\n" +
            "  - Buff增益\n" +
            "  - Debuff减益\n" +
            "  - 控制效果",
            MessageType.Info);
    }

    private void DrawFPPropertyWithValue(SerializedProperty property, string label, string propertyName, string unit)
    {
        if (property == null) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property, new GUIContent(label));
        
        var rawValueProp = property.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float value = rawValue / 65536f;
            
            GUIStyle valueStyle = new GUIStyle(EditorStyles.label);
            valueStyle.normal.textColor = new Color(0.5f, 1f, 0.8f);
            
            EditorGUILayout.LabelField($"≈ {value:F2} {unit}", valueStyle, GUILayout.Width(100));
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTickTimeline()
    {
        if (durationProp == null || tickIntervalProp == null) return;
        
        var durationRaw = durationProp.FindPropertyRelative("RawValue");
        var intervalRaw = tickIntervalProp.FindPropertyRelative("RawValue");
        
        if (durationRaw == null || intervalRaw == null) return;
        
        float duration = durationRaw.longValue / 65536f;
        float interval = intervalRaw.longValue / 65536f;
        
        if (interval <= 0 || duration <= 0) return;
        
        int tickCount = Mathf.FloorToInt(duration / interval);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("触发时间轴", EditorStyles.boldLabel);
        
        Rect rect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        
        // 背景
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.2f));
        
        // 时间轴线
        Rect timelineRect = new Rect(rect.x + 10, rect.y + rect.height / 2, rect.width - 20, 2);
        EditorGUI.DrawRect(timelineRect, new Color(0.5f, 0.5f, 0.5f));
        
        // 绘制tick标记
        Handles.color = new Color(0.3f, 1f, 0.5f);
        float scale = (rect.width - 20) / duration;
        
        for (int i = 0; i <= tickCount && i < 20; i++)
        {
            float time = i * interval;
            float x = rect.x + 10 + time * scale;
            Vector2 top = new Vector2(x, rect.y + 10);
            Vector2 bottom = new Vector2(x, rect.y + rect.height - 10);
            
            Handles.DrawLine(top, bottom);
            
            // 绘制tick标记点
            Handles.DrawSolidDisc(new Vector2(x, rect.y + rect.height / 2), Vector3.forward, 3f);
        }
        
        if (tickCount > 20)
        {
            GUI.Label(new Rect(rect.x + rect.width - 30, rect.y + rect.height / 2 - 10, 30, 20), "...", EditorStyles.boldLabel);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"总计触发次数: {tickCount} 次 | 总持续: {duration:F2}秒", EditorStyles.miniLabel);
    }

    private void DrawAreaPreview()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("💡 技能场将在目标位置创建持续效果区域", MessageType.None);
    }

    private void DrawEffectTypeDetails()
    {
        if (effectTypeProp == null) return;
        
        EditorGUILayout.Space(5);
        int effectType = effectTypeProp.enumValueIndex;
        
        string description = effectType switch
        {
            0 => "💥 造成持续伤害",
            1 => "💚 提供持续治疗",
            2 => "⬆️ 施加增益Buff",
            3 => "⬇️ 施加减益Debuff",
            4 => "🔒 施加控制效果",
            _ => "⚙️ 自定义效果"
        };
        
        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        style.normal.textColor = new Color(0.8f, 0.9f, 1f);
        EditorGUILayout.LabelField(description, style);
    }

    private void DrawEffectBar(SerializedProperty property)
    {
        if (property == null) return;
        
        int value = property.intValue;
        bool isNegative = value < 0;
        int absValue = Mathf.Abs(value);
        
        Rect barRect = GUILayoutUtility.GetRect(100, 18);
        
        // 背景
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));
        
        // 效果条
        float percent = Mathf.Min(absValue / 50f, 1f);
        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * percent, barRect.height);
        
        Color barColor = isNegative ? 
            Color.Lerp(new Color(1f, 0.5f, 0.3f), new Color(1f, 0.2f, 0.2f), percent) : 
            Color.Lerp(new Color(0.3f, 1f, 0.5f), new Color(0.2f, 1f, 0.2f), percent);
        EditorGUI.DrawRect(fillRect, barColor);
        
        // 文字
        GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel);
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;
        GUI.Label(barRect, $"{value}", textStyle);
    }

    private void DrawTotalEffectCalculation()
    {
        if (durationProp == null || tickIntervalProp == null || damagePerTickProp == null) return;
        
        var durationRaw = durationProp.FindPropertyRelative("RawValue");
        var intervalRaw = tickIntervalProp.FindPropertyRelative("RawValue");
        
        if (durationRaw == null || intervalRaw == null) return;
        
        float duration = durationRaw.longValue / 65536f;
        float interval = intervalRaw.longValue / 65536f;
        int damagePerTick = damagePerTickProp.intValue;
        
        if (interval > 0)
        {
            int tickCount = Mathf.FloorToInt(duration / interval);
            int totalEffect = tickCount * damagePerTick;
            
            EditorGUILayout.Space(5);
            Rect calcRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(calcRect, new Color(0.2f, 0.3f, 0.4f, 0.5f));
            
            GUIStyle calcStyle = new GUIStyle(EditorStyles.boldLabel);
            calcStyle.alignment = TextAnchor.MiddleCenter;
            calcStyle.normal.textColor = totalEffect >= 0 ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 1f, 0.3f);
            
            string effectText = damagePerTick >= 0 ? "总伤害" : "总治疗";
            GUI.Label(calcRect, $"📊 {effectText}: {Mathf.Abs(totalEffect)} ({tickCount} 次 × {Mathf.Abs(damagePerTick)})", calcStyle);
        }
    }

    private void DrawTargetingVisualization()
    {
        if (affectAlliesProp == null || affectEnemiesProp == null) return;
        
        bool allies = affectAlliesProp.boolValue;
        bool enemies = affectEnemiesProp.boolValue;
        
        EditorGUILayout.Space(5);
        Rect rect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
        
        float boxWidth = rect.width / 2 - 10;
        
        // 友军框
        Rect alliesRect = new Rect(rect.x, rect.y, boxWidth, rect.height);
        Color alliesColor = allies ? new Color(0.3f, 0.8f, 1f, 0.3f) : new Color(0.2f, 0.2f, 0.2f, 0.3f);
        EditorGUI.DrawRect(alliesRect, alliesColor);
        
        GUIStyle alliesStyle = new GUIStyle(EditorStyles.boldLabel);
        alliesStyle.alignment = TextAnchor.MiddleCenter;
        alliesStyle.normal.textColor = allies ? new Color(0.3f, 0.8f, 1f) : Color.gray;
        GUI.Label(alliesRect, "👥\n友军", alliesStyle);
        
        // 敌军框
        Rect enemiesRect = new Rect(rect.x + boxWidth + 20, rect.y, boxWidth, rect.height);
        Color enemiesColor = enemies ? new Color(1f, 0.3f, 0.3f, 0.3f) : new Color(0.2f, 0.2f, 0.2f, 0.3f);
        EditorGUI.DrawRect(enemiesRect, enemiesColor);
        
        GUIStyle enemiesStyle = new GUIStyle(EditorStyles.boldLabel);
        enemiesStyle.alignment = TextAnchor.MiddleCenter;
        enemiesStyle.normal.textColor = enemies ? new Color(1f, 0.3f, 0.3f) : Color.gray;
        GUI.Label(enemiesRect, "⚔️\n敌军", enemiesStyle);
        
        // 警告提示
        if (!allies && !enemies)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("⚠️ 警告：未选择任何目标，技能场将不会影响任何单位！", MessageType.Warning);
        }
    }

    private void DrawRemainingProperties()
    {
        DrawPropertiesExcluding(serializedObject,
            "m_Script",
            "Identifier",
            "Duration",
            "TickInterval",
            "EffectAreaShape",
            "EffectType",
            "DamagePerTick",
            "AffectAllies",
            "AffectEnemies",
            "MaxTargets"
        );
    }
}
#endif