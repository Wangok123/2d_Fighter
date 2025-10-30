namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class AbilitySystem : SystemMainThreadFilter<AbilitySystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            // Skip if dead
            if (filter.Status->IsDead)
            {
                return;
            }

            // Get input
            SimpleInput2D input = default;
            if (frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = *frame.GetPlayerInput(playerLink->Player);
            }

            // Get attack config
            var attackConfig = frame.FindAsset(filter.AttackData->AttackConfig);
            if (attackConfig == null)
            {
                return;
            }

            // Update combo reset timer
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }

            // Update attack cooldown
            if (filter.AttackData->AttackCooldown.IsRunning(frame))
            {
                filter.AttackData->IsAttacking = false;
                return;
            }

            // Handle light attack
            if (input.LP.WasPressed)
            {
                ProcessLightAttack(frame, ref filter, attackConfig);
            }
            // Handle heavy attack
            else if (input.HP.WasPressed)
            {
                ProcessHeavyAttack(frame, ref filter, attackConfig);
            }
        }

        private void ProcessLightAttack(Frame frame, ref Filter filter, AttackConfig config)
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
            frame.Events.AttackPerformed(filter.Entity, false, filter.AttackData->ComboCounter, damage);

            // Log attack for debugging
            Log.Debug($"Light Attack - Combo: {filter.AttackData->ComboCounter}, Damage: {damage}");

            // TODO: Apply damage to nearby enemies (implement hit detection)
        }

        private void ProcessHeavyAttack(Frame frame, ref Filter filter, AttackConfig config)
        {
            // Reset combo on heavy attack
            filter.AttackData->ComboCounter = 0;

            // Apply heavy attack
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, config.HeavyAttackCooldown);

            // Fire attack event
            frame.Events.AttackPerformed(filter.Entity, true, 0, config.HeavyAttackDamage);

            // Log attack for debugging
            Log.Debug($"Heavy Attack - Damage: {config.HeavyAttackDamage}");

            // TODO: Apply damage to nearby enemies (implement hit detection)
        }

        public struct Filter
        {
            public EntityRef Entity;
            public CharacterStatus* Status;
            public AttackData* AttackData;
        }
    }
}
