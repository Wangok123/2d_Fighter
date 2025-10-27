namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PlayerLink* PlayerLink;
            public Status* Status;
            public MovementData* MovementData;
            public KCC2D* KCC;
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Status->IsDead == true)
            {
                return;
            }

            SimpleInput2D input = default;
            if(frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink))
            {
                input = *frame.GetPlayerInput(playerLink->Player);
            }
            var config = frame.FindAsset(filter.KCC->Config);
            filter.KCC->Input = input;
            config.Move(frame, filter.Entity, filter.Transform, filter.KCC);
            UpdateIsFacingRight(frame, ref filter, input);
        }
        
        private void UpdateIsFacingRight(Frame frame, ref Filter filter, SimpleInput2D input)
        {
            bool noInput = !input.Left.IsDown && !input.Right.IsDown;
            if (noInput)
                return;
            
            filter.MovementData->IsFacingRight = input.Right.IsDown;
        }
    }
}
