using System.Collections.Generic;
using Core;
using GameProtocol;
using LATLog;
using LATMath;
using LATPhysics;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.Battle.Data;
using UnityCore.GameModule.Battle.GamePhysics;
using UnityCore.GameModule.Battle.Logic;
using UnityCore.GameModule.Battle.Manager.System;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace UnityCore.GameModule.Battle.Manager.MainGame
{
    public class MainGameManager : CoreModule
    {
        public bool IsTickFight;

        private BattleHeroManager _battleHeroManager = new BattleHeroManager();
        private PhysicEnvManager _physicEnvManager = new PhysicEnvManager();
        private BattleNetworkManager _battleNetworkManager = new BattleNetworkManager();

        public MapRoot MapRoot { get; set; }
        public GameObject UnitRoot { get; set; }
        public GameCameraSystem GameCameraSystem { get; set; }

        #region Properties

        public List<Hero> HeroDataList => _battleHeroManager.HeroDataList;

        private int _playerIndex = -1;
        public int PlayerIndex
        {
            get => _playerIndex;
            private set
            {
                if (_playerIndex != value)
                {
                    _playerIndex = value;
                    GameDebug.Log($"PlayerIndex changed to: {_playerIndex}");
                }
            }
        }

        public MainLogicUnit GetSelfHero()
        {
            return _battleHeroManager.GetSelfHero(PlayerIndex);
        }

        #endregion
        
        #region Events

        private EventGroup _eventGroup = new EventGroup();

        public void RegisterEvents()
        {
            _eventGroup.AddListener<BattleEventDefine.OperationKeyNotificationArgs>(OnOperationKeyNotification);
        }

        public void UnRegisterEvents()
        {
            _eventGroup.RemoveAllListener();
        }

        private void OnOperationKeyNotification(IEventMessage args)
        {
            if (args is not BattleEventDefine.OperationKeyNotificationArgs msg)
            {
                return;
            }

            _battleHeroManager.InputKey(msg.Notification.KeyList);
            if (IsTickFight)
            {
                _battleHeroManager.InputKey(msg.Notification.KeyList);
                _battleHeroManager.Tick();
            }
        }

        #endregion

        public void InitBattleHeros(BattleHeroDto[] battleHeroes, int mapID)
        {
            _battleHeroManager.UnitRoot = UnitRoot.transform;
            _battleHeroManager.Init(battleHeroes, mapID);
        }

        public void InitPhysicEnv()
        {
            _physicEnvManager.Init(MapRoot);
        }

        public List<LATColliderBase> GetEnvColliders()
        {
            return _physicEnvManager.GetEnvColliders();
        }

        public void InitCamera(int posIndex)
        {
            if (GameCameraSystem == null)
            {
                GameDebug.LogError("GameCameraSystem is not initialized.");
                return;
            }
            
            var heroDataList = _battleHeroManager.HeroDataList;
            Hero hero = heroDataList.Find(h => h.PosIndex == posIndex);
            GameCameraSystem.Init(hero.ViewUnit.transform);
        }
        
        public void InitPlayerIndex(int dataPosIndex)
        {
            PlayerIndex = dataPosIndex;
        }
        
        public void UnInit()
        {
            _battleHeroManager.UnInit();
            _physicEnvManager.UnInit();
        }

        #region Network

        public bool SendMoveKey(LATVector3 moveKey)
        {
            return _battleNetworkManager.SendMoveKey(moveKey);
        }

        public void SendSkillKey(int skillID, Vector3 vector3)
        {
            
        }

        #endregion

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }


    }
}