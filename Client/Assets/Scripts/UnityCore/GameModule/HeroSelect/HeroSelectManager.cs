using Core;
using GameProtocol;
using LatProtocol;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.Loading;
using UnityCore.GameModule.Loading.Tasks;

namespace UnityCore.GameModule.HeroSelect
{
    public class HeroSelectManager : CoreModule
    {
        private int _windowID;
        
        public void BindEvents()
        {
            UniEvent.AddListener<ResponseEventDefine.LoadResNotificationArgs>(OnLoadResNotification);
        }

        private void UnBindEvents()
        {
            UniEvent.RemoveListener<ResponseEventDefine.LoadResNotificationArgs>(OnLoadResNotification);
        }
        
        public void OpenHeroSelectWindow()
        {
            _windowID = Game.UI.OpenUI(Constant.UIWindow.SelectWnd);
        }

        public void ConfirmHeroSelect(int currentHeroId)
        {
            var matchManager = GameModuleManager.GetModule<MatchManager>();
            uint roomID = matchManager.GetRoomId();
            
            SendSelectRequest request = new SendSelectRequest
            {
                RoomId = roomID,
                HeroId = currentHeroId
            };
            
            Game.Network.SendMsg((ushort)ProtocolID.SendSelectRequest, request);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            Game.UI.CloseUI(_windowID);
            
            UnBindEvents();
        }
        
        private void OnLoadResNotification(IEventMessage args)
        {
            if (args is not ResponseEventDefine.LoadResNotificationArgs msg)
            {
                return;
            }

            var sceneName = "map_" + msg.MapId;
            var loadingManager = GameModuleManager.GetModule<LoadingManager>();
            loadingManager.AddTask(new PlayingSceneLoadingTask(sceneName));
            loadingManager.AddTask(new LoadingFinishTask(UIWindow.PlayWnd));
            loadingManager.Load(UIWindow.LoadWnd, msg, () => { Game.Audio.StopBackgroundMusic(); }
                , () =>
                {
                    GameModuleManager.RemoveModule<LoadingManager>();
                });
        }
    }
}