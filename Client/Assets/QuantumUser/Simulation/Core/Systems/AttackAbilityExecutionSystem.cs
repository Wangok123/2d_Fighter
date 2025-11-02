namespace Quantum
{
    using Photon.Deterministic;
    using System.Collections.Generic;

    /// <summary>
    /// Signal-based system that executes attack abilities
    /// Listens to OnAbilityExecute signal and processes attack abilities
    /// </summary>
    public unsafe class AttackAbilityExecutionSystem : SystemSignalsOnly, ISignalOnAbilityExecute
    {
        public void OnAbilityExecute(Frame frame, EntityRef entity, AbilityId abilityId, SimpleInput2D input)
        {
            // Get required components
            if (!frame.Unsafe.TryGetPointer(entity, out AttackData* attackData))
            {
                return;
            }

            if (!frame.Unsafe.TryGetPointer(entity, out CharacterLevel* level))
            {
                return;
            }

            // Get modular config
            if (!attackData->ModularConfig.Id.IsValid)
            {
                return;
            }

            var modularConfig = frame.FindAsset(attackData->ModularConfig);
            if (modularConfig == null || modularConfig.AttackAbilities == null)
            {
                return;
            }

            // Process attack abilities by priority
            ProcessAttackAbilitiesByPriority(frame, entity, attackData, level, input, modularConfig);
        }

        private void ProcessAttackAbilitiesByPriority(Frame frame, EntityRef entity, AttackData* attackData,
            CharacterLevel* level, SimpleInput2D input, ModularCharacterConfig config)
        {
            // Collect all matching attack abilities with their priorities
            var abilitiesToProcess = new List<(int priority, AttackAbilityComponent ability)>();

            foreach (var abilityRef in config.AttackAbilities)
            {
                var ability = frame.FindAsset(abilityRef);
                if (ability != null && IsAbilityUnlocked(level, ability))
                {
                    // Check if input matches this ability
                    if (ShouldExecuteAttackAbility(input, ability))
                    {
                        abilitiesToProcess.Add((ability.Priority, ability));
                    }
                }
            }

            // Sort by priority (highest first) and execute first match
            abilitiesToProcess.Sort((a, b) => b.priority.CompareTo(a.priority));

            if (abilitiesToProcess.Count > 0)
            {
                ExecuteAttackAbility(frame, attackData, entity, level, input, abilitiesToProcess[0].ability);
            }
        }

        private bool IsAbilityUnlocked(CharacterLevel* level, AbilityComponentBase ability)
        {
            if (level == null) return ability.UnlockedByDefault;
            return ability.UnlockedByDefault || level->CurrentLevel >= ability.RequiredLevel;
        }

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
    }
}
