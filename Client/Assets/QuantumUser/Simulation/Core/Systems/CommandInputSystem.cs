namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// System for tracking command inputs for special moves (Street Fighter style)
    /// </summary>
    public unsafe class CommandInputSystem : SystemMainThreadFilter<CommandInputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CommandInputData* CommandData;
            public AttackData* AttackData;
            public MovementData* MovementData;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            // Get character attack config
            var characterConfig = frame.FindAsset(filter.AttackData->AttackConfig);
            if (characterConfig == null)
            {
                return;
            }

            // Get input
            SimpleInput2D input = default;
            if (frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = *frame.GetPlayerInput(playerLink->Player);
            }

            // Convert current input to command input
            int currentInput = ConvertToCommandInput(input, filter.MovementData->IsFacingRight);

            // Add to buffer if it's a new input
            if (currentInput != (int)CommandInput.None)
            {
                AddInputToBuffer(frame, ref filter, currentInput, characterConfig);
            }

            // Expire old inputs
            if (filter.CommandData->InputExpiryTimer.IsRunning(frame) == false && filter.CommandData->InputBufferIndex > 0)
            {
                // Clear buffer after expiry
                filter.CommandData->InputBufferIndex = 0;
            }
        }

        private int ConvertToCommandInput(SimpleInput2D input, bool isFacingRight)
        {
            // Check for button presses first (higher priority)
            if (input.LP.WasPressed) return (int)CommandInput.LP;
            if (input.HP.WasPressed) return (int)CommandInput.HP;
            if (input.Dash.WasPressed) return (int)CommandInput.Dash;
            if (input.Jump.WasPressed) return (int)CommandInput.Jump;

            // Check for directional inputs
            bool down = input.Down.IsDown;
            bool up = input.Up.IsDown;
            bool right = input.Right.IsDown;
            bool left = input.Left.IsDown;

            // Adjust for facing direction (relative to character)
            bool forward = isFacingRight ? right : left;
            bool backward = isFacingRight ? left : right;

            // Convert to numpad notation (relative to facing direction)
            if (down && forward) return (int)CommandInput.DownRight;
            if (down && backward) return (int)CommandInput.DownLeft;
            if (down) return (int)CommandInput.Down;
            if (up && forward) return (int)CommandInput.UpRight;
            if (up && backward) return (int)CommandInput.UpLeft;
            if (up) return (int)CommandInput.Up;
            if (forward) return (int)CommandInput.Right;
            if (backward) return (int)CommandInput.Left;

            return (int)CommandInput.None;
        }

        private void AddInputToBuffer(Frame frame, ref Filter filter, int input, CharacterAttackConfig config)
        {
            // Don't add duplicate inputs
            if (filter.CommandData->InputBufferIndex > 0)
            {
                int lastInput = filter.CommandData->InputBuffer[filter.CommandData->InputBufferIndex - 1];
                if (lastInput == input)
                {
                    return;
                }
            }

            // Add input to buffer
            if (filter.CommandData->InputBufferIndex < config.MaxInputBufferSize)
            {
                filter.CommandData->InputBuffer[filter.CommandData->InputBufferIndex] = input;
                filter.CommandData->InputBufferIndex++;
            }
            else
            {
                // Shift buffer left and add new input
                for (int i = 0; i < config.MaxInputBufferSize - 1; i++)
                {
                    filter.CommandData->InputBuffer[i] = filter.CommandData->InputBuffer[i + 1];
                }
                filter.CommandData->InputBuffer[config.MaxInputBufferSize - 1] = input;
            }

            // Reset expiry timer
            filter.CommandData->InputExpiryTimer = FrameTimer.FromSeconds(frame, config.CommandInputWindow);
        }

        /// <summary>
        /// Check if the input buffer matches a special move sequence
        /// </summary>
        public static bool MatchesSequence(CommandInputData* commandData, int[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return false;
            }

            int sequenceLength = sequence.Length;
            int bufferIndex = commandData->InputBufferIndex;

            if (bufferIndex < sequenceLength)
            {
                return false;
            }

            // Check if the last N inputs match the sequence
            int startIndex = bufferIndex - sequenceLength;
            for (int i = 0; i < sequenceLength; i++)
            {
                if (commandData->InputBuffer[startIndex + i] != sequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clear the input buffer (called after executing a special move)
        /// </summary>
        public static void ClearInputBuffer(CommandInputData* commandData)
        {
            commandData->InputBufferIndex = 0;
        }
    }
}
