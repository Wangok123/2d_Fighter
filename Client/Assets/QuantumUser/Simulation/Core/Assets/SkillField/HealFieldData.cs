using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class HealFieldData : SkillFieldData
    {
        [Header("治疗设置")]
        [Tooltip("每Tick治疗量")]
        public FP HealPerTick = 5;
        
        [Tooltip("是否基于最大生命值百分比治疗")]
        public bool HealByPercentage = false;
        
        [Tooltip("治疗百分比（0-1）")]
        [Range(0f, 1f)]
        public FP HealPercentage = FP._0_10;
        
        [Tooltip("最小治疗量（百分比模式）")]
        public FP MinHealAmount = 1;

        [Header("额外效果")]
        [Tooltip("是否提供护盾")]
        public bool GrantShield = false;
        
        [Tooltip("护盾值")]
        public FP ShieldAmount = 10;
        
        [Tooltip("护盾持续时间")]
        public FP ShieldDuration = 5;
        
        [Tooltip("是否移除负面状态")]
        public bool RemoveDebuffs = false;

        public override void ApplyEffect(Frame frame, EntityRef skillFieldEntity, EntityRef target, FPVector2 hitPoint)
        {
            ApplyHealToTarget(frame, target);
            
            if (GrantShield)
            {
                ApplyShieldToTarget(frame, target);
            }
            
            if (RemoveDebuffs)
            {
                RemoveDebuffsFromTarget(frame, target);
            }
        }

        private void ApplyHealToTarget(Frame frame, EntityRef target)
        {
        }

        private void ApplyShieldToTarget(Frame frame, EntityRef target)
        {
        }

        private void RemoveDebuffsFromTarget(Frame frame, EntityRef target)
        {
        }
    }
}