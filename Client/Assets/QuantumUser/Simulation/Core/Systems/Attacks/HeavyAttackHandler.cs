namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Handler for heavy attack with charging mechanics (medium priority).
    /// Handles both charging state and charged attack execution.
    /// </summary>
    public unsafe class HeavyAttackHandler : IAttackHandler
    {
        public int Priority => 50; // Medium priority

        public bool CanExecute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            // Can execute if HP is pressed (for charging) or if releasing charged attack
            return input.HP.IsDown || filter.AttackData->IsChargingHeavy;
        }

        public bool Execute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            ProcessHeavyCharging(frame, ref filter, input, config);
            return true; // Always return true to prevent lower priority attacks during charging
        }

        private void ProcessHeavyCharging(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            if (input.HP.IsDown)
            {
                if (!filter.AttackData->IsChargingHeavy)
                {
                    // Start charging
                    filter.AttackData->IsChargingHeavy = true;
                    filter.AttackData->HeavyChargeTime = 0;
                }
                else
                {
                    // Continue charging
                    filter.AttackData->HeavyChargeTime += frame.DeltaTime;
                    
                    // Clamp to max charge time
                    if (filter.AttackData->HeavyChargeTime > config.MaxChargeTime)
                    {
                        filter.AttackData->HeavyChargeTime = config.MaxChargeTime;
                    }
                }
            }
            else if (filter.AttackData->IsChargingHeavy)
            {
                // Release charged attack
                ProcessChargedHeavyAttack(frame, ref filter, config);
                filter.AttackData->IsChargingHeavy = false;
                filter.AttackData->HeavyChargeTime = 0;
            }
        }

        private void ProcessChargedHeavyAttack(Frame frame, ref NormalAttackSystem.Filter filter, AttackConfig config)
        {
            // Reset combo on heavy attack
            filter.AttackData->ComboCounter = 0;

            // Calculate charge level (0 to 1)
            FP chargeLevel = 0;
            if (filter.AttackData->HeavyChargeTime >= config.MinChargeTime)
            {
                chargeLevel = FPMath.Clamp01((filter.AttackData->HeavyChargeTime - config.MinChargeTime) / 
                                             (config.MaxChargeTime - config.MinChargeTime));
            }

            // Calculate damage with charge multiplier
            FP damageMultiplier = FP._1 + (chargeLevel * (config.FullChargeDamageMultiplier - FP._1));
            FP damage = config.HeavyAttackDamage * damageMultiplier;

            // Apply heavy attack
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, config.HeavyAttackCooldown);

            // Fire attack event with charge level
            frame.Events.AttackPerformed(filter.Entity, true, 0, damage, chargeLevel);

            // Log attack for debugging
            Log.Debug($"Charged Heavy Attack - Charge: {chargeLevel * 100}%, Damage: {damage}");
        }
    }
}
