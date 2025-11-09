using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    [System.Serializable]
    public class KnockbackCurveProfile
    {
        [Header("Core Config (Stored in qtn struct)")]
        [Tooltip("击退模式")]
        public KnockbackMode Mode = KnockbackMode.Physics;
        
        [Tooltip("水平速度衰减率（每秒）")]
        public FP HorizontalDecayRate = FP._1 + FP._0_50;
        
        [Tooltip("是否使用重力影响垂直速度")]
        public bool UseGravity = true;
        
        [Tooltip("曲线持续时间（秒）")]
        public FP CurveDuration = FP._1;
        
        [Tooltip("线性衰减率（每秒）")]
        public FP LinearDecayRate = FP._8;
        
        [Tooltip("最小速度阈值")]
        public FP MinThreshold = FP._0_50;
        
        [Header("Curve Mode Only (Unity AnimationCurve)")]
        [Tooltip("水平速度曲线（归一化时间 0-1）")]
        public AnimationCurve HorizontalCurve = AnimationCurve.Linear(0, 1, 1, 0);
        
        [Tooltip("垂直速度曲线（归一化时间 0-1）")]
        public AnimationCurve VerticalCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public KnockbackConfig ToConfig()
        {
            KnockbackConfig config = default;
            config.Mode = Mode;
            config.HorizontalDecayRate = HorizontalDecayRate;
            config.UseGravity = UseGravity;
            config.CurveDuration = CurveDuration;
            config.LinearDecayRate = LinearDecayRate;
            config.MinThreshold = MinThreshold;
            return config;
        }
    }
}