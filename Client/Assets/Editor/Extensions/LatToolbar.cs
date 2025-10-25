using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [InitializeOnLoad]
    public static class LatToolbar
    {
        private static readonly Type KToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject _sCurrentToolbar;

        static LatToolbar()
        {
            EditorApplication.update += OnUpdate;
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
                    flexGrow = 10,
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
            //自定义按钮加在此处
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("从开始场景启动", EditorGUIUtility.FindTexture("PlayButton"))))
            {
                JumpToBuildingScene.JumpToFirstBuildSettingsSceneAndPlay();
            }

            if (GUILayout.Button(new GUIContent("UI")))
            {
                JumpToBuildingScene.JumpToUiScene();
            }

            if (GUILayout.Button(new GUIContent("Battle")))
            {
                JumpToBuildingScene.JumpToBattleScene();
            }

            if (GUILayout.Button(new GUIContent("Loading")))
            {
                JumpToBuildingScene.JumpToLoadingScene();
            }

            if (GUILayout.Button(new GUIContent("Persistent")))
            {
                JumpToBuildingScene.JumpToPersistentScene();
            }

            if (GUILayout.Button(new GUIContent("Test")))
            {
                JumpToBuildingScene.JumpToTestScene();
            }

            if (GUILayout.Button(new GUIContent("Lancher")))
            {
                JumpToBuildingScene.JumpToLancherScene();
            }

            GUILayout.EndHorizontal();
        }
    }
}