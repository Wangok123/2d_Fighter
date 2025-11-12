using UnityEditor;
using UnityEngine;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(PlayerMovementData))]
    public class PlayerMovementDataEditor : UnityEditor.Editor
    {
        private SerializedProperty defaultKCC2DConfigProp;

        private void OnEnable()
        {
            defaultKCC2DConfigProp = serializedObject.FindProperty("DefaultKCC2DConfig");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            
            // Header
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.3f, 0.6f, 1f);
            EditorGUILayout.LabelField("玩家移动配置 (Player Movement Configuration)", headerStyle);
            
            EditorGUILayout.Space(10);
            
            // KCC Settings Section
            DrawSectionHeader("角色控制器设置 (Character Controller Settings)", new Color(0.2f, 0.8f, 0.5f));
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(defaultKCC2DConfigProp, 
                new GUIContent("默认KCC2D配置 (Default KCC2D Config)", 
                "角色运动控制器的默认配置，包含移动速度、加速度等参数"));
            
            // Show reference info
            if (defaultKCC2DConfigProp != null)
            {
                SerializedProperty idProp = defaultKCC2DConfigProp.FindPropertyRelative("Id");
                if (idProp != null)
                {
                    SerializedProperty valueProp = idProp.FindPropertyRelative("Value");
                    if (valueProp != null && valueProp.longValue != 0)
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.HelpBox("已关联KCC2D配置资源 (KCC2D Config Asset Linked)", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.HelpBox("未关联KCC2D配置！请设置默认配置 (No KCC2D Config! Please assign a config)", MessageType.Warning);
                    }
                }
            }
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(10);
            
            // Information box
            EditorGUILayout.HelpBox(
                "KCC2D (Kinematic Character Controller 2D) 配置控制角色的移动行为，包括：\n" +
                "• 移动速度 (Movement Speed)\n" +
                "• 加速度/减速度 (Acceleration/Deceleration)\n" +
                "• 跳跃参数 (Jump Parameters)\n" +
                "• 重力设置 (Gravity Settings)\n" +
                "• 碰撞检测 (Collision Detection)",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSectionHeader(string title, Color color)
        {
            GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
            sectionStyle.normal.textColor = color;
            EditorGUILayout.LabelField(title, sectionStyle);
            EditorGUILayout.Space(2);
        }
    }
}
