using Photon.Deterministic;

namespace Quantum
{
    public unsafe class WallJumpAbilityData : AbilityData
    {
        [RangeEx(1, 50)]
        public FP WallJumpImpulseX = 10;
        
        [RangeEx(1, 50)]
        public FP WallJumpImpulseY = 12;
        
        public bool FlipOnWallJump = true;

        [RangeEx(0, 2)]
        public FP HorizontalVelocityMultiplier = FP._1;

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (abilityState.IsActiveStartTick)
            {
                OnWallJumpStart(frame, entityRef);
            }

            return abilityState;
        }

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType,
            ref Ability ability)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            
            if (kcc->State != KCCState.WALLED)
            {
                return false;
            }

            return base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);
        }

        private void OnWallJumpStart(Frame frame, EntityRef entityRef)
        {
            var abilityEnable = frame.Unsafe.GetPointer<AbilityEnable>(entityRef);

            bool wallJumpEnabled = abilityEnable->MovementWallJumpEnabled;
            if (!wallJumpEnabled)
            {
                return;
            }

            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);

            if (kcc->State != KCCState.WALLED)
            {
                return;
            }

            FP wallContactDirection = kcc->Closest.Contact.Normal.X > 0 ? FP._1 : -FP._1;
            
            FP horizontalImpulse = WallJumpImpulseX * wallContactDirection * HorizontalVelocityMultiplier;
            
            FPVector2 impulse = new FPVector2(horizontalImpulse, WallJumpImpulseY);

            kcc->Jump(frame, entityRef, impulse);
            frame.Events.Jumped(entityRef, KCCState.JUMPED, KCCState.WALLED, impulse);
            kcc->SetState(frame, KCCState.JUMPED);

            if (FlipOnWallJump)
            {
                if (frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movementData))
                {
                    movementData->IsFacingRight = wallContactDirection > 0;
                }
            }
        }
    }
}