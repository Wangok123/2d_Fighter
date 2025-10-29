using Core.AnimationSystem;
using UnityEngine;

namespace Quantum.QuantumView
{
    /// <summary>
    /// Warrior角色动画管理器示例
    /// Example animation manager for Warrior character
    /// 
    /// 使用说明 / Usage:
    /// 1. 将此组件添加到Warrior角色GameObject上
    /// 2. Animator会自动从同一GameObject获取
    /// 3. 在代码中调用 PlayAnimation() 方法播放动画
    /// 
    /// 优势 / Advantages:
    /// - 避免在Unity Animator中手动连接24个动画状态的复杂转换
    /// - 使用代码管理动画，更易维护和扩展
    /// - 支持动画过渡和即时播放
    /// - 易于添加新的动画状态
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class WarriorAnimationManager : MonoBehaviour
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

        private AnimationStateManager _stateManager;
        private Animator _animator;

        public AnimationStateManager StateManager => _stateManager;
        public string CurrentAnimation => _stateManager?.CurrentState ?? string.Empty;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            InitializeAnimationManager();
        }

        /// <summary>
        /// 初始化动画管理器并注册所有动画状态
        /// Initialize animation manager and register all animation states
        /// </summary>
        private void InitializeAnimationManager()
        {
            _stateManager = new AnimationStateManager(_animator);

            // 注册基础移动动画 / Register basic movement animations
            _stateManager.RegisterState(AnimationStates.Idle, layer: 0, crossfadeDuration: 0.1f, isDefault: true);
            _stateManager.RegisterState(AnimationStates.Run, layer: 0, crossfadeDuration: 0.15f);
            _stateManager.RegisterState(AnimationStates.Jump, layer: 0, crossfadeDuration: 0.1f);
            _stateManager.RegisterState(AnimationStates.JumpToFall, layer: 0, crossfadeDuration: 0.05f);
            _stateManager.RegisterState(AnimationStates.Fall, layer: 0, crossfadeDuration: 0.1f);

            // 注册攻击动画 / Register attack animations
            _stateManager.RegisterState(AnimationStates.Attack, layer: 0, crossfadeDuration: 0.05f);
            _stateManager.RegisterState(AnimationStates.DashAttack, layer: 0, crossfadeDuration: 0.05f);
            _stateManager.RegisterState(AnimationStates.DashAttackNoDust, layer: 0, crossfadeDuration: 0.05f);

            // 注册冲刺动画 / Register dash animations
            _stateManager.RegisterState(AnimationStates.Dash, layer: 0, crossfadeDuration: 0.05f);
            _stateManager.RegisterState(AnimationStates.DashNoDust, layer: 0, crossfadeDuration: 0.05f);

            // 注册下蹲和滑铲动画 / Register crouch and slide animations
            _stateManager.RegisterState(AnimationStates.Crouch, layer: 0, crossfadeDuration: 0.1f);
            _stateManager.RegisterState(AnimationStates.Slide, layer: 0, crossfadeDuration: 0.05f);

            // 注册受伤和死亡动画 / Register hurt and death animations
            _stateManager.RegisterState(AnimationStates.Hurt, layer: 0, crossfadeDuration: 0.05f);
            _stateManager.RegisterState(AnimationStates.HurtNoEffect, layer: 0, crossfadeDuration: 0.05f);
            _stateManager.RegisterState(AnimationStates.Death, layer: 0, crossfadeDuration: 0.05f);
            _stateManager.RegisterState(AnimationStates.DeathNoEffect, layer: 0, crossfadeDuration: 0.05f);

            // 注册墙壁和边缘动画 / Register wall and edge animations
            _stateManager.RegisterState(AnimationStates.WallSlide, layer: 0, crossfadeDuration: 0.1f);
            _stateManager.RegisterState(AnimationStates.WallSlideNoDust, layer: 0, crossfadeDuration: 0.1f);
            _stateManager.RegisterState(AnimationStates.EdgeGrab, layer: 0, crossfadeDuration: 0.1f);
            _stateManager.RegisterState(AnimationStates.EdgeIdle, layer: 0, crossfadeDuration: 0.1f);

            // 注册梯子动画 / Register ladder animation
            _stateManager.RegisterState(AnimationStates.Ladder, layer: 0, crossfadeDuration: 0.15f);

            Debug.Log("Warrior Animation Manager initialized with all states");
        }

        // 便捷方法 / Convenience methods

        public void PlayIdle() => _stateManager.PlayAnimation(AnimationStates.Idle);
        public void PlayRun() => _stateManager.PlayAnimation(AnimationStates.Run);
        public void PlayJump() => _stateManager.PlayAnimation(AnimationStates.Jump);
        public void PlayFall() => _stateManager.PlayAnimation(AnimationStates.Fall);
        public void PlayAttack() => _stateManager.PlayAnimation(AnimationStates.Attack);
        public void PlayDash() => _stateManager.PlayAnimation(AnimationStates.Dash);
        public void PlayCrouch() => _stateManager.PlayAnimation(AnimationStates.Crouch);
        public void PlaySlide() => _stateManager.PlayAnimation(AnimationStates.Slide);
        public void PlayHurt() => _stateManager.PlayAnimation(AnimationStates.Hurt);
        public void PlayDeath() => _stateManager.PlayAnimation(AnimationStates.Death);

        /// <summary>
        /// 播放指定动画
        /// Play specified animation
        /// </summary>
        public void PlayAnimation(string stateName, bool forceReplay = false)
        {
            _stateManager?.PlayAnimation(stateName, forceReplay);
        }

        /// <summary>
        /// 检查当前是否在播放某个动画
        /// Check if currently playing an animation
        /// </summary>
        public bool IsPlaying(string stateName)
        {
            return _stateManager?.IsPlaying(stateName) ?? false;
        }

        /// <summary>
        /// 检查当前动画是否播放完成
        /// Check if current animation is finished
        /// </summary>
        public bool IsCurrentAnimationFinished()
        {
            return _stateManager?.IsAnimationFinished() ?? false;
        }
    }
}
