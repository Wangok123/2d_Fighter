using System;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct Ability
    {
        public struct AbilityState
        {
            public bool IsDelayed;
            public bool IsActive;
            public bool IsActiveStartTick;
            public bool IsActiveEndTick;
            public bool IsOnCooldown;
        }

        public bool HasBufferedInput => InputBufferTimer.IsRunning;
        public bool IsDelayed => DelayTimer.IsRunning;
        public bool IsActive => DurationTimer.IsRunning;
        public bool IsDelayedOrActive => IsDelayed || IsActive;
        public bool IsOnCooldown => CooldownTimer.IsRunning;
        
        public bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerRef playerRef)
        {
            if (IsOnCooldown)
            {
                return false;
            }

            CharacterStatus* playerStatus = frame.Unsafe.GetPointer<CharacterStatus>(entityRef);
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);

            if (abilityInventory->HasActiveAbility)
            {
                return false;
            }
            
            KCC2D* kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            AbilityData abilityData = frame.FindAsset<AbilityData>(AbilityData.Id);
            PlayerMovementData playerMovementData = frame.FindAsset<PlayerMovementData>(playerStatus->PlayerMovementData.Id);

            InputBufferTimer.Reset();
            DelayTimer.Start(abilityData.Delay);
            if (!abilityData.StartCooldownAfterDelay)
            {
                CooldownTimer.Start(abilityData.Cooldown);
            }

            abilityInventory->ActiveAbilityInfo.ActiveAbilityType = AbilityType;
            abilityInventory->ActiveAbilityInfo.CastDirection = GetCastDirection(frame, playerRef, abilityData, entityRef);
            abilityInventory->ActiveAbilityInfo.CastVelocity = kcc->CombinedVelocity;

            playerMovementData.UpdateKCCSettings(frame, entityRef);

            return true;
        }
        
        private FPVector2 GetCastDirection(Frame frame, PlayerRef playerRef, AbilityData abilityData, EntityRef entity)
        {
            SimpleInput2D input = *frame.GetPlayerInput(playerRef);
            FPVector2 direction = FPVector2.Zero;

            // 尝试从输入获取方向（按优先级）
            if ((abilityData.CastDirectionType & AbilityCastDirectionType.Aim) == AbilityCastDirectionType.Aim 
                && input.AimDirection != default)
            {
                direction = input.AimDirection;
            }
            else if ((abilityData.CastDirectionType & AbilityCastDirectionType.Movement) == AbilityCastDirectionType.Movement)
            {
                direction = GetMovementDirection(input);
            }
            
            // 如果没有获取到有效方向，使用角色朝向
            if (direction == FPVector2.Zero 
                && (abilityData.CastDirectionType & AbilityCastDirectionType.FacingDirection) == AbilityCastDirectionType.FacingDirection)
            {
                direction = GetFacingDirection(frame, entity);
            }
    
            // 确保返回标准化的方向
            if (direction == FPVector2.Zero)
            {
                // 兜底：使用默认朝向
                direction = GetFacingDirection(frame, entity);
            }
    
            return direction.Normalized;
        }
        
        // 辅助方法：从输入获取移动方向
        private FPVector2 GetMovementDirection(SimpleInput2D input)
        {
            FPVector2 direction = FPVector2.Zero;
    
            if (input.Left.IsDown) direction.X -= FP._1;
            if (input.Right.IsDown) direction.X += FP._1;
            if (input.Up.IsDown) direction.Y += FP._1;
            if (input.Down.IsDown) direction.Y -= FP._1;
    
            return direction;
        }

        // 辅助方法：获取角色朝向
        private FPVector2 GetFacingDirection(Frame frame, EntityRef playerRef)
        {
    
            if (frame.Unsafe.TryGetPointer<MovementData>(playerRef, out var movementData))
            {
                return movementData->IsFacingRight ? FPVector2.Right : FPVector2.Left;
            }
    
            // 兜底：默认向右
            return FPVector2.Right;
        }

    }
}