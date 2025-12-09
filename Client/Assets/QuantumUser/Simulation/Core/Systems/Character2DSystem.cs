using UnityEngine.Scripting;

namespace Quantum
{
    using Photon.Deterministic;

    [Preserve]
    public unsafe class Character2DSystem : SystemMainThreadFilter<Character2DSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public CharacterController2D* KCC;
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
            bool isKnockedBack = frame.Has<KnockbackComponent>(filter.EntityRef) && 
                                 frame.Get<KnockbackComponent>(filter.EntityRef).IsKnockedBack;
            
            if (isKnockedBack)
            {
                filter.KCC->Velocity = FPVector2.Lerp(filter.KCC->Velocity, FPVector2.Zero, FP._10 * frame.DeltaTime);
            }
            
            FPVector2 movementDirection;
            
            if (isKnockedBack)
            {
                movementDirection = FPVector2.Zero;
            }
            else
            {
                movementDirection = GetMovementDirection(frame, filter.EntityRef);
                
                if (movementDirection.SqrMagnitude > FP._1)
                {
                    movementDirection = movementDirection.Normalized;
                }
            }
            
            filter.KCC->Move(frame, filter.EntityRef, movementDirection);
        }
        
        private FPVector2 GetMovementDirection(Frame frame, EntityRef entityRef)
        {
            return FPVector2.Zero;
        }
    }
}