#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Quantum;

[CustomPropertyDrawer(typeof(KnockbackCurveProfile))]
public class KnockbackCurveProfileDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var modeProperty = property.FindPropertyRelative("Mode");
        var mode = (KnockbackMode)modeProperty.enumValueIndex;

        EditorGUILayout.PropertyField(modeProperty);
        EditorGUILayout.Space(5);

        switch (mode)
        {
            case KnockbackMode.Physics:
                EditorGUILayout.LabelField("Physics 模式参数", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("HorizontalDecayRate"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("UseGravity"));
                break;

            case KnockbackMode.CustomCurve:
                EditorGUILayout.LabelField("CustomCurve 模式参数", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("CurveDuration"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("HorizontalCurve"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("VerticalCurve"));
                break;

            case KnockbackMode.LinearDecay:
                EditorGUILayout.LabelField("LinearDecay 模式参数", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("LinearDecayRate"));
                break;
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("通用参数", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("MinThreshold"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return -2f;
    }
}
#endif
