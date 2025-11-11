using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class BoomerangProjectileData : ProjectileData
    {
        [Header("回旋镖弹道设置")]
        [Tooltip("前进速度")]
        public FP ForwardSpeed = 10;
        
        [Tooltip("返回速度")]
        public FP ReturnSpeed = 12;
        
        [Tooltip("最大飞行距离")]
        public FP MaxDistance = 10;
        
        [Tooltip("回收距离")]
        public FP CatchDistance = FP._0_50;
        
        [Tooltip("失去主人后的行为")]
        public BoomerangBehaviorOnOwnerLost BehaviorOnOwnerLost = BoomerangBehaviorOnOwnerLost.Destroy;
        
        [Tooltip("前进阶段是否旋转")]
        public bool RotateWhileForward = true;
        
        [Tooltip("旋转速度（度/秒）")]
        public FP RotationSpeed = 360;
        
        // 修改：添加可选KCC配置
        [Header("物理设置（可选）")]
        [Tooltip("是否使用KCC2D物理")]
        public bool UseKCC = false;
        
        [Tooltip("KCC配置")]
        public AssetRef<CharacterController2DConfig> KCCConfig;
    }

    public enum BoomerangBehaviorOnOwnerLost
    {
        Destroy,
        ContinueStraight,
        FallDown
    }
}