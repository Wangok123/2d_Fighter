using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class StraightProjectileData : ProjectileData
    {
        [Header("直线弹道设置")]
        [Tooltip("移动速度")]
        public FP MoveSpeed = 10;
    }
}