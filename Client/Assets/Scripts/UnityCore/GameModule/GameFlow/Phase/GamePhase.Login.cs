using Core.StateMachine;
using UnityCore.Base;
using UnityCore.Constant;

namespace UnityCore.GameModule.GameFlow.Phase
{
    public partial class GamePhase
    {
        public class LoginPhase : IStateNode
        {
            private int _loginId;
            
            public void OnCreate(StateMachine machine)
            {
            }

            public void OnEnter()
            {
                _loginId = Game.UI.OpenUI(UIWindow.LoginWnd);
                Game.Audio.PlayBackgroundMusic("Audios_main");
                Game.Network.Init();
            }

            public void OnExit()
            {
                Game.UI.CloseUI(_loginId);
            }

            public void OnUpdate()
            {
            }
        }
    }
}