using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(PushFieldData))]
    public class PushFieldDataEditor : UnityEditor.Editor
    {
        private SerializedProperty duration;
        private SerializedProperty tickInterval;
        private SerializedProperty visualPrototype;
        private SerializedProperty effectArea;
        private SerializedProperty targetLayer;
        private SerializedProperty affectAllies;
        private SerializedProperty affectEnemies;
        private SerializedProperty damagePerTick;
        private SerializedProperty knockbackStatusEffectData;
        private SerializedProperty fieldType;
        private SerializedProperty forceStrength;
        private SerializedProperty direction;
        private SerializedProperty customDirection;
        private SerializedProperty falloffWithDistance;
        private SerializedProperty maxEffectRange;
        private SerializedProperty continuousForce;

        private void OnEnable()
        {
            duration = serializedObject.FindProperty("Duration");
            tickInterval = serializedObject.FindProperty("TickInterval");
            visualPrototype = serializedObject.FindProperty("VisualPrototype");
            effectArea = serializedObject.FindProperty("EffectArea");
            targetLayer = serializedObject.FindProperty("TargetLayer");
            affectAllies = serializedObject.FindProperty("AffectAllies");
            affectEnemies = serializedObject.FindProperty("AffectEnemies");
            damagePerTick = serializedObject.FindProperty("DamagePerTick");
            knockbackStatusEffectData = serializedObject.FindProperty("KnockbackStatusEffectData");
            fieldType = serializedObject.FindProperty("FieldType");
            forceStrength = serializedObject.FindProperty("ForceStrength");
            direction = serializedObject.FindProperty("Direction");
            customDirection = serializedObject.FindProperty("CustomDirection");
            falloffWithDistance = serializedObject.FindProperty("FalloffWithDistance");
            maxEffectRange = serializedObject.FindProperty("MaxEffectRange");
            continuousForce = serializedObject.FindProperty("ContinuousForce");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(duration, new GUIContent("持续时间"));
            EditorGUILayout.PropertyField(tickInterval, new GUIContent("Tick间隔"));
            EditorGUILayout.PropertyField(visualPrototype, new GUIContent("视觉Prototype"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("效果范围", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(effectArea, new GUIContent("范围形状"));
            EditorGUILayout.PropertyField(targetLayer, new GUIContent("影响层"));
            EditorGUILayout.PropertyField(affectAllies, new GUIContent("影响友军"));
            EditorGUILayout.PropertyField(affectEnemies, new GUIContent("影响敌人"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("伤害设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(damagePerTick, new GUIContent("每Tick伤害"));
            EditorGUILayout.PropertyField(knockbackStatusEffectData, new GUIContent("击退配置数据"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("力场设置", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(fieldType, new GUIContent("力场类型"));
            EditorGUILayout.PropertyField(forceStrength, new GUIContent("力场强度"));
            EditorGUILayout.PropertyField(direction, new GUIContent("力场方向"));

            ForceDirection directionValue = (ForceDirection)direction.intValue;
            if (directionValue == ForceDirection.CustomDirection)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customDirection, new GUIContent("自定义方向"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("高级设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(falloffWithDistance, new GUIContent("受距离衰减"));

            if (falloffWithDistance.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxEffectRange, new GUIContent("最大影响距离"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(continuousForce, new GUIContent("持续施加力"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
