using UnityEngine.Scripting;

namespace Quantum
{
    using Photon.Deterministic;

    [Preserve]
    public unsafe class Character2DSystem : SystemMainThreadFilter<Character2DSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public CharacterStatusComponent* CharacterStatus;
            public CharacterController2D* KCC;
        }
        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.CharacterStatus->IsKnockedBack)
            {
                return;
            }
            
            AbilityData activeAbilityData = default;
            
            // 移动方向计算
            FPVector2 movementDirection;
            
            if (filter.CharacterStatus->IsIncapacitated)
            {
                // 技能激活或失能状态下不允许移动
                movementDirection = FPVector2.Zero;
            }
            else
            {
                // 这里需要根据你的AI逻辑来设置移动方向
                // 示例：可以从AI组件或导航系统获取
                movementDirection = GetMovementDirection(frame, filter.EntityRef);
                
                // 限制移动方向的最大长度为1
                if (movementDirection.SqrMagnitude > FP._1)
                {
                    movementDirection = movementDirection.Normalized;
                }
            }
            
            // 【重构】统一执行移动（仅在非重生状态）
            if (!filter.CharacterStatus->IsRespawning)
            {
                filter.KCC->Move(frame, filter.EntityRef, movementDirection);
            }
            
            // 更新最终的移动状态
        }

        // 获取移动方向的辅助方法（需要根据你的AI系统实现）
        private FPVector2 GetMovementDirection(Frame frame, EntityRef entityRef)
        {
            // TODO: 根据你的AI系统实现
            // 示例1: 从AI导航组件获取
            // if (frame.Unsafe.TryGetPointer<AINavigation>(entityRef, out var aiNav))
            // {
            //     return aiNav->DesiredDirection;
            // }
            
            // 示例2: 简单的追踪玩家逻辑
            // EntityRef playerEntity = GetNearestPlayer(frame, entityRef);
            // if (playerEntity != default)
            // {
            //     Transform2D* enemyTransform = frame.Unsafe.GetPointer<Transform2D>(entityRef);
            //     Transform2D* playerTransform = frame.Unsafe.GetPointer<Transform2D>(playerEntity);
            //     FPVector2 direction = (playerTransform->Position - enemyTransform->Position).Normalized;
            //     return direction;
            // }

            return FPVector2.Zero;
        }

        // 将2D向量转换为旋转角度
        private FP GetRotationFromDirection(FPVector2 direction)
        {
            if (direction == FPVector2.Zero)
            {
                return FP._0;
            }
            
            // 计算目标方向的角度（弧度）
            return FPMath.Atan2(direction.Y, direction.X);
        }
    }
}
