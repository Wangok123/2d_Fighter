using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct KCC2D
    {
        public void ResetVelocity()
        {
            _kinematicVelocity = FPVector2.Zero;
            _dynamicVelocity = FPVector2.Zero;
        }
    }
}
