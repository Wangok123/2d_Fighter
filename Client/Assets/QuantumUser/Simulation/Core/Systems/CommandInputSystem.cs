namespace Quantum
{
    public unsafe class CommandInputSystem: SystemMainThreadFilter<CommandInputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public CommandInputComponent* CommandInput;
            public MovementComponent* Movement;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.CommandInput->InputExpiryTimer.IsRunning(frame))
            {
                if (filter.CommandInput->InputExpiryTimer.IsExpired(frame))
                {
                    filter.CommandInput->ClearBuffer();
                }
            }
        }
    }
}