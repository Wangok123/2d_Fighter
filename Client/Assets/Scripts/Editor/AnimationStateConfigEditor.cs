#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityCore.AnimationSystem;
using System.Collections.Generic;

/// <summary>
/// AnimationStateConfig 的自定义编辑器
/// Custom Editor for AnimationStateConfig - Visualizes animation states with timeline and state cards
/// </summary>
[CustomEditor(typeof(AnimationStateConfig), true)]
public class AnimationStateConfigEditor : Editor
{
    private SerializedProperty animationStatesProp;
    private SerializedProperty defaultStateNameProp;
    private bool[] foldouts;
    private RuntimeAnimatorController animatorController;

    private void OnEnable()
    {
        animationStatesProp = serializedObject.FindProperty("AnimationStates");
        defaultStateNameProp = serializedObject.FindProperty("DefaultStateName");
        
        // 初始化折叠状态
        if (animationStatesProp != null)
        {
            foldouts = new bool[animationStatesProp.arraySize];
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AnimationStateConfig config = (AnimationStateConfig)target;

        // 标题
        CustomEditorStyles.DrawHeader("动画状态配置 Animation State Config", "🎬");

        EditorGUILayout.Space(10);

        // ======= 工具栏 =======
        CustomEditorStyles.BeginColoredBox(new Color(0.2f, 0.3f, 0.4f, 0.3f));
        CustomEditorStyles.DrawSubHeader("快速工具 Quick Tools", CustomEditorStyles.Icons.Config);

        EditorGUILayout.BeginHorizontal();
        
        // Animator Controller 引用
        animatorController = EditorGUILayout.ObjectField(
            "Animator Controller",
            animatorController,
            typeof(RuntimeAnimatorController),
            false
        ) as RuntimeAnimatorController;

        // 自动生成按钮
        EditorGUI.BeginDisabledGroup(animatorController == null);
        if (GUILayout.Button("自动生成 Auto Generate", GUILayout.Width(150)))
        {
            config.AutoGenerateFromAnimator(animatorController);
            EditorUtility.SetDirty(config);
            serializedObject.Update();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        if (animatorController == null)
        {
            EditorGUILayout.HelpBox("拖入 Animator Controller 以自动生成动画状态配置", MessageType.Info);
        }

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= 默认状态设置 =======
        CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);
        CustomEditorStyles.DrawSubHeader("默认状态 Default State", CustomEditorStyles.Icons.Info);
        
        EditorGUILayout.PropertyField(defaultStateNameProp, new GUIContent("默认状态名称 Default State Name"));
        
        // 验证默认状态是否存在
        string defaultStateName = defaultStateNameProp.stringValue;
        bool defaultStateExists = false;
        if (animationStatesProp != null)
        {
            for (int i = 0; i < animationStatesProp.arraySize; i++)
            {
                var stateProp = animationStatesProp.GetArrayElementAtIndex(i);
                var stateNameProp = stateProp.FindPropertyRelative("StateName");
                if (stateNameProp != null && stateNameProp.stringValue == defaultStateName)
                {
                    defaultStateExists = true;
                    break;
                }
            }
        }

        if (!string.IsNullOrEmpty(defaultStateName))
        {
            if (defaultStateExists)
            {
                EditorGUILayout.HelpBox($"✓ 默认状态 '{defaultStateName}' 已配置", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"⚠ 警告: 默认状态 '{defaultStateName}' 在状态列表中不存在", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ 请设置默认状态名称", MessageType.Warning);
        }

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= 动画状态列表 =======
        CustomEditorStyles.DrawSubHeader($"动画状态列表 Animation States ({animationStatesProp.arraySize})", "🎭");

        EditorGUILayout.Space(5);

        // 添加/删除按钮
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("+ 添加状态 Add State", GUILayout.Width(150)))
        {
            animationStatesProp.InsertArrayElementAtIndex(animationStatesProp.arraySize);
            System.Array.Resize(ref foldouts, animationStatesProp.arraySize);
            foldouts[animationStatesProp.arraySize - 1] = true;
        }

        if (GUILayout.Button("全部展开 Expand All", GUILayout.Width(120)))
        {
            for (int i = 0; i < foldouts.Length; i++)
            {
                foldouts[i] = true;
            }
        }

        if (GUILayout.Button("全部折叠 Collapse All", GUILayout.Width(120)))
        {
            for (int i = 0; i < foldouts.Length; i++)
            {
                foldouts[i] = false;
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // 确保 foldouts 数组大小正确
        if (foldouts == null || foldouts.Length != animationStatesProp.arraySize)
        {
            foldouts = new bool[animationStatesProp.arraySize];
        }

        // 绘制每个动画状态
        for (int i = 0; i < animationStatesProp.arraySize; i++)
        {
            DrawAnimationStateElement(i);
        }

        // 如果没有状态，显示提示
        if (animationStatesProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("没有动画状态。点击 '+ 添加状态' 或使用 '自动生成' 创建状态。", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // ======= 统计信息 =======
        CustomEditorStyles.BeginColoredBox(new Color(0.2f, 0.3f, 0.4f, 0.3f));
        CustomEditorStyles.DrawSubHeader("配置统计 Statistics", CustomEditorStyles.Icons.Info);
        
        int totalStates = animationStatesProp.arraySize;
        int layerCount = GetUniqueLayerCount();
        float avgCrossfade = GetAverageCrossfadeDuration();

        EditorGUILayout.LabelField($"• 总状态数 Total States: {totalStates}");
        EditorGUILayout.LabelField($"• 使用层级数 Layers Used: {layerCount}");
        EditorGUILayout.LabelField($"• 平均过渡时间 Avg Crossfade: {avgCrossfade:F3}s");
        EditorGUILayout.LabelField($"• 默认状态 Default State: {(string.IsNullOrEmpty(defaultStateName) ? "未设置" : defaultStateName)}");

        CustomEditorStyles.EndColoredBox();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制单个动画状态元素
    /// </summary>
    private void DrawAnimationStateElement(int index)
    {
        SerializedProperty stateProp = animationStatesProp.GetArrayElementAtIndex(index);
        SerializedProperty stateNameProp = stateProp.FindPropertyRelative("StateName");
        SerializedProperty layerProp = stateProp.FindPropertyRelative("Layer");
        SerializedProperty crossfadeProp = stateProp.FindPropertyRelative("CrossfadeDuration");
        SerializedProperty descriptionProp = stateProp.FindPropertyRelative("Description");

        string stateName = stateNameProp.stringValue;
        if (string.IsNullOrEmpty(stateName)) stateName = $"State {index}";

        // 检查是否为默认状态
        bool isDefaultState = stateNameProp.stringValue == defaultStateNameProp.stringValue;

        // 状态卡片背景色
        Color cardColor = isDefaultState ? 
            new Color(0.3f, 0.5f, 0.3f, 0.3f) : 
            CustomEditorStyles.Colors.BackgroundMedium;

        CustomEditorStyles.BeginColoredBox(cardColor);

        // 标题栏
        EditorGUILayout.BeginHorizontal();

        // 折叠按钮
        foldouts[index] = EditorGUILayout.Foldout(foldouts[index], "", true);

        // 状态名称和默认标记
        GUIStyle titleStyle = new GUIStyle(CustomEditorStyles.SubHeaderStyle);
        if (isDefaultState)
        {
            titleStyle.normal.textColor = CustomEditorStyles.Colors.StatusValid;
            EditorGUILayout.LabelField($"⭐ {stateName} (默认)", titleStyle);
        }
        else
        {
            EditorGUILayout.LabelField($"🎭 {stateName}", titleStyle);
        }

        GUILayout.FlexibleSpace();

        // 图层徽章
        Rect layerBadgeRect = GUILayoutUtility.GetRect(60, 18);
        CustomEditorStyles.DrawBadge(layerBadgeRect, $"Layer {layerProp.intValue}", 
            new Color(0.3f, 0.4f, 0.6f));

        // 删除按钮
        if (GUILayout.Button("✗", GUILayout.Width(25), GUILayout.Height(18)))
        {
            animationStatesProp.DeleteArrayElementAtIndex(index);
            System.Array.Resize(ref foldouts, animationStatesProp.arraySize);
            EditorGUILayout.EndHorizontal();
            CustomEditorStyles.EndColoredBox();
            EditorGUILayout.Space(5);
            return;
        }

        EditorGUILayout.EndHorizontal();

        // 展开时显示详细信息
        if (foldouts[index])
        {
            EditorGUI.indentLevel++;

            // 状态名称
            EditorGUILayout.PropertyField(stateNameProp, new GUIContent("状态名称 State Name"));

            // 图层
            EditorGUILayout.PropertyField(layerProp, new GUIContent("动画层级 Layer"));

            // 过渡时间
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(crossfadeProp, new GUIContent("过渡时间 Crossfade (s)"));
            float crossfade = crossfadeProp.floatValue;
            Color crossfadeColor = CustomEditorStyles.GetTimingColor(crossfade);
            CustomEditorStyles.DrawColoredValue($"≈ {crossfade:F3}s", crossfadeColor);
            EditorGUILayout.EndHorizontal();

            // 过渡时间进度条
            Rect cfRect = GUILayoutUtility.GetRect(0, 15, GUILayout.ExpandWidth(true));
            CustomEditorStyles.DrawProgressBar(cfRect, crossfade, crossfadeColor, $"{crossfade:F3}s");

            // 描述
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("备注说明 Description"));

            EditorGUI.indentLevel--;
        }

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(5);
    }

    /// <summary>
    /// 获取使用的唯一层级数
    /// </summary>
    private int GetUniqueLayerCount()
    {
        HashSet<int> layers = new HashSet<int>();
        for (int i = 0; i < animationStatesProp.arraySize; i++)
        {
            var stateProp = animationStatesProp.GetArrayElementAtIndex(i);
            var layerProp = stateProp.FindPropertyRelative("Layer");
            if (layerProp != null)
            {
                layers.Add(layerProp.intValue);
            }
        }
        return layers.Count;
    }

    /// <summary>
    /// 获取平均过渡时间
    /// </summary>
    private float GetAverageCrossfadeDuration()
    {
        if (animationStatesProp.arraySize == 0) return 0f;

        float total = 0f;
        for (int i = 0; i < animationStatesProp.arraySize; i++)
        {
            var stateProp = animationStatesProp.GetArrayElementAtIndex(i);
            var crossfadeProp = stateProp.FindPropertyRelative("CrossfadeDuration");
            if (crossfadeProp != null)
            {
                total += crossfadeProp.floatValue;
            }
        }
        return total / animationStatesProp.arraySize;
    }
}
#endif
