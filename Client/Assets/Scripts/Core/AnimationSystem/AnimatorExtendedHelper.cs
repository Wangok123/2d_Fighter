using UnityEngine;

namespace Core.AnimationSystem
{
    /// <summary>
    /// Animator扩展辅助类 - 提供IK、状态查询等高级功能
    /// Animator Extended Helper - Provides IK, state queries, and advanced features
    /// 
    /// 用法 / Usage:
    /// - IK位置和旋转控制
    /// - 状态信息查询
    /// - 动画事件支持
    /// </summary>
    public class AnimatorExtendedHelper
    {
        private readonly Animator _animator;

        public AnimatorExtendedHelper(Animator animator)
        {
            _animator = animator;
        }

        #region State Info / 状态信息

        /// <summary>
        /// 获取当前状态信息
        /// Get current state info
        /// </summary>
        public AnimatorStateInfo GetCurrentStateInfo(int layer = 0)
        {
            return _animator.GetCurrentAnimatorStateInfo(layer);
        }

        /// <summary>
        /// 获取下一个状态信息
        /// Get next state info
        /// </summary>
        public AnimatorStateInfo GetNextStateInfo(int layer = 0)
        {
            return _animator.GetNextAnimatorStateInfo(layer);
        }

        /// <summary>
        /// 检查当前是否在某个状态
        /// Check if currently in a state
        /// </summary>
        public bool IsInState(string stateName, int layer = 0)
        {
            int stateHash = Animator.StringToHash(stateName);
            return GetCurrentStateInfo(layer).shortNameHash == stateHash;
        }

        /// <summary>
        /// 检查是否在过渡中
        /// Check if in transition
        /// </summary>
        public bool IsInTransition(int layer = 0)
        {
            return _animator.IsInTransition(layer);
        }

        /// <summary>
        /// 获取当前状态的归一化时间
        /// Get normalized time of current state
        /// </summary>
        public float GetNormalizedTime(int layer = 0)
        {
            return GetCurrentStateInfo(layer).normalizedTime;
        }

        /// <summary>
        /// 检查动画是否播放完成
        /// Check if animation is finished
        /// </summary>
        public bool IsAnimationFinished(int layer = 0, float threshold = 0.95f)
        {
            var stateInfo = GetCurrentStateInfo(layer);
            return stateInfo.normalizedTime >= threshold && !IsInTransition(layer);
        }

        #endregion

        #region IK Support / IK支持

        /// <summary>
        /// 设置IK位置权重
        /// Set IK position weight
        /// </summary>
        public void SetIKPositionWeight(AvatarIKGoal goal, float weight)
        {
            _animator.SetIKPositionWeight(goal, weight);
        }

        /// <summary>
        /// 设置IK旋转权重
        /// Set IK rotation weight
        /// </summary>
        public void SetIKRotationWeight(AvatarIKGoal goal, float weight)
        {
            _animator.SetIKRotationWeight(goal, weight);
        }

        /// <summary>
        /// 设置IK位置
        /// Set IK position
        /// </summary>
        public void SetIKPosition(AvatarIKGoal goal, Vector3 position)
        {
            _animator.SetIKPosition(goal, position);
        }

        /// <summary>
        /// 设置IK旋转
        /// Set IK rotation
        /// </summary>
        public void SetIKRotation(AvatarIKGoal goal, Quaternion rotation)
        {
            _animator.SetIKRotation(goal, rotation);
        }

        /// <summary>
        /// 获取IK位置
        /// Get IK position
        /// </summary>
        public Vector3 GetIKPosition(AvatarIKGoal goal)
        {
            return _animator.GetIKPosition(goal);
        }

        /// <summary>
        /// 获取IK旋转
        /// Get IK rotation
        /// </summary>
        public Quaternion GetIKRotation(AvatarIKGoal goal)
        {
            return _animator.GetIKRotation(goal);
        }

        /// <summary>
        /// 设置Look At权重
        /// Set Look At weight
        /// </summary>
        public void SetLookAtWeight(float weight)
        {
            _animator.SetLookAtWeight(weight);
        }

        /// <summary>
        /// 设置Look At权重（完整参数）
        /// Set Look At weight (full parameters)
        /// </summary>
        public void SetLookAtWeight(float weight, float bodyWeight = 0f, float headWeight = 1f, 
            float eyesWeight = 0f, float clampWeight = 0.5f)
        {
            _animator.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
        }

        /// <summary>
        /// 设置Look At位置
        /// Set Look At position
        /// </summary>
        public void SetLookAtPosition(Vector3 lookAtPosition)
        {
            _animator.SetLookAtPosition(lookAtPosition);
        }

        #endregion

        #region Body Parts / 身体部位

        /// <summary>
        /// 设置身体部位位置
        /// Set body part position
        /// </summary>
        public void SetBoneLocalRotation(HumanBodyBones bone, Quaternion rotation)
        {
            Transform boneTransform = _animator.GetBoneTransform(bone);
            if (boneTransform != null)
            {
                boneTransform.localRotation = rotation;
            }
        }

        /// <summary>
        /// 获取骨骼变换
        /// Get bone transform
        /// </summary>
        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            return _animator.GetBoneTransform(bone);
        }

        #endregion

        #region Speed Control / 速度控制

        /// <summary>
        /// 设置动画速度
        /// Set animation speed
        /// </summary>
        public void SetSpeed(float speed)
        {
            _animator.speed = speed;
        }

        /// <summary>
        /// 获取动画速度
        /// Get animation speed
        /// </summary>
        public float GetSpeed()
        {
            return _animator.speed;
        }

        /// <summary>
        /// 暂停动画
        /// Pause animation
        /// </summary>
        public void Pause()
        {
            SetSpeed(0f);
        }

        /// <summary>
        /// 恢复动画
        /// Resume animation
        /// </summary>
        public void Resume()
        {
            SetSpeed(1f);
        }

        #endregion

        #region Culling / 剔除

        /// <summary>
        /// 设置剔除模式
        /// Set culling mode
        /// </summary>
        public void SetCullingMode(AnimatorCullingMode mode)
        {
            _animator.cullingMode = mode;
        }

        /// <summary>
        /// 获取剔除模式
        /// Get culling mode
        /// </summary>
        public AnimatorCullingMode GetCullingMode()
        {
            return _animator.cullingMode;
        }

        #endregion

        #region Target Matching / 目标匹配

        /// <summary>
        /// 匹配目标（用于精确的位置和旋转匹配）
        /// Match target (for precise position and rotation matching)
        /// </summary>
        public void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, 
            AvatarTarget targetBodyPart, MatchTargetWeightMask weightMask, 
            float startNormalizedTime, float targetNormalizedTime)
        {
            _animator.MatchTarget(matchPosition, matchRotation, targetBodyPart, 
                weightMask, startNormalizedTime, targetNormalizedTime);
        }

        /// <summary>
        /// 中断目标匹配
        /// Interrupt target matching
        /// </summary>
        public void InterruptMatchTarget()
        {
            _animator.InterruptMatchTarget();
        }

        /// <summary>
        /// 检查是否在目标匹配中
        /// Check if in target matching
        /// </summary>
        public bool IsMatchingTarget()
        {
            return _animator.isMatchingTarget;
        }

        #endregion
    }
}
