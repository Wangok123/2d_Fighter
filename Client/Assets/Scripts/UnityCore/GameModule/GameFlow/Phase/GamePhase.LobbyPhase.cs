using System.Collections.Generic;
using Core.StateMachine;
using UnityCore.Base;
using UnityCore.GameModule.Lobby;

namespace UnityCore.GameModule.GameFlow.Phase
{
    public partial class GamePhase
    {
        public class LobbyPhase : IStateNode
        {
            private LobbyManager _lobbyManager;

            public void OnCreate(StateMachine machine)
            {
            }

            public void OnEnter()
            {
                _lobbyManager = GameModuleManager.GetModule<LobbyManager>();
                _lobbyManager.OpenLobby();
            }

            public void OnExit()
            {
            }

            public void OnUpdate()
            {
            }
        }
    }
}