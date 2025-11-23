using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(SlowFieldData))]
    public class SlowFieldDataEditor : UnityEditor.Editor
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
        private SerializedProperty speedReductionPercent;
        private SerializedProperty slowLingerDuration;
        private SerializedProperty stackableSlows;
        private SerializedProperty maxStacks;
        private SerializedProperty additionalSlowPerStack;
        private SerializedProperty showSlowEffect;
        private SerializedProperty effectType;

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
            speedReductionPercent = serializedObject.FindProperty("SpeedReductionPercent");
            slowLingerDuration = serializedObject.FindProperty("SlowLingerDuration");
            stackableSlows = serializedObject.FindProperty("StackableSlows");
            maxStacks = serializedObject.FindProperty("MaxStacks");
            additionalSlowPerStack = serializedObject.FindProperty("AdditionalSlowPerStack");
            showSlowEffect = serializedObject.FindProperty("ShowSlowEffect");
            effectType = serializedObject.FindProperty("EffectType");
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
            EditorGUILayout.LabelField("减速设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(speedReductionPercent, new GUIContent("移动速度减少百分比（0-1）"));
            EditorGUILayout.PropertyField(slowLingerDuration, new GUIContent("减速持续时间（离开区域后）"));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(stackableSlows, new GUIContent("叠加减速"));

            if (stackableSlows.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxStacks, new GUIContent("最大叠加层数"));
                EditorGUILayout.PropertyField(additionalSlowPerStack, new GUIContent("每层额外减速"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("视觉效果", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showSlowEffect, new GUIContent("显示减速特效"));

            if (showSlowEffect.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(effectType, new GUIContent("减速特效类型"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
