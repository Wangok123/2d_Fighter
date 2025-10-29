using System;
using System.Collections.Generic;
using UnityEngine;
using LATLog;

namespace UnityCore.AnimationSystem
{
    /// <summary>
    /// 动画状态管理器 - 用于管理大量动画状态，避免手动连接复杂的Animator状态机
    /// Animation State Manager - Used to manage many animation states, avoiding manual connection of complex Animator state machines
    /// 
    /// 性能优化说明 / Performance Optimization:
    /// - 使用int哈希值作为字典键，避免字符串比较开销
    /// - Uses int hash as dictionary key to avoid string comparison overhead
    /// - 字符串仅用于调试和错误日志
    /// - Strings only used for debugging and error logging
    /// </summary>
    public class AnimationStateManager
    {
        private readonly Animator _animator;
        private readonly Dictionary<int, AnimationStateInfo> _animations;
        private int _currentStateHash;
        private int _defaultStateHash;
        private bool _isTransitioning;

        // 辅助工具 / Helper Tools
        private AnimatorParameterHelper _parameterHelper;
        private AnimatorLayerHelper _layerHelper;
        private AnimatorExtendedHelper _extendedHelper;

        public string CurrentState => GetStateNameByHash(_currentStateHash);
        public Animator Animator => _animator;

        /// <summary>
        /// 参数辅助工具（延迟初始化）
        /// Parameter helper (lazy initialization)
        /// </summary>
        public AnimatorParameterHelper ParameterHelper
        {
            get
            {
                if (_parameterHelper == null)
                    _parameterHelper = new AnimatorParameterHelper(_animator);
                return _parameterHelper;
            }
        }

        /// <summary>
        /// 层级辅助工具（延迟初始化）
        /// Layer helper (lazy initialization)
        /// </summary>
        public AnimatorLayerHelper LayerHelper
        {
            get
            {
                if (_layerHelper == null)
                    _layerHelper = new AnimatorLayerHelper(_animator);
                return _layerHelper;
            }
        }

        /// <summary>
        /// 扩展辅助工具（延迟初始化）
        /// Extended helper (lazy initialization)
        /// </summary>
        public AnimatorExtendedHelper ExtendedHelper
        {
            get
            {
                if (_extendedHelper == null)
                    _extendedHelper = new AnimatorExtendedHelper(_animator);
                return _extendedHelper;
            }
        }

