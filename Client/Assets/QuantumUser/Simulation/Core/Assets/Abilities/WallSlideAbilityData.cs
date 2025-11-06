namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class WallSlideAbilityData : AbilityData
    {
        [RangeEx(0, 20)]
        public FP SlideSpeed = 2;
        
        [RangeEx(0, 1)]
        public FP SlideSpeedMultiplier = FP._0_50;
        
        public bool RequireInputTowardWall = true;
        
        [RangeEx(60, 120)]
        public FP MinWallAngle = 75;
        
        [RangeEx(60, 120)]
        public FP MaxWallAngle = 105;

        public WallSlideAbilityData()
        {
            Priority = AbilityPriority.Passive;
            CanBeCancelledByHigherPriority = true;
            CanCancelLowerPriority = false;
            
            InputBuffer = 0;
            Delay = 0;
            Duration = FP._100;
            Cooldown = 0;
        }

        public override Ability.AbilityState UpdateAbility(Frame frame, EntityRef entityRef, Ability* ability)
        {
            Ability.AbilityState abilityState = base.UpdateAbility(frame, entityRef, ability);

            if (abilityState.IsActiveStartTick)
            {
                OnWallSlideStart(frame, entityRef);
            }

            if (abilityState.IsActive)
            {
                if (!CheckWallSlideConditions(frame, entityRef))
                {
                    ability->StopAbility(frame, entityRef);
                }
                else
                {
                    UpdateWallSlide(frame, entityRef);
                }
            }

            if (abilityState.IsActiveEndTick)
            {
                OnWallSlideEnd(frame, entityRef);
            }

            return abilityState;
        }

        protected override bool ShouldBufferInput(Frame frame, EntityRef entityRef, AbilityType abilityType, Ability* ability, SimpleInput2D input)
        {
            return CheckWallSlideConditions(frame, entityRef);
        }

        public override bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            if (!CheckWallSlideConditions(frame, entityRef))
            {
                return false;
            }

            ability.BufferInput(frame);
            return TryActivateAbilityInternal(frame, entityRef, playerLink, abilityType, ref ability);
        }

        private bool CheckWallSlideConditions(Frame frame, EntityRef entityRef)
        {
            var abilityEnable = frame.Unsafe.GetPointer<AbilityEnable>(entityRef);
            if (!abilityEnable->MovementWallSlideEnabled)
            {
                return false;
            }

            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            
            bool isAirborne = kcc->State == KCCState.JUMPED || 
                              kcc->State == KCCState.DOUBLE_JUMPED || 
                              kcc->State == KCCState.FREE_FALLING ||
                              kcc->State == KCCState.WALLED;

            if (!isAirborne)
            {
                return false;
            }

            if (kcc->KinematicVerticalSpeed > 0)
            {
                return false;
            }

            if (!IsNearWall(frame, entityRef, out FP wallNormalX))
            {
                return false;
            }

            if (RequireInputTowardWall)
            {
                int wallDirection = wallNormalX > 0 ? -1 : 1;
                if (kcc->LastInputDirection != wallDirection)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnWallSlideStart(Frame frame, EntityRef entityRef)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            kcc->SetState(frame, KCCState.WALLED);
            
            frame.Events.Landed(entityRef, kcc->KinematicVerticalSpeed, KCCState.WALLED);
        }

        private void UpdateWallSlide(Frame frame, EntityRef entityRef)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            ApplyWallSlideSpeed(frame, kcc);
        }

        private void OnWallSlideEnd(Frame frame, EntityRef entityRef)
        {
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            
            if (kcc->State == KCCState.WALLED)
            {
                kcc->SetState(frame, KCCState.FREE_FALLING);
            }
        }

        protected override void OnAbilityCancelled(Frame frame, EntityRef entityRef, AbilityType cancelledAbilityType)
        {
            if (cancelledAbilityType == AbilityType.MovementWallSlide)
            {
                OnWallSlideEnd(frame, entityRef);
            }
        }

        private void ApplyWallSlideSpeed(Frame frame, KCC2D* kcc)
        {
            if (kcc->KinematicVerticalSpeed < -SlideSpeed)
            {
                FP targetSpeed = -SlideSpeed * SlideSpeedMultiplier;
                kcc->KinematicVerticalSpeed = FPMath.Max(kcc->KinematicVerticalSpeed, targetSpeed);
            }
        }

        private bool IsNearWall(Frame frame, EntityRef entityRef, out FP wallNormalX)
        {
            wallNormalX = 0;
            
            var kcc = frame.Unsafe.GetPointer<KCC2D>(entityRef);
            
            if (kcc->Closest.ContactType == KCCContactType.WALL)
            {
                wallNormalX = kcc->Closest.Contact.Normal.X;
                FP angle = FPVector2.Angle(FPVector2.Up, kcc->Closest.Contact.Normal);
                return angle >= MinWallAngle && angle <= MaxWallAngle;
            }

            return false;
        }
    }
}
