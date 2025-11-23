using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(VortexFieldData))]
    public class VortexFieldDataEditor : UnityEditor.Editor
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
        private SerializedProperty rotationDirection;
        private SerializedProperty centripetalForce;
        private SerializedProperty tangentialForce;
        private SerializedProperty dealDamage;
        private SerializedProperty damagePerTickVortex;
        private SerializedProperty coreRadius;
        private SerializedProperty stunInCore;
        private SerializedProperty stunDuration;

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
            rotationDirection = serializedObject.FindProperty("RotationDirection");
            centripetalForce = serializedObject.FindProperty("CentripetalForce");
            tangentialForce = serializedObject.FindProperty("TangentialForce");
            dealDamage = serializedObject.FindProperty("DealDamage");
            damagePerTickVortex = serializedObject.FindProperty("DamagePerTick");
            coreRadius = serializedObject.FindProperty("CoreRadius");
            stunInCore = serializedObject.FindProperty("StunInCore");
            stunDuration = serializedObject.FindProperty("StunDuration");
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
            EditorGUILayout.LabelField("旋涡设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rotationDirection, new GUIContent("旋转方向"));
            EditorGUILayout.PropertyField(centripetalForce, new GUIContent("向心力强度"));
            EditorGUILayout.PropertyField(tangentialForce, new GUIContent("切向力强度（旋转速度）"));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(dealDamage, new GUIContent("造成伤害"));

            if (dealDamage.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(damagePerTickVortex, new GUIContent("每Tick伤害"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("高级设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(coreRadius, new GUIContent("旋涡核心半径"));
            EditorGUILayout.PropertyField(stunInCore, new GUIContent("在核心区域眩晕"));

            if (stunInCore.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(stunDuration, new GUIContent("眩晕持续时间"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
