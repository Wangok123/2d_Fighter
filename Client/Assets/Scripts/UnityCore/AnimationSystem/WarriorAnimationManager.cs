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
            // 基础状态 / Basic states
            public const string Idle = "Idle";
            public const string Run = "Run";
            public const string Jump = "jump";
            public const string JumpToFall = "JumptoFall";
            public const string Fall = "Fall";
            
            // 攻击状态 / Attack states
            public const string Attack = "Attack";
            public const string DashAttack = "Dash-Attack";
            public const string DashAttackNoDust = "Dash-Attack NoDust";
            
            // 投技状态 / Throw states
            public const string ThrowStart = "Throw_Start";
            public const string ThrowHold = "Throw_Hold";
            public const string ThrowExecute = "Throw_Execute";
            public const string ThrowRecovery = "Throw_Recovery";
            
            // 冲刺状态 / Dash states
            public const string Dash = "Dash";
            public const string DashNoDust = "Dash NoDust";
            
            // 下蹲和滑铲 / Crouch and slide
            public const string Crouch = "Croush";
            public const string Slide = "Slide";
            
            // 受击状态 / Hit states
            public const string Hurt = "Hurt";
            public const string HurtNoEffect = "Hurt NoEffect";
            public const string HurtGround = "Hurt_Ground";    // 地面受击
            public const string HurtAir = "Hurt_Air";          // 空中受击
            public const string Knockdown = "Knockdown";       // 击倒
            
            // 死亡状态 / Death states
            public const string Death = "Death";
            public const string DeathNoEffect = "Death NoEffect";
            
            // 墙壁和边缘 / Wall and edge
            public const string WallSlide = "Wall-Slide";
            public const string WallSlideNoDust = "WallSlide NoDust";
            public const string EdgeGrab = "Edge-Grab";
            public const string EdgeIdle = "Edge-Idle";
            
            // 其他 / Others
            public const string Ladder = "Ladder";
        }

        /// <summary>
        /// 注册Warrior的所有动画状态（带优先级和取消机制）
        /// Register all Warrior animation states (with priority and cancel mechanism)
        /// </summary>
        protected override void RegisterAnimationStates()
        {
            // 注册基础移动动画 / Register basic movement animations
            // 优先级：Idle (最低) < Movement
            RegisterState(AnimationStates.Idle, 
                layer: 0, 
                crossfadeDuration: 0.1f, 
                isDefault: true,
                priority: AnimationPriority.Idle,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            RegisterState(AnimationStates.Run, 
                layer: 0, 
                crossfadeDuration: 0.15f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            // 跳跃动画 - 中低优先级，可被攻击和受击打断
            RegisterState(AnimationStates.Jump, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Jump,
                cancelPolicy: AnimationCancelPolicy.CancellableInWindow,
                cancelWindows: new[] {
                    new CancelWindow(0.2f, 0.6f, new[] { AnimationStates.Attack, AnimationStates.ThrowStart })
                });

            RegisterState(AnimationStates.JumpToFall, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Jump,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            RegisterState(AnimationStates.Fall, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Jump,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            // 注册攻击动画 / Register attack animations
            // 攻击动画有取消窗口，可以连招
            RegisterState(AnimationStates.Attack, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Attack,
                cancelPolicy: AnimationCancelPolicy.CancellableInWindow,
                cancelWindows: new[] {
                    // 在动画40%-70%可以取消到另一个攻击或技能
                    new CancelWindow(0.4f, 0.7f, new[] { 
                        AnimationStates.Attack, 
                        AnimationStates.DashAttack,
                        AnimationStates.ThrowStart 
                    }),
                    // 在动画80%以后可以取消到任何动作
                    new CancelWindow(0.8f, 1.0f)
                });

            RegisterState(AnimationStates.DashAttack, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Skill,
                cancelPolicy: AnimationCancelPolicy.CancellableOnEnd);

            RegisterState(AnimationStates.DashAttackNoDust, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Skill,
                cancelPolicy: AnimationCancelPolicy.CancellableOnEnd);

            // 注册投技动画 / Register throw animations
            // 投技有高优先级，但一旦开始执行就不可取消
            RegisterState(AnimationStates.ThrowStart, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Throw,
                cancelPolicy: AnimationCancelPolicy.CancellableInWindow,
                cancelWindows: new[] {
                    // 只在起手的前20%可以被打断
                    new CancelWindow(0.0f, 0.2f)
                });

            RegisterState(AnimationStates.ThrowHold, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Throw,
                cancelPolicy: AnimationCancelPolicy.OnlyByHigherPriority);

            RegisterState(AnimationStates.ThrowExecute, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Throw,
                cancelPolicy: AnimationCancelPolicy.NonCancellable);

            RegisterState(AnimationStates.ThrowRecovery, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Attack,
                cancelPolicy: AnimationCancelPolicy.CancellableOnEnd);

            // 注册冲刺动画 / Register dash animations
            RegisterState(AnimationStates.Dash, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.CancellableInWindow,
                cancelWindows: new[] {
                    new CancelWindow(0.3f, 0.8f, new[] { AnimationStates.DashAttack })
                });

            RegisterState(AnimationStates.DashNoDust, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.CancellableInWindow,
                cancelWindows: new[] {
                    new CancelWindow(0.3f, 0.8f, new[] { AnimationStates.DashAttack })
                });

            // 注册下蹲和滑铲动画 / Register crouch and slide animations
            RegisterState(AnimationStates.Crouch, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            RegisterState(AnimationStates.Slide, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.CancellableOnEnd);

            // 注册受伤动画 / Register hurt animations
            // 受击动画有很高优先级，几乎可以打断任何动作
            RegisterState(AnimationStates.Hurt, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Hit,
                cancelPolicy: AnimationCancelPolicy.OnlyByHigherPriority);

            RegisterState(AnimationStates.HurtNoEffect, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Hit,
                cancelPolicy: AnimationCancelPolicy.OnlyByHigherPriority);

            // 地面受击和空中受击分开处理
            RegisterState(AnimationStates.HurtGround, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Hit,
                cancelPolicy: AnimationCancelPolicy.OnlyByHigherPriority);

            RegisterState(AnimationStates.HurtAir, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Hit,
                cancelPolicy: AnimationCancelPolicy.OnlyByHigherPriority);

            RegisterState(AnimationStates.Knockdown, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Knockdown,
                cancelPolicy: AnimationCancelPolicy.NonCancellable);

            // 注册死亡动画 / Register death animations
            // 死亡动画最高优先级，绝对不可取消
            RegisterState(AnimationStates.Death, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Death,
                cancelPolicy: AnimationCancelPolicy.NonCancellable);

            RegisterState(AnimationStates.DeathNoEffect, 
                layer: 0, 
                crossfadeDuration: 0.05f,
                priority: AnimationPriority.Death,
                cancelPolicy: AnimationCancelPolicy.NonCancellable);

            // 注册墙壁和边缘动画 / Register wall and edge animations
            RegisterState(AnimationStates.WallSlide, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            RegisterState(AnimationStates.WallSlideNoDust, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            RegisterState(AnimationStates.EdgeGrab, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            RegisterState(AnimationStates.EdgeIdle, 
                layer: 0, 
                crossfadeDuration: 0.1f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);

            // 注册梯子动画 / Register ladder animation
            RegisterState(AnimationStates.Ladder, 
                layer: 0, 
                crossfadeDuration: 0.15f,
                priority: AnimationPriority.Movement,
                cancelPolicy: AnimationCancelPolicy.AlwaysCancellable);
        }

        /// <summary>
        /// 初始化完成后的回调
        /// Callback after initialization
        /// </summary>
        protected override void OnAnimationManagerInitialized()
        {
            Debug.Log("Warrior Animation Manager initialized with all states and cancel mechanism");
        }

        #region Warrior特有的便捷方法 / Warrior-specific Convenience Methods

        // 基础动作 / Basic actions
        public void PlayIdle() => PlayAnimation(AnimationStates.Idle);
        public void PlayRun() => PlayAnimation(AnimationStates.Run);
        public void PlayJump() => PlayAnimation(AnimationStates.Jump);
        public void PlayFall() => PlayAnimation(AnimationStates.Fall);
        
        // 攻击动作 / Attack actions
        public void PlayAttack() => PlayAnimation(AnimationStates.Attack);
        public void PlayDashAttack() => PlayAnimation(AnimationStates.DashAttack);
        
        // 投技动作 / Throw actions
        public void PlayThrowStart() => PlayAnimation(AnimationStates.ThrowStart);
        public void PlayThrowHold() => PlayAnimation(AnimationStates.ThrowHold);
        public void PlayThrowExecute() => PlayAnimation(AnimationStates.ThrowExecute);
        public void PlayThrowRecovery() => PlayAnimation(AnimationStates.ThrowRecovery);
        
        // 移动动作 / Movement actions
        public void PlayDash() => PlayAnimation(AnimationStates.Dash);
        public void PlayCrouch() => PlayAnimation(AnimationStates.Crouch);
        public void PlaySlide() => PlayAnimation(AnimationStates.Slide);
        
        // 受击动作 / Hit reactions
        public void PlayHurt() => PlayAnimation(AnimationStates.Hurt);
        public void PlayHurtGround() => PlayAnimation(AnimationStates.HurtGround);
        public void PlayHurtAir() => PlayAnimation(AnimationStates.HurtAir);
        public void PlayKnockdown() => PlayAnimation(AnimationStates.Knockdown);
        public void PlayDeath() => PlayAnimation(AnimationStates.Death);

        /// <summary>
        /// 智能受击 - 根据角色是否在地面自动选择受击动画
        /// Smart hit reaction - Automatically choose hit animation based on ground state
        /// </summary>
        /// <param name="isGrounded">是否在地面</param>
        public void PlaySmartHurt(bool isGrounded)
        {
            if (isGrounded)
            {
                PlayHurtGround();
            }
            else
            {
                PlayHurtAir();
            }
        }

        #endregion
    }
}
