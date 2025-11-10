#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 自定义编辑器，用于优化 AttackAbilityData 的显示
/// 将攻击相关字段进行分组和可视化
/// </summary>
[CustomEditor(typeof(AttackAbilityData), true)]
public class AttackAbilityDataEditor : AbilityDataEditor
{
    private SerializedProperty attackShapeProp;
    private SerializedProperty hitboxActiveTimeProp;
    private SerializedProperty hitboxActiveDurationProp;
    private SerializedProperty knockbackForceProp;
    private SerializedProperty knockbackDirectionXProp;
    private SerializedProperty knockbackDirectionYProp;
    private SerializedProperty hitstunDurationProp;
    private SerializedProperty hitTypeProp;

    private bool showAttackRangeSettings = true;
    private bool showAttackTimingSettings = true;
    private bool showKnockbackSettings = true;
    private bool showHitstunSettings = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        attackShapeProp = serializedObject.FindProperty("AttackShape");
        hitboxActiveTimeProp = serializedObject.FindProperty("HitboxActiveTime");
        hitboxActiveDurationProp = serializedObject.FindProperty("HitboxActiveDuration");
        knockbackForceProp = serializedObject.FindProperty("KnockbackForce");
        knockbackDirectionXProp = serializedObject.FindProperty("KnockbackDirectionX");
        knockbackDirectionYProp = serializedObject.FindProperty("KnockbackDirectionY");
        hitstunDurationProp = serializedObject.FindProperty("HitstunDuration");
        hitTypeProp = serializedObject.FindProperty("HitType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 先绘制基类的内容（但不包括子类字段）
        DrawBaseClassProperties();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("攻击特定设置 (Attack Settings)", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 攻击范围设置
        showAttackRangeSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAttackRangeSettings, "🎯 攻击范围 (Attack Range)");
        if (showAttackRangeSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(attackShapeProp, new GUIContent("攻击判定形状", "定义攻击的碰撞检测形状"));
            DrawShapePreview();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 攻击时序设置
        showAttackTimingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAttackTimingSettings, "⏱ 攻击时序 (Attack Timing)");
        if (showAttackTimingSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithSeconds(hitboxActiveTimeProp, "判定激活时间", "从动画开始到判定触发的延迟（启动帧）");
            DrawFPPropertyWithSeconds(hitboxActiveDurationProp, "判定持续时间", "判定生效的时间窗口（判定帧）");
            
            // 绘制时序可视化
            DrawTimingVisualization();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 击退设置
        showKnockbackSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showKnockbackSettings, "💥 击退设置 (Knockback)");
        if (showKnockbackSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithValue(knockbackForceProp, "击退力度");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("击退方向 (Direction)", GUILayout.Width(146));
            EditorGUILayout.LabelField("X:", GUILayout.Width(15));
            DrawFPPropertyInline(knockbackDirectionXProp);
            EditorGUILayout.LabelField("Y:", GUILayout.Width(15));
            DrawFPPropertyInline(knockbackDirectionYProp);
            EditorGUILayout.EndHorizontal();
            
            // 绘制方向可视化
            DrawKnockbackDirectionVisualization();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 硬直设置
        showHitstunSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showHitstunSettings, "🛑 硬直设置 (Hitstun)");
        if (showHitstunSettings)
        {
            EditorGUI.indentLevel++;
            DrawFPPropertyWithSeconds(hitstunDurationProp, "硬直时间", "敌人受击后的硬直持续时间");
            EditorGUILayout.PropertyField(hitTypeProp, new GUIContent("受击类型", "Light/Heavy/Launch等类型"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // 绘制其他子类特有字段（例如ComboAttackAbilityData的额外字段）
        DrawRemainingProperties();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBaseClassProperties()
    {
        // 手动绘制基类内容，但不自动绘制所有字段
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // 跳过Script和已经处理的字段
            if (iterator.name == "m_Script" || IsAttackSpecificProperty(iterator.name))
                continue;
                
            EditorGUILayout.PropertyField(iterator, true);
        }
    }

    private bool IsAttackSpecificProperty(string propertyName)
    {
        return propertyName == "AttackShape" ||
               propertyName == "HitboxActiveTime" ||
               propertyName == "HitboxActiveDuration" ||
               propertyName == "KnockbackForce" ||
               propertyName == "KnockbackDirectionX" ||
               propertyName == "KnockbackDirectionY" ||
               propertyName == "HitstunDuration" ||
               propertyName == "HitType" ||
               propertyName == "MaxComboCount" ||
               propertyName == "ComboWindow" ||
               propertyName == "ComboSteps";
    }

    private void DrawRemainingProperties()
    {
        // 绘制子类特有的其他字段
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
            "_uiAbilityPrefab",
            "AttackShape",
            "HitboxActiveTime",
            "HitboxActiveDuration",
            "KnockbackForce",
            "KnockbackDirectionX",
            "KnockbackDirectionY",
            "HitstunDuration",
            "HitType"
        );
    }

    private void DrawFPPropertyWithValue(SerializedProperty property, string label)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property, new GUIContent(label));
        
        var rawValueProp = property.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float value = rawValue / 65536f;
            EditorGUILayout.LabelField($"≈ {value:F2}", GUILayout.Width(70));
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawFPPropertyInline(SerializedProperty property)
    {
        var rawValueProp = property.FindPropertyRelative("RawValue");
        if (rawValueProp != null)
        {
            long rawValue = rawValueProp.longValue;
            float value = rawValue / 65536f;
            float newValue = EditorGUILayout.FloatField(value, GUILayout.Width(50));
            
            if (Mathf.Abs(newValue - value) > 0.001f)
            {
                rawValueProp.longValue = (long)(newValue * 65536f);
            }
        }
    }

    private void DrawTimingVisualization()
    {
        var hitboxStartRaw = hitboxActiveTimeProp.FindPropertyRelative("RawValue");
        var hitboxDurationRaw = hitboxActiveDurationProp.FindPropertyRelative("RawValue");
        
        if (hitboxStartRaw != null && hitboxDurationRaw != null)
        {
            float startTime = hitboxStartRaw.longValue / 65536f;
            float duration = hitboxDurationRaw.longValue / 65536f;
            float endTime = startTime + duration;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"时序: 启动 {startTime:F2}s -> 判定 {startTime:F2}s~{endTime:F2}s", EditorStyles.miniLabel);
            
            // 绘制简单的时间轴
            Rect rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 8, rect.width, 4), new Color(0.3f, 0.3f, 0.3f));
            
