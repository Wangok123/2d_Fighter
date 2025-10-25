using System.Collections;
using GameProtocol;
using LatProtocol;
using UnityCore.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityCore.GameModule.Loading.Tasks
{
    public class PlayingSceneLoadingTask : ILoadingTask
    {
        private float _loadTime;
        private float _interval = 0.3f; // 每隔0.3秒发送一次更新进度请求
        private float _progress = 0f;
        private bool _isDone = false;
        private string _sceneName;

        public float Progress => _progress;
        public int Weight => 1;
        public bool IsDone => _isDone;

        public PlayingSceneLoadingTask(string sceneName)
        {
            _sceneName = sceneName;
        }

        public IEnumerator Execute()
        {
            var loadingSceneHandle = Game.YooAsset.LoadScene(_sceneName, LoadSceneMode.Additive);
            // 每隔一段时间更新进度，同时0.3秒更新一次发送一次请求
            float lastProgress = 0f;
            var matchManager = GameModuleManager.GetModule<MatchManager>();
            
            
            while (!loadingSceneHandle.IsDone)
            {
                // 当进度有变化时发送进度更新请求
                if (Mathf.Abs(loadingSceneHandle.Progress - lastProgress) > 0.01f)
                {
                    lastProgress = loadingSceneHandle.Progress;
                    
                    Game.Network.SendMsg((ushort)ProtocolID.SendLoadProgressRequest, new SendLoadProgressRequest
                    {
                        RoomId = matchManager.GetRoomId(),
                        Percent = Mathf.RoundToInt(lastProgress * 100)
                    });
                }
            
                _progress = loadingSceneHandle.Progress;
                yield return null;
            }

            _isDone = true;
            _progress = 1f; // 确保进度最终为100%
            Game.Network.SendMsg((ushort)ProtocolID.SendLoadProgressRequest, new SendLoadProgressRequest
            {
                RoomId = matchManager.GetRoomId(),
                Percent = 100
            });
        }
    }
}