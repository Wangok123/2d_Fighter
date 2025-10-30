namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class AbilitySystem : SystemMainThreadFilter<AbilitySystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
        }

        public struct Filter
        {
            public EntityRef Entity;
        }
    }
}
