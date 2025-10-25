using Core;
using UnityCore.Base;

namespace UnityCore.GameModule.Lobby
{
    public class LobbyManager : CoreModule
    {
        private const int UIListID = 1000; // Example ID for Lobby Window
        public override int Priority => 9;
        
        private bool _isOpened = false;

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            CloseLobby();
        }

        #region UI

        public void OpenLobby()
        {
            if (_isOpened)
            {
                return;
            }
            
            Game.UI.OpenUI(UIListID, Constant.UIWindow.LobbyWnd);
            _isOpened = true;
        }
        
        private void CloseLobby()
        {
            Game.UI.CloseUIByUIListID(UIListID);
            _isOpened = false;
        }

        #endregion
        
    }
}