using Core.ReferencePool;
using GameProtocol;
using LatProtocol;
using UniRx;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.Extensions.UI;
using UnityCore.GameModule.GameFlow;
using UnityCore.Network;
using UnityCore.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace UnityCore.UI.LoginWnd
{
    public class LoginWnd : UIFormLogic
    {
        [SerializeField] private InputField usernameInputField;
        [SerializeField] private InputField passwordInputField;
        [SerializeField] private Toggle localAreaNetworkToggle;
        [SerializeField] private LatButton loginButton;
        [SerializeField] private LatButton gmButton;

        private string _username;
        private string _password;

        private readonly EventGroup _eventGroup = new EventGroup();

        protected internal override void OnInit(object mData)
        {
            base.OnInit(mData);

            loginButton.onClick
                .AsObservable()
                .Subscribe(_ => OnLoginButtonClicked())
                .AddTo(this);
            gmButton.onClick
                .AsObservable()
                .Subscribe(_ => OnGmButtonClicked())
                .AddTo(this);
            usernameInputField
                .OnValueChangedAsObservable()
                .Subscribe(value => _username = value)
                .AddTo(this);
            passwordInputField
                .OnValueChangedAsObservable()
                .Subscribe(value => _password = value)
                .AddTo(this);
            localAreaNetworkToggle
                .OnValueChangedAsObservable()
                .Subscribe(OnToggleChanged)
                .AddTo(this);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            Random random = new Random();
            usernameInputField.text = random.Next(100, 999).ToString();
            passwordInputField.text = random.Next(100, 999).ToString();
        }

        private void OnLoginButtonClicked()
        {
            if (_username.Length >= 3 && _password.Length >= 3)
            {
                LoginRequest loginRequest = new LoginRequest
                {
                    Account = _username,
                    Password = _password
                };

                NetworkManager.SendMsg((ushort)ProtocolID.LoginRequest, loginRequest);
            }
            else
            {
                Game.UI.OpenTipsUI("Username or Password is too short");
            }
        }

        private void OnGmButtonClicked()
        {
#if UNITY_EDITOR
            var gameFlowManager = GameModuleManager.GetModule<GameFlowManager>();
            gameFlowManager.EnterGmPhase();
#endif
        }

        private void OnToggleChanged(bool value)
        {
            if (value)
            {
            }
            else
            {
            }
        }
    }
}