using Quantum;
using UnityEngine;

namespace QuantumUser.View
{
    /// <summary>
    /// 动画视图组件 - 将Quantum动画状态同步到Unity Animator
    /// Animation View Component - Syncs Quantum animation state to Unity Animator
    /// 
    /// 这个组件运行在Unity层，负责表现
    /// This component runs on Unity layer, responsible for presentation
    /// </summary>
    public class QuantumAnimationView : QuantumEntityViewComponent
    {
        [Header("References")]
        [SerializeField] private Animator _animator;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;

        // 缓存
        private int _lastStateHash = 0;
        private AnimatorStateInfo _currentAnimatorState;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        public override void OnUpdateView()
        {
            if (_animator == null || !VerifiedFrame.Unsafe.TryGetPointer<AnimationState>(EntityRef, out var animState))
                return;

            // 检查动画状态是否改变
            if (animState->CurrentStateHash != _lastStateHash)
            {
                // 播放新动画
                PlayAnimationFromHash(animState->CurrentStateHash, animState->TransitionDuration.AsFloat);
                _lastStateHash = animState->CurrentStateHash;

                if (_showDebugInfo)
                {
                    Debug.Log($"[QuantumAnimationView] Playing animation hash: {animState->CurrentStateHash}, Priority: {animState->Priority}");
                }
            }

            // 更新Animator状态信息（用于同步回Quantum）
            UpdateAnimatorState();
        }

        /// <summary>
        /// 从哈希值播放动画
        /// Play animation from hash
        /// </summary>
        private void PlayAnimationFromHash(int stateHash, float crossfadeDuration)
        {
            if (stateHash == 0)
                return;

            if (crossfadeDuration > 0)
            {
                _animator.CrossFade(stateHash, crossfadeDuration, 0);
            }
            else
            {
                _animator.Play(stateHash, 0);
            }
        }

        /// <summary>
        /// 更新Animator状态信息
        /// Update animator state info
        /// </summary>
        private void UpdateAnimatorState()
        {
            _currentAnimatorState = _animator.GetCurrentAnimatorStateInfo(0);
        }

        /// <summary>
        /// 获取当前动画的归一化时间
        /// Get current animation normalized time
        /// </summary>
        public float GetNormalizedTime()
        {
            return _currentAnimatorState.normalizedTime;
        }

        /// <summary>
        /// 检查动画是否完成
        /// Check if animation is finished
        /// </summary>
        public bool IsAnimationFinished()
        {
            return _currentAnimatorState.normalizedTime >= 1.0f && !_animator.IsInTransition(0);
        }

        #region Debug

        private void OnGUI()
        {
            if (!_showDebugInfo || VerifiedFrame == null)
                return;

            if (!VerifiedFrame.Unsafe.TryGetPointer<AnimationState>(EntityRef, out var animState))
                return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
            
            if (screenPos.z > 0)
            {
                GUI.Label(new Rect(screenPos.x - 100, Screen.height - screenPos.y, 200, 80),
                    $"Animation View\n" +
                    $"State Hash: {animState->CurrentStateHash}\n" +
                    $"Priority: {animState->Priority}\n" +
                    $"Normalized Time: {_currentAnimatorState.normalizedTime:F2}\n" +
                    $"Transitioning: {animState->IsTransitioning}");
            }
        }

        #endregion
    }
}
