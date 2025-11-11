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

        public virtual void OnSkillFieldSpawned(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, EntityRef owner, FPVector2 position)
        {
        }

        public virtual void OnSkillFieldTick(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

        public virtual void OnSkillFieldDestroyed(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
        }

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

        public virtual void ApplyEffect(Frame frame, EntityRef skillFieldEntity, EntityRef target, FPVector2 hitPoint)
        {
        }
        
        protected virtual HitCollection DetectTargetsInEffectArea(Frame frame, EntityRef skillFieldEntity)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);

            Shape2D shape = EffectArea.CreateShape(frame);
            return frame.Physics2D.OverlapShape(*transform, shape, TargetLayer, QueryOptions.HitDynamics);
        }
    }
}