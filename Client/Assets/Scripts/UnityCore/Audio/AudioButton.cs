using System;
using UniRx;
using UnityCore.Base;
using UnityCore.Extensions.UI;
using UnityEngine;

namespace UnityCore.Audio
{
    public class AudioButton : MonoBehaviour
    {
        [SerializeField] private string audioName = "audios_loginBtnClick";
        private LatButton _button;

        private AudioClip _audioClip;

        private void Awake()
        {
            _button = GetComponent<LatButton>();
            if (_button == null)
            {
                return;
            }

            _button.onClick
                .AsObservable()
                .Subscribe(_ => PlayAudio())
                .AddTo(this);
        }

        private void PlayAudio()
        {
            if (string.IsNullOrEmpty(audioName))
            {
                return;
            }

            audioName = audioName.ToLower();

            Game.YooAsset.LoadAudioAsset(audioName, clip =>
            {
                _audioClip = clip;
                Game.Audio.PlaySingleSound(clip);
            });
        }

        private void OnDestroy()
        {
            if (_audioClip != null)
            {
                Game.YooAsset.UnloadAsset(audioName);
            }
        }
    }
}