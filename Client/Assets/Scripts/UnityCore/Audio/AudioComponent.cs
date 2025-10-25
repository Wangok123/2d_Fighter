using System.Collections.Generic;
using DG.Tweening;
using UnityCore.Base;
using UnityEditor;
using UnityEngine;

namespace UnityCore.Audio
{
    public class AudioComponent : LatComponent
    {
        [Header("是否静音初始值")] [SerializeField] private bool muteDefault = false;

        [Header("背景音乐优先级初始值")] [SerializeField]
        private int backgroundPriorityDefault = 0;

        [Header("单通道音效优先级初始值")] [SerializeField]
        private int singlePriorityDefault = 10;

        [Header("多通道音效优先级初始值")] [SerializeField]
        private int multiplePriorityDefault = 20;

        [Header("世界音效优先级初始值")] [SerializeField]
        private int worldPriorityDefault = 30;

        [Header("背景音乐音量初始值")] [SerializeField] private float backgroundVolumeDefault = 0.6f;

        [Header("单通道音效音量初始值")] [SerializeField]
        private float singleVolumeDefault = 1;

        [Header("多通道音效音量初始值")] [SerializeField]
        private float multipleVolumeDefault = 1;

        [Header("世界音效音量初始值")] [SerializeField] private float worldVolumeDefault = 1;

        /// <summary>
        /// 单通道音效播放结束事件
        /// </summary>
        public event AudioAction SingleSoundEndOfPlayEvent;

        private AudioSource _backgroundAudio;

        private AudioSource BackgroundAudio
        {
            get
            {
                if (_backgroundAudio == null)
                {
                    _backgroundAudio = CreateAudioSource("BackgroundAudio", BackgroundPriority, BackgroundVolume, 1, 0);
                }

                return _backgroundAudio;
            }
        }

        private AudioSource _singleAudio;

        private AudioSource SingleAudio
        {
            get
            {
                if (_singleAudio == null)
                {
                    _singleAudio = CreateAudioSource("SingleAudio", SinglePriority, SingleVolume, 1, 0);
                }

                return _singleAudio;
            }
        }

        private List<AudioSource> _multipleAudios = new List<AudioSource>();
        private Dictionary<GameObject, AudioSource> _worldAudios = new Dictionary<GameObject, AudioSource>();


        private bool _singleSoundPlayDetector = false;
        private bool _isMute = false;
        private int _backgroundPriority = 0;
        private int _singlePriority = 10;
        private int _multiplePriority = 20;
        private int _worldPriority = 30;
        private float _backgroundVolume = 0.6f;
        private float _singleVolume = 1;
        private float _multipleVolume = 1;
        private float _worldVolume = 1;

        private string _currentBackgroundMusicName = string.Empty;

        protected override void Awake()
        {
            base.Awake();

            Mute = muteDefault;
            BackgroundPriority = backgroundPriorityDefault;
            SinglePriority = singlePriorityDefault;
            MultiplePriority = multiplePriorityDefault;
            WorldPriority = worldPriorityDefault;
            BackgroundVolume = backgroundVolumeDefault;
            SingleVolume = singleVolumeDefault;
            MultipleVolume = multipleVolumeDefault;
            WorldVolume = worldVolumeDefault;

            IsInit = true;
        }

        public void Update()
        {
            if (_singleSoundPlayDetector)
            {
                if (!SingleAudio.isPlaying)
                {
                    _singleSoundPlayDetector = false;
                    SingleSoundEndOfPlayEvent?.Invoke();
                }
            }
        }

        /// <summary>
        /// 静音
        /// </summary>
        public bool Mute
        {
            get => _isMute;
            set
            {
                if (_isMute != value)
                {
                    _isMute = value;
                    BackgroundAudio.mute = _isMute;
                    SingleAudio.mute = _isMute;
                    for (int i = 0; i < _multipleAudios.Count; i++)
                    {
                        _multipleAudios[i].mute = _isMute;
                    }

                    foreach (var audio in _worldAudios)
                    {
                        audio.Value.mute = _isMute;
                    }
                }
            }
        }

        /// <summary>
        /// 背景音乐优先级
        /// </summary>
        public int BackgroundPriority
        {
            get { return _backgroundPriority; }
            set
            {
                if (_backgroundPriority != value)
                {
                    _backgroundPriority = value;
                    BackgroundAudio.priority = _backgroundPriority;
                }
            }
        }

        /// <summary>
        /// 单通道音效优先级
        /// </summary>
        public int SinglePriority
        {
            get { return _singlePriority; }
            set
            {
                if (_singlePriority != value)
                {
                    _singlePriority = value;
                    SingleAudio.priority = _singlePriority;
                }
            }
        }

        /// <summary>
        /// 多通道音效优先级
        /// </summary>
        public int MultiplePriority
        {
            get { return _multiplePriority; }
            set
            {
                if (_multiplePriority != value)
                {
                    _multiplePriority = value;
                    for (int i = 0; i < _multipleAudios.Count; i++)
                    {
                        _multipleAudios[i].priority = _multiplePriority;
                    }
                }
            }
        }

        /// <summary>
        /// 世界音效优先级
        /// </summary>
        public int WorldPriority
        {
            get { return _worldPriority; }
            set
            {
                if (_worldPriority != value)
                {
                    _worldPriority = value;
                    foreach (var audio in _worldAudios)
                    {
                        audio.Value.priority = _worldPriority;
                    }
                }
            }
        }

        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float BackgroundVolume
        {
            get { return _backgroundVolume; }
            set
            {
                if (!Mathf.Approximately(_backgroundVolume, value))
                {
                    _backgroundVolume = value;
                    BackgroundAudio.volume = _backgroundVolume;
                }
            }
        }

        /// <summary>
        /// 单通道音效音量
        /// </summary>
        public float SingleVolume
        {
            get { return _singleVolume; }
            set
            {
                if (!Mathf.Approximately(_singleVolume, value))
                {
                    _singleVolume = value;
                    SingleAudio.volume = _singleVolume;
                }
            }
        }

        /// <summary>
        /// 多通道音效音量
        /// </summary>
        public float MultipleVolume
        {
            get => _multipleVolume;
            set
            {
                if (!Mathf.Approximately(_multipleVolume, value))
                {
                    _multipleVolume = value;
                    for (int i = 0; i < _multipleAudios.Count; i++)
                    {
                        _multipleAudios[i].volume = _multipleVolume;
                    }
                }
            }
        }

        /// <summary>
        /// 世界音效音量
        /// </summary>
        public float WorldVolume
        {
            get => _worldVolume;
            set
            {
                if (!Mathf.Approximately(_worldVolume, value))
                {
                    _worldVolume = value;
                    foreach (var audio in _worldAudios)
                    {
                        audio.Value.volume = _worldVolume;
                    }
                }
            }
        }

        public void PlayBackgroundMusic(string assetName, bool isLoop = true, float speed = 1)
        {
            assetName = assetName.ToLower();

            if (_currentBackgroundMusicName == assetName)
            {
                return;
            }

            if (string.IsNullOrEmpty(_currentBackgroundMusicName) == false)
            {
                Game.YooAsset.UnloadAsset(_currentBackgroundMusicName);
            }

            _currentBackgroundMusicName = assetName;
            Game.YooAsset.LoadAudioAsset(assetName, clip => { PlayBackgroundMusic(clip, isLoop, speed); }
            );
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlayBackgroundMusic(AudioClip clip, bool isLoop = true, float speed = 1)
        {
            if (BackgroundAudio.isPlaying)
            {
                BackgroundAudio.Stop();
            }

            BackgroundAudio.clip = clip;
            BackgroundAudio.loop = isLoop;
            BackgroundAudio.pitch = speed;
            BackgroundAudio.Play();
        }

        /// <summary>
        /// 暂停播放背景音乐
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void PauseBackgroundMusic(bool isGradual = true)
        {
            if (isGradual)
            {
                BackgroundAudio.DOFade(0, 2).OnComplete(() =>
                {
                    BackgroundAudio.volume = BackgroundVolume;
                    BackgroundAudio.Pause();
                });
            }
            else
            {
                BackgroundAudio.Pause();
            }
        }

        /// <summary>
        /// 恢复播放背景音乐
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void UnPauseBackgroundMusic(bool isGradual = true)
        {
            if (isGradual)
            {
                BackgroundAudio.UnPause();
                BackgroundAudio.volume = 0;
                BackgroundAudio.DOFade(BackgroundVolume, 2);
            }
            else
            {
                BackgroundAudio.UnPause();
            }
        }

        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (BackgroundAudio.isPlaying)
            {
                BackgroundAudio.Stop();
            }
        }

