using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(JumpAbilityData))]
    public class JumpAbilityDataEditor : UnityEditor.Editor
    {
        private SerializedProperty jumpImpulse;
        private SerializedProperty jumpHeightMultiplier;
        private SerializedProperty allowVariableHeight;
        private SerializedProperty minJumpHeightPercent;
        private SerializedProperty horizontalVelocityMultiplier;

        private void OnEnable()
        {
            jumpImpulse = serializedObject.FindProperty("JumpImpulse");
            jumpHeightMultiplier = serializedObject.FindProperty("JumpHeightMultiplier");
            allowVariableHeight = serializedObject.FindProperty("AllowVariableHeight");
            minJumpHeightPercent = serializedObject.FindProperty("MinJumpHeightPercent");
            horizontalVelocityMultiplier = serializedObject.FindProperty("HorizontalVelocityMultiplier");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Jump Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(jumpImpulse, new GUIContent("跳跃力度"));
            EditorGUILayout.PropertyField(jumpHeightMultiplier, new GUIContent("跳跃高度倍率"));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(allowVariableHeight, new GUIContent("允许可变跳跃高度"));

            if (allowVariableHeight.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(minJumpHeightPercent, new GUIContent("最小跳跃高度百分比"));
                EditorGUILayout.HelpBox("当玩家提前松开跳跃键时，跳跃高度将至少为此百分比", MessageType.Info);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("起跳时水平速度调整", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(horizontalVelocityMultiplier, new GUIContent("水平速度倍率"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
