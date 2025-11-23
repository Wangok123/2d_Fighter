using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(DamageFieldData))]
    public class DamageFieldDataEditor : UnityEditor.Editor
    {
        private SerializedProperty duration;
        private SerializedProperty tickInterval;
        private SerializedProperty visualPrototype;
        private SerializedProperty effectArea;
        private SerializedProperty targetLayer;
        private SerializedProperty affectAllies;
        private SerializedProperty affectEnemies;
        private SerializedProperty damagePerTick;
        private SerializedProperty damageType;
        private SerializedProperty applyDOT;
        private SerializedProperty dotDuration;
        private SerializedProperty dotDamagePerSecond;
        private SerializedProperty applyKnockback;
        private SerializedProperty knockbackStatusEffectData;

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
            damageType = serializedObject.FindProperty("DamageType");
            applyDOT = serializedObject.FindProperty("ApplyDOT");
            dotDuration = serializedObject.FindProperty("DOTDuration");
            dotDamagePerSecond = serializedObject.FindProperty("DOTDamagePerSecond");
            applyKnockback = serializedObject.FindProperty("ApplyKnockback");
            knockbackStatusEffectData = serializedObject.FindProperty("KnockbackStatusEffectData");
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
            EditorGUILayout.PropertyField(damageType, new GUIContent("伤害类型"));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(applyDOT, new GUIContent("造成DOT（持续伤害）"));

            if (applyDOT.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(dotDuration, new GUIContent("DOT持续时间"));
                EditorGUILayout.PropertyField(dotDamagePerSecond, new GUIContent("DOT每秒伤害"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("击退设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applyKnockback, new GUIContent("应用击退"));

            if (applyKnockback.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(knockbackStatusEffectData, new GUIContent("击退配置数据"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
