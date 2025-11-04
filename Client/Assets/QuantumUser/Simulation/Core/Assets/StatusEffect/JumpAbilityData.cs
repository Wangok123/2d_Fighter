using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class JumpAbilityData : AbilityData
    {
        [RangeEx(1, 50)] public FP JumpImpulse = 10;

        [RangeEx(0, 2)] public FP JumpHeightMultiplier = FP._1;

        public bool AllowVariableHeight = true;

        [RangeEx(0, 1)] public FP MinJumpHeightPercent = FP._0_50;

        [Header("起跳时水平速度调整")] [RangeEx(0, 2)] public FP HorizontalVelocityMultiplier = FP._1;

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (abilityState.IsActiveStartTick)
            {
                OnJumpStart(frame, entityRef);
            }

            if (abilityState.IsActive)
            {
                UpdateJump(frame, entityRef);
            }

            if (abilityState.IsActiveEndTick)
            {
                OnJumpEnd(frame, entityRef);
            }

            return abilityState;
        }

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink,
            AbilityType abilityType, ref Ability ability)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            
            if (kcc->State != KCCState.GROUNDED)
            {
                return false;
            }

            return base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);
        }

        private void OnJumpStart(Frame frame, EntityRef entityRef)
        {
            var abilityEnable = frame.Unsafe.GetPointer<AbilityEnable>(entityRef);

            bool jumpEnabled = abilityEnable->MovementJumpEnabled;
            if (!jumpEnabled)
            {
                return;
            }

            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);

            if (kcc->State != KCCState.GROUNDED)
            {
                return;
            }

            FP horizontalSpeed = kcc->KinematicHorizontalSpeed;

            horizontalSpeed *= HorizontalVelocityMultiplier;

            FP verticalImpulse = JumpImpulse * JumpHeightMultiplier;
            FPVector2 impulse = new FPVector2(horizontalSpeed, verticalImpulse);

            kcc->Jump(frame, entityRef, impulse);
            frame.Events.Jumped(entityRef, KCCState.JUMPED, KCCState.GROUNDED, impulse);
            kcc->SetState(frame, KCCState.JUMPED);
        }

        private void UpdateJump(Frame frame, EntityRef entityRef)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);

            if (!AllowVariableHeight)
            {
                return;
            }

            if (kcc->State != KCCState.JUMPED)
            {
                return;
            }

            if (!kcc->Input.Jump.IsDown && kcc->KinematicVerticalSpeed > FP._0)
            {
                FP minJumpSpeed = JumpImpulse * JumpHeightMultiplier * MinJumpHeightPercent;
                if (kcc->KinematicVerticalSpeed > minJumpSpeed)
                {
                    kcc->KinematicVerticalSpeed = minJumpSpeed;
                }
            }
        }

        private void OnJumpEnd(Frame frame, EntityRef entityRef)
        {
        }
    }
}