using System;
using System.Collections.Generic;
using UnityEngine;
using LATLog;

namespace Core.AnimationSystem
{
    /// <summary>
    /// 动画状态管理器 - 用于管理大量动画状态，避免手动连接复杂的Animator状态机
    /// Animation State Manager - Used to manage many animation states, avoiding manual connection of complex Animator state machines
    /// </summary>
    public class AnimationStateManager
    {
        private readonly Animator _animator;
        private readonly Dictionary<string, AnimationStateInfo> _animations;
        private string _currentState;
        private string _defaultState;
        private bool _isTransitioning;

        public string CurrentState => _currentState;
        public Animator Animator => _animator;

        public AnimationStateManager(Animator animator)
        {
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));
            _animations = new Dictionary<string, AnimationStateInfo>(50);
            _currentState = string.Empty;
            _defaultState = string.Empty;
            _isTransitioning = false;
        }

        /// <summary>
        /// 注册动画状态
        /// Register an animation state
        /// </summary>
        /// <param name="stateName">状态名称（与Animator中的State名称一致）</param>
        /// <param name="layer">动画层级，默认为0</param>
        /// <param name="crossfadeDuration">过渡时间，默认0.1秒</param>
        /// <param name="isDefault">是否为默认状态</param>
        public void RegisterState(string stateName, int layer = 0, float crossfadeDuration = 0.1f, bool isDefault = false)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                GameDebug.LogError("Animation state name cannot be null or empty");
                return;
            }

            if (_animations.ContainsKey(stateName))
            {
                GameDebug.LogWarning($"Animation state already registered: {stateName}");
                return;
            }

            var stateInfo = new AnimationStateInfo
            {
                StateName = stateName,
                Layer = layer,
                CrossfadeDuration = crossfadeDuration,
                StateHash = Animator.StringToHash(stateName)
            };

            _animations.Add(stateName, stateInfo);

            if (isDefault)
            {
                _defaultState = stateName;
            }
        }

        /// <summary>
        /// 批量注册动画状态
        /// Register multiple animation states at once
        /// </summary>
        public void RegisterStates(params string[] stateNames)
        {
            foreach (var stateName in stateNames)
            {
                RegisterState(stateName);
            }
        }

        /// <summary>
        /// 播放动画
        /// Play an animation
        /// </summary>
        /// <param name="stateName">状态名称</param>
        /// <param name="forceReplay">是否强制重新播放（即使当前已经是这个动画）</param>
        public void PlayAnimation(string stateName, bool forceReplay = false)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                GameDebug.LogError("Animation state name cannot be null or empty");
                return;
            }

            // 如果已经在播放相同的动画且不强制重播，则忽略
            if (_currentState == stateName && !forceReplay)
            {
                return;
            }

            if (!_animations.TryGetValue(stateName, out var stateInfo))
            {
                GameDebug.LogError($"Animation state not registered: {stateName}");
                return;
            }

            // 使用CrossFade实现平滑过渡
            _animator.CrossFade(stateInfo.StateHash, stateInfo.CrossfadeDuration, stateInfo.Layer);
            _currentState = stateName;
        }

        /// <summary>
        /// 直接播放动画（无过渡）
        /// Play animation immediately without transition
        /// </summary>
        public void PlayAnimationImmediate(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                GameDebug.LogError("Animation state name cannot be null or empty");
                return;
            }

            if (!_animations.TryGetValue(stateName, out var stateInfo))
            {
                GameDebug.LogError($"Animation state not registered: {stateName}");
                return;
            }

            _animator.Play(stateInfo.StateHash, stateInfo.Layer);
            _currentState = stateName;
        }

        /// <summary>
        /// 返回默认状态
        /// Return to default state
        /// </summary>
        public void PlayDefaultState()
        {
            if (string.IsNullOrEmpty(_defaultState))
            {
                GameDebug.LogWarning("No default state set");
                return;
            }

            PlayAnimation(_defaultState);
        }

        /// <summary>
        /// 设置动画速度
        /// Set animation speed
        /// </summary>
        public void SetSpeed(float speed)
        {
            _animator.speed = speed;
        }

        /// <summary>
        /// 检查动画是否正在播放
        /// Check if an animation is currently playing
        /// </summary>
        public bool IsPlaying(string stateName)
        {
            return _currentState == stateName;
        }

        /// <summary>
        /// 获取当前动画的归一化时间 (0-1)
        /// Get normalized time of current animation (0-1)
        /// </summary>
        public float GetNormalizedTime(int layer = 0)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.normalizedTime;
        }

        /// <summary>
        /// 检查当前动画是否播放完成
        /// Check if current animation is finished
        /// </summary>
        public bool IsAnimationFinished(int layer = 0)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(layer);
        }

        /// <summary>
        /// 设置Animator参数
        /// Set Animator parameter
        /// </summary>
        public void SetBool(string name, bool value)
        {
            _animator.SetBool(name, value);
        }

        public void SetInt(string name, int value)
        {
            _animator.SetInteger(name, value);
        }

        public void SetFloat(string name, float value)
        {
            _animator.SetFloat(name, value);
        }

        public void SetTrigger(string name)
        {
            _animator.SetTrigger(name);
        }

        /// <summary>
        /// 清除所有已注册的状态
        /// Clear all registered states
        /// </summary>
        public void Clear()
        {
            _animations.Clear();
            _currentState = string.Empty;
            _defaultState = string.Empty;
        }
    }

    /// <summary>
    /// 动画状态信息
    /// Animation state information
    /// </summary>
    internal class AnimationStateInfo
    {
        public string StateName;
        public int Layer;
        public float CrossfadeDuration;
        public int StateHash;
    }
}
