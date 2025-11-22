using UnityEngine;
using Photon.Deterministic;
using Quantum.Physics2D;

namespace Quantum
{
    public unsafe partial class SkillFieldData : AssetObject
    {
        [Header("基础设置")]
        [Tooltip("持续时间")]
        public FP Duration = 5;
        
        [Tooltip("Tick间隔")]
        public FP TickInterval = FP._0_50;
        
        [Tooltip("视觉Prototype")]
        public EntityPrototype VisualPrototype;

        [Header("效果范围")]
        [Tooltip("范围形状")]
        public Shape2DConfig EffectArea;
        
        [Tooltip("影响层")]
        public LayerMask TargetLayer = 1 << 6;
        
        [Tooltip("是否影响友军")]
        public bool AffectAllies = false;
        
        [Tooltip("是否影响敌人")]
        public bool AffectEnemies = true;

        [Header("伤害设置")]
        [Tooltip("每次Tick伤害")]
        public FP DamagePerTick = 5;
        
        [Tooltip("击退配置数据")]
        public AssetRef<KnockbackStatusEffectData> KnockbackStatusEffectData;

        public virtual bool ShouldAffectTarget(Frame frame, EntityRef owner, EntityRef target)
        {
            if (target == owner)
                return false;

            bool isSameTeam = CheckSameTeam(frame, owner, target);

            if (isSameTeam && !AffectAllies)
                return false;

            if (!isSameTeam && !AffectEnemies)
                return false;

            return true;
        }

        protected virtual bool CheckSameTeam(Frame frame, EntityRef entity1, EntityRef entity2)
        {
            return false;
        }

        public virtual FP GetDamagePerTick(Frame frame, EntityRef skillFieldEntity)
        {
            return DamagePerTick;
        }

        public virtual AssetRef<KnockbackStatusEffectData> GetKnockbackStatusEffectData(Frame frame, EntityRef skillFieldEntity)
        {
            return KnockbackStatusEffectData;
        }

        public virtual void OnCustomSpawn(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual void OnCustomTick(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual void OnCustomDestroy(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }
    }
}
