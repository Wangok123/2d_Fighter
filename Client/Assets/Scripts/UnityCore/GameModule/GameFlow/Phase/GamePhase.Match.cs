using Core.StateMachine;
using UnityCore.Base;

namespace UnityCore.GameModule.GameFlow.Phase
{
    public partial class GamePhase
    {
        public class Match : IStateNode
        {
            private MatchManager _matchManager;

            public void OnCreate(StateMachine machine)
            {
            }

            public void OnEnter()
            {
                _matchManager = GameModuleManager.GetModule<MatchManager>();
                _matchManager.Initialize();
                _matchManager.EnterStartMatchQueue();
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