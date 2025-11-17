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
        
        public bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerRef playerRef, AbilityType abilityType)
        {
            if (IsOnCooldown)
            {
                return false;
            }

            CharacterStatusComponent* playerStatus = frame.Unsafe.GetPointer<CharacterStatusComponent>(entityRef);
            if (playerStatus->IsIncapacitated)
            {
                return false;
            }
    
    
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);

            if (abilityInventory->HasActiveAbility)
            {
                return false;
            }
    
            KCC2D* kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            AbilityData abilityData = frame.FindAsset<AbilityData>(AbilityData.Id);

            RuntimeDuration = abilityData.Duration;

            InputBufferTimer.Reset();
            DelayTimer.Start(abilityData.Delay);
            if (!abilityData.StartCooldownAfterDelay)
            {
                CooldownTimer.Start(abilityData.Cooldown);
            }

            abilityInventory->ActiveAbilityInfo.ActiveAbilityType = abilityType;
            abilityInventory->ActiveAbilityInfo.CastDirection = GetCastDirection(frame, playerRef, abilityData, entityRef);
            abilityInventory->ActiveAbilityInfo.CastVelocity = kcc->CombinedVelocity;

            if (abilityData.DisableMovementDuringAbility)
            {
                if (frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                {
                    abilityEnable->MovementEnabled = false;
                }
                
            }

            return true;
        }
        
        public AbilityState Update(Frame frame, EntityRef entityRef)
        {
            AbilityState state = new AbilityState();

            InputBufferTimer.Tick(frame.DeltaTime);
            CooldownTimer.Tick(frame.DeltaTime);

            state.IsOnCooldown = IsOnCooldown;

            if (IsDelayedOrActive)
            {
                CharacterStatusComponent* playerStatus = frame.Unsafe.GetPointer<CharacterStatusComponent>(entityRef);

                if (playerStatus->IsIncapacitated)
                {
                    StopAbility(frame, entityRef);

                    return state;
                }

                FP delayTimeLeft = DelayTimer.TimeLeft;

                if (IsDelayed)
                {
                    DelayTimer.Tick(frame.DeltaTime);

                    if (DelayTimer.IsRunning)
                    {
                        state.IsDelayed = true;
                    }
                    else
                    {
                        state.IsActiveStartTick = true;

                        // 使用运行时存储的Duration，而不是重新从Asset读取
                        DurationTimer.Start(RuntimeDuration);
        
                        AbilityData abilityData = frame.FindAsset<AbilityData>(AbilityData.Id);
                        if (abilityData.StartCooldownAfterDelay)
                        {
                            CooldownTimer.Start(abilityData.Cooldown);
                        }
                    }
                }

                if (IsActive)
                {
                    state.IsActive = true;

                    DurationTimer.Tick(frame.DeltaTime - delayTimeLeft);

                    if (DurationTimer.IsDone)
                    {
                        state.IsActiveEndTick = true;

                        AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);
                        AbilityType currentAbilityType = abilityInventory->ActiveAbilityInfo.ActiveAbilityType;

                        StopAbility(frame, entityRef);
                        
                        if (currentAbilityType != AbilityType.None)
                        {
                            frame.Events.AbilityEnded(entityRef, currentAbilityType);
                        }
                    }
                }
            }

            return state;
        }

        public void BufferInput(Frame frame)
        {
            AbilityData abilityData = frame.FindAsset<AbilityData>(AbilityData.Id);

            InputBufferTimer.Start(abilityData.InputBuffer);
        }

        public void StopAbility(Frame frame, EntityRef entityRef)
        {
            MovementComponent* movement = frame.Unsafe.GetPointer<MovementComponent>(entityRef);
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(entityRef);
            MovementData playerMovementData = frame.FindAsset<MovementData>(movement->MovementData.Id);

            AbilityData abilityData = frame.FindAsset<AbilityData>(AbilityData.Id);

            abilityInventory->ActiveAbilityInfo.ActiveAbilityType = AbilityType.None;

            DelayTimer.Reset();
            DurationTimer.Reset();

            playerMovementData.UpdateKCCSettings(frame, entityRef);
    
            if (abilityData.DisableMovementDuringAbility)
            {
                if (frame.Unsafe.TryGetPointer<AbilityEnable>(entityRef, out var abilityEnable))
                {
                    if (frame.Unsafe.TryGetPointer<CharacterStatusComponent>(entityRef, out var hitReaction))
                    {
                        if (!hitReaction->IsKnockedBack)
                        {
                            abilityEnable->MovementEnabled = true;
                        }
                    }
                    else
                    {
                        abilityEnable->MovementEnabled = true;
                    }
                }
            }
        }


        public void ResetCooldown()
        {
            CooldownTimer.Reset();
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
            return input.DigitalDirection;
        }

        // 辅助方法：获取角色朝向
        private FPVector2 GetFacingDirection(Frame frame, EntityRef playerRef)
        {
    
            if (frame.Unsafe.TryGetPointer<MovementComponent>(playerRef, out var movementData))
            {
                return movementData->IsFacingRight ? FPVector2.Right : FPVector2.Left;
            }
    
            // 兜底：默认向右
            return FPVector2.Right;
        }

    }
}