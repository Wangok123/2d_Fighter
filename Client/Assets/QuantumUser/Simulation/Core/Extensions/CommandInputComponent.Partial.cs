using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe partial struct CommandInputComponent
    {
        private const int MAX_INPUT_BUFFER_SIZE = 8;
        private static readonly FP INPUT_EXPIRY_TIME = FP._0_50;

        public void RecordInput(Frame frame, CommandInput input)
        {
            if (input == CommandInput.None)
                return;

            if (InputBufferSize >= MAX_INPUT_BUFFER_SIZE)
            {
                ShiftBuffer();
            }

            InputBuffer[InputBufferIndex] = (int)input;
            InputBufferIndex = (InputBufferIndex + 1) % MAX_INPUT_BUFFER_SIZE;
            InputBufferSize = Mathf.Min(InputBufferSize + 1, MAX_INPUT_BUFFER_SIZE);

            InputExpiryTimer = FrameTimer.FromSeconds(frame, INPUT_EXPIRY_TIME);
        }

        public bool CheckCommandSequence(Frame frame, CommandInput[] sequence)
        {
            if (sequence == null || sequence.Length == 0 || sequence.Length > InputBufferSize)
                return false;

            if (!InputExpiryTimer.IsRunning(frame))
                return false;

            int sequenceIndex = sequence.Length - 1;
            int bufferIndex = (InputBufferIndex - 1 + MAX_INPUT_BUFFER_SIZE) % MAX_INPUT_BUFFER_SIZE;

            for (int i = 0; i < InputBufferSize && sequenceIndex >= 0; i++)
            {
                CommandInput bufferInput = (CommandInput)InputBuffer[bufferIndex];

                if (bufferInput == sequence[sequenceIndex])
                {
                    sequenceIndex--;
                }

                bufferIndex = (bufferIndex - 1 + MAX_INPUT_BUFFER_SIZE) % MAX_INPUT_BUFFER_SIZE;
            }

            return sequenceIndex < 0;
        }

        public void ClearBuffer()
        {
            InputBufferSize = 0;
            InputBufferIndex = 0;
            InputExpiryTimer = FrameTimer.None;

            for (int i = 0; i < MAX_INPUT_BUFFER_SIZE; i++)
            {
                InputBuffer[i] = 0;
            }
        }

        private void ShiftBuffer()
        {
            for (int i = 0; i < MAX_INPUT_BUFFER_SIZE - 1; i++)
            {
                InputBuffer[i] = InputBuffer[i + 1];
            }

            InputBufferSize--;
            InputBufferIndex = (InputBufferIndex - 1 + MAX_INPUT_BUFFER_SIZE) % MAX_INPUT_BUFFER_SIZE;
        }

        public CommandInput GetDirectionInput(SimpleInput2D input, bool isFacingRight)
        {
            bool left = input.Left;
            bool right = input.Right;
            bool up = input.Up;
            bool down = input.Down;

            if (!isFacingRight)
            {
                (left, right) = (right, left);
            }

            if (down && right)
                return CommandInput.DownRight;
            if (down && left)
                return CommandInput.DownLeft;
            if (up && right)
                return CommandInput.UpRight;
            if (up && left)
                return CommandInput.UpLeft;
            if (down)
                return CommandInput.Down;
            if (right)
                return CommandInput.Right;
            if (up)
                return CommandInput.Up;
            if (left)
                return CommandInput.Left;

            return CommandInput.None;
        }

        public CommandInput GetButtonInput(SimpleInput2D input)
        {
            if (input.LP.WasPressed)
                return CommandInput.LP;
            if (input.HP.WasPressed)
                return CommandInput.HP;
            if (input.Dash.WasPressed)
                return CommandInput.Dash;
            if (input.Jump.WasPressed)
                return CommandInput.Jump;

            return CommandInput.None;
        }
    }
}