using Photon.Deterministic;

namespace Quantum
{
    /// <summary>
    /// Integration layer between KCC2D system and AbilityEnable component.
    /// Provides utility methods to sync abilities with KCC2D settings.
    /// Inspired by Sports Arena Brawler's extensible ability system.
    /// </summary>
    public static unsafe class KCCAbilityIntegration
    {
        /// <summary>
        /// Gets KCC2D settings modified based on enabled abilities from AbilityEnable component.
        /// This allows runtime control of KCC2D features based on character abilities.
        /// </summary>
        /// <param name="frame">The current frame</param>
        /// <param name="entity">Entity to get settings for</param>
        /// <param name="baseConfig">Base KCC2D configuration</param>
        /// <returns>Modified settings if AbilityEnable exists, null otherwise</returns>
        public static KCC2DSettings? GetSettingsWithAbilityOverrides(Frame frame, EntityRef entity, KCC2DConfig baseConfig)
        {
            // Check if entity has AbilityEnable component
            if (!frame.Unsafe.TryGetPointer<AbilityEnable>(entity, out var abilityEnable))
            {
                return null; // No ability component, use default settings
            }

            // Create modified settings from base
            KCC2DSettings settings = default;
            baseConfig.BaseSettings.Materialize(frame, ref settings);

            // Override settings based on enabled abilities
            ApplyAbilityOverrides(ref settings, abilityEnable);

            return settings;
        }

        /// <summary>
        /// Applies ability-based overrides to KCC2D settings.
        /// </summary>
        private static void ApplyAbilityOverrides(ref KCC2DSettings settings, AbilityEnable* abilityEnable)
        {
            // Movement ability overrides
            if (!abilityEnable->MovementDoubleJumpEnabled)
            {
                settings.DoubleJumpEnabled = false;
            }

            if (!abilityEnable->MovementDashEnabled)
            {
                // Disable dash by setting duration to 0
                // Note: KCC2D checks for dash input, we need to handle this at input level too
                settings.DashDuration = FP._0;
            }

            if (!abilityEnable->MovementWallJumpEnabled)
            {
                settings.WallJumpEnabled = false;
            }

            // Note: MovementAirDashEnabled and MovementGlideEnabled may need custom implementation
            // as base KCC2D doesn't have these features built-in
        }

        /// <summary>
        /// Checks if a specific ability is enabled for an entity.
        /// Provides a unified way to check abilities across the codebase.
        /// </summary>
        public static bool IsAbilityEnabled(Frame frame, EntityRef entity, AbilityId abilityId)
        {
            if (!frame.Unsafe.TryGetPointer<AbilityEnable>(entity, out var abilityEnable))
            {
                return true; // No ability component means all abilities enabled by default
            }

            return IsAbilityEnabled(abilityEnable, abilityId);
        }

        /// <summary>
        /// Checks if a specific ability is enabled using the AbilityEnable pointer.
        /// </summary>
        public static bool IsAbilityEnabled(AbilityEnable* abilityEnable, AbilityId abilityId)
        {
            if (abilityEnable == null)
            {
                return true;
            }

            // Map AbilityId to corresponding enabled flag
            switch (abilityId)
            {
                // Movement abilities
                case AbilityId.MovementDoubleJump:
                    return abilityEnable->MovementDoubleJumpEnabled;
                case AbilityId.MovementDash:
                    return abilityEnable->MovementDashEnabled;
                case AbilityId.MovementWallJump:
                    return abilityEnable->MovementWallJumpEnabled;
                case AbilityId.MovementAirDash:
                    return abilityEnable->MovementAirDashEnabled;
                case AbilityId.MovementGlide:
                    return abilityEnable->MovementGlideEnabled;

                // Attack abilities
                case AbilityId.AttackLight:
                    return abilityEnable->AttackLightEnabled;
                case AbilityId.AttackHeavy:
                    return abilityEnable->AttackHeavyEnabled;
                case AbilityId.AttackRanged:
                    return abilityEnable->AttackRangedEnabled;
                case AbilityId.AttackArea:
                    return abilityEnable->AttackAreaEnabled;

                // Defense abilities
                case AbilityId.DefenseBlock:
                    return abilityEnable->DefenseBlockEnabled;
                case AbilityId.DefenseParry:
                    return abilityEnable->DefenseParryEnabled;
                case AbilityId.DefenseDodge:
                    return abilityEnable->DefenseDodgeEnabled;
                case AbilityId.DefenseShield:
                    return abilityEnable->DefenseShieldEnabled;

                // Special abilities
                case AbilityId.SpecialUltimate:
                    return abilityEnable->SpecialUltimateEnabled;
                case AbilityId.SpecialTransformation:
                    return abilityEnable->SpecialTransformationEnabled;
                case AbilityId.SpecialSummon:
                    return abilityEnable->SpecialSummonEnabled;

                default:
                    return true; // Unknown abilities enabled by default
            }
        }

