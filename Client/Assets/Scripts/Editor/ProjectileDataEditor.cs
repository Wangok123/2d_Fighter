using UnityEditor;
using UnityEngine;
using Quantum;
using Photon.Deterministic;

namespace QuantumEditor
{
    [CustomEditor(typeof(ProjectileData), true)]
    public class ProjectileDataEditor : Editor
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
            DrawFPFieldWithLabel("Lifetime", "生命周期 (Lifetime)", "弹道存在的时间（秒）");
            DrawPropertyWithLabel("VisualPrototype", "视觉原型 (Visual Prototype)", "弹道的视觉表现预制体");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Damage Settings
            DrawSectionHeader("伤害设置 (Damage Settings)", new Color(1f, 0.3f, 0.3f));
            EditorGUI.indentLevel++;
            DrawFPFieldWithLabel("BaseDamage", "基础伤害 (Base Damage)", "弹道造成的基础伤害值");
            DrawFPFieldWithLabel("HitstunDuration", "受击硬直时间 (Hitstun Duration)", "目标被击中后的硬直时间（秒）");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Knockback Settings
            DrawSectionHeader("击退设置 (Knockback Settings)", new Color(1f, 0.7f, 0.2f));
            EditorGUI.indentLevel++;
            DrawFPFieldWithLabel("KnockbackForce", "击退力度 (Knockback Force)", "击退效果的力度大小");
            DrawPropertyWithLabel("KnockbackType", "击退类型 (Knockback Type)", "击退方向的计算方式");
            
            SerializedProperty knockbackTypeProp = serializedObject.FindProperty("KnockbackType");
            if (knockbackTypeProp != null && knockbackTypeProp.enumValueIndex == 4) // Fixed type
            {
                DrawFPVector2Field("FixedKnockbackDirection", "固定击退方向 (Fixed Direction)", "固定的击退方向向量");
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // Collision Settings
            DrawSectionHeader("碰撞设置 (Collision Settings)", new Color(0.3f, 0.9f, 0.4f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("CollisionShape", "碰撞形状 (Collision Shape)", "弹道的碰撞检测形状");
            DrawPropertyWithLabel("CollisionLayer", "碰撞层 (Collision Layer)", "弹道所在的碰撞层");
            DrawPropertyWithLabel("PierceTargets", "穿透目标 (Pierce Targets)", "是否可以穿透目标继续前进");
            
            SerializedProperty pierceProp = serializedObject.FindProperty("PierceTargets");
            if (pierceProp != null && pierceProp.boolValue)
            {
                DrawPropertyWithLabel("MaxPierceCount", "最大穿透数 (Max Pierce Count)", "最多可以穿透的目标数量（-1为无限）");
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            // Draw remaining properties for derived classes
            DrawRemainingProperties();
            
            // Help box with knockback type descriptions
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "击退类型说明 (Knockback Type Description):\n" +
                "• AwayFromSource: 远离弹道位置\n" +
                "• AwayFromAttacker: 远离发射者位置\n" +
                "• ProjectileDirection: 沿弹道方向\n" +
                "• Up: 向上击飞\n" +
                "• Fixed: 使用固定方向",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawRemainingProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            
            var drawnProperties = new System.Collections.Generic.HashSet<string>
            {
                "m_Script", "Lifetime", "VisualPrototype", "BaseDamage", "KnockbackForce",
                "KnockbackType", "FixedKnockbackDirection", "HitstunDuration",
                "CollisionShape", "CollisionLayer", "PierceTargets", "MaxPierceCount"
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
                
                SerializedProperty xProp = property.FindPropertyRelative("X").FindPropertyRelative("RawValue");
                SerializedProperty yProp = property.FindPropertyRelative("Y").FindPropertyRelative("RawValue");
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
                case "StraightProjectile":
                    return "直线弹道配置 (Straight Projectile Configuration)";
                case "ArcProjectile":
                    return "抛物线弹道配置 (Arc Projectile Configuration)";
                case "HomingProjectile":
                    return "追踪弹道配置 (Homing Projectile Configuration)";
                case "BoomerangProjectile":
                    return "回旋弹道配置 (Boomerang Projectile Configuration)";
                case "GrenadeProjectile":
                    return "手榴弹弹道配置 (Grenade Projectile Configuration)";
                default:
                    return $"弹道配置 (Projectile Configuration) - {typeName}";
            }
        }
    }
}
