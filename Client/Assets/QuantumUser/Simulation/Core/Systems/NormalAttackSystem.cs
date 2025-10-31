namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// System for handling different attack types with inheritance-based configuration.
    /// 
    /// Attack types inherit from AttackConfig base class:
    /// - LightAttackConfig: Fast combo attacks (inherits Priority, Damage, Cooldown + adds combo system)
    /// - HeavyAttackConfig: Powerful charged attacks (inherits base + adds charge system)
    /// - Future: SpecialAttackConfig, RangedAttackConfig, etc. can be added similarly
    /// 
    /// This inheritance-based design provides a consistent base while allowing specific behaviors
    /// to be added through derived classes, maintaining Quantum's deterministic execution model.
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

            // Get character attack config
            var characterConfig = frame.FindAsset(filter.AttackData->AttackConfig);
            if (characterConfig == null)
            {
                return;
            }

            // Update timers
            UpdateAttackTimers(frame, ref filter, characterConfig);

            // Early return if on cooldown
            if (filter.AttackData->AttackCooldown.IsRunning(frame))
            {
                filter.AttackData->IsAttacking = false;
                return;
            }

            // Process attacks based on configured priority order
            ProcessAttacksByPriority(frame, ref filter, input, characterConfig);
        }

        /// <summary>
        /// Process attacks in order of their configured priority values.
        /// Higher priority values are processed first.
        /// Loads each attack type's config and processes them by priority.
        /// </summary>
        private void ProcessAttacksByPriority(Frame frame, ref Filter filter, SimpleInput2D input, CharacterAttackConfig characterConfig)
        {
            // Load individual attack configs
            var lightConfig = frame.FindAsset(characterConfig.LightAttack);
            var heavyConfig = frame.FindAsset(characterConfig.HeavyAttack);

            // Determine priorities (use default if config is missing)
            int specialPriority = characterConfig.SpecialMovePriority;
            int heavyPriority = heavyConfig != null ? heavyConfig.Priority : 50;
            int lightPriority = lightConfig != null ? lightConfig.Priority : 10;

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
                        if (TryExecuteSpecialMove(frame, ref filter, commandData, characterConfig))
                        {
                            return; // Attack executed, stop processing
                        }
                    }
                }

                // Check and execute heavy attack
                if (heavyPriority == currentPriority && heavyConfig != null)
                {
                    bool heavyExecuted = ProcessHeavyAttack(frame, ref filter, input, heavyConfig);
                    if (heavyExecuted)
                    {
                        return; // Attack executed, stop processing
                    }
                }

                // Check and execute light attack
                if (lightPriority == currentPriority && lightConfig != null && input.LP.WasPressed)
                {
                    ProcessLightAttack(frame, ref filter, lightConfig);
                    return; // Attack executed, stop processing
                }
            }
        }

        // ============================================================================
        // Timer Management
        // ============================================================================

        private void UpdateAttackTimers(Frame frame, ref Filter filter, CharacterAttackConfig characterConfig)
        {
            // Reset combo if timer expired
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }
        }

        // ============================================================================
        // Special Move System (Priority: Configurable via CharacterAttackConfig.SpecialMovePriority)
        // ============================================================================

        /// <summary>
        /// Try to execute a special move if input sequence matches.
        /// Priority is configurable via CharacterAttackConfig.SpecialMovePriority (default: 100).
        /// </summary>
        private bool TryExecuteSpecialMove(Frame frame, ref Filter filter, CommandInputData* commandData, CharacterAttackConfig characterConfig)
        {
            // Check each configured special move
            if (characterConfig.SpecialMoves == null || characterConfig.SpecialMoves.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < characterConfig.SpecialMoves.Length; i++)
            {
                var moveRef = characterConfig.SpecialMoves[i];
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
        // Heavy Attack System (Priority: Configurable via HeavyAttackConfig.Priority)
        // ============================================================================

        /// <summary>
        /// Process heavy attack with charging mechanics.
        /// Holding HP button charges the attack, releasing executes it.
        /// Priority is configurable via HeavyAttackConfig.Priority (default: 50).
        /// </summary>
        /// <returns>True if attack was executed, false otherwise</returns>
        private bool ProcessHeavyAttack(Frame frame, ref Filter filter, SimpleInput2D input, HeavyAttackConfig config)
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

        private void ExecuteChargedHeavyAttack(Frame frame, ref Filter filter, HeavyAttackConfig config)
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
            FP damage = config.Damage * damageMultiplier;

            // Apply heavy attack
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, config.Cooldown);

            // Fire attack event with charge level
            frame.Events.AttackPerformed(filter.Entity, true, 0, damage, chargeLevel);

            // Log for debugging
            Log.Debug($"Charged Heavy Attack - Charge: {chargeLevel * 100}%, Damage: {damage}");
        }

        // ============================================================================
        // Light Attack System (Priority: Configurable via LightAttackConfig.Priority)
        // ============================================================================

        /// <summary>
        /// Process light attack with combo system.
        /// Consecutive light attacks within combo window increase damage.
        /// Priority is configurable via LightAttackConfig.Priority (default: 10).
        /// </summary>
        private void ProcessLightAttack(Frame frame, ref Filter filter, LightAttackConfig config)
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
            FP damage = config.Damage * config.ComboDamageMultipliers[comboIndex];

            // Apply attack
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, config.Cooldown);
            filter.AttackData->ComboResetTimer = FrameTimer.FromSeconds(frame, config.ComboWindow);

            // Fire attack event
            frame.Events.AttackPerformed(filter.Entity, false, filter.AttackData->ComboCounter, damage, 0);

            // Log for debugging
            Log.Debug($"Light Attack - Combo: {filter.AttackData->ComboCounter}, Damage: {damage}");

            // TODO: Apply damage to nearby enemies (implement hit detection)
        }
    }
}
