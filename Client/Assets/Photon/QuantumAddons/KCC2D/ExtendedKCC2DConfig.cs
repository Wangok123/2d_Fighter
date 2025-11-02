using Quantum.Physics2D;
using Quantum.Prototypes;
using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Extended KCC2D configuration with support for additional movement abilities.
    /// This demonstrates how to extend the base KCC2D system with custom states and abilities,
    /// similar to the Sports Arena Brawler sample.
    /// 
    /// Supported additional abilities:
    /// - Air Dash: Dash while in the air
    /// - Glide: Slow fall when holding jump
    /// </summary>
    public unsafe class ExtendedKCC2DConfig : KCC2DConfig
    {
        [Space(5)]
        [Header("Extended Movement Abilities")]
        [Tooltip("Enable air dash ability")]
        public bool AirDashSupported = true;

        [Tooltip("Air dash speed")]
        public FP AirDashSpeed = 15;

        [Tooltip("Air dash duration")]
        public FP AirDashDuration = FP._0_20;

        [Space(5)]
        [Tooltip("Enable glide ability")]
        public bool GlideSupported = true;

        [Tooltip("Gravity multiplier when gliding (lower = slower fall)")]
        public FP GlideGravityMultiplier = FP._0_25;

        [Tooltip("Maximum fall speed when gliding")]
        public FP GlideMaxFallSpeed = 5;

        // State tracking for extended abilities
        private bool _isGliding = false;
        private bool _hasUsedAirDash = false;

        /// <summary>
        /// Custom state processing before default KCC2D state computation.
        /// Returns true if custom state was handled and default processing should be skipped.
        /// </summary>
        protected override bool OnPreComputeState()
        {
            // Reset air dash usage when grounded
            if (_context.KCC->State == KCCState.GROUNDED)
            {
                _hasUsedAirDash = false;
            }

            // No custom states override default processing
            return false;
        }

        /// <summary>
        /// Custom state processing after default KCC2D state computation.
        /// Used to add visual or audio feedback for extended abilities.
        /// </summary>
        protected override void OnPostComputeState()
        {
            // Update gliding state
            UpdateGlideState();
        }

        /// <summary>
        /// Custom jump processing for air dash.
        /// Returns true if custom jump was handled and default jump should be skipped.
        /// </summary>
        protected override bool OnProcessJump()
        {
            // Process air dash on dash button instead of jump
            // This is handled in ProcessDash override instead
            return false;
        }

        /// <summary>
        /// Custom force integration for gliding.
        /// </summary>
        protected override void OnIntegrateForces()
        {
            // Apply glide physics
            if (_isGliding)
            {
                // Reduce gravity when gliding
                _context.KCC->KinematicVerticalSpeed *= GlideGravityMultiplier;

                // Clamp fall speed when gliding
                if (_context.KCC->KinematicVerticalSpeed < -GlideMaxFallSpeed)
                {
                    _context.KCC->KinematicVerticalSpeed = -GlideMaxFallSpeed;
                }
            }
        }

        /// <summary>
        /// Custom input filtering for extended abilities.
        /// </summary>
        protected override SimpleInput2D OnFilterInput(SimpleInput2D input)
        {
            // Apply base filtering (includes AbilityEnable checks)
            input = base.OnFilterInput(input);

            // Filter air dash input if not enabled
            if (AirDashSupported && _context.Frame.Unsafe.TryGetPointer<AbilityEnable>(_context.Entity, out var abilityEnable))
            {
                if (!abilityEnable->MovementAirDashEnabled && 
                    _context.KCC->State != KCCState.GROUNDED && 
                    input.Dash.WasPressed)
                {
                    // Block air dash if not enabled
                    input.Dash = default;
                }
            }

            return input;
        }

        /// <summary>
        /// Override dash processing to add air dash support.
        /// </summary>
        private void ProcessExtendedDash()
        {
            // Check for air dash
            if (AirDashSupported && 
                _context.KCC->Input.Dash.WasPressed && 
                _context.KCC->State != KCCState.GROUNDED &&
                _context.KCC->State != KCCState.DASHING &&
                !_hasUsedAirDash)
            {
                // Check if air dash ability is enabled
                if (KCCAbilityIntegration.IsAbilityEnabled(_context.Frame, _context.Entity, AbilityId.MovementAirDash))
                {
                    // Execute air dash
                    _context.KCC->SetState(_context.Frame, KCCState.DASHING, AirDashDuration);
                    _context.KCC->KinematicHorizontalSpeed = _context.KCC->LastInputDirection * AirDashSpeed;
                    _context.KCC->KinematicVerticalSpeed = 0; // Suspend vertical movement during air dash
                    _hasUsedAirDash = true;

                    // Fire event (you can create a custom event for air dash)
                    Log.Debug($"Air Dash executed for entity {_context.Entity}");
                }
            }
        }

        /// <summary>
        /// Update gliding state based on input and current state.
        /// </summary>
        private void UpdateGlideState()
        {
            bool wasGliding = _isGliding;
            _isGliding = false;

            if (!GlideSupported)
            {
                return;
            }

            // Check if glide ability is enabled
            if (!KCCAbilityIntegration.IsAbilityEnabled(_context.Frame, _context.Entity, AbilityId.MovementGlide))
            {
                return;
            }

            // Glide when falling and holding jump
            if (_context.KCC->State != KCCState.GROUNDED && 
                _context.KCC->State != KCCState.DASHING &&
                _context.KCC->Input.Jump.IsDown && 
                _context.KCC->KinematicVerticalSpeed < 0)
            {
                _isGliding = true;

                // Fire event when starting to glide
                if (!wasGliding)
                {
                    Log.Debug($"Started gliding for entity {_context.Entity}");
                }
            }
        }

        /// <summary>
        /// Helper method to check if entity is currently gliding.
        /// Can be used by other systems (e.g., animation system).
        /// </summary>
        public bool IsGliding()
        {
            return _isGliding;
        }

        /// <summary>
        /// Helper method to check if entity has used air dash.
        /// Can be used by other systems (e.g., UI to show ability cooldown).
        /// </summary>
        public bool HasUsedAirDash()
        {
            return _hasUsedAirDash;
        }
    }
}
