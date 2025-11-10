using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class DelayedExplosionFieldData : SkillFieldData
    {
        [Header("爆炸延迟")]
        [Tooltip("引爆延迟时间")]
        public FP ExplosionDelay = FP._1;
        
        [Tooltip("是否显示倒计时")]
        public bool ShowCountdown = true;
        
        [Tooltip("是否显示预警圈")]
        public bool ShowWarningCircle = true;

        [Header("爆炸伤害")]
        [Tooltip("爆炸伤害")]
        public FP ExplosionDamage = 50;
        
        [Tooltip("伤害类型")]
        public DamageType DamageType = DamageType.Physical;
        
        [Tooltip("是否有伤害衰减")]
        public bool DamageFalloff = true;
        
        [Tooltip("中心伤害倍率")]
        [Range(1f, 2f)]
        public FP CenterDamageMultiplier = FP._1_50;

        [Header("击退效果")]
        [Tooltip("是否应用击退")]
        public bool ApplyKnockback = true;
        
        [Tooltip("击退力度")]
        public FP KnockbackForce = 10;
        
        [Tooltip("击退方向")]
        public ExplosionKnockbackType KnockbackType = ExplosionKnockbackType.FromCenter;
        
        [Tooltip("受击硬直时间")]
        public FP HitstunDuration = FP._0_50;

        [Header("视觉效果")]
        [Tooltip("爆炸特效Prototype")]
        public EntityPrototype ExplosionEffect;
        
        [Tooltip("预警特效Prototype")]
        public EntityPrototype WarningEffect;

        private bool _hasExploded = false;

        public override void OnSkillFieldSpawned(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField, EntityRef owner, FPVector2 position)
        {
            _hasExploded = false;
            
            if (WarningEffect != null)
            {
                frame.Create(WarningEffect);
            }
        }

        public override void OnSkillFieldTick(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
            if (_hasExploded) return;

            FP elapsed = skillField->TickTimer.ElapsedSeconds(frame);

            if (elapsed >= ExplosionDelay)
            {
                Explode(frame, skillFieldEntity, skillField);
                _hasExploded = true;
                
                frame.Destroy(skillFieldEntity);
            }
        }

        private void Explode(Frame frame, EntityRef skillFieldEntity, SkillFieldComponent* skillField)
        {
            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(skillFieldEntity);
            FPVector2 explosionCenter = transform->Position;

            if (ExplosionEffect != null)
            {
                EntityRef effect = frame.Create(ExplosionEffect);
                if (frame.Unsafe.TryGetPointer<Transform2D>(effect, out var effectTransform))
                {
                    effectTransform->Position = explosionCenter;
                }
            }

            FindAndDamageTargets(frame, skillField->Owner, explosionCenter);
        }

        private void FindAndDamageTargets(Frame frame, EntityRef owner, FPVector2 center)
        {
            if (EffectArea == null) return;

            var filter = frame.Filter<Transform2D>();
            
            while (filter.NextUnsafe(out var entity, out var transform))
            {
                if (entity == owner) continue;

                if (!IsInRange(center, transform->Position))
                    continue;

                if (!ShouldAffectTarget(frame, owner, entity))
                    continue;

                ApplyExplosionDamage(frame, owner, entity, center, transform->Position);
            }
        }

        private bool IsInRange(FPVector2 center, FPVector2 targetPos)
        {
            if (EffectArea.ShapeType == Shape2DType.Circle)
            {
                FP distance = FPVector2.Distance(center, targetPos);
                return distance <= EffectArea.CircleRadius;
            }

            return false;
        }

        private void ApplyExplosionDamage(Frame frame, EntityRef owner, EntityRef target, FPVector2 center, FPVector2 targetPos)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            FP damage = CalculateDamage(center, targetPos);

            if (ApplyKnockback)
            {
                FPVector2 knockbackVelocity = CalculateKnockback(center, targetPos);
                hitReaction->ApplyKnockback(frame, target, knockbackVelocity, HitstunDuration);
            }
        }

        private FP CalculateDamage(FPVector2 center, FPVector2 targetPos)
        {
            if (!DamageFalloff)
                return ExplosionDamage;

            FP distance = FPVector2.Distance(center, targetPos);
            FP maxRange = EffectArea.CircleRadius;

            if (distance <= FP._0_01)
                return ExplosionDamage * CenterDamageMultiplier;

            FP ratio = FP._1 - (distance / maxRange);
            FP damageMultiplier = FP._1 + (CenterDamageMultiplier - FP._1) * ratio;
            
            return ExplosionDamage * damageMultiplier;
        }

        private FPVector2 CalculateKnockback(FPVector2 center, FPVector2 targetPos)
        {
            FPVector2 direction = KnockbackType switch
            {
                ExplosionKnockbackType.FromCenter => (targetPos - center).Normalized,
                ExplosionKnockbackType.Up => FPVector2.Up,
                ExplosionKnockbackType.None => FPVector2.Zero,
                _ => FPVector2.Zero
            };

            return direction * KnockbackForce;
        }
        
    }

    public enum ExplosionKnockbackType
    {
        None,
        FromCenter,
        Up
    }
}
