using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe class PlayerHitReactionData : HitReactionData
    {
        [Header("Hitstun Settings")]
        [Tooltip("轻击硬直倍率")]
        public FP LightHitStunMultiplier = FP._0_50;
        
        [Tooltip("重击硬直倍率")]
        public FP HeavyHitStunMultiplier = FP._1_50;
        
        [Header("Knockback Settings")]
        public KnockbackCurveProfile KnockbackProfile = new KnockbackCurveProfile();
        
        [Header("Combat Behavior")]
        [Tooltip("受击时是否打断当前动作")]
        public bool HitInterruptsActions = true;

        protected override FP CalculateHitstunDuration(FP baseDuration, HitType hitType)
        {
            FP multiplier = hitType switch
            {
                HitType.Light => LightHitStunMultiplier,
                HitType.Heavy => HeavyHitStunMultiplier,
                HitType.Launch => HeavyHitStunMultiplier,
                _ => FP._1
            };
            
            return baseDuration * multiplier;
        }

        protected override KnockbackConfig GetKnockbackConfig()
        {
            return KnockbackProfile.ToConfig();
        }

        protected override FP EvaluateHorizontalCurve(FP normalizedTime)
        {
            if (KnockbackProfile.HorizontalCurve == null)
                return FP._1 - normalizedTime;
            
            float t = normalizedTime.AsFloat;
            return FP.FromFloat_UNSAFE(KnockbackProfile.HorizontalCurve.Evaluate(t));
        }

        protected override FP EvaluateVerticalCurve(FP normalizedTime)
        {
            if (KnockbackProfile.VerticalCurve == null)
                return FP._1 - normalizedTime;
            
            float t = normalizedTime.AsFloat;
            return FP.FromFloat_UNSAFE(KnockbackProfile.VerticalCurve.Evaluate(t));
        }

        protected override void OnHitstunStarted(Frame frame, EntityRef target, HitReactionComponent* hitReaction, HitType hitType)
        {
            if (HitInterruptsActions)
            {
                base.OnHitstunStarted(frame, target, hitReaction, hitType);
            }
        }
    }
}
