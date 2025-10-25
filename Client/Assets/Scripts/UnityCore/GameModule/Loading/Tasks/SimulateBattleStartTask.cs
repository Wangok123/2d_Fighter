using System.Collections;
using GameProtocol;
using LatProtocol;
using UnityCore.Base;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityEngine;

namespace UnityCore.GameModule.Loading.Tasks
{
    public class SimulateBattleStartTask : ILoadingTask
    {
        private bool _isDone;

        public float Progress { get; }
        public int Weight => 1;
        public bool IsDone => _isDone;

        public SimulateBattleStartTask()
        {
            _isDone = false;
            
            var gameManager = GameModuleManager.GetModule<MainGameManager>();
            gameManager.IsTickFight = true;
            UniEvent.AddListener<ResponseEventDefine.BattleStartArgs>(OnBattleStart);
        }

        public IEnumerator Execute()
        {
            BattleStartRequest request = new BattleStartRequest();
            yield return null;
            // Game.Network.SendMsg((ushort)ProtocolID.BattleStartRequest, request);
            // yield return new WaitUntil(() => _isBattleStarted);
            _isDone = true;
        }
        
        private void OnBattleStart(IEventMessage message)
        {
            if (message is not ResponseEventDefine.BattleStartArgs)
            {
                return;
            }

            var gameManager = GameModuleManager.GetModule<MainGameManager>();
            gameManager.IsTickFight = true;
            UniEvent.RemoveListener<ResponseEventDefine.BattleStartArgs>(OnBattleStart);
        }
    }
}