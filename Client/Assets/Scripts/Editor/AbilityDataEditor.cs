using UnityEditor;
using UnityEngine;
using Quantum;
using Photon.Deterministic;

namespace QuantumEditor
{
    [CustomEditor(typeof(AbilityData), true)]
    public class AbilityDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            
            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.3f, 0.6f, 1f);
            
            string typeName = target.GetType().Name;
            string displayName = GetDisplayName(typeName);
            EditorGUILayout.LabelField($"{displayName}", headerStyle);
            
            EditorGUILayout.Space(10);
            
            // Timing Settings
            DrawSectionHeader("时机设置 (Timing Settings)", new Color(1f, 0.7f, 0.3f));
            EditorGUI.indentLevel++;
            DrawFPFieldWithLabel("InputBuffer", "输入缓冲时间 (Input Buffer)", "按键输入后的容错时间（秒）");
            DrawFPFieldWithLabel("Delay", "延迟时间 (Delay)", "激活后到生效的延迟（秒）");
            DrawFPFieldWithLabel("Duration", "持续时间 (Duration)", "技能持续时间（秒）");
            DrawFPFieldWithLabel("Cooldown", "冷却时间 (Cooldown)", "技能冷却时间（秒）");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Direction Settings
            DrawSectionHeader("方向设置 (Direction Settings)", new Color(0.3f, 0.8f, 0.9f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("CastDirectionType", "施放方向类型 (Cast Direction Type)", "技能施放时的朝向判定方式");
            DrawPropertyWithLabel("FaceCastDirection", "面向施放方向 (Face Cast Direction)", "是否在施放时面向施放方向");
            DrawPropertyWithLabel("KeepVelocity", "保持速度 (Keep Velocity)", "施放时是否保持当前移动速度");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Priority Settings
            DrawSectionHeader("优先级设置 (Priority Settings)", new Color(0.9f, 0.3f, 0.5f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("Priority", "优先级 (Priority)", "技能的优先级等级");
            DrawPropertyWithLabel("CanBeCancelledByHigherPriority", "可被高优先级打断 (Can Be Cancelled)", "是否可以被更高优先级技能打断");
            DrawPropertyWithLabel("CanCancelLowerPriority", "可打断低优先级 (Can Cancel Lower)", "是否可以打断更低优先级技能");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Movement Settings
            DrawSectionHeader("移动设置 (Movement Settings)", new Color(0.5f, 0.9f, 0.3f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("DisableMovementDuringAbility", "禁用移动 (Disable Movement)", "技能期间是否禁用角色移动");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Other Settings
            DrawSectionHeader("其他设置 (Other Settings)", new Color(0.7f, 0.7f, 0.7f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("StartCooldownAfterDelay", "延迟后开始冷却 (Cooldown After Delay)", "是否在延迟后才开始计算冷却");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // UI Settings
            DrawSectionHeader("UI设置 (UI Settings)", new Color(0.8f, 0.5f, 0.9f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("_uiAbilityPrefab", "UI预制体 (UI Prefab)", "技能在UI中的显示预制体");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            // Draw any remaining properties (for derived classes)
            DrawRemainingProperties();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawRemainingProperties()
        {
            // Get all serialized properties
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            
            // Skip the base properties we already drew
            var drawnProperties = new System.Collections.Generic.HashSet<string>
            {
                "m_Script", "InputBuffer", "Delay", "Duration", "Cooldown",
                "CastDirectionType", "FaceCastDirection", "KeepVelocity",
                "DisableMovementDuringAbility", "Priority", "CanBeCancelledByHigherPriority",
                "CanCancelLowerPriority", "StartCooldownAfterDelay", "_uiAbilityPrefab"
            };
            
            bool hasExtraProperties = false;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!drawnProperties.Contains(iterator.name))
                {
                    if (!hasExtraProperties)
                    {
                        EditorGUILayout.Space(5);
                        DrawSectionHeader("特殊属性 (Special Properties)", new Color(1f, 0.5f, 0.2f));
                        EditorGUI.indentLevel++;
                        hasExtraProperties = true;
                    }
                    
                    // Try to draw FP fields specially
                    if (iterator.type == "FP")
                    {
                        DrawFPProperty(iterator);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
            }
            
            if (hasExtraProperties)
            {
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSectionHeader(string title, Color color)
        {
            GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
            sectionStyle.normal.textColor = color;
            EditorGUILayout.LabelField(title, sectionStyle);
            EditorGUILayout.Space(2);
        }

        private void DrawFPFieldWithLabel(string propertyName, string label, string tooltip)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
                
                SerializedProperty rawValueProp = property.FindPropertyRelative("RawValue");
                if (rawValueProp != null)
                {
                    long rawValue = rawValueProp.longValue;
                    FP fpValue = FP.FromRaw(rawValue);
                    EditorGUILayout.LabelField($"≈ {fpValue.AsFloat:F2}s", GUILayout.Width(80));
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFPProperty(SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, true);
            
            SerializedProperty rawValueProp = property.FindPropertyRelative("RawValue");
            if (rawValueProp != null)
            {
                long rawValue = rawValueProp.longValue;
                FP fpValue = FP.FromRaw(rawValue);
                EditorGUILayout.LabelField($"≈ {fpValue.AsFloat:F2}", GUILayout.Width(80));
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPropertyWithLabel(string propertyName, string label, string tooltip)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
            }
        }

        private string GetDisplayName(string typeName)
        {
            if (typeName.EndsWith("Data"))
            {
                typeName = typeName.Substring(0, typeName.Length - 4);
            }
            
            switch (typeName)
            {
                case "JumpAbility":
                    return "跳跃技能配置 (Jump Ability Configuration)";
                case "DoubleJumpAbility":
                    return "二段跳技能配置 (Double Jump Ability Configuration)";
                case "DashAbility":
                    return "冲刺技能配置 (Dash Ability Configuration)";
                case "WallJumpAbility":
                    return "蹬墙跳技能配置 (Wall Jump Ability Configuration)";
                case "WallSlideAbility":
                    return "蹬墙滑行技能配置 (Wall Slide Ability Configuration)";
                case "LightAttackAbility":
                case "ComboAttackAbility":
                    return "轻攻击技能配置 (Light Attack Ability Configuration)";
                case "HeavyAttackAbility":
                case "ChargeAttackAbility":
                    return "重攻击技能配置 (Heavy Attack Ability Configuration)";
                case "CommandAttackAbility":
                    return "指令攻击技能配置 (Command Attack Ability Configuration)";
                default:
                    return $"技能配置 (Ability Configuration) - {typeName}";
            }
        }
    }
}
