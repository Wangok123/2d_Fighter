namespace Quantum
{
    using Photon.Deterministic;
    
    /// <summary>
    /// Focused system that handles modular ability execution
    /// Replaces the monolithic ModularAbilitySystem with better code organization
    /// Processes abilities from ModularCharacterConfig using priority-based execution
    /// </summary>
    public unsafe class AbilityInputSystem : SystemMainThreadFilter<AbilityInputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterStatus* Status;
            public AttackData* AttackData;
            public CharacterLevel* Level;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Skip if dead
            if (filter.Status->IsDead)
            {
                return;
            }

            // Get modular config
            if (!filter.AttackData->ModularConfig.Id.IsValid)
            {
                return; // No modular config, skip
            }

            var modularConfig = frame.FindAsset(filter.AttackData->ModularConfig);
            if (modularConfig == null)
            {
                return;
            }

            // Get input
            SimpleInput2D input = default;
            if (frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = *frame.GetPlayerInput(playerLink->Player);
            }

            // Update timers
            UpdateAbilityTimers(frame, ref filter);

            // Early return if on cooldown
            if (filter.AttackData->AttackCooldown.IsRunning(frame))
            {
                filter.AttackData->IsAttacking = false;
                return;
            }

            // Process abilities by priority
            ProcessAbilitiesByPriority(frame, filter.Entity, filter.AttackData, filter.Level, input, modularConfig);
        }

        private void UpdateAbilityTimers(Frame frame, ref Filter filter)
        {
            // Reset combo if timer expired
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }
        }

        private void ProcessAbilitiesByPriority(Frame frame, EntityRef entityRef, AttackData* attackData,
            CharacterLevel* level, SimpleInput2D input, ModularCharacterConfig config)
        {
            // Track the highest priority ability to execute
            int highestPriority = int.MinValue;
            System.Action abilityToExecute = null;

            // Check attack abilities
            if (config.AttackAbilities != null)
            {
                foreach (var abilityRef in config.AttackAbilities)
                {
                    var ability = frame.FindAsset(abilityRef);
                    if (ability != null && IsAbilityUnlocked(level, ability))
                    {
                        if (ShouldExecuteAttackAbility(input, ability))
                        {
                            if (ability.Priority > highestPriority)
                            {
                                highestPriority = ability.Priority;
                                abilityToExecute = () => ExecuteAttackAbility(frame, attackData, entityRef, level, input, ability);
                            }
                        }
                    }
                }
            }

            // Check special abilities
            if (config.SpecialAbilities != null)
            {
                foreach (var abilityRef in config.SpecialAbilities)
                {
                    var ability = frame.FindAsset(abilityRef);
                    if (ability != null && IsAbilityUnlocked(level, ability))
                    {
                        if (frame.Unsafe.TryGetPointer(entityRef, out CommandInputData* commandData))
                        {
                            if (ShouldExecuteSpecialAbility(commandData, ability))
                            {
                                if (ability.Priority > highestPriority)
                                {
                                    highestPriority = ability.Priority;
                                    abilityToExecute = () => ExecuteSpecialAbility(frame, attackData, entityRef, commandData, ability);
                                }
                            }
                        }
                    }
                }
            }

            // Execute the highest priority ability if any
            if (abilityToExecute != null)
            {
                abilityToExecute();
            }
        }

        #region Ability Unlock Check
        
        private bool IsAbilityUnlocked(CharacterLevel* level, AbilityComponentBase ability)
        {
            if (level == null) return ability.UnlockedByDefault;
            return ability.UnlockedByDefault || level->CurrentLevel >= ability.RequiredLevel;
        }
        
        #endregion

        #region Attack Ability Execution

        private bool ShouldExecuteAttackAbility(SimpleInput2D input, AttackAbilityComponent ability)
        {
            switch (ability.AttackType)
            {
                case AttackAbilityType.LightMelee:
                    return input.LP.WasPressed;
                case AttackAbilityType.HeavyMelee:
                    return input.HP.WasPressed || input.HP.IsDown;
                default:
                    return false;
            }
        }

        private void ExecuteAttackAbility(Frame frame, AttackData* attackData, EntityRef entity, CharacterLevel* level,
            SimpleInput2D input, AttackAbilityComponent ability)
        {
            // Handle combo system
            if (ability.CanCombo)
            {
                if (attackData->ComboCounter < ability.MaxComboCount)
                {
                    attackData->ComboCounter++;
                }
                else
                {
                    attackData->ComboCounter = 1;
                }
            }
            else
            {
                attackData->ComboCounter = 0;
            }

            // Calculate damage
            FP damage = ability.BaseDamage;
            if (level != null)
            {
                damage += ability.DamagePerLevel * level->CurrentLevel;
            }

            // Apply combo multiplier
            if (ability.CanCombo && attackData->ComboCounter > 0)
            {
                int comboIndex = FPMath.Clamp(attackData->ComboCounter - 1, 0,
                    ability.ComboDamageMultipliers.Length - 1);
                damage *= ability.ComboDamageMultipliers[comboIndex];
            }

            // Apply charge multiplier for heavy attacks
            if (ability.CanCharge && input.HP.IsDown)
            {
                // Charging logic
                attackData->IsChargingHeavy = true;
                attackData->HeavyChargeTime += frame.DeltaTime;
                return; // Don't execute yet, still charging
            }
            else if (ability.CanCharge && attackData->IsChargingHeavy)
            {
                // Release charged attack
                FP chargeLevel = FPMath.Clamp01((attackData->HeavyChargeTime - ability.MinChargeTime) /
                                                (ability.MaxChargeTime - ability.MinChargeTime));
                FP chargeMultiplier = FP._1 + (chargeLevel * (ability.FullChargeDamageMultiplier - FP._1));
                damage *= chargeMultiplier;

                attackData->IsChargingHeavy = false;
                attackData->HeavyChargeTime = 0;
            }

            // Apply attack
            attackData->IsAttacking = true;
            attackData->AttackCooldown = FrameTimer.FromSeconds(frame, ability.Cooldown);

            if (ability.CanCombo)
            {
                attackData->ComboResetTimer = FrameTimer.FromSeconds(frame, ability.ComboWindow);
            }

            // Fire event
            bool isHeavy = ability.AttackType == AttackAbilityType.HeavyMelee;
            frame.Events.AttackPerformed(entity, isHeavy, attackData->ComboCounter, damage, 0);

            Log.Debug($"Attack Ability: {ability.AbilityName} - Type: {ability.AttackType}, Damage: {damage}");
        }

        #endregion

        #region Special Ability Execution

        private bool ShouldExecuteSpecialAbility(CommandInputData* commandData, SpecialAbilityComponent ability)
        {
            if (ability.InputSequence == null || ability.InputSequence.Length == 0)
            {
                return false;
            }

            return CommandInputSystem.MatchesSequence(commandData, ability.InputSequence);
        }

        private void ExecuteSpecialAbility(Frame frame, AttackData* attackData, EntityRef entity,
            CommandInputData* commandData, SpecialAbilityComponent ability)
        {
            // Clear input buffer
            CommandInputSystem.ClearInputBuffer(commandData);

            // Apply cooldown
            attackData->IsAttacking = true;
            attackData->AttackCooldown = FrameTimer.FromSeconds(frame, ability.Cooldown);

            // Fire event
            frame.Events.SpecialMovePerformed(entity, 0, ability.Damage);

            Log.Info($"Special Ability: {ability.AbilityName} - Type: {ability.SpecialType}, Damage: {ability.Damage}");
        }

        #endregion
    }
}
