using Photon.Deterministic;

namespace Quantum
{
    public class KnockbackStatusEffectData : AssetObject
    {
        public FP KnockbackDistanceXZ = 6;
        public FP KnockbackDistanceY = 1;
        public FPAnimationCurve KnockbackCurveXZ;
        public FPAnimationCurve KnockbackCurveY;
    }
}