using UnityCore.Base;
using UnityEngine;

namespace UnityCore.Extensions.Audio
{
    public class AudioPlayerSingle : MonoBehaviour
    {
        private string _audioName;

        public void Play(string audioName)
        {
            if (string.IsNullOrEmpty(_audioName))
            {
                return;
            }

            _audioName = audioName;
            Game.YooAsset.LoadAudioAsset(audioName, OnLoadSuccess);
        }

        private void OnLoadSuccess(AudioClip audioClip)
        {
            Game.Audio.PlaySingleSound(audioClip);
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(_audioName))
            {
                Game.YooAsset.UnloadAsset(_audioName);
            }
        }
    }
}