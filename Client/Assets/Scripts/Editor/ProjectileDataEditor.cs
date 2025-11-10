#if UNITY_EDITOR
using UnityEditor;
using Quantum;

[CustomEditor(typeof(ProjectileData), true)]
public class ProjectileDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("飞行道具配置", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "飞行道具系统支持两种类型：\n" +
            "• Bullet - 持续飞行的弹道类道具，可设置移动模式\n" +
            "• SkillField - 区域效果场，可定时触发效果",
            MessageType.Info);

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(SkillFieldData), true)]
public class SkillFieldDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("技能场配置", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "技能场系统说明：\n" +
            "• 可在任意位置创建持续性区域效果\n" +
            "• 支持定时Tick触发效果\n" +
            "• 可配置影响友军/敌人\n" +
            "• 支持多种效果类型：伤害、治疗、Buff、Debuff、控制",
            MessageType.Info);

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif