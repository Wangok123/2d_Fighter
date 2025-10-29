using UnityEngine;
using LATLog;

namespace UnityCore.AnimationSystem
{
    /// <summary>
    /// 动画状态控制器组件 - 简化在Unity中使用AnimationStateManager
    /// Animation State Controller Component - Simplifies using AnimationStateManager in Unity
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationStateController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("动画状态配置 / Animation state config")]
        [SerializeField] private AnimationStateConfig _animationConfig;

        [Tooltip("是否在Start时自动初始化 / Auto initialize on Start")]
        [SerializeField] private bool _autoInitialize = true;

        [Tooltip("是否在Start时播放默认状态 / Play default state on Start")]
        [SerializeField] private bool _playDefaultOnStart = true;

        private Animator _animator;
        private AnimationStateManager _stateManager;

        public AnimationStateManager StateManager => _stateManager;
        public Animator Animator => _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (_autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 初始化动画状态管理器
        /// Initialize animation state manager
        /// </summary>
        public void Initialize()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_animator == null)
            {
                GameDebug.LogError("Animator component not found!");
                return;
            }

            _stateManager = new AnimationStateManager(_animator);

            if (_animationConfig != null)
            {
                _animationConfig.ApplyToManager(_stateManager);
                GameDebug.Log($"Animation state manager initialized with {_animationConfig.AnimationStates.Count} states");
            }
            else
            {
                GameDebug.LogWarning("No animation config assigned, creating empty state manager");
            }

            if (_playDefaultOnStart && _stateManager != null)
            {
                _stateManager.PlayDefaultState();
            }
        }

        /// <summary>
        /// 播放动画
        /// Play animation
        /// </summary>
        public void PlayAnimation(string stateName, bool forceReplay = false)
        {
            if (_stateManager == null)
            {
                GameDebug.LogError("State manager not initialized!");
                return;
            }

            _stateManager.PlayAnimation(stateName, forceReplay);
        }

        /// <summary>
        /// 直接播放动画（无过渡）
        /// Play animation immediately
        /// </summary>
        public void PlayAnimationImmediate(string stateName)
        {
            if (_stateManager == null)
            {
                GameDebug.LogError("State manager not initialized!");
                return;
            }

            _stateManager.PlayAnimationImmediate(stateName);
        }

        /// <summary>
        /// 播放默认状态
        /// Play default state
        /// </summary>
        public void PlayDefaultState()
        {
            if (_stateManager == null)
            {
                GameDebug.LogError("State manager not initialized!");
                return;
            }

            _stateManager.PlayDefaultState();
        }

        /// <summary>
        /// 检查动画是否正在播放
        /// Check if animation is playing
        /// </summary>
        public bool IsPlaying(string stateName)
        {
            if (_stateManager == null)
            {
                return false;
            }

            return _stateManager.IsPlaying(stateName);
        }

        /// <summary>
        /// 设置动画配置（运行时）
        /// Set animation config at runtime
        /// </summary>
        public void SetConfig(AnimationStateConfig config)
        {
            _animationConfig = config;
            if (_stateManager != null)
            {
                _stateManager.Clear();
                _animationConfig.ApplyToManager(_stateManager);
            }
        }
    }
}
