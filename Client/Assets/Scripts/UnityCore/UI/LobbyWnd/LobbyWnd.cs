using UniRx;
using UnityCore.Base;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.Extensions.UI;
using UnityCore.GameModule.GameFlow;
using UnityCore.GameModule.Lobby;
using UnityCore.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.UI.LobbyWnd
{
    public class LobbyWnd : UIFormLogic
    {
        [SerializeField] private LatButton matchButton;
        [SerializeField] private GameObject matchRoot;
        [SerializeField] private Text predictionTimeText;
        [SerializeField] private Text countDownText;

        private readonly EventGroup _eventGroup = new EventGroup();

        protected internal override void OnInit(object mData)
        {
            base.OnInit(mData);

            matchButton.onClick
                .AsObservable()
                .Subscribe(_ => OnMatchClick())
                .AddTo(this);
        }
        
        private void OnEnable()
        {
            _eventGroup.AddListener<ResponseEventDefine.MatchStartArgs>(OnHandleEventMessage);
            _eventGroup.AddListener<ResponseEventDefine.ConfirmNotificationArgs>(OnConfirmNtfEventMessage);
        }
        
        private void OnDisable()
        {
            _eventGroup.RemoveAllListener();
        }

        protected internal override void OnOpen(object mData)
        {
            base.OnOpen(mData);
            
            matchRoot.SetActive(false);
        }
        
        private int _timeCountdown = 0;
        private bool _isMatching = false;
        private float _deltaSum = 0f;
        private void UpdateCountTime()
        {
            int minutes = _timeCountdown / 60;
            int seconds = _timeCountdown % 60;
            string timeString = $"{minutes:D2}:{seconds:D2}";
            countDownText.text = timeString;
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (_isMatching)
            {
                _deltaSum += elapseSeconds;
                if (_deltaSum >= 1f)
                {
                    _deltaSum -= 1f;
                    _timeCountdown++;
                    UpdateCountTime();
                }
            }
        }

        public void SetMatchState(bool isMatching, int predictTime = 0)
        {
            if (isMatching)
            {
                matchRoot.SetActive(true);
                _isMatching = true;
                int minutes = predictTime / 60;
                int seconds = predictTime % 60;
                string timeString = $"{minutes:D2}:{seconds:D2}";
                predictionTimeText.text = timeString;
            }
            else
            {
                matchRoot.SetActive(false);
                _isMatching = false;
                _timeCountdown = 0;
                predictionTimeText.text = "00:00";   
            }
        }

        private void OnMatchClick()
        {
            var gameFlowManager = GameModuleManager.GetModule<GameFlowManager>();
            gameFlowManager.EnterMatchPhase();
        }
        
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (!(message is ResponseEventDefine.MatchStartArgs args))
            {
                return;
            }
            
            var time = args.PredictionTime;
            if (time > 0)
            {
                SetMatchState(true, time);
            }
            else
            {
                SetMatchState(false);
            }
        }

        private void OnConfirmNtfEventMessage(IEventMessage message)
        {
            if (!(message is ResponseEventDefine.ConfirmNotificationArgs args))
            {
                return;
            }

            SetMatchState(false);
        }
    }
}