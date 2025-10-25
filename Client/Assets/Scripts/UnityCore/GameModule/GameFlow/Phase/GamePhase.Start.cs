using Core.StateMachine;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.EventDefine;
using UnityCore.EventSystem;

namespace UnityCore.GameModule.GameFlow.Phase
{
    public partial class GamePhase
    {
        public class StartPhase : IStateNode
        {
            private int _startId;
            
            public void OnCreate(StateMachine machine)
            {
            }

            public void OnEnter()
            {
                _startId = Game.UI.OpenUI(UIWindow.StartWnd);
            }

            public void OnExit()
            {
                Game.UI.CloseUI(_startId);
            }

            public void OnUpdate()
            {
            }
        }
    }
}