            float scale = rect.width / Mathf.Max(endTime + 0.1f, 1f);
            float startX = rect.x + startTime * scale;
            float width = duration * scale;
            
            EditorGUI.DrawRect(new Rect(startX, rect.y + 6, width, 8), new Color(1f, 0.5f, 0f));
        }
    }

    private void DrawKnockbackDirectionVisualization()
    {
        var xRaw = knockbackDirectionXProp.FindPropertyRelative("RawValue");
        var yRaw = knockbackDirectionYProp.FindPropertyRelative("RawValue");
        
        if (xRaw != null && yRaw != null)
        {
            float x = xRaw.longValue / 65536f;
            float y = yRaw.longValue / 65536f;
            
            EditorGUILayout.Space(5);
            
            Rect rect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
            Rect centerRect = new Rect(rect.x + 50, rect.y + 50, rect.width - 100, rect.height - 100);
            
            // 背景
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
            
            // 中心点
            Vector2 center = new Vector2(rect.x + 50, rect.y + 50);
            EditorGUI.DrawRect(new Rect(center.x - 2, center.y - 2, 4, 4), Color.white);
            
            // 方向箭头
            Vector2 direction = new Vector2(x, -y).normalized * 40f;
            Vector2 end = center + direction;
            
            Handles.color = Color.yellow;
            Handles.DrawLine(center, end);
            
            // 绘制箭头头部
            Vector2 arrowDir = direction.normalized * 8f;
            Vector2 perpendicular = new Vector2(-arrowDir.y, arrowDir.x) * 0.5f;
            Handles.DrawLine(end, end - arrowDir + perpendicular);
            Handles.DrawLine(end, end - arrowDir - perpendicular);
            
            EditorGUILayout.LabelField($"方向: ({x:F2}, {y:F2})", EditorStyles.miniLabel);
        }
    }

    private void DrawShapePreview()
    {
        if (attackShapeProp == null) return;
        
        var shapeTypeProp = attackShapeProp.FindPropertyRelative("ShapeType");
        if (shapeTypeProp != null)
        {
            string shapeType = shapeTypeProp.enumNames[shapeTypeProp.enumValueIndex];
            EditorGUILayout.LabelField($"形状类型: {shapeType}", EditorStyles.miniLabel);
        }
    }
}
#endif
