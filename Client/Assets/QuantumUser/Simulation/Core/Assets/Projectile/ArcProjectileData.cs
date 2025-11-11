using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class ArcProjectileData : ProjectileData
    {
        [Header("抛物线弹道设置")]
        [Tooltip("初始速度")]
        public FP InitialSpeed = 10;
        
        [Tooltip("初始向上速度")]
        public FP InitialUpwardVelocity = 5;
        
        [Tooltip("重力加速度")]
        public FP Gravity = 10;

        [Header("地面限制")]
        [Tooltip("最低高度限制（防止穿地）")]
        public FP MinimumHeight = FP._0;
        
        [Tooltip("是否启用地面限制")]
        public bool EnableGroundClamp = true;
    }
}