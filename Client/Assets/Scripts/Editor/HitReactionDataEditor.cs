#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// 自定义编辑器，用于优化 HitReactionData 及其子类的显示
/// 将受击反应相关字段进行分组和可视化
/// </summary>
[CustomEditor(typeof(HitReactionData), true)]
public class HitReactionDataEditor : Editor
{
    private SerializedProperty identifierProp;
    private SerializedProperty canBeKnockedBackProp;
    private SerializedProperty canBeHitstunnedProp;
    private SerializedProperty lightHitStunMultiplierProp;
    private SerializedProperty heavyHitStunMultiplierProp;
    private SerializedProperty knockbackProfileProp;
    private SerializedProperty hitInterruptsActionsProp;

    private bool showCoreSettings = true;
    private bool showHitstunSettings = true;
    private bool showKnockbackSettings = true;
    private bool showBehaviorSettings = true;

    private void OnEnable()
    {
        identifierProp = serializedObject.FindProperty("Identifier");
        canBeKnockedBackProp = serializedObject.FindProperty("CanBeKnockedBack");
        canBeHitstunnedProp = serializedObject.FindProperty("CanBeHitstunned");
        
        // PlayerHitReactionData 特有的属性
        lightHitStunMultiplierProp = serializedObject.FindProperty("LightHitStunMultiplier");
        heavyHitStunMultiplierProp = serializedObject.FindProperty("HeavyHitStunMultiplier");
        knockbackProfileProp = serializedObject.FindProperty("KnockbackProfile");
        hitInterruptsActionsProp = serializedObject.FindProperty("HitInterruptsActions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Asset Identifier
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("资源标识", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(identifierProp);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // 核心标志设置
        showCoreSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCoreSettings, "🎯 核心标志 (Core Flags)");
        if (showCoreSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(canBeKnockedBackProp, new GUIContent("可被击退", "是否允许被击退"));
            EditorGUILayout.PropertyField(canBeHitstunnedProp, new GUIContent("可被硬直", "是否允许进入硬直状态"));
            
            // 显示状态提示
            if (!canBeKnockedBackProp.boolValue && !canBeHitstunnedProp.boolValue)
            {
                EditorGUILayout.HelpBox("⚠️ 当前配置为免疫击退和硬直，角色将完全无视受击效果", MessageType.Warning);
            }
            else if (!canBeKnockedBackProp.boolValue)
            {
                EditorGUILayout.HelpBox("ℹ️ 仅免疫击退，但仍会受到硬直影响", MessageType.Info);
            }
            else if (!canBeHitstunnedProp.boolValue)
            {
                EditorGUILayout.HelpBox("ℹ️ 仅免疫硬直，但仍会被击退", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(5);

        // 硬直设置（仅 PlayerHitReactionData 有）
        if (lightHitStunMultiplierProp != null && heavyHitStunMultiplierProp != null)
        {
            showHitstunSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showHitstunSettings, "🛑 硬直设置 (Hitstun Settings)");
            if (showHitstunSettings)
            {
                EditorGUI.indentLevel++;
                DrawFPPropertyWithMultiplier(lightHitStunMultiplierProp, "轻击硬直倍率", "对轻击造成的硬直时间的倍数");
                DrawFPPropertyWithMultiplier(heavyHitStunMultiplierProp, "重击硬直倍率", "对重击造成的硬直时间的倍数");
                
                // 显示倍率参考
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("参考: 1.0 = 基础时间, 0.5 = 减半, 1.5 = 增加50%", EditorStyles.miniLabel);
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);
        }

        // 击退设置（仅 PlayerHitReactionData 有）
        if (knockbackProfileProp != null)
        {
            showKnockbackSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showKnockbackSettings, "💥 击退配置 (Knockback Settings)");
            if (showKnockbackSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(knockbackProfileProp, new GUIContent("击退配置文件", "定义击退的物理行为和曲线"));
                
                // 显示当前模式信息
                var modeProp = knockbackProfileProp.FindPropertyRelative("Mode");
                if (modeProp != null)
                {
                    string modeName = modeProp.enumNames[modeProp.enumValueIndex];
                    DrawKnockbackModeInfo(modeName);
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);
        }

        // 战斗行为设置（仅 PlayerHitReactionData 有）
        if (hitInterruptsActionsProp != null)
        {
            showBehaviorSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBehaviorSettings, "⚔️ 战斗行为 (Combat Behavior)");
            if (showBehaviorSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(hitInterruptsActionsProp, new GUIContent("受击打断动作", "受击时是否打断当前正在执行的动作"));
                
                if (hitInterruptsActionsProp.boolValue)
                {
                    EditorGUILayout.HelpBox("✓ 受击时会打断当前动作（推荐）", MessageType.None);
                }
                else
                {
                    EditorGUILayout.HelpBox("⚠️ 受击时不会打断动作（可能导致异常行为）", MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(5);
        }

        // 绘制其他未处理的字段
        DrawRemainingProperties();

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

    private void DrawKnockbackModeInfo(string modeName)
    {
        EditorGUILayout.Space(5);
        
        string info = modeName switch
        {
            "Physics" => "物理模式: 使用水平衰减和可选重力，模拟真实物理效果",
            "CustomCurve" => "自定义曲线模式: 使用动画曲线精确控制击退轨迹",
            "LinearDecay" => "线性衰减模式: 速度均匀衰减，简单直接",
            _ => "未知模式"
        };
        
        EditorGUILayout.HelpBox($"当前模式: {modeName}\n{info}", MessageType.Info);
    }

    private void DrawRemainingProperties()
    {
        // 绘制所有未在自定义UI中处理的属性
        DrawPropertiesExcluding(serializedObject,
            "m_Script",
            "Identifier",
            "CanBeKnockedBack",
            "CanBeHitstunned",
            "LightHitStunMultiplier",
            "HeavyHitStunMultiplier",
            "KnockbackProfile",
            "HitInterruptsActions"
        );
    }
}
#endif
