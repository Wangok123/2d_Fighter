using System.Collections;
using System.Collections.Generic;
using Core;
using GameProtocol;
using Google.Protobuf;
using LATLog;
using LatProtocol;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.GameFlow;
using UnityCore.GameModule.Loading;
using UnityCore.GameModule.Loading.Tasks;
using UnityCore.Tools;
using UnityEngine;

namespace UnityCore.GameModule.GM
{
    public class GmManager : CoreModule
    {
        public static bool IsGmGame;
        private EventGroup _eventGroup;
        private bool _loadResourcesCompleted;
        
        private uint _frameId = 0;
        private List<OperationKey> opKeyList = new List<OperationKey>();

        public void Init()
        {
            _eventGroup = new EventGroup();
            IsGmGame = true;
        }

        public void StartSimulate()
        {
            _eventGroup.AddListener<ResponseEventDefine.LoadFinishNotificationArgs>(OnLoadingFinish);
            CoroutineManager.Instance.Coroutine(BattleSimulate());
        }

        private IEnumerator BattleSimulate()
        {
            SimulateLoadResources();
            yield return new WaitUntil(() => _loadResourcesCompleted);
        }

        private void SimulateLoadResources()
        {
            uint mapId = 102;
            var posIndex = 0;
            var heroDataList = new[]
            {
                new BattleHeroDto
                {
                    HeroId = 1001,
                    UserName = "PlayerA"
                },
                new BattleHeroDto
                {
                    HeroId = 1002,
                    UserName = "PlayerB"
                }
            };
            
            // GM命令，直接跳转到加载逻辑
            OnGMLoadResNotification(mapId, posIndex, heroDataList);
        }

        private void OnGMLoadResNotification(uint mapId, int posIndex, BattleHeroDto[] heroDataList)
        {
            var sceneName = "map_" + mapId;
            var loadingManager = GameModuleManager.GetModule<LoadingManager>();
            loadingManager.AddTask(new PlayingSceneGMLoadingTask(sceneName));

            InitManagerTask.InitData data = new InitManagerTask.InitData()
            {
                HeroDataArray = heroDataList,
                MapID = (int)mapId,
                PosIndex = posIndex
            };
                
            loadingManager.AddTask(new InitManagerTask(data));
            loadingManager.AddTask(new SimulateBattleStartTask());
            loadingManager.Load(onStart:() =>
                {
                    Game.Audio.StopBackgroundMusic();
                }
                , onComplete: () => { GameModuleManager.RemoveModule<LoadingManager>(); });
        }
        
        private void OnLoadingFinish(IEventMessage message)
        {
            if (message is not ResponseEventDefine.LoadFinishNotificationArgs)
            {
                return;
            }

            _loadResourcesCompleted = true;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            _eventGroup.RemoveAllListener();
            IsGmGame = false;
        }

        public void SendMsgGM(ushort protocolId, IMessage msg)
        {
            if (!IsGmGame)
            {
                GameDebug.LogError("GmManager is not initialized or not in GM mode.");
                return;
            }

            switch (protocolId)
            {
                case (ushort)ProtocolID.SendOperationKeyRequest:
                    var request = msg as SendOperationKeyRequest;
                    if (request == null)
                    {
                        GameDebug.LogError("Invalid message type for SendOperationKeyRequest.");
                        return;
                    }
                    UpdateOpKey(request.OpKey);
                    break;
                
                default:
                    GameDebug.LogWarning($"Received unsupported protocol ID: {protocolId}");
                    break;
            }
        }

        public override void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            ++_frameId;
            OperationKeyNotification notification = new OperationKeyNotification
            {
                FrameId = _frameId,
            };

            int count = opKeyList.Count;
            if (count > 0)
            {
                for (int i = 0; i < opKeyList.Count; i++)
                {
                    var key = opKeyList[i];
                    notification.KeyList.Add(key);
                }
            }
            
            opKeyList.Clear();
            BattleEventDefine.OperationKeyNotificationArgs.SendEventMessage(notification);
        }

        private void UpdateOpKey(OperationKey key)
        {
            opKeyList.Add(key);
        }
    }
}