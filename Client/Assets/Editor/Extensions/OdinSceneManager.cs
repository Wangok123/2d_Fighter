using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public class OdinSceneManager : OdinEditorWindow
    {
        [MenuItem("工具/场景管理器 &s")]
        private static void OpenWindow()
        {
            var window = GetWindow<OdinSceneManager>();
            window.titleContent = new GUIContent("场景管理器", EditorGUIUtility.IconContent("SceneAsset Icon").image);
            window.Show();
        }

        [BoxGroup("场景切换")]
        [ValueDropdown("GetSceneOptions")]
        [LabelText("选择场景")]
        [OnValueChanged("OnSceneSelected")]
        [PropertyOrder(1)]
        public string SelectedScenePath;

        [BoxGroup("场景切换")]
        [HorizontalGroup("场景切换/Buttons")]
        [Button("切换场景", ButtonSizes.Medium)]
        [PropertyOrder(2)]
        private void SwitchToScene()
        {
            if (string.IsNullOrEmpty(SelectedScenePath))
            {
                Debug.LogWarning("请先选择一个场景!");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(SelectedScenePath);
            }
        }

        [BoxGroup("场景切换")]
        [HorizontalGroup("场景切换/Buttons")]
        [Button("切换并播放", ButtonSizes.Medium)]
        [PropertyOrder(3)]
        private void SwitchAndPlay()
        {
            if (string.IsNullOrEmpty(SelectedScenePath))
            {
                Debug.LogWarning("请先选择一个场景!");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(SelectedScenePath);
                EditorApplication.isPlaying = true;
            }
        }

        [BoxGroup("快速访问")]
        [PropertyOrder(4)]
        [TableList(AlwaysExpanded = true, ShowIndexLabels = true)]
        public List<SceneInfo> QuickAccessScenes = new List<SceneInfo>();

        [BoxGroup("设置")]
        [FolderPath]
        [LabelText("场景文件夹")]
        [PropertyOrder(5)]
        public string ScenesFolder = "Assets/Scenes";

        [BoxGroup("设置")]
        [Button("刷新场景列表", ButtonSizes.Medium)]
        [PropertyOrder(6)]
        private void RefreshScenes()
        {
            QuickAccessScenes.Clear();

            if (!Directory.Exists(ScenesFolder))
            {
                Debug.LogWarning($"场景文件夹不存在: {ScenesFolder}");
                return;
            }

            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder });
            foreach (var guid in sceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneName = Path.GetFileNameWithoutExtension(path);
                QuickAccessScenes.Add(new SceneInfo
                {
                    SceneName = sceneName,
                    ScenePath = path,
                    SceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path)
                });
            }

            QuickAccessScenes = QuickAccessScenes.OrderBy(s => s.SceneName).ToList();
        }

        private IEnumerable<ValueDropdownItem<string>> GetSceneOptions()
        {
            var scenes = new List<ValueDropdownItem<string>>();

            if (!Directory.Exists(ScenesFolder))
            {
                return scenes;
            }

            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder });
            foreach (var guid in sceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneName = Path.GetFileNameWithoutExtension(path);
                scenes.Add(new ValueDropdownItem<string>(sceneName, path));
            }

            return scenes.OrderBy(s => s.Text);
        }

        private void OnSceneSelected()
        {
            if (!string.IsNullOrEmpty(SelectedScenePath))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(SelectedScenePath));
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (QuickAccessScenes.Count == 0)
            {
                RefreshScenes();
            }
        }

        [System.Serializable]
        public class SceneInfo
        {
            [ReadOnly]
            [HideLabel]
            [PreviewField(55, ObjectFieldAlignment.Left)]
            [HorizontalGroup("Scene", Width = 60)]
            public SceneAsset SceneAsset;

            [ReadOnly]
            [HideLabel]
            [HorizontalGroup("Scene")]
            public string SceneName;

            [HideInInspector]
            public string ScenePath;

            [Button("打开", ButtonSizes.Small)]
            [HorizontalGroup("Scene", Width = 60)]
            private void OpenScene()
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(ScenePath);
                }
            }

            [Button("▶", ButtonSizes.Small)]
            [HorizontalGroup("Scene", Width = 30)]
            private void OpenAndPlay()
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(ScenePath);
                    EditorApplication.isPlaying = true;
                }
            }
        }
    }
}
