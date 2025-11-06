using Photon.Deterministic;

namespace Quantum
{
    public unsafe class DoubleJumpAbilityData : AbilityData
    {
        [RangeEx(1, 50)] public FP DoubleJumpImpulse = 8;

        [RangeEx(0, 2)] public FP JumpHeightMultiplier = FP._1;

        public bool AllowFromFreeFalling = true;

        [RangeEx(0, 2)] public FP HorizontalVelocityMultiplier = FP._1;

        public bool AllowVariableHeight = true;

        [RangeEx(0, 1)] public FP MinJumpHeightPercent = FP._0_50;

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (abilityState.IsActiveStartTick)
            {
                OnDoubleJumpStart(frame, entityRef);
            }

            if (abilityState.IsActive)
            {
                UpdateDoubleJump(frame, entityRef);
            }

            if (abilityState.IsActiveEndTick)
            {
                OnDoubleJumpEnd(frame, entityRef);
            }

            return abilityState;
        }

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink,
            AbilityType abilityType, ref Ability ability)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
    
            // 只在第一次跳跃后或自由落体时才能激活二段跳
            bool canDoubleJump = kcc->State == KCCState.JUMPED ||
                                 (AllowFromFreeFalling && kcc->State == KCCState.FREE_FALLING);

            if (!canDoubleJump)
            {
                return false;
            }

            return base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);
        }

        private void OnDoubleJumpStart(Frame frame, EntityRef entityRef)
        {
            var abilityEnable = frame.Unsafe.GetPointer<AbilityEnable>(entityRef);

            bool doubleJumpEnabled = abilityEnable->MovementDoubleJumpEnabled;
            if (!doubleJumpEnabled)
            {
                return;
            }

            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);

            bool canDoubleJump = kcc->State == KCCState.JUMPED ||
                                 (AllowFromFreeFalling && kcc->State == KCCState.FREE_FALLING);

            if (!canDoubleJump)
            {
                return;
            }

            FP horizontalSpeed = kcc->KinematicHorizontalSpeed;

            horizontalSpeed *= HorizontalVelocityMultiplier;

            FP verticalImpulse = DoubleJumpImpulse * JumpHeightMultiplier;
            FPVector2 impulse = new FPVector2(horizontalSpeed, verticalImpulse);

            KCCState previousState = kcc->State;

            kcc->Jump(frame, entityRef, impulse);
            frame.Events.Jumped(entityRef, KCCState.DOUBLE_JUMPED, previousState, impulse);
            kcc->SetState(frame, KCCState.DOUBLE_JUMPED, 1);
        }

        private void UpdateDoubleJump(Frame frame, EntityRef entityRef)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);

            if (!AllowVariableHeight)
            {
                return;
            }

            if (kcc->State != KCCState.DOUBLE_JUMPED)
            {
                return;
            }

            if (!kcc->Input.Jump.IsDown && kcc->KinematicVerticalSpeed > FP._0)
            {
                FP minJumpSpeed = DoubleJumpImpulse * JumpHeightMultiplier * MinJumpHeightPercent;
                if (kcc->KinematicVerticalSpeed > minJumpSpeed)
                {
                    kcc->KinematicVerticalSpeed = minJumpSpeed;
                }
            }
        }

        private void OnDoubleJumpEnd(Frame frame, EntityRef entityRef)
        {
        }
    }
}