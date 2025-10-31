namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Handler for light attack with combo system (lowest priority).
    /// Handles combo counting and damage scaling.
    /// </summary>
    public unsafe class LightAttackHandler : IAttackHandler
    {
        public int Priority => 10; // Lowest priority

        public bool CanExecute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            return input.LP.WasPressed;
        }

        public bool Execute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            ProcessLightAttack(frame, ref filter, config);
            return true;
        }

        private void ProcessLightAttack(Frame frame, ref NormalAttackSystem.Filter filter, AttackConfig config)
        {
            // Check combo window
            if (filter.AttackData->ComboCounter < config.MaxComboCount)
            {
                filter.AttackData->ComboCounter++;
            }
            else
            {
                // Reset combo if at max
                filter.AttackData->ComboCounter = 1;
            }

            // Calculate damage based on combo
            int comboIndex = FPMath.Clamp(filter.AttackData->ComboCounter - 1, 0, config.ComboDamageMultipliers.Length - 1);
            FP damage = config.LightAttackDamage * config.ComboDamageMultipliers[comboIndex];

            // Apply attack
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, config.LightAttackCooldown);
            filter.AttackData->ComboResetTimer = FrameTimer.FromSeconds(frame, config.ComboWindow);

            // Fire attack event
            frame.Events.AttackPerformed(filter.Entity, false, filter.AttackData->ComboCounter, damage, 0);

            // Log attack for debugging
            Log.Debug($"Light Attack - Combo: {filter.AttackData->ComboCounter}, Damage: {damage}");

            // TODO: Apply damage to nearby enemies (implement hit detection)
        }
    }
}
