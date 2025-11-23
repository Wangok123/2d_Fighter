using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(ProjectileData), true)]
    public class ProjectileDataEditor : UnityEditor.Editor
    {
        private SerializedProperty lifetime;
        private SerializedProperty visualPrototype;
        private SerializedProperty baseDamage;
        private SerializedProperty knockbackStatusEffectData;
        private SerializedProperty collisionShape;
        private SerializedProperty collisionLayer;
        private SerializedProperty pierceTargets;
        private SerializedProperty maxPierceCount;

        protected virtual void OnEnable()
        {
            lifetime = serializedObject.FindProperty("Lifetime");
            visualPrototype = serializedObject.FindProperty("VisualPrototype");
            baseDamage = serializedObject.FindProperty("BaseDamage");
            knockbackStatusEffectData = serializedObject.FindProperty("KnockbackStatusEffectData");
            collisionShape = serializedObject.FindProperty("CollisionShape");
            collisionLayer = serializedObject.FindProperty("CollisionLayer");
            pierceTargets = serializedObject.FindProperty("PierceTargets");
            maxPierceCount = serializedObject.FindProperty("MaxPierceCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(lifetime, new GUIContent("生命周期（秒）"));
            EditorGUILayout.PropertyField(visualPrototype, new GUIContent("视觉Prototype"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("伤害设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(baseDamage, new GUIContent("基础伤害"));
            EditorGUILayout.PropertyField(knockbackStatusEffectData, new GUIContent("击退配置数据"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("碰撞设置", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(collisionShape, new GUIContent("碰撞形状"));
            EditorGUILayout.PropertyField(collisionLayer, new GUIContent("碰撞层"));
            EditorGUILayout.PropertyField(pierceTargets, new GUIContent("穿透目标"));

            if (pierceTargets.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(maxPierceCount, new GUIContent("最大穿透数量（-1为无限）"));
                EditorGUI.indentLevel--;
            }

            DrawCustomInspector();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawCustomInspector()
        {
        }
    }
}
