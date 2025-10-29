using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AnimationSystem
{
    /// <summary>
    /// 动画状态配置 - 用于在Inspector中配置动画状态
    /// Animation State Config - Used to configure animation states in the Inspector
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationStateConfig", menuName = "Game/Animation State Config")]
    public class AnimationStateConfig : ScriptableObject
    {
        [Tooltip("动画状态列表 / Animation state list")]
        public List<AnimationState> AnimationStates = new List<AnimationState>();

        [Tooltip("默认状态名称 / Default state name")]
        public string DefaultStateName = "Idle";

        /// <summary>
        /// 应用配置到AnimationStateManager
        /// Apply config to AnimationStateManager
        /// </summary>
        public void ApplyToManager(AnimationStateManager manager)
        {
            if (manager == null)
            {
                Debug.LogError("AnimationStateManager is null");
                return;
            }

            foreach (var state in AnimationStates)
            {
                bool isDefault = state.StateName == DefaultStateName;
                manager.RegisterState(state.StateName, state.Layer, state.CrossfadeDuration, isDefault);
            }
        }

        /// <summary>
        /// 从动画控制器自动生成配置
        /// Auto-generate config from animator controller
        /// </summary>
        public void AutoGenerateFromAnimator(RuntimeAnimatorController controller)
        {
            if (controller == null)
            {
                Debug.LogError("Animator controller is null");
                return;
            }

            AnimationStates.Clear();

            foreach (var clip in controller.animationClips)
            {
                if (clip != null && !ContainsState(clip.name))
                {
                    AnimationStates.Add(new AnimationState
                    {
                        StateName = clip.name,
                        Layer = 0,
                        CrossfadeDuration = 0.1f
                    });
                }
            }

            Debug.Log($"Auto-generated {AnimationStates.Count} animation states from controller");
        }

        private bool ContainsState(string stateName)
        {
            foreach (var state in AnimationStates)
            {
                if (state.StateName == stateName)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 动画状态定义
    /// Animation state definition
    /// </summary>
    [Serializable]
    public class AnimationState
    {
        [Tooltip("状态名称（需与Animator中的State名称一致）/ State name (must match State name in Animator)")]
        public string StateName;

        [Tooltip("动画层级 / Animation layer")]
        public int Layer = 0;

        [Tooltip("过渡时间（秒）/ Crossfade duration (seconds)")]
        [Range(0f, 1f)]
        public float CrossfadeDuration = 0.1f;

        [Tooltip("备注说明 / Description")]
        public string Description;
    }
}