        public void PlaySingleSound(string assetName, bool isLoop = false, float speed = 1)
        {
            assetName = assetName.ToLower();

            Game.YooAsset.LoadAudioAsset(assetName, clip => { PlaySingleSound(clip, isLoop, speed); }
            );
        }

        /// <summary>
        /// 播放单通道音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlaySingleSound(AudioClip clip, bool isLoop = false, float speed = 1)
        {
            if (SingleAudio.isPlaying)
            {
                SingleAudio.Stop();
            }

            SingleAudio.clip = clip;
            SingleAudio.loop = isLoop;
            SingleAudio.pitch = speed;
            SingleAudio.Play();
            _singleSoundPlayDetector = true;
        }

        /// <summary>
        /// 暂停播放单通道音效
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void PauseSingleSound(bool isGradual = true)
        {
            if (isGradual)
            {
                SingleAudio.DOFade(0, 2).OnComplete(() =>
                {
                    SingleAudio.volume = SingleVolume;
                    SingleAudio.Pause();
                });
            }
            else
            {
                SingleAudio.Pause();
            }
        }

        /// <summary>
        /// 恢复播放单通道音效
        /// </summary>
        /// <param name="isGradual">是否渐进式</param>
        public void UnPauseSingleSound(bool isGradual = true)
        {
            if (isGradual)
            {
                SingleAudio.UnPause();
                SingleAudio.volume = 0;
                SingleAudio.DOFade(SingleVolume, 2);
            }
            else
            {
                SingleAudio.UnPause();
            }
        }

        /// <summary>
        /// 停止播放单通道音效
        /// </summary>
        public void StopSingleSound()
        {
            if (SingleAudio.isPlaying)
            {
                SingleAudio.Stop();
            }
        }

        public void PlayMultipleSound(string assetName, bool isLoop = false, float speed = 1)
        {
            assetName = assetName.ToLower();

            Game.YooAsset.LoadAudioAsset(assetName, clip => { PlayMultipleSound(clip, isLoop, speed); }
            );
        }

        /// <summary>
        /// 播放多通道音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlayMultipleSound(AudioClip clip, bool isLoop = false, float speed = 1)
        {
            AudioSource audioSource = ExtractIdleMultipleAudioSource();
            audioSource.clip = clip;
            audioSource.loop = isLoop;
            audioSource.pitch = speed;
            audioSource.Play();
        }

        /// <summary>
        /// 停止播放指定的多通道音效
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        public void StopMultipleSound(AudioClip clip)
        {
            for (int i = 0; i < _multipleAudios.Count; i++)
            {
                if (_multipleAudios[i].isPlaying)
                {
                    if (_multipleAudios[i].clip == clip)
                    {
                        _multipleAudios[i].Stop();
                    }
                }
            }
        }

        /// <summary>
        /// 停止播放所有多通道音效
        /// </summary>
        public void StopAllMultipleSound()
        {
            for (int i = 0; i < _multipleAudios.Count; i++)
            {
                if (_multipleAudios[i].isPlaying)
                {
                    _multipleAudios[i].Stop();
                }
            }
        }

        /// <summary>
        /// 销毁所有闲置中的多通道音效的音源
        /// </summary>
        public void ClearIdleMultipleAudioSource()
        {
            for (int i = 0; i < _multipleAudios.Count; i++)
            {
                if (!_multipleAudios[i].isPlaying)
                {
                    AudioSource audioSource = _multipleAudios[i];
                    _multipleAudios.RemoveAt(i);
                    i -= 1;
                    Destroy(audioSource.gameObject);
                }
            }
        }

