using Quantum.QuantumView;
using UnityEngine;

namespace UnityCore.AnimationSystem
{
    /// <summary>
    /// Warrior角色动画管理器
    /// Warrior character animation manager
    /// 
    /// 使用说明 / Usage:
    /// 1. 将此组件添加到Warrior角色GameObject上
    /// 2. Animator会自动从同一GameObject获取
    /// 3. 在代码中调用 PlayAnimation() 方法或便捷方法播放动画
    /// 
    /// 扩展说明 / Extension Guide:
    /// - 此类继承自CharacterAnimationManager基类
    /// - 可参考此类为其他角色创建动画管理器
    /// - 只需定义动画状态常量并实现RegisterAnimationStates()方法
    /// </summary>
    public class WarriorAnimationManager : CharacterAnimationManager
    {
        // Warrior动画状态常量 / Warrior animation state constants
        public static class AnimationStates
        {
            public const string Idle = "Idle";
            public const string Run = "Run";
            public const string Jump = "jump";
            public const string JumpToFall = "JumptoFall";
            public const string Fall = "Fall";
            public const string Attack = "Attack";
            public const string DashAttack = "Dash-Attack";
            public const string DashAttackNoDust = "Dash-Attack NoDust";
            public const string Dash = "Dash";
            public const string DashNoDust = "Dash NoDust";
            public const string Crouch = "Croush";
            public const string Slide = "Slide";
            public const string Hurt = "Hurt";
            public const string HurtNoEffect = "Hurt NoEffect";
            public const string Death = "Death";
            public const string DeathNoEffect = "Death NoEffect";
            public const string WallSlide = "Wall-Slide";
            public const string WallSlideNoDust = "WallSlide NoDust";
            public const string EdgeGrab = "Edge-Grab";
            public const string EdgeIdle = "Edge-Idle";
            public const string Ladder = "Ladder";
        }

        /// <summary>
        /// 注册Warrior的所有动画状态
        /// Register all Warrior animation states
        /// </summary>
        protected override void RegisterAnimationStates()
        {

            // 注册基础移动动画 / Register basic movement animations
            RegisterState(AnimationStates.Idle, layer: 0, crossfadeDuration: 0.1f, isDefault: true);
            RegisterState(AnimationStates.Run, layer: 0, crossfadeDuration: 0.15f);
            RegisterState(AnimationStates.Jump, layer: 0, crossfadeDuration: 0.1f);
            RegisterState(AnimationStates.JumpToFall, layer: 0, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.Fall, layer: 0, crossfadeDuration: 0.1f);

            // 注册攻击动画 / Register attack animations
            RegisterState(AnimationStates.Attack, layer: 0, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.DashAttack, layer: 0, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.DashAttackNoDust, layer: 0, crossfadeDuration: 0.05f);

            // 注册冲刺动画 / Register dash animations
            RegisterState(AnimationStates.Dash, layer: 0, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.DashNoDust, layer: 0, crossfadeDuration: 0.05f);

            // 注册下蹲和滑铲动画 / Register crouch and slide animations
            RegisterState(AnimationStates.Crouch, layer: 0, crossfadeDuration: 0.1f);
            RegisterState(AnimationStates.Slide, layer: 0, crossfadeDuration: 0.05f);

            // 注册受伤和死亡动画 / Register hurt and death animations
            RegisterState(AnimationStates.Hurt, layer: 0, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.HurtNoEffect, layer: 0, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.Death, layer: 0, crossfadeDuration: 0.05f);
            RegisterState(AnimationStates.DeathNoEffect, layer: 0, crossfadeDuration: 0.05f);

            // 注册墙壁和边缘动画 / Register wall and edge animations
            RegisterState(AnimationStates.WallSlide, layer: 0, crossfadeDuration: 0.1f);
            RegisterState(AnimationStates.WallSlideNoDust, layer: 0, crossfadeDuration: 0.1f);
            RegisterState(AnimationStates.EdgeGrab, layer: 0, crossfadeDuration: 0.1f);
            RegisterState(AnimationStates.EdgeIdle, layer: 0, crossfadeDuration: 0.1f);

            // 注册梯子动画 / Register ladder animation
            RegisterState(AnimationStates.Ladder, layer: 0, crossfadeDuration: 0.15f);
        }

        /// <summary>
        /// 初始化完成后的回调
        /// Callback after initialization
        /// </summary>
        protected override void OnAnimationManagerInitialized()
        {
            Debug.Log("Warrior Animation Manager initialized with all states");
        }

        #region Warrior特有的便捷方法 / Warrior-specific Convenience Methods

        public void PlayIdle() => PlayAnimation(AnimationStates.Idle);
        public void PlayRun() => PlayAnimation(AnimationStates.Run);
        public void PlayJump() => PlayAnimation(AnimationStates.Jump);
        public void PlayFall() => PlayAnimation(AnimationStates.Fall);
        public void PlayAttack() => PlayAnimation(AnimationStates.Attack);
        public void PlayDash() => PlayAnimation(AnimationStates.Dash);
        public void PlayCrouch() => PlayAnimation(AnimationStates.Crouch);
        public void PlaySlide() => PlayAnimation(AnimationStates.Slide);
        public void PlayHurt() => PlayAnimation(AnimationStates.Hurt);
        public void PlayDeath() => PlayAnimation(AnimationStates.Death);

        #endregion
    }
}