        public AnimationStateManager(Animator animator)
        {
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));
            _animations = new Dictionary<int, AnimationStateInfo>(50);
            _currentStateHash = 0;
            _defaultStateHash = 0;
            _isTransitioning = false;
        }

        /// <summary>
        /// 根据Hash获取状态名称（用于调试）
        /// Get state name by hash (for debugging)
        /// </summary>
        private string GetStateNameByHash(int hash)
        {
            if (hash == 0) return string.Empty;
            if (_animations.TryGetValue(hash, out var info))
            {
                return info.StateName;
            }
            return string.Empty;
        }

        /// <summary>
        /// 注册动画状态
        /// Register an animation state
        /// </summary>
        /// <param name="stateName">状态名称（与Animator中的State名称一致）</param>
        /// <param name="layer">动画层级，默认为0</param>
        /// <param name="crossfadeDuration">过渡时间，默认0.1秒</param>
        /// <param name="isDefault">是否为默认状态</param>
        /// <param name="priority">动画优先级，用于取消机制</param>
        /// <param name="cancelPolicy">取消策略</param>
        /// <param name="cancelWindows">取消窗口列表</param>
        public void RegisterState(
            string stateName, 
            int layer = 0, 
            float crossfadeDuration = 0.1f, 
            bool isDefault = false,
            AnimationPriority priority = AnimationPriority.Idle,
            AnimationCancelPolicy cancelPolicy = AnimationCancelPolicy.AlwaysCancellable,
            CancelWindow[] cancelWindows = null)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                GameDebug.LogError("Animation state name cannot be null or empty");
                return;
            }

            int stateHash = Animator.StringToHash(stateName);

            if (_animations.ContainsKey(stateHash))
            {
                GameDebug.LogWarning($"Animation state already registered: {stateName}");
                return;
            }

            var stateInfo = new AnimationStateInfo
            {
                StateName = stateName,
                Layer = layer,
                CrossfadeDuration = crossfadeDuration,
                StateHash = stateHash,
                Priority = priority,
                CancelPolicy = cancelPolicy,
                CancelWindows = cancelWindows
            };

            _animations.Add(stateHash, stateInfo);

            if (isDefault)
            {
                _defaultStateHash = stateHash;
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
        /// <param name="ignoreCancelRules">是否忽略取消规则（用于强制切换）</param>
        public void PlayAnimation(string stateName, bool forceReplay = false, bool ignoreCancelRules = false)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                GameDebug.LogError("Animation state name cannot be null or empty");
                return;
            }

            int stateHash = Animator.StringToHash(stateName);
            PlayAnimationByHash(stateHash, stateName, forceReplay, ignoreCancelRules);
        }

        /// <summary>
        /// 通过Hash播放动画（性能优化版本）
        /// Play animation by hash (performance optimized version)
        /// </summary>
        /// <param name="stateHash">状态Hash值</param>
        /// <param name="forceReplay">是否强制重新播放</param>
        /// <param name="ignoreCancelRules">是否忽略取消规则</param>
        private void PlayAnimationByHash(int stateHash, string stateNameForLog, bool forceReplay = false, bool ignoreCancelRules = false)
        {
            // 如果已经在播放相同的动画且不强制重播，则忽略
            if (_currentStateHash == stateHash && !forceReplay)
            {
                return;
            }

            if (!_animations.TryGetValue(stateHash, out var targetStateInfo))
            {
                GameDebug.LogError($"Animation state not registered: {stateNameForLog}");
                return;
            }

            // 检查是否可以取消当前动画 / Check if current animation can be cancelled
            if (!ignoreCancelRules && _currentStateHash != 0 && !CanCancelCurrentAnimation(targetStateInfo))
            {
                GameDebug.LogWarning($"Cannot cancel current animation to play: {stateNameForLog}");
                return;
            }

            // 使用CrossFade实现平滑过渡
            _animator.CrossFade(targetStateInfo.StateHash, targetStateInfo.CrossfadeDuration, targetStateInfo.Layer);
            _currentStateHash = stateHash;
        }

        /// <summary>
        /// 检查当前动画是否可以被取消
        /// Check if current animation can be cancelled
        /// </summary>
        private bool CanCancelCurrentAnimation(AnimationStateInfo targetState)
        {
            // 如果没有当前状态，直接返回true
            if (_currentStateHash == 0)
                return true;

            // 获取当前动画状态信息
            if (!_animations.TryGetValue(_currentStateHash, out var currentState))
                return true;

            // 1. 检查优先级 / Check priority
            if (targetState.Priority > currentState.Priority)
            {
                // 目标动画优先级更高，可以打断
                return true;
            }

            if (targetState.Priority < currentState.Priority)
            {
                // 目标动画优先级更低，不能打断
                return false;
            }

            // 2. 优先级相同，检查取消策略 / Same priority, check cancel policy
            switch (currentState.CancelPolicy)
            {
                case AnimationCancelPolicy.NonCancellable:
                    // 不可取消
                    return false;

                case AnimationCancelPolicy.AlwaysCancellable:
                    // 总是可以取消
                    return true;

                case AnimationCancelPolicy.OnlyByHigherPriority:
                    // 仅可被更高优先级打断（已经在优先级检查中处理）
                    return false;

                case AnimationCancelPolicy.CancellableOnEnd:
                    // 检查是否接近结束
                    float normalizedTime = GetNormalizedTime(currentState.Layer);
                    return normalizedTime >= 0.8f; // 80%以上可以取消

                case AnimationCancelPolicy.CancellableInWindow:
                    // 检查是否在取消窗口内
                    return IsInCancelWindow(currentState, targetState.StateName);

                default:
                    return true;
            }
        }

        /// <summary>
        /// 检查当前动画是否在取消窗口内
        /// Check if current animation is in cancel window
        /// </summary>
        private bool IsInCancelWindow(AnimationStateInfo currentState, string targetAnimationName)
        {
            if (currentState.CancelWindows == null || currentState.CancelWindows.Length == 0)
                return false;

            float normalizedTime = GetNormalizedTime(currentState.Layer);

            foreach (var window in currentState.CancelWindows)
            {
                if (window.IsInWindow(normalizedTime) && window.CanCancelTo(targetAnimationName))
                {
                    return true;
                }
            }

            return false;
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

            int stateHash = Animator.StringToHash(stateName);

            if (!_animations.TryGetValue(stateHash, out var stateInfo))
            {
                GameDebug.LogError($"Animation state not registered: {stateName}");
                return;
            }

            _animator.Play(stateInfo.StateHash, stateInfo.Layer);
            _currentStateHash = stateHash;
        }

        /// <summary>
        /// 返回默认状态
        /// Return to default state
        /// </summary>
        public void PlayDefaultState()
        {
            if (_defaultStateHash == 0)
            {
                GameDebug.LogWarning("No default state set");
                return;
            }

            if (!_animations.TryGetValue(_defaultStateHash, out var stateInfo))
            {
                GameDebug.LogError("Default state not found in registered animations");
                return;
            }

            _animator.CrossFade(_defaultStateHash, stateInfo.CrossfadeDuration, stateInfo.Layer);
            _currentStateHash = _defaultStateHash;
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
            int stateHash = Animator.StringToHash(stateName);
            return _currentStateHash == stateHash;
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
        /// 获取当前动画的优先级
        /// Get current animation priority
        /// </summary>
        public AnimationPriority GetCurrentPriority()
        {
            if (_currentStateHash == 0)
                return AnimationPriority.Idle;

            if (_animations.TryGetValue(_currentStateHash, out var stateInfo))
            {
                return stateInfo.Priority;
            }

            return AnimationPriority.Idle;
        }

        /// <summary>
        /// 获取指定动画的优先级
        /// Get priority of specified animation
        /// </summary>
        public AnimationPriority GetPriority(string stateName)
        {
            int stateHash = Animator.StringToHash(stateName);
            if (_animations.TryGetValue(stateHash, out var stateInfo))
            {
                return stateInfo.Priority;
            }

            return AnimationPriority.Idle;
        }

        /// <summary>
        /// 检查是否可以播放指定动画（不实际播放）
        /// Check if specified animation can be played (without actually playing)
        /// </summary>
        public bool CanPlayAnimation(string stateName)
        {
            int stateHash = Animator.StringToHash(stateName);
            if (!_animations.TryGetValue(stateHash, out var targetStateInfo))
            {
                return false;
            }

            if (_currentStateHash == 0)
                return true;

            return CanCancelCurrentAnimation(targetStateInfo);
        }

        /// <summary>
        /// 清除所有已注册的状态
        /// Clear all registered states
        /// </summary>
        public void Clear()
        {
            _animations.Clear();
            _currentStateHash = 0;
            _defaultStateHash = 0;
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
        
        // 取消机制相关字段 / Cancel mechanism related fields
        public AnimationPriority Priority = AnimationPriority.Idle;
        public AnimationCancelPolicy CancelPolicy = AnimationCancelPolicy.AlwaysCancellable;
        public CancelWindow[] CancelWindows = null;
    }
}
