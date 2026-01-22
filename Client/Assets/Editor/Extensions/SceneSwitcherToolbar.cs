using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [InitializeOnLoad]
    public static class SceneSwitcherToolbar
    {
        private static readonly Type KToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject _sCurrentToolbar;
        private static List<string> _sceneNames = new List<string>();
        private static List<string> _scenePaths = new List<string>();
        private static int _selectedSceneIndex = 0;
        private static readonly string ScenesFolder = "Assets/Scenes";

        static SceneSwitcherToolbar()
        {
            RefreshSceneList();
            EditorApplication.update += OnUpdate;
        }

        private static void RefreshSceneList()
        {
            _sceneNames.Clear();
            _scenePaths.Clear();

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
                _scenePaths.Add(path);
                _sceneNames.Add(sceneName);
            }

            _sceneNames = _sceneNames.OrderBy(name => name).ToList();
            _scenePaths = _scenePaths.OrderBy(path => Path.GetFileNameWithoutExtension(path)).ToList();

            var currentScene = EditorSceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(currentScene.path))
            {
                _selectedSceneIndex = _scenePaths.IndexOf(currentScene.path);
                if (_selectedSceneIndex < 0) _selectedSceneIndex = 0;
            }
        }

        private static void OnUpdate()
        {
            if (_sCurrentToolbar != null) return;
            var toolbars = Resources.FindObjectsOfTypeAll(KToolbarType);
            _sCurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
            if (_sCurrentToolbar == null) return;
            var root = _sCurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (root == null) return;
            var concreteRoot = root.GetValue(_sCurrentToolbar) as VisualElement;
            var toolbarZone = concreteRoot.Q("ToolbarZoneRightAlign");
            var parent = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                }
            };
            var container = new IMGUIContainer();
            container.onGUIHandler += OnGuiBody;
            parent.Add(container);
            toolbarZone.Add(parent);
        }

        private static void OnGuiBody()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("从开始场景启动", EditorGUIUtility.FindTexture("PlayButton")), GUILayout.Width(120)))
            {
                if (_scenePaths.Count > 0)
                {
                    JumpToSceneAndPlay(_scenePaths[0], true);
                }
            }

            GUILayout.Space(10);

            if (_sceneNames.Count == 0)
            {
                GUILayout.Label("无可用场景", GUILayout.Width(100));
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                _selectedSceneIndex = EditorGUILayout.Popup(_selectedSceneIndex, _sceneNames.ToArray(), GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    JumpToScene(_scenePaths[_selectedSceneIndex]);
                }

                if (GUILayout.Button(new GUIContent("▶", "打开并播放"), GUILayout.Width(30)))
                {
                    JumpToSceneAndPlay(_scenePaths[_selectedSceneIndex], true);
                }
            }

            if (GUILayout.Button("刷新", GUILayout.Width(40)))
            {
                RefreshSceneList();
            }

            GUILayout.EndHorizontal();
        }

        private static void JumpToScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("场景路径为空!");
                return;
            }

            if (!File.Exists(scenePath))
            {
                Debug.LogError($"场景文件不存在: {scenePath}");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }

        private static void JumpToSceneAndPlay(string scenePath, bool play = false)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("场景路径为空!");
                return;
            }

            if (!File.Exists(scenePath))
            {
                Debug.LogError($"场景文件不存在: {scenePath}");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath);

                if (play)
                {
                    EditorApplication.isPlaying = true;
                }
            }
        }
    }
}
