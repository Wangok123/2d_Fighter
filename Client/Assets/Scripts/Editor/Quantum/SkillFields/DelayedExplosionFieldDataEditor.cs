using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(DelayedExplosionFieldData))]
    public class DelayedExplosionFieldDataEditor : UnityEditor.Editor
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
        private SerializedProperty explosionDelay;
        private SerializedProperty explosionDamage;
        private SerializedProperty damageFalloff;
        private SerializedProperty centerDamageMultiplier;
        private SerializedProperty applyKnockback;
        private SerializedProperty explosionEffect;
        private SerializedProperty warningEffect;

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
            explosionDelay = serializedObject.FindProperty("ExplosionDelay");
            explosionDamage = serializedObject.FindProperty("ExplosionDamage");
            damageFalloff = serializedObject.FindProperty("DamageFalloff");
            centerDamageMultiplier = serializedObject.FindProperty("CenterDamageMultiplier");
            applyKnockback = serializedObject.FindProperty("ApplyKnockback");
            explosionEffect = serializedObject.FindProperty("ExplosionEffect");
            warningEffect = serializedObject.FindProperty("WarningEffect");
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
            EditorGUILayout.LabelField("爆炸延迟", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(explosionDelay, new GUIContent("引爆延迟时间"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("爆炸伤害", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(explosionDamage, new GUIContent("爆炸伤害"));
            EditorGUILayout.PropertyField(damageFalloff, new GUIContent("有伤害衰减"));

            if (damageFalloff.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(centerDamageMultiplier, new GUIContent("中心伤害倍率"));
                EditorGUILayout.HelpBox("中心伤害 = 爆炸伤害 × 中心倍率\n边缘伤害 = 爆炸伤害", MessageType.Info);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("击退效果", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(applyKnockback, new GUIContent("应用击退"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("视觉效果", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(explosionEffect, new GUIContent("爆炸特效Prototype"));
            EditorGUILayout.PropertyField(warningEffect, new GUIContent("预警特效Prototype"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
