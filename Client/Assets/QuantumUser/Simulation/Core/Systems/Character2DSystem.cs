namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class Character2DSystem : SystemMainThreadFilter<Character2DSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
        }

        public struct Filter
        {
            public EntityRef EntityRef;
            public CharacterStatusComponent* PlayerStatus;
            public Transform2D* Transform;
            public CharacterController2D* KCC;
            public AbilityInventory* AbilityInventory;
        }
    }
}
