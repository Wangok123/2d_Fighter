// DEPRECATED: This system has been replaced by AbilityInputSystem
// The new system uses ModularCharacterConfig as the single source of configuration
// and provides better code organization with focused responsibility
// This file is kept for backward compatibility but should not be used in new code.

namespace Quantum
{
    using Photon.Deterministic;
    using System.Collections.Generic;

    /// <summary>
    /// System for handling modular ability components in an ECS-style approach.
    /// This system processes abilities from ModularCharacterConfig and executes them
    /// based on their priority and availability.
    /// 
    /// Compatible with existing systems - can work alongside legacy attack configs.
    /// </summary>
    public unsafe class ModularAbilitySystem : SystemMainThreadFilter<ModularAbilitySystem.Filter>
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

            // Get modular character config
            var modularConfig = GetModularConfig(frame, ref filter);
            if (modularConfig == null)
            {
                // No modular config, system does nothing (legacy system handles it)
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

        /// <summary>
        /// Get the modular character config for this entity
        /// Returns null if entity uses legacy config system
        /// </summary>
        private ModularCharacterConfig GetModularConfig(Frame frame, ref Filter filter)
        {
            // Check if entity has a modular config reference
            if (filter.AttackData->ModularConfig.Id.IsValid)
            {
                return frame.FindAsset(filter.AttackData->ModularConfig);
            }
            
            // No modular config, entity uses legacy system
            return null;
        }

        /// <summary>
        /// Update ability-related timers
        /// </summary>
        private void UpdateAbilityTimers(Frame frame, ref Filter filter)
        {
            // Reset combo if timer expired
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }
        }

        /// <summary>
        /// Process all abilities in order of their priority values
        /// </summary>
        private void ProcessAbilitiesByPriority(Frame frame, EntityRef entityRef, AttackData* attackData,
            CharacterLevel* level, SimpleInput2D input, ModularCharacterConfig config)
        {
            // Collect all abilities with their priorities
            var abilitiesToProcess = new List<(int priority, System.Action action)>();

            // Add attack abilities
            if (config.AttackAbilities != null)
            {
                foreach (var abilityRef in config.AttackAbilities)
                {
                    var ability = frame.FindAsset(abilityRef);
                    if (ability != null && IsAbilityUnlocked(level, ability))
                    {
                        // Check if input matches this ability
                        if (ShouldExecuteAttackAbility(input, ability))
                        {
                            abilitiesToProcess.Add((ability.Priority,
                                () => ExecuteAttackAbility(frame, attackData, entityRef, level, input, ability)));
                        }
                    }
                }
            }

            // Add defense abilities
            if (config.DefenseAbilities != null)
            {
                foreach (var abilityRef in config.DefenseAbilities)
                {
                    var ability = frame.FindAsset(abilityRef);
                    if (ability != null && IsAbilityUnlocked(level, ability))
                    {
                        if (ShouldExecuteDefenseAbility(input, ability))
                        {
                            abilitiesToProcess.Add((ability.Priority,
                                () => ExecuteDefenseAbility(frame, ability)));
                        }
                    }
                }
            }

            // Add special abilities
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
                                abilitiesToProcess.Add((ability.Priority,
                                    () => ExecuteSpecialAbility(frame, attackData, entityRef, commandData, ability)));
                            }
                        }
                    }
                }
            }

            // Sort by priority (highest first) and execute first match
            abilitiesToProcess.Sort((a, b) => b.priority.CompareTo(a.priority));

            foreach (var (priority, action) in abilitiesToProcess)
            {
                action();
                return; // Only execute one ability per frame
            }
        }

        /// <summary>
        /// Check if an ability is unlocked for the current character level
        /// </summary>
        private bool IsAbilityUnlocked(CharacterLevel* level, AbilityComponentBase ability)
        {
            if (level == null) return ability.UnlockedByDefault;
            return ability.UnlockedByDefault || level->CurrentLevel >= ability.RequiredLevel;
        }

        #region Attack Ability Execution

        private bool ShouldExecuteAttackAbility(SimpleInput2D input, AttackAbilityComponent ability)
        {
            // Map ability types to inputs
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
                // Charging logic would go here
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

            Log.Debug($"Modular Attack: {ability.AbilityName} - Type: {ability.AttackType}, Damage: {damage}");
        }

        #endregion

        #region Defense Ability Execution

        private bool ShouldExecuteDefenseAbility(SimpleInput2D input, DefenseAbilityComponent ability)
        {
            // For now, check block button
            // This could be expanded for different defense types
            return input.Block.IsDown;
        }

        private void ExecuteDefenseAbility(Frame frame, DefenseAbilityComponent ability)
        {
            // Defense execution logic
            // This would integrate with a defense/block system
            Log.Debug($"Modular Defense: {ability.AbilityName} - Type: {ability.DefenseType}");
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

            Log.Info($"Modular Special: {ability.AbilityName} - Type: {ability.SpecialType}, Damage: {ability.Damage}");
        }

        #endregion
    }
}