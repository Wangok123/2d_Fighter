using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class GrenadeProjectileData : ProjectileData
    {
        [Header("手榴弹设置")]
        [Tooltip("爆炸区域数据")]
        public AssetRef<SkillFieldData> ExplosionFieldData;
        
        [Tooltip("投掷速度")]
        public FP ThrowSpeed = 10;
        
        [Tooltip("投掷弧度")]
        public FP ThrowArc = 5;
        
        [Tooltip("重力")]
        public FP Gravity = 10;
        
        [Tooltip("地面摩擦力")]
        public FP GroundFriction = FP._0_50;
        
        [Tooltip("爆炸触发类型")]
        public GrenadeDetonateType DetonateType = GrenadeDetonateType.GroundContact;
        
        [Tooltip("延迟引爆时间")]
        public FP DetonationDelay = 2;
        
        [Tooltip("是否允许空中碰撞触发")]
        public bool CanDetonateInAir = false;
        
        [Tooltip("是否在爆炸时生成力场")]
        public bool SpawnSkillFieldOnExplosion = true;
    }

    public enum GrenadeDetonateType
    {
        GroundContact,
        FirstContact,
        Timer
    }
}