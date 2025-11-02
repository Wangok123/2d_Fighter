namespace Quantum
{
    using Photon.Deterministic;
    using System.Collections.Generic;

    /// <summary>
    /// Signal-based system that executes special abilities
    /// Listens to OnAbilityExecute signal and processes special abilities
    /// </summary>
    public unsafe class SpecialAbilityExecutionSystem : SystemSignalsOnly, ISignalOnAbilityExecute
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

            if (!frame.Unsafe.TryGetPointer(entity, out CommandInputData* commandData))
            {
                return;
            }

            // Get modular config
            if (!attackData->ModularConfig.Id.IsValid)
            {
                return;
            }

            var modularConfig = frame.FindAsset(attackData->ModularConfig);
            if (modularConfig == null || modularConfig.SpecialAbilities == null)
            {
                return;
            }

            // Process special abilities by priority
            ProcessSpecialAbilitiesByPriority(frame, entity, attackData, level, commandData, modularConfig);
        }

        private void ProcessSpecialAbilitiesByPriority(Frame frame, EntityRef entity, AttackData* attackData,
            CharacterLevel* level, CommandInputData* commandData, ModularCharacterConfig config)
        {
            // Collect all matching special abilities with their priorities
            var abilitiesToProcess = new List<(int priority, SpecialAbilityComponent ability)>();

            foreach (var abilityRef in config.SpecialAbilities)
            {
                var ability = frame.FindAsset(abilityRef);
                if (ability != null && IsAbilityUnlocked(level, ability))
                {
                    if (ShouldExecuteSpecialAbility(commandData, ability))
                    {
                        abilitiesToProcess.Add((ability.Priority, ability));
                    }
                }
            }

            // Sort by priority (highest first) and execute first match
            abilitiesToProcess.Sort((a, b) => b.priority.CompareTo(a.priority));

            if (abilitiesToProcess.Count > 0)
            {
                ExecuteSpecialAbility(frame, attackData, entity, commandData, abilitiesToProcess[0].ability);
            }
        }

        private bool IsAbilityUnlocked(CharacterLevel* level, AbilityComponentBase ability)
        {
            if (level == null) return ability.UnlockedByDefault;
            return ability.UnlockedByDefault || level->CurrentLevel >= ability.RequiredLevel;
        }

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
    }
}
