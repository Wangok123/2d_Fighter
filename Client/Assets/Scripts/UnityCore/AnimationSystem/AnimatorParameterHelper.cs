using UnityEngine;
using System.Collections.Generic;

namespace UnityCore.AnimationSystem
{
    /// <summary>
    /// Animator参数辅助类 - 提供类型安全的参数管理
    /// Animator Parameter Helper - Provides type-safe parameter management
    /// 
    /// 用法 / Usage:
    /// - 管理Animator参数的读写
    /// - 缓存参数Hash值以提高性能
    /// - 提供类型安全的参数访问
    /// </summary>
    public class AnimatorParameterHelper
    {
        private readonly Animator _animator;
        private readonly Dictionary<string, int> _parameterHashes;
        private readonly Dictionary<string, AnimatorControllerParameterType> _parameterTypes;

        public AnimatorParameterHelper(Animator animator)
        {
            _animator = animator;
            _parameterHashes = new Dictionary<string, int>();
            _parameterTypes = new Dictionary<string, AnimatorControllerParameterType>();
            CacheParameters();
        }

        /// <summary>
        /// 缓存所有参数信息以提高性能
        /// Cache all parameter information for better performance
        /// </summary>
        private void CacheParameters()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return;

            foreach (var param in _animator.parameters)
            {
                _parameterHashes[param.name] = param.nameHash;
                _parameterTypes[param.name] = param.type;
            }
        }

        /// <summary>
        /// 获取参数Hash值（使用缓存）
        /// Get parameter hash (using cache)
        /// </summary>
        private int GetParameterHash(string name)
        {
            if (_parameterHashes.TryGetValue(name, out int hash))
                return hash;
            
            hash = Animator.StringToHash(name);
            _parameterHashes[name] = hash;
            return hash;
        }

        #region Set Parameters / 设置参数

        /// <summary>
        /// 设置Bool参数
        /// Set Bool parameter
        /// </summary>
        public void SetBool(string name, bool value)
        {
            _animator.SetBool(GetParameterHash(name), value);
        }

        /// <summary>
        /// 设置Int参数
        /// Set Int parameter
        /// </summary>
        public void SetInt(string name, int value)
        {
            _animator.SetInteger(GetParameterHash(name), value);
        }

        /// <summary>
        /// 设置Float参数
        /// Set Float parameter
        /// </summary>
        public void SetFloat(string name, float value)
        {
            _animator.SetFloat(GetParameterHash(name), value);
        }

        /// <summary>
        /// 设置Float参数（带平滑过渡）
        /// Set Float parameter with smooth damping
        /// </summary>
        public void SetFloat(string name, float value, float dampTime, float deltaTime)
        {
            _animator.SetFloat(GetParameterHash(name), value, dampTime, deltaTime);
        }

        /// <summary>
        /// 触发Trigger参数
        /// Trigger a Trigger parameter
        /// </summary>
        public void SetTrigger(string name)
        {
            _animator.SetTrigger(GetParameterHash(name));
        }

        /// <summary>
        /// 重置Trigger参数
        /// Reset a Trigger parameter
        /// </summary>
        public void ResetTrigger(string name)
        {
            _animator.ResetTrigger(GetParameterHash(name));
        }

        #endregion

        #region Get Parameters / 获取参数

        /// <summary>
        /// 获取Bool参数值
        /// Get Bool parameter value
        /// </summary>
        public bool GetBool(string name)
        {
            return _animator.GetBool(GetParameterHash(name));
        }

        /// <summary>
        /// 获取Int参数值
        /// Get Int parameter value
        /// </summary>
        public int GetInt(string name)
        {
            return _animator.GetInteger(GetParameterHash(name));
        }

        /// <summary>
        /// 获取Float参数值
        /// Get Float parameter value
        /// </summary>
        public float GetFloat(string name)
        {
            return _animator.GetFloat(GetParameterHash(name));
        }

        #endregion

        #region Parameter Existence Checks / 参数存在性检查

        /// <summary>
        /// 检查参数是否存在
        /// Check if parameter exists
        /// </summary>
        public bool HasParameter(string name)
        {
            return _parameterHashes.ContainsKey(name);
        }

        /// <summary>
        /// 检查参数类型
        /// Check parameter type
        /// </summary>
        public bool HasParameter(string name, AnimatorControllerParameterType type)
        {
            return _parameterTypes.TryGetValue(name, out var paramType) && paramType == type;
        }

        #endregion

        #region Batch Operations / 批量操作

        /// <summary>
        /// 批量设置Bool参数
        /// Batch set Bool parameters
        /// </summary>
        public void SetBools(Dictionary<string, bool> values)
        {
            foreach (var kvp in values)
            {
                SetBool(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 批量设置Int参数
        /// Batch set Int parameters
        /// </summary>
        public void SetInts(Dictionary<string, int> values)
        {
            foreach (var kvp in values)
            {
                SetInt(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 批量设置Float参数
        /// Batch set Float parameters
        /// </summary>
        public void SetFloats(Dictionary<string, float> values)
        {
            foreach (var kvp in values)
            {
                SetFloat(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 重置所有Bool参数为false
        /// Reset all Bool parameters to false
        /// </summary>
        public void ResetAllBools()
        {
            foreach (var kvp in _parameterTypes)
            {
                if (kvp.Value == AnimatorControllerParameterType.Bool)
                {
                    SetBool(kvp.Key, false);
                }
            }
        }

        /// <summary>
        /// 重置所有Trigger
        /// Reset all Triggers
        /// </summary>
        public void ResetAllTriggers()
        {
            foreach (var kvp in _parameterTypes)
            {
                if (kvp.Value == AnimatorControllerParameterType.Trigger)
                {
                    ResetTrigger(kvp.Key);
                }
            }
        }

        #endregion
    }
}
