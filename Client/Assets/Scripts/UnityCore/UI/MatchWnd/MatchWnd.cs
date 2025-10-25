using System.Collections.Generic;
using GameProtocol;
using LATLog;
using LatProtocol;
using UnityCore.Base;
using UnityCore.Extensions.Audio;
using UnityCore.Extensions.UI;
using UnityCore.GameModule;
using UnityCore.Network;
using UnityCore.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.UI.MatchWnd
{
    public class MatchWnd : UIFormLogic
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Text confirmText;
        [SerializeField] private RectTransform leftRoot;
        [SerializeField] private RectTransform rightRoot;
        [SerializeField] private LatButton confirmButton;
        [SerializeField] private AudioPlayerSingle audioPlayerSingle;
        [SerializeField] private List<GameObject> leftPlayerList;
        [SerializeField] private List<GameObject> leftPlayerStateList;
        [SerializeField] private List<GameObject> rightPlayerList;
        [SerializeField] private List<GameObject> rightPlayerStateList;
        
        private const string AudiosMatchReminder = "Audios_matchReminder";
        
        private int _timeCount = 0;
        
        protected internal override void OnInit(object mData)
        {
            base.OnInit(mData);

            _timeCount = ServerConfig.ConfirmationTime;
            audioPlayerSingle.Play(AudiosMatchReminder);
            
            ConfirmDto[] confirmDataArr = mData as ConfirmDto[];
            if (confirmDataArr == null)
            {
                GameDebug.LogError("MatchWnd: ConfirmData is null");
                return;
            }
            
            int count = confirmDataArr.Length / 2;
            for (int i = 0; i < 5; i++) 
            {
                leftPlayerList[i].SetActive(i < count);
            }
            
            for (int i = 0; i < 5; i++)
            {
                rightPlayerList[i].SetActive(i < count);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(leftRoot);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rightRoot);
        }

        protected internal override void OnOpen(object mData)
        {
            ConfirmDto[] confirmDataArr = mData as ConfirmDto[];
            if (confirmDataArr == null)
            {
                GameDebug.LogError("MatchWnd: ConfirmData is null");
                return;
            }
            
            base.OnOpen(mData);

            int count = confirmDataArr.Length / 2;
            
            for (int i = 0; i < count; i++)
            {
                leftPlayerStateList[i].SetActive(confirmDataArr[i].ConfirmDone);
            }
            
            for (int j = count; j < confirmDataArr.Length; j++)
            {
                rightPlayerStateList[j - count].SetActive(confirmDataArr[j].ConfirmDone);
            }
            
            
            int confirmCount = 0;
            foreach (var confirmData in confirmDataArr)
            {
                if (confirmData.ConfirmDone)
                {
                    confirmCount++;
                }
            }
            
            confirmText.text = $"{confirmCount}/{confirmDataArr.Length}";
        }

        private void OnEnable()
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
        }

        private void OnDisable()
        {
            confirmButton.onClick.RemoveListener(OnConfirmButtonClick);
        }
        
        private void OnConfirmButtonClick()
        {
            var matchManager = GameModuleManager.GetModule<MatchManager>();
            SendConfirmRequest request = new SendConfirmRequest
            {
                RoomId = matchManager.GetRoomId(),
            };
            
            NetworkManager.SendMsg((ushort)ProtocolID.SendConfirmRequest, request);
        }

        private float _deltaSum = 0f;
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            _deltaSum += elapseSeconds;
            if (_deltaSum >= 1f)
            {
                _deltaSum -= 1f;
                _timeCount--;
                timeText.text = $"{_timeCount}s";

                if (_timeCount <= 0)
                {
                    Game.UI.CloseUI(UIForm);
                }
            }
        }
    }
}