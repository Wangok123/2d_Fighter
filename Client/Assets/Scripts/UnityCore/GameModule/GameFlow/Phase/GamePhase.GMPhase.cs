using Core.StateMachine;
using UnityCore.Base;
using UnityCore.GameModule.GM;

namespace UnityCore.GameModule.GameFlow.Phase
{
    public partial class GamePhase
    {
#if UNITY_EDITOR
        public class GmPhase : IStateNode
        {
            private GmManager _gmManager;
            public void OnCreate(StateMachine machine)
            {
                _gmManager = GameModuleManager.GetModule<GmManager>();
                _gmManager.Init();
            }

            public void OnEnter()
            {
                _gmManager.StartSimulate();
            }

            public void OnExit()
            {
            }

            public void OnUpdate()
            {
            }
        }
#endif
    }
}