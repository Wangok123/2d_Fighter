using Core;
using Core.StateMachine;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.Loading;
using UnityCore.GameModule.Loading.Tasks;
using GamePhase = UnityCore.GameModule.GameFlow.Phase.GamePhase;

namespace UnityCore.GameModule.GameFlow
{
    public class GameFlowManager : CoreModule
    {
        public override int Priority => 10;

        private StateMachine _userStateMachine;
        private EventGroup _eventGroup;

        public void Init()
        {
            _eventGroup = new EventGroup();

            _userStateMachine = new StateMachine(this);
#if UNITY_EDITOR
            _userStateMachine.AddNode<GamePhase.GmPhase>();
#endif
            _userStateMachine.AddNode<GamePhase.LoginPhase>();
            _userStateMachine.AddNode<GamePhase.StartPhase>();
            _userStateMachine.AddNode<GamePhase.LobbyPhase>();
            _userStateMachine.AddNode<GamePhase.Match>();
            _userStateMachine.AddNode<GamePhase.HeroSelectingPhase>();
            _userStateMachine.AddNode<GamePhase.Playing>();
        }

        public void BindEvent()
        {
            _eventGroup.AddListener<SceneEventDefine.ChangeToHomeScene>(OnChangeToHomeScene);
            _eventGroup.AddListener<ResponseEventDefine.LoginSuccessArgs>(OnLoginSuccess);
        }

        public void UnbindEvent()
        {
            _eventGroup.RemoveAllListener();
        }

        public void EnterLobby()
        {
            _userStateMachine.ChangeState<GamePhase.LobbyPhase>();
        }

        public void EnterMatchPhase()
        {
            _userStateMachine.ChangeState<GamePhase.Match>();
        }

        public void EnterHeroSelectingPhase()
        {
            _userStateMachine.ChangeState<GamePhase.HeroSelectingPhase>();
        }

        public void EnterPlayingPhase()
        {
            _userStateMachine.ChangeState<GamePhase.Playing>();
        }

        public void EnterGmPhase()
        {
#if UNITY_EDITOR
            _userStateMachine.ChangeState<GamePhase.GmPhase>();
#endif
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            _userStateMachine.Update();
        }

        internal override void Shutdown()
        {
        }


        #region Event Handlers

        private void OnLoginSuccess(IEventMessage evt)
        {
            if (evt is ResponseEventDefine.LoginSuccessArgs loginResponse)
            {
                _userStateMachine.ChangeState<GamePhase.StartPhase>();
            }
        }

        private void OnChangeToHomeScene(IEventMessage evt)
        {
            if (evt is SceneEventDefine.ChangeToHomeScene)
            {
                _userStateMachine.Run<GamePhase.LoginPhase>();
            }
        }

        #endregion
    }
}