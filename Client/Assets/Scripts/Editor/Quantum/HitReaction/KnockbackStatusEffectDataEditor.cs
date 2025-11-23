using UnityEngine;
using UnityEditor;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(KnockbackStatusEffectData))]
    public class KnockbackStatusEffectDataEditor : UnityEditor.Editor
    {
        private SerializedProperty knockbackType;
        private SerializedProperty knockbackApplicationMode;
        private SerializedProperty knockbackForce;
        private SerializedProperty knockBackDuration;
        private SerializedProperty fixedKnockbackDirection;
        private SerializedProperty knockbackCurveX;
        private SerializedProperty knockbackCurveY;
        private SerializedProperty knockbackDistanceX;
        private SerializedProperty knockbackDistanceY;

        private void OnEnable()
        {
            knockbackType = serializedObject.FindProperty("KnockbackType");
            knockbackApplicationMode = serializedObject.FindProperty("KnockbackApplicationMode");
            knockbackForce = serializedObject.FindProperty("KnockbackForce");
            knockBackDuration = serializedObject.FindProperty("KnockBackDuration");
            fixedKnockbackDirection = serializedObject.FindProperty("FixedKnockbackDirection");
            knockbackCurveX = serializedObject.FindProperty("KnockbackCurveX");
            knockbackCurveY = serializedObject.FindProperty("KnockbackCurveY");
            knockbackDistanceX = serializedObject.FindProperty("KnockbackDistanceX");
            knockbackDistanceY = serializedObject.FindProperty("KnockbackDistanceY");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Common Knockback Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(knockbackType, new GUIContent("击退类型"));
            EditorGUILayout.PropertyField(knockbackApplicationMode, new GUIContent("击退应用模式"));

            EditorGUILayout.Space(10);

            KnockbackApplicationMode mode = (KnockbackApplicationMode)knockbackApplicationMode.intValue;

            if (mode == KnockbackApplicationMode.Physics2D)
            {
                DrawPhysics2DSettings();
            }
            else if (mode == KnockbackApplicationMode.CharacterController)
            {
                DrawCharacterControllerSettings();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPhysics2DSettings()
        {
            EditorGUILayout.LabelField("Physics Knockback Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(knockbackForce, new GUIContent("击退力度"));
        }

        private void DrawCharacterControllerSettings()
        {
            EditorGUILayout.LabelField("Character Controller Knockback Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(knockBackDuration, new GUIContent("击退持续时间"));

            AttackKnockbackType kbType = (AttackKnockbackType)knockbackType.intValue;
            
            if (kbType == AttackKnockbackType.Fixed || kbType == AttackKnockbackType.AttackerFacingDirection)
            {
                EditorGUILayout.PropertyField(fixedKnockbackDirection, new GUIContent("固定击退方向"));
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(knockbackCurveX, new GUIContent("X轴击退曲线"));
            EditorGUILayout.PropertyField(knockbackCurveY, new GUIContent("Y轴击退曲线"));
            EditorGUILayout.PropertyField(knockbackDistanceX, new GUIContent("X轴击退距离"));
            EditorGUILayout.PropertyField(knockbackDistanceY, new GUIContent("Y轴击退距离"));
        }
    }
}
