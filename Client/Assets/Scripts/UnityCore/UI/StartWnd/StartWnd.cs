using LATLog;
using UniRx;
using UnityCore.Base;
using UnityCore.Entities.User;
using UnityCore.Extensions.UI;
using UnityCore.GameModule;
using UnityCore.GameModule.GameFlow;
using UnityCore.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.UI.StartWnd
{
    public class StartWnd : UIFormLogic
    {
        [SerializeField] private LatButton startButton;
        [SerializeField] private LatButton exitButton;
        [SerializeField] private Text accountNameText;

        UserDataComponent _userDataComponent;

        protected internal override void OnInit(object mData)
        {
            base.OnInit(mData);
            
            startButton.onClick
                .AsObservable()
                .Subscribe(_ => OnStartButtonClicked())
                .AddTo(this);
            
            
            var userData = Game.World.GetSingletonEntity<UserDataEntity>();
            _userDataComponent = userData.GetComponent<UserDataComponent>();
            if (_userDataComponent == null)
            {
                GameDebug.LogError("UserDataComponent is null.");
                return;
            }
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            UpdateAccountName();
        }

        private void OnStartButtonClicked()
        {
            var flowManager = GameModuleManager.GetModule<GameFlowManager>();
            flowManager.EnterLobby();
        }

        private void UpdateAccountName()
        {
            accountNameText.text = _userDataComponent.Username;
        }
    }
}