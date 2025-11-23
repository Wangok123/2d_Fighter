using Photon.Deterministic;

namespace Quantum.Core.Utils
{
    public unsafe class MovementControllerHelper
    {
        /// <summary>
        /// 获取实体的地面状态
        /// </summary>
        public static bool IsGrounded(Frame frame, EntityRef entity)
        {
            // 优先检查 KCC2D（玩家）
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                return kcc->State == KCCState.GROUNDED;
            }
            
            // 检查 CharacterController2D（敌人/NPC）
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc2d))
            {
                return cc2d->Grounded;
            }
            
            return false;
        }

        /// <summary>
        /// 获取实体的垂直速度
        /// </summary>
        public static FP GetVerticalVelocity(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                return kcc->KinematicVerticalSpeed;
            }
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc2d))
            {
                return cc2d->Velocity.Y;
            }
            
            return FP._0;
        }

        /// <summary>
        /// 设置实体的垂直速度
        /// </summary>
        public static void SetVerticalVelocity(Frame frame, EntityRef entity, FP velocity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                kcc->KinematicVerticalSpeed = velocity;
                return;
            }
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc2d))
            {
                cc2d->Velocity = new FPVector2(cc2d->Velocity.X, velocity);
                return;
            }
        }

        /// <summary>
        /// 获取实体的水平速度
        /// </summary>
        public static FP GetHorizontalVelocity(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                return kcc->KinematicHorizontalSpeed;
            }
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc2d))
            {
                return cc2d->Velocity.X;
            }
            
            return FP._0;
        }

        /// <summary>
        /// 设置实体的水平速度
        /// </summary>
        public static void SetHorizontalVelocity(Frame frame, EntityRef entity, FP velocity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                kcc->KinematicHorizontalSpeed = velocity;
                return;
            }
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc2d))
            {
                cc2d->Velocity = new FPVector2(velocity, cc2d->Velocity.Y);
                return;
            }
        }

        /// <summary>
        /// 设置实体的速度
        /// </summary>
        public static void SetVelocity(Frame frame, EntityRef entity, FPVector2 velocity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                kcc->KinematicHorizontalSpeed = velocity.X;
                kcc->KinematicVerticalSpeed = velocity.Y;
                return;
            }
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc2d))
            {
                cc2d->Velocity = velocity;
                return;
            }
        }

        /// <summary>
        /// 获取实体的速度
        /// </summary>
        public static FPVector2 GetVelocity(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                return new FPVector2(kcc->KinematicHorizontalSpeed, kcc->KinematicVerticalSpeed);
            }
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc2d))
            {
                return cc2d->Velocity;
            }
            
            return FPVector2.Zero;
        }

        /// <summary>
        /// 检查实体是否拥有运动控制器
        /// </summary>
        public static bool HasMovementController(Frame frame, EntityRef entity)
        {
            return frame.Has<KCC2D>(entity) || frame.Has<CharacterController2D>(entity);
        }
    }
}