using UnityEditor;
using UnityEngine;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(HitReactionData), true)]
    public class HitReactionDataEditor : Editor
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
            
            // Core Flags Section
            DrawSectionHeader("核心标志 (Core Flags)", new Color(1f, 0.5f, 0.3f));
            EditorGUI.indentLevel++;
            DrawPropertyWithLabel("CanBeKnockedBack", "可被击退 (Can Be Knocked Back)", "角色是否可以被击退");
            DrawPropertyWithLabel("CanBeHitstunned", "可被硬直 (Can Be Hitstunned)", "角色是否可以进入受击硬直状态");
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            // Draw remaining properties for derived classes
            DrawRemainingProperties();
            
            EditorGUILayout.Space(10);
            
            // Information box
            EditorGUILayout.HelpBox(
                "受击反应配置说明 (Hit Reaction Configuration):\n\n" +
                "此配置控制角色受到攻击时的反应行为。\n\n" +
                "• 击退 (Knockback): 受击时被推动的效果\n" +
                "• 硬直 (Hitstun): 受击时无法行动的状态\n\n" +
                "这些设置可以根据不同角色类型进行定制，例如：\n" +
                "- 重型角色可能更难被击退\n" +
                "- BOSS可能免疫某些控制效果",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawRemainingProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            
            var drawnProperties = new System.Collections.Generic.HashSet<string>
            {
                "m_Script", "CanBeKnockedBack", "CanBeHitstunned"
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
                        DrawSectionHeader("扩展属性 (Extended Properties)", new Color(0.7f, 0.4f, 0.9f));
                        EditorGUI.indentLevel++;
                        hasExtraProperties = true;
                    }
                    
                    EditorGUILayout.PropertyField(iterator, true);
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
                case "PlayerHitReaction":
                    return "玩家受击反应配置 (Player Hit Reaction Configuration)";
                default:
                    return $"受击反应配置 (Hit Reaction Configuration) - {typeName}";
            }
        }
    }
}
