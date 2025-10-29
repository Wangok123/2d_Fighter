using Core.AnimationSystem;
using UnityEngine;

namespace Quantum.QuantumView
{
    /// <summary>
    /// 角色动画管理器基类 - 提供可扩展的动画管理框架
    /// Base class for character animation managers - Provides extensible animation management framework
    /// 
    /// 使用说明 / Usage:
    /// 1. 继承此类创建特定角色的动画管理器
    /// 2. 重写 RegisterAnimationStates() 方法注册角色特定的动画状态
    /// 3. 可选：添加便捷方法用于播放常用动画
    /// 
    /// 优势 / Advantages:
    /// - 统一的动画管理接口
    /// - 减少重复代码
    /// - 易于为不同角色创建动画管理器
    /// - 支持自定义动画注册逻辑
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public abstract class CharacterAnimationManager : MonoBehaviour
    {
        protected AnimationStateManager _stateManager;
        protected Animator _animator;

        /// <summary>
        /// 获取动画状态管理器
        /// Get animation state manager
        /// </summary>
        public AnimationStateManager StateManager => _stateManager;

        /// <summary>
        /// 获取当前动画状态名称
        /// Get current animation state name
        /// </summary>
        public string CurrentAnimation => _stateManager?.CurrentState ?? string.Empty;

        /// <summary>
        /// 获取Animator组件
        /// Get Animator component
        /// </summary>
        public Animator Animator => _animator;

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
            InitializeAnimationManager();
        }

        /// <summary>
        /// 初始化动画管理器
        /// Initialize animation manager
        /// </summary>
        private void InitializeAnimationManager()
        {
            _stateManager = new AnimationStateManager(_animator);
            RegisterAnimationStates();
            OnAnimationManagerInitialized();
        }

        /// <summary>
        /// 注册动画状态 - 子类必须实现此方法
        /// Register animation states - Subclasses must implement this method
        /// </summary>
        protected abstract void RegisterAnimationStates();

        /// <summary>
        /// 动画管理器初始化完成后调用（可选重写）
        /// Called after animation manager is initialized (optional override)
        /// </summary>
        protected virtual void OnAnimationManagerInitialized()
        {
            // 子类可以重写此方法以执行额外的初始化逻辑
            // Subclasses can override this method to perform additional initialization
        }

        #region Common Animation Methods / 通用动画方法

        /// <summary>
        /// 播放指定动画
        /// Play specified animation
        /// </summary>
        public virtual void PlayAnimation(string stateName, bool forceReplay = false)
        {
            _stateManager?.PlayAnimation(stateName, forceReplay);
        }

        /// <summary>
        /// 立即播放动画（无过渡）
        /// Play animation immediately without transition
        /// </summary>
        public virtual void PlayAnimationImmediate(string stateName)
        {
            _stateManager?.PlayAnimationImmediate(stateName);
        }

        /// <summary>
        /// 播放默认动画状态
        /// Play default animation state
        /// </summary>
        public virtual void PlayDefaultState()
        {
            _stateManager?.PlayDefaultState();
        }

        /// <summary>
        /// 检查当前是否在播放某个动画
        /// Check if currently playing an animation
        /// </summary>
        public virtual bool IsPlaying(string stateName)
        {
            return _stateManager?.IsPlaying(stateName) ?? false;
        }

        /// <summary>
        /// 检查当前动画是否播放完成
        /// Check if current animation is finished
        /// </summary>
        public virtual bool IsCurrentAnimationFinished()
        {
            return _stateManager?.IsAnimationFinished() ?? false;
        }

        /// <summary>
        /// 设置动画播放速度
        /// Set animation playback speed
        /// </summary>
        public virtual void SetAnimationSpeed(float speed)
        {
            _stateManager?.SetSpeed(speed);
        }

        /// <summary>
        /// 获取当前动画的归一化时间
        /// Get normalized time of current animation
        /// </summary>
        public virtual float GetAnimationNormalizedTime(int layer = 0)
        {
            return _stateManager?.GetNormalizedTime(layer) ?? 0f;
        }

        #endregion

        #region Helper Methods / 辅助方法

        /// <summary>
        /// 批量注册动画状态（使用默认参数）
        /// Register multiple animation states with default parameters
        /// </summary>
        protected void RegisterStates(params string[] stateNames)
        {
            foreach (var stateName in stateNames)
            {
                _stateManager.RegisterState(stateName);
            }
        }

        /// <summary>
        /// 注册单个动画状态
        /// Register a single animation state
        /// </summary>
        protected void RegisterState(string stateName, int layer = 0, float crossfadeDuration = 0.1f, bool isDefault = false)
        {
            _stateManager.RegisterState(stateName, layer, crossfadeDuration, isDefault);
        }

        #endregion
    }
}
