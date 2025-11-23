using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(HealFieldData))]
    public class HealFieldDataEditor : UnityEditor.Editor
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
        private SerializedProperty healPerTick;
        private SerializedProperty healByPercentage;
        private SerializedProperty healPercentage;
        private SerializedProperty minHealAmount;
        private SerializedProperty grantShield;
        private SerializedProperty shieldAmount;
        private SerializedProperty shieldDuration;
        private SerializedProperty removeDebuffs;

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
            healPerTick = serializedObject.FindProperty("HealPerTick");
            healByPercentage = serializedObject.FindProperty("HealByPercentage");
            healPercentage = serializedObject.FindProperty("HealPercentage");
            minHealAmount = serializedObject.FindProperty("MinHealAmount");
            grantShield = serializedObject.FindProperty("GrantShield");
            shieldAmount = serializedObject.FindProperty("ShieldAmount");
            shieldDuration = serializedObject.FindProperty("ShieldDuration");
            removeDebuffs = serializedObject.FindProperty("RemoveDebuffs");
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
            EditorGUILayout.LabelField("治疗设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(healByPercentage, new GUIContent("基于最大生命值百分比治疗"));

            if (healByPercentage.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(healPercentage, new GUIContent("治疗百分比（0-1）"));
                EditorGUILayout.PropertyField(minHealAmount, new GUIContent("最小治疗量"));
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(healPerTick, new GUIContent("每Tick治疗量"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("额外效果", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(grantShield, new GUIContent("提供护盾"));

            if (grantShield.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(shieldAmount, new GUIContent("护盾值"));
                EditorGUILayout.PropertyField(shieldDuration, new GUIContent("护盾持续时间"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(removeDebuffs, new GUIContent("移除负面状态"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
