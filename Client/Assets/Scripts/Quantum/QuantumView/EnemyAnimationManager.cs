using UnityEngine;

namespace Quantum.QuantumView
{
    /// <summary>
    /// 敌人角色动画管理器示例
    /// Example enemy character animation manager
    /// 
    /// 这是一个示例，展示如何使用CharacterAnimationManager基类为不同角色创建动画管理器
    /// This is an example showing how to use CharacterAnimationManager base class for different characters
    /// 
    /// 使用步骤 / Usage Steps:
    /// 1. 继承CharacterAnimationManager
    /// 2. 定义角色特有的动画状态常量
    /// 3. 实现RegisterAnimationStates()方法
    /// 4. 添加角色特有的便捷方法（可选）
    /// </summary>
    public class EnemyAnimationManager : CharacterAnimationManager
    {
        // 敌人动画状态常量 / Enemy animation state constants
        public static class AnimationStates
        {
            public const string Idle = "Idle";
            public const string Walk = "Walk";
            public const string Run = "Run";
            public const string Attack1 = "Attack1";
            public const string Attack2 = "Attack2";
            public const string Hurt = "Hurt";
            public const string Death = "Death";
            public const string Patrol = "Patrol";
            public const string Alert = "Alert";
        }

        /// <summary>
        /// 注册敌人的所有动画状态
        /// Register all enemy animation states
        /// </summary>
        protected override void RegisterAnimationStates()
        {
            // 注册基础动画 / Register basic animations
            RegisterState(AnimationStates.Idle, isDefault: true);
            RegisterState(AnimationStates.Walk, crossfadeDuration: 0.2f);
            RegisterState(AnimationStates.Run, crossfadeDuration: 0.15f);
            
            // 注册攻击动画 / Register attack animations
            RegisterState(AnimationStates.Attack1, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.Attack2, crossfadeDuration: 0.05f);
            
            // 注册受伤和死亡 / Register hurt and death
            RegisterState(AnimationStates.Hurt, crossfadeDuration: 0.1f);
            RegisterState(AnimationStates.Death, crossfadeDuration: 0.1f);
            
            // 注册AI行为动画 / Register AI behavior animations
            RegisterState(AnimationStates.Patrol, crossfadeDuration: 0.2f);
            RegisterState(AnimationStates.Alert, crossfadeDuration: 0.1f);
        }

        /// <summary>
        /// 初始化完成后的回调
        /// Callback after initialization
        /// </summary>
        protected override void OnAnimationManagerInitialized()
        {
            Debug.Log("Enemy Animation Manager initialized");
        }

        #region 敌人特有的便捷方法 / Enemy-specific Convenience Methods

        public void PlayIdle() => PlayAnimation(AnimationStates.Idle);
        public void PlayWalk() => PlayAnimation(AnimationStates.Walk);
        public void PlayRun() => PlayAnimation(AnimationStates.Run);
        public void PlayAttack1() => PlayAnimation(AnimationStates.Attack1);
        public void PlayAttack2() => PlayAnimation(AnimationStates.Attack2);
        public void PlayHurt() => PlayAnimation(AnimationStates.Hurt);
        public void PlayDeath() => PlayAnimation(AnimationStates.Death);
        public void PlayPatrol() => PlayAnimation(AnimationStates.Patrol);
        public void PlayAlert() => PlayAnimation(AnimationStates.Alert);

        #endregion
    }
}
