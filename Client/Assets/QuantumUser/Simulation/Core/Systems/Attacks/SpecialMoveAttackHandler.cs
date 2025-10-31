namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Handler for special move attacks (highest priority).
    /// Checks for command input sequences and executes special moves.
    /// </summary>
    public unsafe class SpecialMoveAttackHandler : IAttackHandler
    {
        public int Priority => 100; // Highest priority

        public bool CanExecute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            // Check if entity has command input data
            if (!frame.Unsafe.TryGetPointer(filter.Entity, out CommandInputData* commandData))
            {
                return false;
            }

            // Check if any special move sequence matches
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
                        if (level->CurrentLevel >= moveConfig.RequiredLevel)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // No level component, allow execution
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Execute(Frame frame, ref NormalAttackSystem.Filter filter, SimpleInput2D input, AttackConfig config)
        {
            if (!frame.Unsafe.TryGetPointer(filter.Entity, out CommandInputData* commandData))
            {
                return false;
            }

            // Find matching special move
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

        private void ExecuteSpecialMove(Frame frame, ref NormalAttackSystem.Filter filter, SpecialMoveConfig moveConfig)
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
    }
}
