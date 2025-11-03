namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Lightweight system that handles player input and movement execution
    /// Replaces the monolithic MovementSystem with better separation of concerns
    /// </summary>
    public unsafe class MovementInputSystem : SystemMainThreadFilter<MovementInputSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PlayerLink* PlayerLink;
            public CharacterStatus* Status;
            public MovementData* MovementData;
            public KCC2D* KCC;
            public AbilityEnable* AbilityEnabled;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Status->IsDead == true)
            {
                return;
            }

            // Get input
            SimpleInput2D input = *frame.GetPlayerInput(filter.PlayerLink->Player);

            var config = frame.FindAsset(filter.KCC->Config);
            filter.KCC->Input = input;
            config.MoveWithAbility(frame, filter.Entity, filter.Transform, filter.KCC, filter.AbilityEnabled);

            UpdateIsFacingRight(input, filter.MovementData);
        }
        
        private void UpdateIsFacingRight(SimpleInput2D input, MovementData* movementData)
        {
            bool noInput = !input.Left.IsDown && !input.Right.IsDown;
            if (noInput)
                return;

            // When both directions are pressed simultaneously, maintain current facing direction
            // This prevents rapid direction flipping and maintains control consistency
            if (input.Left.IsDown && input.Right.IsDown)
                return;

            movementData->IsFacingRight = input.Right.IsDown;
        }
    }
}