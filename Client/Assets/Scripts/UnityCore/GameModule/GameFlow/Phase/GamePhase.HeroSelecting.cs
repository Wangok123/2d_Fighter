using Core.StateMachine;
using UnityCore.Base;
using UnityCore.GameModule.HeroSelect;
using UnityCore.GameModule.Lobby;

namespace UnityCore.GameModule.GameFlow.Phase
{
    public partial class GamePhase
    {
        public class HeroSelectingPhase : IStateNode
        {
            private HeroSelectManager _heroSelectManager;
            
            public void OnCreate(StateMachine machine)
            {
            }

            public void OnEnter()
            {
                GameModuleManager.RemoveModule<LobbyManager>();
                _heroSelectManager = GameModuleManager.GetModule<HeroSelectManager>();
                _heroSelectManager.BindEvents();
                _heroSelectManager.OpenHeroSelectWindow();
            }

            public void OnExit()
            {
                GameModuleManager.RemoveModule<HeroSelectManager>();
            }

            public void OnUpdate()
            {
            }
        }
    }
}