namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Signal-based system that processes player input for movement
    /// Fires OnMovementInput signal for other systems to handle
    /// </summary>
    public unsafe class MovementInputSystem : SystemMainThreadFilter<MovementInputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
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
            SimpleInput2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);
            
            // Fire signal for other systems to process
            frame.Signals.OnMovementInput(filter.Entity, input);
        }
    }
}
