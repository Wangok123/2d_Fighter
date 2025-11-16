namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public CharacterStatusComponent* PlayerStatus;
            public Transform3D* Transform;
            public CharacterController3D* KCC;
            public AbilityInventory* AbilityInventory;
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
        }

        
    }
}