        /// <summary>
        /// Enables or disables a specific ability at runtime.
        /// Useful for power-ups, debuffs, or progression systems.
        /// </summary>
        public static void SetAbilityEnabled(Frame frame, EntityRef entity, AbilityId abilityId, bool enabled)
        {
            if (!frame.Unsafe.TryGetPointer<AbilityEnable>(entity, out var abilityEnable))
            {
                return; // No ability component, cannot set
            }

            SetAbilityEnabled(abilityEnable, abilityId, enabled);
        }

        /// <summary>
        /// Enables or disables a specific ability using the AbilityEnable pointer.
        /// </summary>
        public static void SetAbilityEnabled(AbilityEnable* abilityEnable, AbilityId abilityId, bool enabled)
        {
            if (abilityEnable == null)
            {
                return;
            }

            // Map AbilityId to corresponding enabled flag
            switch (abilityId)
            {
                // Movement abilities
                case AbilityId.MovementDoubleJump:
                    abilityEnable->MovementDoubleJumpEnabled = enabled;
                    break;
                case AbilityId.MovementDash:
                    abilityEnable->MovementDashEnabled = enabled;
                    break;
                case AbilityId.MovementWallJump:
                    abilityEnable->MovementWallJumpEnabled = enabled;
                    break;
                case AbilityId.MovementAirDash:
                    abilityEnable->MovementAirDashEnabled = enabled;
                    break;
                case AbilityId.MovementGlide:
                    abilityEnable->MovementGlideEnabled = enabled;
                    break;

                // Attack abilities
                case AbilityId.AttackLight:
                    abilityEnable->AttackLightEnabled = enabled;
                    break;
                case AbilityId.AttackHeavy:
                    abilityEnable->AttackHeavyEnabled = enabled;
                    break;
                case AbilityId.AttackRanged:
                    abilityEnable->AttackRangedEnabled = enabled;
                    break;
                case AbilityId.AttackArea:
                    abilityEnable->AttackAreaEnabled = enabled;
                    break;

                // Defense abilities
                case AbilityId.DefenseBlock:
                    abilityEnable->DefenseBlockEnabled = enabled;
                    break;
                case AbilityId.DefenseParry:
                    abilityEnable->DefenseParryEnabled = enabled;
                    break;
                case AbilityId.DefenseDodge:
                    abilityEnable->DefenseDodgeEnabled = enabled;
                    break;
                case AbilityId.DefenseShield:
                    abilityEnable->DefenseShieldEnabled = enabled;
                    break;

                // Special abilities
                case AbilityId.SpecialUltimate:
                    abilityEnable->SpecialUltimateEnabled = enabled;
                    break;
                case AbilityId.SpecialTransformation:
                    abilityEnable->SpecialTransformationEnabled = enabled;
                    break;
                case AbilityId.SpecialSummon:
                    abilityEnable->SpecialSummonEnabled = enabled;
                    break;
            }
        }

        /// <summary>
        /// Initializes all abilities to a default enabled state.
        /// Useful for character initialization.
        /// </summary>
        public static void EnableAllAbilities(AbilityEnable* abilityEnable)
        {
            if (abilityEnable == null) return;

            abilityEnable->MovementDoubleJumpEnabled = true;
            abilityEnable->MovementDashEnabled = true;
            abilityEnable->MovementWallJumpEnabled = true;
            abilityEnable->MovementAirDashEnabled = true;
            abilityEnable->MovementGlideEnabled = true;
            abilityEnable->AttackLightEnabled = true;
            abilityEnable->AttackHeavyEnabled = true;
            abilityEnable->AttackRangedEnabled = true;
            abilityEnable->AttackAreaEnabled = true;
            abilityEnable->DefenseBlockEnabled = true;
            abilityEnable->DefenseParryEnabled = true;
            abilityEnable->DefenseDodgeEnabled = true;
            abilityEnable->DefenseShieldEnabled = true;
            abilityEnable->SpecialUltimateEnabled = true;
            abilityEnable->SpecialTransformationEnabled = true;
            abilityEnable->SpecialSummonEnabled = true;
        }

        /// <summary>
        /// Disables all abilities.
        /// Useful for stunned/disabled states.
        /// </summary>
        public static void DisableAllAbilities(AbilityEnable* abilityEnable)
        {
            if (abilityEnable == null) return;

            abilityEnable->MovementDoubleJumpEnabled = false;
            abilityEnable->MovementDashEnabled = false;
            abilityEnable->MovementWallJumpEnabled = false;
            abilityEnable->MovementAirDashEnabled = false;
            abilityEnable->MovementGlideEnabled = false;
            abilityEnable->AttackLightEnabled = false;
            abilityEnable->AttackHeavyEnabled = false;
            abilityEnable->AttackRangedEnabled = false;
            abilityEnable->AttackAreaEnabled = false;
            abilityEnable->DefenseBlockEnabled = false;
            abilityEnable->DefenseParryEnabled = false;
            abilityEnable->DefenseDodgeEnabled = false;
            abilityEnable->DefenseShieldEnabled = false;
            abilityEnable->SpecialUltimateEnabled = false;
            abilityEnable->SpecialTransformationEnabled = false;
            abilityEnable->SpecialSummonEnabled = false;
        }
    }
}
