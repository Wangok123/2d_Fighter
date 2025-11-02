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
            public CommandInputData* CommandData;
        }

        private struct PendingAbility
        {
            public byte AbilityType; // 0 = Attack, 1 = Special
            public int Index; // 在配置数组中的索引
            public int Priority;
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
                return;
            }

            var modularConfig = frame.FindAsset(filter.AttackData->ModularConfig);

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
            ProcessAbilitiesByPriority(frame, ref filter, input, modularConfig);
        }

        private void UpdateAbilityTimers(Frame frame, ref Filter filter)
        {
            // Reset combo if timer expired
            if (filter.AttackData->ComboResetTimer.IsRunning(frame) == false && filter.AttackData->ComboCounter > 0)
            {
                filter.AttackData->ComboCounter = 0;
            }
        }

        private void ProcessAbilitiesByPriority(Frame frame, ref Filter filter, SimpleInput2D input,
            ModularCharacterConfig config)
        {
            PendingAbility pendingAbility = default;
            pendingAbility.Priority = int.MinValue;

            // Check attack abilities
            if (config.AttackAbilities != null)
            {
                for (int i = 0; i < config.AttackAbilities.Length; i++)
                {
                    var abilityRef = config.AttackAbilities[i];
                    var ability = frame.FindAsset(abilityRef);

                    if (ability != null && IsAbilityUnlocked(filter.Level, ability))
                    {
                        if (ShouldExecuteAttackAbility(frame, filter.Entity, input, ability))
                        {
                            if (ability.Priority > pendingAbility.Priority)
                            {
                                pendingAbility.AbilityType = 0; // Attack ability
                                pendingAbility.Index = i;
                                pendingAbility.Priority = ability.Priority;
                            }
                        }
                    }
                }
            }

            // Check special abilities
            if (config.SpecialAbilities != null)
            {
                for (int i = 0; i < config.SpecialAbilities.Length; i++)
                {
                    var abilityRef = config.SpecialAbilities[i];
                    var ability = frame.FindAsset(abilityRef);

                    if (ability != null && IsAbilityUnlocked(filter.Level, ability))
                    {
                        if (ShouldExecuteSpecialAbility(frame, filter.Entity, 
                                frame.Unsafe.GetPointer<CommandInputData>(filter.Entity), ability))
                        {
                            if (ability.Priority > pendingAbility.Priority)
                            {
                                pendingAbility.AbilityType = 1; // Special ability
                                pendingAbility.Index = i;
                                pendingAbility.Priority = ability.Priority;
                            }
                        }
                    }
                }
            }

            if (pendingAbility.Priority > int.MinValue)
            {
                if (config.AttackAbilities == null)
                {
                    return;
                }

                if (pendingAbility.AbilityType == 1)
                {
                    var ability = frame.FindAsset(config.SpecialAbilities[pendingAbility.Index]);
                    ExecuteSpecialAbility(frame, ref filter, ability);
                }
                else
                {
                    var ability = frame.FindAsset(config.AttackAbilities[pendingAbility.Index]);
                    ExecuteAttackAbility(frame, ref filter, input, ability);
                }
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

        private bool ShouldExecuteAttackAbility(Frame frame, EntityRef entity, SimpleInput2D input, AttackAbilityComponent ability)
        {
            // Check if ability is enabled in AbilityEnable component
            if (!KCCAbilityIntegration.IsAbilityEnabled(frame, entity, ability.AbilityId))
            {
                return false;
            }

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

        private void ExecuteAttackAbility(Frame frame, ref Filter filter, SimpleInput2D input,
            AttackAbilityComponent ability)
        {
            // Handle combo system
            if (ability.CanCombo)
            {
                if (filter.AttackData->ComboCounter < ability.MaxComboCount)
                {
                    filter.AttackData->ComboCounter++;
                }
                else
                {
                    filter.AttackData->ComboCounter = 1;
                }
            }
            else
            {
                filter.AttackData->ComboCounter = 0;
            }

            // Calculate damage
            FP damage = ability.BaseDamage;
            if (filter.Level != null)
            {
                damage += ability.DamagePerLevel * filter.Level->CurrentLevel;
            }

            // Apply combo multiplier
            if (ability.CanCombo && filter.AttackData->ComboCounter > 0)
            {
                int comboIndex = FPMath.Clamp(filter.AttackData->ComboCounter - 1, 0,
                    ability.ComboDamageMultipliers.Length - 1);
                damage *= ability.ComboDamageMultipliers[comboIndex];
            }

            // Apply charge multiplier for heavy attacks
            if (ability.CanCharge && input.HP.IsDown)
            {
                // Charging logic
                filter.AttackData->IsChargingHeavy = true;
                filter.AttackData->HeavyChargeTime += frame.DeltaTime;
                return; // Don't execute yet, still charging
            }
            else if (ability.CanCharge && filter.AttackData->IsChargingHeavy)
            {
                // Release charged attack
                FP chargeLevel = FPMath.Clamp01((filter.AttackData->HeavyChargeTime - ability.MinChargeTime) /
                                                (ability.MaxChargeTime - ability.MinChargeTime));
                FP chargeMultiplier = FP._1 + (chargeLevel * (ability.FullChargeDamageMultiplier - FP._1));
                damage *= chargeMultiplier;

                filter.AttackData->IsChargingHeavy = false;
                filter.AttackData->HeavyChargeTime = 0;
            }

            // Apply attack
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, ability.Cooldown);

            if (ability.CanCombo)
            {
                filter.AttackData->ComboResetTimer = FrameTimer.FromSeconds(frame, ability.ComboWindow);
            }

            // Fire event
            bool isHeavy = ability.AttackType == AttackAbilityType.HeavyMelee;
            frame.Events.AttackPerformed(filter.Entity, isHeavy, filter.AttackData->ComboCounter, damage, 0);

            Log.Debug($"Attack Ability: {ability.AbilityName} - Type: {ability.AttackType}, Damage: {damage}");
        }

        #endregion

        #region Special Ability Execution

        private bool ShouldExecuteSpecialAbility(Frame frame, EntityRef entity, CommandInputData* commandData, SpecialAbilityComponent ability)
        {
            if (ability.InputSequence == null || ability.InputSequence.Length == 0)
            {
                return false;
            }

            // Check if ability is enabled in AbilityEnable component
            if (!KCCAbilityIntegration.IsAbilityEnabled(frame, entity, ability.AbilityId))
            {
                return false;
            }

            return CommandInputSystem.MatchesSequence(commandData, ability.InputSequence);
        }

        private void ExecuteSpecialAbility(Frame frame, ref Filter filter, SpecialAbilityComponent ability)
        {
            frame.Signals.OnClearInputBuffer(filter.Entity);

            // Apply cooldown
            filter.AttackData->IsAttacking = true;
            filter.AttackData->AttackCooldown = FrameTimer.FromSeconds(frame, ability.Cooldown);

            // Fire event
            frame.Events.SpecialMovePerformed(filter.Entity, 0, ability.Damage);

            Log.Info($"Special Ability: {ability.AbilityName} - Type: {ability.SpecialType}, Damage: {ability.Damage}");
        }

        #endregion
    }
}