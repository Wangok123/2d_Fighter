using UnityEngine;

namespace UnityCore.AnimationSystem
{
    /// <summary>
    /// Animator层级辅助类 - 管理动画层级
    /// Animator Layer Helper - Manages animation layers
    /// 
    /// 用法 / Usage:
    /// - 控制动画层级的权重
    /// - 查询层级状态
    /// - 管理多层动画混合
    /// </summary>
    public class AnimatorLayerHelper
    {
        private readonly Animator _animator;

        public AnimatorLayerHelper(Animator animator)
        {
            _animator = animator;
        }

        /// <summary>
        /// 设置层级权重
        /// Set layer weight
        /// </summary>
        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (layerIndex >= 0 && layerIndex < _animator.layerCount)
            {
                _animator.SetLayerWeight(layerIndex, Mathf.Clamp01(weight));
            }
        }

        /// <summary>
        /// 设置层级权重（通过名称）
        /// Set layer weight by name
        /// </summary>
        public void SetLayerWeight(string layerName, float weight)
        {
            int layerIndex = _animator.GetLayerIndex(layerName);
            if (layerIndex >= 0)
            {
                SetLayerWeight(layerIndex, weight);
            }
        }

        /// <summary>
        /// 获取层级权重
        /// Get layer weight
        /// </summary>
        public float GetLayerWeight(int layerIndex)
        {
            if (layerIndex >= 0 && layerIndex < _animator.layerCount)
            {
                return _animator.GetLayerWeight(layerIndex);
            }
            return 0f;
        }

        /// <summary>
        /// 获取层级权重（通过名称）
        /// Get layer weight by name
        /// </summary>
        public float GetLayerWeight(string layerName)
        {
            int layerIndex = _animator.GetLayerIndex(layerName);
            return layerIndex >= 0 ? GetLayerWeight(layerIndex) : 0f;
        }

        /// <summary>
        /// 平滑过渡层级权重
        /// Smooth transition of layer weight
        /// </summary>
        public void TransitionLayerWeight(int layerIndex, float targetWeight, float speed, float deltaTime)
        {
            if (layerIndex >= 0 && layerIndex < _animator.layerCount)
            {
                float currentWeight = _animator.GetLayerWeight(layerIndex);
                float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, speed * deltaTime);
                _animator.SetLayerWeight(layerIndex, newWeight);
            }
        }

        /// <summary>
        /// 淡入层级（从0到1）
        /// Fade in layer (from 0 to 1)
        /// </summary>
        public void FadeInLayer(int layerIndex, float speed, float deltaTime)
        {
            TransitionLayerWeight(layerIndex, 1f, speed, deltaTime);
        }

        /// <summary>
        /// 淡出层级（从当前值到0）
        /// Fade out layer (from current to 0)
        /// </summary>
        public void FadeOutLayer(int layerIndex, float speed, float deltaTime)
        {
            TransitionLayerWeight(layerIndex, 0f, speed, deltaTime);
        }

        /// <summary>
        /// 启用层级（设置权重为1）
        /// Enable layer (set weight to 1)
        /// </summary>
        public void EnableLayer(int layerIndex)
        {
            SetLayerWeight(layerIndex, 1f);
        }

        /// <summary>
        /// 禁用层级（设置权重为0）
        /// Disable layer (set weight to 0)
        /// </summary>
        public void DisableLayer(int layerIndex)
        {
            SetLayerWeight(layerIndex, 0f);
        }

        /// <summary>
        /// 检查层级是否激活
        /// Check if layer is active
        /// </summary>
        public bool IsLayerActive(int layerIndex, float threshold = 0.01f)
        {
            return GetLayerWeight(layerIndex) > threshold;
        }

        /// <summary>
        /// 获取层级数量
        /// Get layer count
        /// </summary>
        public int GetLayerCount()
        {
            return _animator.layerCount;
        }

        /// <summary>
        /// 获取层级名称
        /// Get layer name
        /// </summary>
        public string GetLayerName(int layerIndex)
        {
            if (layerIndex >= 0 && layerIndex < _animator.layerCount)
            {
                return _animator.GetLayerName(layerIndex);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取层级索引
        /// Get layer index
        /// </summary>
        public int GetLayerIndex(string layerName)
        {
            return _animator.GetLayerIndex(layerName);
        }
    }
}
