using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(ArcProjectileData))]
    public class ArcProjectileDataEditor : ProjectileDataEditor
    {
        private SerializedProperty initialSpeed;
        private SerializedProperty initialUpwardVelocity;
        private SerializedProperty minimumHeight;
        private SerializedProperty enableGroundClamp;

        protected override void OnEnable()
        {
            base.OnEnable();

            initialSpeed = serializedObject.FindProperty("InitialSpeed");
            initialUpwardVelocity = serializedObject.FindProperty("InitialUpwardVelocity");
            minimumHeight = serializedObject.FindProperty("MinimumHeight");
            enableGroundClamp = serializedObject.FindProperty("EnableGroundClamp");
        }

        protected override void DrawCustomInspector()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("抛物线弹道设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(initialSpeed, new GUIContent("初始速度"));
            EditorGUILayout.PropertyField(initialUpwardVelocity, new GUIContent("初始向上速度"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("地面限制", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableGroundClamp, new GUIContent("启用地面限制"));

            if (enableGroundClamp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(minimumHeight, new GUIContent("最低高度限制（防止穿地）"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
