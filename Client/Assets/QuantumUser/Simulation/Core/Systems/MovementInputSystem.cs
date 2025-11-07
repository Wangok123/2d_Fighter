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
            public CharacterStatusComponent* Status;
            public MovementComponent* MovementData;
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

            UpdateIsFacingRight(input, filter.MovementData, filter.KCC);
        }
        
        private void UpdateIsFacingRight(SimpleInput2D input, MovementComponent* movementData, KCC2D* kcc)
        {
            FP horizontalVelocity = kcc->CombinedVelocity.X;
    
            // 只有当速度足够大时才更新朝向，避免微小抖动
            FP threshold = FP._0_10; // 0.1的速度阈值
    
            if (FPMath.Abs(horizontalVelocity) > threshold)
            {
                movementData->IsFacingRight = horizontalVelocity > 0;
            }
        }
    }
}