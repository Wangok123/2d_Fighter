using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public static class JumpToBuildingScene
    {
        /// <summary>
        /// 跳转到Build Settings中的第一个场景并开始游戏
        /// </summary>
        public static void JumpToFirstBuildSettingsSceneAndPlay()
        {
            JumpToBuildSettingsSceneAndPlay(0, true);
        }

        public static void JumpToUiScene()
        {
            JumpToSceneAndPlay("Assets/Scenes/UI.unity");
        }

        public static void JumpToBattleScene()
        {
            JumpToSceneAndPlay("Assets/Scenes/Battle.unity");
        }

        public static void JumpToLoadingScene()
        {
            JumpToSceneAndPlay("Assets/Scenes/Loading.unity");
        }

        public static void JumpToPersistentScene()
        {
            JumpToSceneAndPlay("Assets/Scenes/Persistent.unity");
        }

        public static void JumpToTestScene()
        {
            JumpToSceneAndPlay("Assets/Scenes/Test.unity");
        }

        public static void JumpToLancherScene()
        {
            JumpToSceneAndPlay("Assets/Scenes/Lancher.unity");
        }

        /// <summary>
        /// 跳转到Build Settings中的指定场景并开始游戏
        /// </summary>
        /// <param name="buildIndex">Build Settings中的场景索引</param>
        /// <param name="play"></param>
        private static void JumpToBuildSettingsSceneAndPlay(int buildIndex, bool play = false)
        {
            var scenes = EditorBuildSettings.scenes;

            if (scenes.Length == 0)
            {
                Debug.LogError("No scenes in Build Settings!");
                return;
            }

            if (buildIndex < 0 || buildIndex >= scenes.Length)
            {
                Debug.LogError(
                    $"Build index {buildIndex} is out of range. Available build indices: 0 to {scenes.Length - 1}");
                return;
            }

            if (!scenes[buildIndex].enabled)
            {
                Debug.LogWarning($"Scene at build index {buildIndex} is disabled in Build Settings. Loading anyway.");
            }

            JumpToSceneAndPlay(scenes[buildIndex].path, play);
        }

        /// <summary>
        /// 跳转到指定场景并开始游戏
        /// </summary>
        /// <param name="scenePath">场景路径，例如 "Assets/Scenes/GameScene.unity"</param>
        /// <param name="play"></param>
        private static void JumpToSceneAndPlay(string scenePath, bool play = false)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("Scene path is empty!");
                return;
            }

            // 检查场景是否存在
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError($"Scene not found at path: {scenePath}");
                return;
            }

            // 检查是否有未保存的更改
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // 打开选定的场景
                EditorSceneManager.OpenScene(scenePath);

                if (play)
                {
                    // 开始游戏
                    EditorApplication.isPlaying = true;
                }
            }
        }
    }
}