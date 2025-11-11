using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class PushFieldData : SkillFieldData
    {
        [Header("力场设置")]
        [Tooltip("力场类型")]
        public ForceFieldType FieldType = ForceFieldType.Push;
        
        [Tooltip("力场强度")]
        public FP ForceStrength = 5;
        
        [Tooltip("力场方向")]
        public ForceDirection Direction = ForceDirection.FromCenter;
        
        [Tooltip("自定义方向（使用CustomDirection时）")]
        public FPVector2 CustomDirection = FPVector2.Up;

        [Header("高级设置")]
        [Tooltip("是否受距离衰减")]
        public bool FalloffWithDistance = true;
        
        [Tooltip("最大影响距离")]
        public FP MaxEffectRange = 5;
        
        [Tooltip("是否持续施加力")]
        public bool ContinuousForce = true;
    }

    public enum ForceFieldType
    {
        Push,
        Pull
    }

    public enum ForceDirection
    {
        FromCenter,
        ToCenter,
        CustomDirection
    }
}