using UnityEditor;
using UnityEngine;
using Quantum;
using Photon.Deterministic;

namespace QuantumEditor
{
    [CustomEditor(typeof(SkillFieldData), true)]
    public class SkillFieldDataEditor : Editor
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
            
            // Basic Settings
            DrawSectionHeader("基础设置 (Basic Settings)", new Color(0.5f, 0.7f, 1f));
            EditorGUI.indentLevel++;
            DrawFPFieldWithLabel("Duration", "持续时间 (Duration)", "技能场持续存在的时间（秒）");
            DrawFPFieldWithLabel("TickInterval", "Tick间隔 (Tick Interval)", "效果触发的时间间隔（秒）");
            DrawPropertyWithLabel("VisualPrototype", "视觉原型 (Visual Prototype)", "技能场的视觉表现预制体");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Effect Range Settings
            DrawSectionHeader("效果范围 (Effect Range)", new Color(0.3f, 0.9f, 0.6f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("EffectArea", "范围形状 (Effect Area)", "技能场影响的区域形状");
            DrawPropertyWithLabel("TargetLayer", "影响层 (Target Layer)", "技能场能够影响的物理层");
            DrawPropertyWithLabel("AffectAllies", "影响友军 (Affect Allies)", "是否对友方单位产生效果");
            DrawPropertyWithLabel("AffectEnemies", "影响敌人 (Affect Enemies)", "是否对敌方单位产生效果");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Damage Settings
            DrawSectionHeader("伤害设置 (Damage Settings)", new Color(1f, 0.3f, 0.3f));
            EditorGUI.indentLevel++;
            DrawFPFieldWithLabel("DamagePerTick", "每次Tick伤害 (Damage Per Tick)", "每个Tick周期造成的伤害值");
            DrawFPFieldWithLabel("HitstunDuration", "受击硬直时间 (Hitstun Duration)", "目标被击中后的硬直时间（秒）");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Knockback Settings
            DrawSectionHeader("击退设置 (Knockback Settings)", new Color(1f, 0.7f, 0.2f));
            EditorGUI.indentLevel++;
            DrawFPFieldWithLabel("KnockbackForce", "击退力度 (Knockback Force)", "击退效果的力度大小");
            DrawFPVector2Field("KnockbackDirection", "击退方向 (Knockback Direction)", "目标被击退的方向向量");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            // Draw remaining properties for derived classes
            DrawRemainingProperties();
            
            // Help box
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "技能场说明 (Skill Field Description):\n\n" +
                "技能场是一个持续存在的区域效果，会在指定的Tick间隔内对进入范围的目标产生影响。\n\n" +
                "• Duration: 技能场存在的总时间\n" +
                "• TickInterval: 效果触发的频率（如每0.5秒触发一次）\n" +
                "• DamagePerTick: 每次触发时造成的伤害\n\n" +
                "可以通过 AffectAllies 和 AffectEnemies 控制影响的目标类型。",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawRemainingProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            
            var drawnProperties = new System.Collections.Generic.HashSet<string>
            {
                "m_Script", "Duration", "TickInterval", "VisualPrototype",
                "EffectArea", "TargetLayer", "AffectAllies", "AffectEnemies",
                "DamagePerTick", "KnockbackForce", "KnockbackDirection", "HitstunDuration"
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
                        DrawSectionHeader("特殊属性 (Special Properties)", new Color(0.8f, 0.5f, 0.9f));
                        EditorGUI.indentLevel++;
                        hasExtraProperties = true;
                    }
                    
                    if (iterator.type == "FP")
                    {
                        DrawFPProperty(iterator);
                    }
                    else if (iterator.type == "FPVector2")
                    {
                        DrawFPVector2Property(iterator);
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
                    EditorGUILayout.LabelField($"≈ {fpValue.AsFloat:F2}", GUILayout.Width(80));
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFPVector2Field(string propertyName, string label, string tooltip)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip));
                
                SerializedProperty xProp = property.FindPropertyRelative("X")?.FindPropertyRelative("RawValue");
                SerializedProperty yProp = property.FindPropertyRelative("Y")?.FindPropertyRelative("RawValue");
                if (xProp != null && yProp != null)
                {
                    FP xValue = FP.FromRaw(xProp.longValue);
                    FP yValue = FP.FromRaw(yProp.longValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"预览: ({xValue.AsFloat:F2}, {yValue.AsFloat:F2})", EditorStyles.miniLabel);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
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

        private void DrawFPVector2Property(SerializedProperty property)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(property, true);
            
            SerializedProperty xProp = property.FindPropertyRelative("X")?.FindPropertyRelative("RawValue");
            SerializedProperty yProp = property.FindPropertyRelative("Y")?.FindPropertyRelative("RawValue");
            if (xProp != null && yProp != null)
            {
                FP xValue = FP.FromRaw(xProp.longValue);
                FP yValue = FP.FromRaw(yProp.longValue);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"预览: ({xValue.AsFloat:F2}, {yValue.AsFloat:F2})", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
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
                case "DamageField":
                    return "伤害场配置 (Damage Field Configuration)";
                case "HealField":
                    return "治疗场配置 (Heal Field Configuration)";
                case "SlowField":
                    return "减速场配置 (Slow Field Configuration)";
                case "PushField":
                    return "推力场配置 (Push Field Configuration)";
                case "VortexField":
                    return "漩涡场配置 (Vortex Field Configuration)";
                case "DelayedExplosionField":
                    return "延迟爆炸场配置 (Delayed Explosion Field Configuration)";
                default:
                    return $"技能场配置 (Skill Field Configuration) - {typeName}";
            }
        }
    }
}
