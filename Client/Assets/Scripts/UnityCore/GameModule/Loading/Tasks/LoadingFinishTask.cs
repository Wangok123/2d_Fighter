using System.Collections;
using GameProtocol;
using LatProtocol;
using UnityCore.Base;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.GameFlow;
using UnityEngine;

namespace UnityCore.GameModule.Loading.Tasks
{
    public class LoadingFinishTask : ILoadingTask
    {
        private float _loadTime;
        private float _progress;
        private bool _isDone;

        public float Progress => _progress;
        public int Weight => 1;
        public bool IsDone => _isDone;

        private string _wndName;
        
        private bool _loadingComplete;

        public LoadingFinishTask(string wndName)
        {
            _wndName = wndName;
            BindEvent();
        }
        
        private void BindEvent()
        {
            UniEvent.AddListener<ResponseEventDefine.LoadFinishNotificationArgs>(OnLoadingFinish);
        }
        
        private void UnBindEvent()
        {
            UniEvent.RemoveListener<ResponseEventDefine.LoadFinishNotificationArgs>(OnLoadingFinish);
        }

        private void OnLoadingFinish(IEventMessage message)
        {
            if (message is not ResponseEventDefine.LoadFinishNotificationArgs)
            {
                return;
            }

            _loadingComplete = true;
            UnBindEvent();
        }

        public IEnumerator Execute()
        {
            Game.UI.OpenUI(_wndName);
            var matchManager = GameModuleManager.GetModule<MatchManager>();
            yield return null;

            _isDone = true;
            _progress = 1f;
            Game.Network.SendMsg((ushort)ProtocolID.LoadingFinishRequest, new LoadingFinishRequest
            {
                RoomId = matchManager.GetRoomId()
            });

            yield return new WaitUntil(() => _loadingComplete);
            
            var gameFlowManager = GameModuleManager.GetModule<GameFlowManager>();
            gameFlowManager.EnterPlayingPhase();
        }
    }
}