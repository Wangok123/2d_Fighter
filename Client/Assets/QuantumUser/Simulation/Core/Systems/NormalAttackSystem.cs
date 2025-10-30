namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class NormalAttackSystem : SystemMainThreadFilter<NormalAttackSystem.Filter>
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

            // Check for special moves first (higher priority)
            if (frame.Unsafe.TryGetPointer(filter.Entity, out CommandInputData* commandData))
            {
                if (TryExecuteSpecialMove(frame, ref filter, commandData, attackConfig))
                {
                    return;
                }
            }

            // Handle heavy attack charging
            ProcessHeavyCharging(frame, ref filter, input, attackConfig);

            // Handle light attack
            if (input.LP.WasPressed)
            {
                ProcessLightAttack(frame, ref filter, attackConfig);
            }
        }

        private bool TryExecuteSpecialMove(Frame frame, ref Filter filter, CommandInputData* commandData, AttackConfig attackConfig)
        {
            // Check each special move
            for (int i = 0; i < filter.AttackData->SpecialMoves.Length; i++)
            {
                var moveRef = filter.AttackData->SpecialMoves[i];
                if (moveRef.Id.IsValid == false)
                {
                    continue;
                }

                var moveConfig = frame.FindAsset(moveRef);
                if (moveConfig == null)
                {
                    continue;
                }

                // Check if input sequence matches
                if (CommandInputSystem.MatchesSequence(commandData, moveConfig.InputSequence))
                {
                    // Check level requirement
                    if (frame.Unsafe.TryGetPointer(filter.Entity, out CharacterLevel* level))
                    {
                        if (level->CurrentLevel < moveConfig.RequiredLevel)
                        {
                            continue;
                        }
                    }

                    // Execute special move
                    ExecuteSpecialMove(frame, ref filter, moveConfig);
                    CommandInputSystem.ClearInputBuffer(commandData);
                    return true;
                }
            }

            return false;
        }

        private void ExecuteSpecialMove(Frame frame, ref Filter filter, SpecialMoveConfig moveConfig)
        {
            // Reset combo
            filter.AttackData->ComboCounter = 0;

            // Apply cooldown
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, moveConfig.Cooldown);

            // Fire event
            frame.Events.SpecialMovePerformed(filter.Entity, moveConfig.MoveId, moveConfig.Damage);

            // Log
            Log.Info($"Special Move: {moveConfig.MoveName} (ID: {moveConfig.MoveId}), Damage: {moveConfig.Damage}");
        }

        private void ProcessHeavyCharging(Frame frame, ref Filter filter, SimpleInput2D input, AttackConfig config)
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

        private void ProcessChargedHeavyAttack(Frame frame, ref Filter filter, AttackConfig config)
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
            frame.Events.AttackPerformed(filter.Entity, false, filter.AttackData->ComboCounter, damage, 0);

            // Log attack for debugging
            Log.Debug($"Light Attack - Combo: {filter.AttackData->ComboCounter}, Damage: {damage}");

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
