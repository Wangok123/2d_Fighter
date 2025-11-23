using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(ChargeAttackAbilityData))]
    public class ChargeAttackAbilityDataEditor : UnityEditor.Editor
    {
        private SerializedProperty minChargeTime;
        private SerializedProperty maxChargeTime;
        private SerializedProperty canMoveWhileCharging;
        private SerializedProperty minChargeDamageMultiplier;
        private SerializedProperty maxChargeDamageMultiplier;
        private SerializedProperty scaleKnockbackWithCharge;
        private SerializedProperty minChargeKnockbackMultiplier;
        private SerializedProperty maxChargeKnockbackMultiplier;
        private SerializedProperty scaleAttackRangeWithCharge;
        private SerializedProperty maxChargeRangeMultiplier;

        private void OnEnable()
        {
            minChargeTime = serializedObject.FindProperty("MinChargeTime");
            maxChargeTime = serializedObject.FindProperty("MaxChargeTime");
            canMoveWhileCharging = serializedObject.FindProperty("CanMoveWhileCharging");
            minChargeDamageMultiplier = serializedObject.FindProperty("MinChargeDamageMultiplier");
            maxChargeDamageMultiplier = serializedObject.FindProperty("MaxChargeDamageMultiplier");
            scaleKnockbackWithCharge = serializedObject.FindProperty("ScaleKnockbackWithCharge");
            minChargeKnockbackMultiplier = serializedObject.FindProperty("MinChargeKnockbackMultiplier");
            maxChargeKnockbackMultiplier = serializedObject.FindProperty("MaxChargeKnockbackMultiplier");
            scaleAttackRangeWithCharge = serializedObject.FindProperty("ScaleAttackRangeWithCharge");
            maxChargeRangeMultiplier = serializedObject.FindProperty("MaxChargeRangeMultiplier");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Charge Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(minChargeTime, new GUIContent("最小蓄力时间"));
            EditorGUILayout.PropertyField(maxChargeTime, new GUIContent("最大蓄力时间"));
            EditorGUILayout.PropertyField(canMoveWhileCharging, new GUIContent("蓄力时可以移动"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Charge Damage Scaling", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(minChargeDamageMultiplier, new GUIContent("最小蓄力伤害倍率"));
            EditorGUILayout.PropertyField(maxChargeDamageMultiplier, new GUIContent("最大蓄力伤害倍率"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Charge Knockback Scaling", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(scaleKnockbackWithCharge, new GUIContent("根据蓄力缩放击退"));

            if (scaleKnockbackWithCharge.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(minChargeKnockbackMultiplier, new GUIContent("最小蓄力击退倍率"));
                EditorGUILayout.PropertyField(maxChargeKnockbackMultiplier, new GUIContent("最大蓄力击退倍率"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Charge Visual Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(scaleAttackRangeWithCharge, new GUIContent("根据蓄力缩放攻击范围"));

            if (scaleAttackRangeWithCharge.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxChargeRangeMultiplier, new GUIContent("最大蓄力范围倍率"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
