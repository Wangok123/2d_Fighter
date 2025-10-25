using Core;
using Core.StateMachine;
using GameProtocol;
using LATLog;
using LatProtocol;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.Entities.Core;
using UnityCore.Entities.Match;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.GameFlow;
using UnityCore.Network;

namespace UnityCore.GameModule
{
    public class MatchManager : CoreModule
    {
        public const int UIListID = 1001; // Example ID for Match Window

        private StateMachine _stateMachine;
        private readonly EventGroup _eventGroup = new();
        private int _serialId = -1;

        private Entity _matchEntity;

        public void Initialize()
        {
            _eventGroup.AddListener<ResponseEventDefine.MatchStartArgs>(OnMatchStartEventMessage);
            _eventGroup.AddListener<ResponseEventDefine.ConfirmNotificationArgs>(OnConfirmNtfEventMessage);
            _eventGroup.AddListener<ResponseEventDefine.SelectNotificationArgs>(OnHandleEventMessage);
            _matchEntity = Game.World.CreateEntity();
        }

        internal override void Shutdown()
        {
            _eventGroup.RemoveAllListener();
        }

        private void OnHandleEventMessage(IEventMessage message)
        {
            if (!(message is ResponseEventDefine.SelectNotificationArgs args))
            {
                return;
            }

            CloseMatchWindow();
            var gameFlowManager = GameModuleManager.GetModule<GameFlowManager>();
            gameFlowManager.EnterHeroSelectingPhase();
        }

        private void OnMatchStartEventMessage(IEventMessage message)
        {
            if (!(message is ResponseEventDefine.MatchStartArgs args))
            {
                return;
            }
        }

        private void OnConfirmNtfEventMessage(IEventMessage message)
        {
            if (!(message is ResponseEventDefine.ConfirmNotificationArgs args))
            {
                return;
            }

            bool dismiss = args.IsDismiss;
            if (dismiss)
            {
                CloseMatchWindow();
                var gameFlowManager = GameModuleManager.GetModule<GameFlowManager>();
                gameFlowManager.EnterLobby();
            }
            else
            {
                var comp = _matchEntity.GetComponent<MatchRoomComponent>();
                if (comp != null)
                {
                    comp.RoomId = args.RoomId;
                }
                else
                {
                    MatchRoomComponent matchRoomComponent = new MatchRoomComponent();
                    matchRoomComponent.RoomId = args.RoomId;
                    _matchEntity.AddComponent(matchRoomComponent);
                }

                OpenMatchWindow(args.ConfirmDataArr);
            }
        }

        private void OpenMatchWindow(object data)
        {
            if (_serialId == -1)
            {
                _serialId = Game.UI.OpenUI(UIWindow.MatchWnd, data);
            }
            else
            {
                var uiForm = Game.UI.GetUIForm(_serialId);
                uiForm.OnOpen(data);
            }
        }

        private void CloseMatchWindow()
        {
            if (_serialId != -1)
            {
                Game.UI.CloseUI(_serialId);
                _serialId = -1;
            }
        }

        public uint GetRoomId()
        {
            var comp = _matchEntity.GetComponent<MatchRoomComponent>();
            if (comp != null)
            {
                return comp.RoomId;
            }
            else
            {
                GameDebug.LogError(
                    "MatchManager: MatchRoomComponent is null, please check if the entity is created correctly.");
                return 0;
            }
        }

        public void EnterStartMatchQueue()
        {
            StartMatch(MatchType._1Vs1);
        }

        private void StartMatch(MatchType matchType)
        {
            MatchRequest matchRequest = new MatchRequest
            {
                Status = matchType
            };

            NetworkManager.SendMsg((ushort)ProtocolID.MatchRequest, matchRequest);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }
    }
}