        /// <summary>
        /// 播放世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="speed">播放速度</param>
        public void PlayWorldSound(GameObject attachTarget, AudioClip clip, bool isLoop = false, float speed = 1)
        {
            if (_worldAudios.TryGetValue(attachTarget, out var worldAudio))
            {
                if (worldAudio.isPlaying)
                {
                    worldAudio.Stop();
                }

                worldAudio.clip = clip;
                worldAudio.loop = isLoop;
                worldAudio.pitch = speed;
                worldAudio.Play();
            }
            else
            {
                AudioSource wAudio = AttachAudioSource(attachTarget, WorldPriority, WorldVolume, 1, 1);
                _worldAudios.Add(attachTarget, wAudio);
                wAudio.clip = clip;
                wAudio.loop = isLoop;
                wAudio.pitch = speed;
                wAudio.Play();
            }
        }

        /// <summary>
        /// 暂停播放指定的世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="isGradual">是否渐进式</param>
        public void PauseWorldSound(GameObject attachTarget, bool isGradual = true)
        {
            if (_worldAudios.TryGetValue(attachTarget, out var worldAudio))
            {
                if (isGradual)
                {
                    worldAudio.DOFade(0, 2).OnComplete(() =>
                    {
                        worldAudio.volume = WorldVolume;
                        worldAudio.Pause();
                    });
                }
                else
                {
                    worldAudio.Pause();
                }
            }
        }

        /// <summary>
        /// 恢复播放指定的世界音效
        /// </summary>
        /// <param name="attachTarget">附加目标</param>
        /// <param name="isGradual">是否渐进式</param>
        public void UnPauseWorldSound(GameObject attachTarget, bool isGradual = true)
        {
            if (_worldAudios.TryGetValue(attachTarget, out var worldAudio))
            {
                if (isGradual)
                {
                    worldAudio.UnPause();
                    worldAudio.volume = 0;
                    worldAudio.DOFade(WorldVolume, 2);
                }
                else
                {
                    worldAudio.UnPause();
                }
            }
        }

        /// <summary>
        /// 停止播放所有的世界音效
        /// </summary>
        public void StopAllWorldSound()
        {
            foreach (var pair in _worldAudios)
            {
                if (pair.Value.isPlaying)
                {
                    pair.Value.Stop();
                }
            }
        }

        /// <summary>
        /// 销毁所有闲置中的世界音效的音源
        /// </summary>
        public void ClearIdleWorldAudioSource()
        {
            HashSet<GameObject> removeSet = new HashSet<GameObject>();
            foreach (var pair in _worldAudios)
            {
                if (!pair.Value.isPlaying)
                {
                    removeSet.Add(pair.Key);
                    Destroy(pair.Value);
                }
            }

            foreach (var item in removeSet)
            {
                _worldAudios.Remove(item);
            }
        }

        //创建一个音源
        private AudioSource CreateAudioSource(string sourceName, int priority, float volume, float speed,
            float spatialBlend)
        {
            sourceName = sourceName.ToLower();

            GameObject audioObj = new GameObject(sourceName);
            audioObj.transform.SetParent(transform);
            audioObj.transform.localPosition = Vector3.zero;
            audioObj.transform.localRotation = Quaternion.identity;
            audioObj.transform.localScale = Vector3.one;
            AudioSource audioComponent = audioObj.AddComponent<AudioSource>();
            audioComponent.playOnAwake = false;
            audioComponent.priority = priority;
            audioComponent.volume = volume;
            audioComponent.pitch = speed;
            audioComponent.spatialBlend = spatialBlend;
            audioComponent.mute = _isMute;
            return audioComponent;
        }

        //附加一个音源
        private AudioSource AttachAudioSource(GameObject target, int priority, float volume, float speed,
            float spatialBlend)
        {
            AudioSource audio = target.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.priority = priority;
            audio.volume = volume;
            audio.pitch = speed;
            audio.spatialBlend = spatialBlend;
            audio.mute = _isMute;
            return audio;
        }

        //提取闲置中的多通道音源
        private AudioSource ExtractIdleMultipleAudioSource()
        {
            foreach (var audioSource in _multipleAudios)
            {
                if (!audioSource.isPlaying)
                {
                    return audioSource;
                }
            }

            AudioSource source = CreateAudioSource("MultipleAudio", MultiplePriority, MultipleVolume, 1, 0);
            _multipleAudios.Add(source);
            return source;
        }
    }

    public delegate void AudioAction();
}