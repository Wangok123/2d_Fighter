using System.Collections;
using LATLog;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.EventDefine;
using UnityCore.GameModule.GameFlow;
using UnityEngine.SceneManagement;

namespace UnityCore.GameModule.Loading.Tasks
{
    public class PlayingSceneGMLoadingTask : ILoadingTask
    {
        private float _loadTime;
        private float _interval = 0.3f; // 每隔0.3秒发送一次更新进度请求
        private float _progress = 0f;
        private bool _isDone = false;
        private string _sceneName;

        public float Progress => _progress;
        public int Weight => 1;
        public bool IsDone => _isDone;

        public PlayingSceneGMLoadingTask(string sceneName)
        {
            _sceneName = sceneName;
        }

        public IEnumerator Execute()
        {
            var loadingSceneHandle = Game.YooAsset.LoadScene(_sceneName, LoadSceneMode.Additive);
            
            yield return loadingSceneHandle;
            
            if (loadingSceneHandle.Status == YooAsset.EOperationStatus.Succeed)
            {
                ResponseEventDefine.LoadFinishNotificationArgs.SendEventMessage();
                Game.UI.OpenUI(UIWindow.PlayWnd);
                var gameFlowManager = GameModuleManager.GetModule<GameFlowManager>();
                gameFlowManager.EnterPlayingPhase();
                _isDone = true;
                _progress = 1f; // 确保进度最终为100%
            }
            else
            {
                GameDebug.LogError($"Failed to load scene {_sceneName}");
                _isDone = true;
            }
        }
    }
}