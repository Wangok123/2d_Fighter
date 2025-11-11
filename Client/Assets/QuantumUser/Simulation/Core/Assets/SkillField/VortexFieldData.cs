using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class VortexFieldData : SkillFieldData
    {
        [Header("旋涡设置")]
        [Tooltip("旋转方向")]
        public VortexRotation RotationDirection = VortexRotation.Clockwise;
        
        [Tooltip("向心力强度")]
        public FP CentripetalForce = 3;
        
        [Tooltip("切向力强度（旋转速度）")]
        public FP TangentialForce = 5;
        
        [Tooltip("是否造成伤害")]
        public bool DealDamage = true;
        
        [Tooltip("每Tick伤害")]
        public FP DamagePerTick = 2;

        [Header("高级设置")]
        [Tooltip("旋涡核心半径")]
        public FP CoreRadius = FP._0_50;
        
        [Tooltip("在核心区域是否眩晕")]
        public bool StunInCore = false;
        
        [Tooltip("眩晕持续时间")]
        public FP StunDuration = FP._1;
    }

    public enum VortexRotation
    {
        Clockwise,
        CounterClockwise
    }
}