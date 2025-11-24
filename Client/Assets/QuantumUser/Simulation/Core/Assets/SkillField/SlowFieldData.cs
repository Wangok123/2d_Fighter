using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class SlowFieldData : SkillFieldData
    {
        [Header("减速设置")]
        [Tooltip("移动速度减少百分比（0-1）")]
        [Range(0f, 1f)]
        public FP SpeedReductionPercent = FP._0_50;
        
        [Tooltip("减速持续时间（离开区域后）")]
        public FP SlowLingerDuration = FP._0_50;
        
        [Tooltip("是否叠加减速")]
        public bool StackableSlows = false;
        
        [Tooltip("最大叠加层数")]
        public int MaxStacks = 3;
        
        [Tooltip("每层额外减速")]
        public FP AdditionalSlowPerStack = FP._0_10;

        [Header("视觉效果")]
        [Tooltip("是否显示减速特效")]
        public bool ShowSlowEffect = true;
        
        [Tooltip("减速特效颜色提示")]
        public SlowEffectType EffectType = SlowEffectType.Ice;
        
        public override void ApplyEffect(Frame frame, EntityRef skillFieldEntity, 
            SkillFieldComponent* skillField, EntityRef target, FPVector2 hitPoint)
        {
            // 应用减速逻辑
            // TODO: 实现减速系统后添加
        }
    }

    public enum SlowEffectType
    {
        Ice,
        Mud,
        Web,
        Gravity
    }
}