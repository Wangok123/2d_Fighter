namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// System for handling different attack types with configurable priority.
    /// 
    /// Attack priorities are now configurable via AttackConfig:
    /// - SpecialMovePriority (default: 100, highest)
    /// - HeavyAttackPriority (default: 50, medium)
    /// - LightAttackPriority (default: 10, lowest)
    /// 
    /// This system uses private methods to separate concerns while maintaining
    /// Quantum's deterministic execution model. Priority values can be adjusted
    /// in the AttackConfig asset to change attack execution order.
    /// </summary>
    public unsafe class NormalAttackSystem : SystemMainThreadFilter<NormalAttackSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterStatus* Status;
            public AttackData* AttackData;
        }

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

            // Update timers
            UpdateAttackTimers(frame, ref filter);

            // Early return if on cooldown
            if (filter.AttackData->AttackCooldown.IsRunning(frame))
            {
                filter.AttackData->IsAttacking = false;
                return;
            }

            // Process attacks based on configured priority order
            ProcessAttacksByPriority(frame, ref filter, input, attackConfig);
        }

        /// <summary>
        /// Process attacks in order of their configured priority values.
        /// Higher priority values are processed first.
        /// </summary>
        private void ProcessAttacksByPriority(Frame frame, ref Filter filter, SimpleInput2D input, AttackConfig config)
        {
            // Determine the highest priority attack type
            int specialPriority = config.SpecialMovePriority;
            int heavyPriority = config.HeavyAttackPriority;
            int lightPriority = config.LightAttackPriority;

            // Process in order: highest to lowest priority
            // We need to check all three and process them in the right order
            
            // Find highest priority
            int maxPriority = FPMath.Max(FPMath.Max(specialPriority, heavyPriority), lightPriority);
            
            // Process attacks from highest to lowest priority
            for (int currentPriority = maxPriority; currentPriority >= 0; currentPriority--)
            {
                // Check and execute special moves
                if (specialPriority == currentPriority)
                {
                    if (frame.Unsafe.TryGetPointer(filter.Entity, out CommandInputData* commandData))
                    {
                        if (TryExecuteSpecialMove(frame, ref filter, commandData, config))
                        {
                            return; // Attack executed, stop processing
                        }
                    }
                }

                // Check and execute heavy attack
                if (heavyPriority == currentPriority)
                {
                    bool heavyExecuted = ProcessHeavyAttack(frame, ref filter, input, config);
                    if (heavyExecuted)
                    {
                        return; // Attack executed, stop processing
                    }
                }

                // Check and execute light attack
                if (lightPriority == currentPriority && input.LP.WasPressed)
                {
                    ProcessLightAttack(frame, ref filter, config);
                    return; // Attack executed, stop processing
                }
            }
        }

        // ============================================================================
        // Timer Management
        // ============================================================================

        private void UpdateAttackTimers(Frame frame, ref Filter filter)
        {
            // Reset combo if timer expired
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }
        }

        // ============================================================================
        // Special Move System (Priority: Configurable via AttackConfig.SpecialMovePriority)
        // ============================================================================

        /// <summary>
        /// Try to execute a special move if input sequence matches.
        /// Priority is configurable via AttackConfig.SpecialMovePriority (default: 100).
        /// </summary>
        private bool TryExecuteSpecialMove(Frame frame, ref Filter filter, CommandInputData* commandData, AttackConfig attackConfig)
        {
            // Check each configured special move
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
                            continue; // Not high enough level
                        }
                    }

                    // Execute the special move
                    ExecuteSpecialMove(frame, ref filter, moveConfig);
                    CommandInputSystem.ClearInputBuffer(commandData);
                    return true;
                }
            }

            return false; // No special move matched
        }

        private void ExecuteSpecialMove(Frame frame, ref Filter filter, SpecialMoveConfig moveConfig)
        {
            // Reset combo on special move
            filter.AttackData->ComboCounter = 0;

            // Apply cooldown
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, moveConfig.Cooldown);

            // Fire event
            frame.Events.SpecialMovePerformed(filter.Entity, moveConfig.MoveId, moveConfig.Damage);

            // Log for debugging
            Log.Info($"Special Move: {moveConfig.MoveName} (ID: {moveConfig.MoveId}), Damage: {moveConfig.Damage}");
        }

        // ============================================================================
        // Heavy Attack System (Priority: Configurable via AttackConfig.HeavyAttackPriority)
        // ============================================================================

        /// <summary>
        /// Process heavy attack with charging mechanics.
        /// Holding HP button charges the attack, releasing executes it.
        /// Priority is configurable via AttackConfig.HeavyAttackPriority (default: 50).
        /// </summary>
        /// <returns>True if attack was executed, false otherwise</returns>
        private bool ProcessHeavyAttack(Frame frame, ref Filter filter, SimpleInput2D input, AttackConfig config)
        {
            if (input.HP.IsDown)
            {
                // Start or continue charging
                if (!filter.AttackData->IsChargingHeavy)
                {
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
                return false; // Charging, not executing yet
            }
            else if (filter.AttackData->IsChargingHeavy)
            {
                // Release charged attack
                ExecuteChargedHeavyAttack(frame, ref filter, config);
                filter.AttackData->IsChargingHeavy = false;
                filter.AttackData->HeavyChargeTime = 0;
                return true; // Attack executed
            }
            
            return false; // No heavy attack input
        }

        private void ExecuteChargedHeavyAttack(Frame frame, ref Filter filter, AttackConfig config)
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

            // Log for debugging
            Log.Debug($"Charged Heavy Attack - Charge: {chargeLevel * 100}%, Damage: {damage}");
        }

        // ============================================================================
        // Light Attack System (Priority: Configurable via AttackConfig.LightAttackPriority)
        // ============================================================================

        /// <summary>
        /// Process light attack with combo system.
        /// Consecutive light attacks within combo window increase damage.
        /// Priority is configurable via AttackConfig.LightAttackPriority (default: 10).
        /// </summary>
        private void ProcessLightAttack(Frame frame, ref Filter filter, AttackConfig config)
        {
            // Increment or reset combo counter
            if (filter.AttackData->ComboCounter < config.MaxComboCount)
            {
                filter.AttackData->ComboCounter++;
            }
            else
            {
                filter.AttackData->ComboCounter = 1; // Reset to first hit
            }

            // Calculate damage based on combo multiplier
            int comboIndex = FPMath.Clamp(filter.AttackData->ComboCounter - 1, 0, config.ComboDamageMultipliers.Length - 1);
            FP damage = config.LightAttackDamage * config.ComboDamageMultipliers[comboIndex];

            // Apply attack
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, config.LightAttackCooldown);
            filter.AttackData->ComboResetTimer = FrameTimer.FromSeconds(frame, config.ComboWindow);

            // Fire attack event
            frame.Events.AttackPerformed(filter.Entity, false, filter.AttackData->ComboCounter, damage, 0);

            // Log for debugging
            Log.Debug($"Light Attack - Combo: {filter.AttackData->ComboCounter}, Damage: {damage}");

            // TODO: Apply damage to nearby enemies (implement hit detection)
        }
    }
}
