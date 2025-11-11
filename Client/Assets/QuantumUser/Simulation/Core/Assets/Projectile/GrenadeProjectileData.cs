using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class GrenadeProjectileData : ProjectileData
    {
        [Header("手榴弹设置")]
        [Tooltip("爆炸区域数据")]
        public AssetRef<SkillFieldData> ExplosionFieldData;
        
        [Header("投掷参数")]
        [Tooltip("投掷速度")]
        public FP ThrowSpeed = 10;
        
        [Tooltip("投掷弧度（初始向上速度）")]
        public FP ThrowArc = 5;
        
        [Header("引爆设置")]
        [Tooltip("爆炸触发类型")]
        public GrenadeDetonateType DetonateType = GrenadeDetonateType.Timer;
        
        [Tooltip("延迟引爆时间")]
        public FP DetonationDelay = 2;
        
        [Tooltip("是否允许空中碰撞触发")]
        public bool CanDetonateInAir = false;
        
        [Tooltip("是否在爆炸时生成力场")]
        public bool SpawnSkillFieldOnExplosion = true;
    }
}