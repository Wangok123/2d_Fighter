using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class HomingProjectileData : ProjectileData
    {
        [Header("追踪弹道设置")]
        [Tooltip("移动速度")]
        public FP MoveSpeed = 10;
        
        [Tooltip("自动追踪最近目标")]
        public bool AutoTargetNearest = true;
        
        [Tooltip("追踪转向速度")]
        public FP TurnSpeed = 5;
        
        [Tooltip("追踪范围（0为无限制）")]
        public FP TrackingRange = 0;
        
        [Tooltip("目标丢失后是否继续直线飞行")]
        public bool ContinueStraightOnLostTarget = true;
    }
}