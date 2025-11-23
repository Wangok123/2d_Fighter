using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(BoomerangProjectileData))]
    public class BoomerangProjectileDataEditor : ProjectileDataEditor
    {
        private SerializedProperty forwardSpeed;
        private SerializedProperty returnSpeed;
        private SerializedProperty maxDistance;
        private SerializedProperty catchDistance;
        private SerializedProperty behaviorOnOwnerLost;
        private SerializedProperty rotateWhileForward;
        private SerializedProperty rotationSpeed;
        private SerializedProperty useKCC;
        private SerializedProperty kccConfig;

        protected override void OnEnable()
        {
            base.OnEnable();

            forwardSpeed = serializedObject.FindProperty("ForwardSpeed");
            returnSpeed = serializedObject.FindProperty("ReturnSpeed");
            maxDistance = serializedObject.FindProperty("MaxDistance");
            catchDistance = serializedObject.FindProperty("CatchDistance");
            behaviorOnOwnerLost = serializedObject.FindProperty("BehaviorOnOwnerLost");
            rotateWhileForward = serializedObject.FindProperty("RotateWhileForward");
            rotationSpeed = serializedObject.FindProperty("RotationSpeed");
            useKCC = serializedObject.FindProperty("UseKCC");
            kccConfig = serializedObject.FindProperty("KCCConfig");
        }

        protected override void DrawCustomInspector()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("回旋镖弹道设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(forwardSpeed, new GUIContent("前进速度"));
            EditorGUILayout.PropertyField(returnSpeed, new GUIContent("返回速度"));
            EditorGUILayout.PropertyField(maxDistance, new GUIContent("最大飞行距离"));
            EditorGUILayout.PropertyField(catchDistance, new GUIContent("回收距离"));
            EditorGUILayout.PropertyField(behaviorOnOwnerLost, new GUIContent("失去主人后的行为"));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(rotateWhileForward, new GUIContent("前进阶段旋转"));

            if (rotateWhileForward.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(rotationSpeed, new GUIContent("旋转速度（度/秒）"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("物理设置（可选）", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(useKCC, new GUIContent("使用KCC2D物理"));

            if (useKCC.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(kccConfig, new GUIContent("KCC配置"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
