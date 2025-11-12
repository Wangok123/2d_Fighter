#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityCore.SceneManagement;
using UnityEngine.AddressableAssets;

/// <summary>
/// GameSceneSO 及其子类的自定义编辑器
/// Custom Editor for GameSceneSO and subclasses - Visualizes scene references
/// </summary>
[CustomEditor(typeof(GameSceneSO), true)]
public class GameSceneSOEditor : Editor
{
    private SerializedProperty sceneReferenceProp;
    private SerializedProperty guidProp;

    private void OnEnable()
    {
        sceneReferenceProp = serializedObject.FindProperty("sceneReference");
        guidProp = serializedObject.FindProperty("_guid");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GameSceneSO sceneSO = (GameSceneSO)target;
        string sceneType = target.GetType().Name;

        // 根据类型选择图标和标题
        string icon = "🎬";
        string title = "场景配置 Scene Configuration";
        Color headerColor = CustomEditorStyles.Colors.BackgroundMedium;

        if (sceneType.Contains("YooAsset"))
        {
            icon = "📦";
            title = "YooAsset 场景配置 YooAsset Scene";
            headerColor = new Color(0.3f, 0.5f, 0.6f, 0.3f);
        }
        else if (sceneType.Contains("UI"))
        {
            icon = CustomEditorStyles.Icons.UI;
            title = "UI 场景配置 UI Scene";
            headerColor = new Color(0.5f, 0.3f, 0.6f, 0.3f);
        }
        else if (sceneType.Contains("Persistent"))
        {
            icon = "🔒";
            title = "持久场景配置 Persistent Scene";
            headerColor = new Color(0.6f, 0.5f, 0.3f, 0.3f);
        }

        // 标题
        CustomEditorStyles.DrawHeader(title, icon);

        EditorGUILayout.Space(10);

        // ======= 场景引用 =======
        CustomEditorStyles.BeginColoredBox(headerColor);
        CustomEditorStyles.DrawSubHeader("场景引用 Scene Reference", "🎯");

        if (sceneReferenceProp != null)
        {
            EditorGUILayout.PropertyField(sceneReferenceProp, new GUIContent("场景资源 Scene Asset"));

            // 检查场景引用状态
            EditorGUILayout.Space(5);
            
            var assetReference = sceneReferenceProp.objectReferenceValue as AssetReference;
            if (assetReference != null && !string.IsNullOrEmpty(assetReference.AssetGUID))
            {
                // 场景已配置
                Rect statusRect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
                CustomEditorStyles.DrawBadge(
                    new Rect(statusRect.x + 10, statusRect.y + 5, statusRect.width - 20, 20),
                    "✓ 场景已配置 Scene Configured",
                    CustomEditorStyles.Colors.StatusValid
                );

                EditorGUILayout.Space(5);
                
                // 显示 GUID 信息
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Asset GUID:", GUILayout.Width(80));
                EditorGUILayout.SelectableLabel(assetReference.AssetGUID, EditorStyles.textField, GUILayout.Height(18));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // 场景未配置
                EditorGUILayout.HelpBox("⚠ 请配置场景资源引用", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("⚠ 未找到 sceneReference 属性", MessageType.Error);
        }

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= GUID 信息 =======
        if (guidProp != null)
        {
            CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);
            CustomEditorStyles.DrawSubHeader("唯一标识 Unique Identifier", CustomEditorStyles.Icons.Info);

            string guid = guidProp.stringValue;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GUID:", GUILayout.Width(50));
            
            if (!string.IsNullOrEmpty(guid))
            {
                EditorGUILayout.SelectableLabel(guid, EditorStyles.textField, GUILayout.Height(18));
                
                // 复制按钮
                if (GUILayout.Button("复制 Copy", GUILayout.Width(80)))
                {
                    EditorGUIUtility.systemCopyBuffer = guid;
                    Debug.Log($"GUID 已复制到剪贴板: {guid}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("(未生成 Not Generated)", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("GUID 在保存资产时自动生成，用于唯一标识此场景配置", MessageType.Info);

            CustomEditorStyles.EndColoredBox();
        }

        EditorGUILayout.Space(10);

        // ======= 类型信息 =======
        CustomEditorStyles.BeginColoredBox(new Color(0.2f, 0.3f, 0.4f, 0.3f));
        CustomEditorStyles.DrawSubHeader("类型信息 Type Information", CustomEditorStyles.Icons.Config);

        EditorGUILayout.LabelField($"• 脚本类型 Script Type: {sceneType}");
        EditorGUILayout.LabelField($"• 基类 Base Class: {target.GetType().BaseType?.Name}");
        EditorGUILayout.LabelField($"• 资产名称 Asset Name: {target.name}");

        // 类型说明
        string typeDescription = GetSceneTypeDescription(sceneType);
        if (!string.IsNullOrEmpty(typeDescription))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("说明 Description:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(typeDescription, EditorStyles.wordWrappedLabel);
        }

        CustomEditorStyles.EndColoredBox();

        EditorGUILayout.Space(10);

        // ======= 快速操作 =======
        CustomEditorStyles.BeginColoredBox(CustomEditorStyles.Colors.BackgroundMedium);
        CustomEditorStyles.DrawSubHeader("快速操作 Quick Actions", "⚡");

        EditorGUILayout.BeginHorizontal();

        // 选中资产
        if (GUILayout.Button("📍 定位资产 Locate Asset"))
        {
            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        // 打开场景（如果已配置）
        EditorGUI.BeginDisabledGroup(sceneReferenceProp == null || sceneReferenceProp.objectReferenceValue == null);
        if (GUILayout.Button("🎬 打开场景 Open Scene"))
        {
            // 这里可以添加打开场景的逻辑
            Debug.Log($"尝试打开场景: {target.name}");
            EditorGUILayout.HelpBox("场景加载功能需要在运行时使用", MessageType.Info);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        CustomEditorStyles.EndColoredBox();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 获取场景类型的描述
    /// </summary>
    private string GetSceneTypeDescription(string sceneType)
    {
        switch (sceneType)
        {
            case "YooAssetSceneSO":
                return "YooAsset 场景配置用于通过 YooAsset 资源管理系统加载的场景。" +
                       "支持热更新和异步加载。";
            
            case "UISceneSO":
                return "UI 场景配置用于管理 UI 相关的场景。" +
                       "通常包含 Canvas、EventSystem 等 UI 组件。";
            
            case "PersistentManagersSO":
                return "持久场景配置用于在游戏运行期间始终保持加载的场景。" +
                       "通常包含全局管理器、音频管理器等持久化对象。";
            
            case "GameSceneSO":
                return "基础游戏场景配置，可用于任何类型的场景引用。" +
                       "使用 Addressable Assets 系统进行资源管理。";
            
            default:
                return "";
        }
    }
}
#endif
