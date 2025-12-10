using Photon.Deterministic;
using Quantum.Physics2D;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class AttackSystem : SystemSignalsOnly,
        ISignalOnAttackHitboxActivate,
        ISignalOnAttackExecute
    {
        public void OnAttackHitboxActivate(Frame frame, EntityRef attacker)
        {
#if UNITY_EDITOR
            if (frame.Unsafe.TryGetPointer<AttackComponent>(attacker, out var attackData))
            {
                frame.Events.AttackHitboxActivated(attacker, attackData->ComboCounter);
            }
#endif
        }

        public void OnAttackExecute(Frame frame, EntityRef attacker)
        {
            if (!frame.Has<AttackComponent>(attacker))
                return;

            Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(attacker);
            AttackComponent* attackComponent = frame.Unsafe.GetPointer<AttackComponent>(attacker);
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(attacker);
            GameSettingsData gameSettingsData =
                frame.FindAsset<GameSettingsData>(frame.RuntimeConfig.GameSettingsData.Id);

            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            AbilityType activeAbilityType = abilityInventory->ActiveAbilityInfo.ActiveAbilityType;

            if (!dic.TryGetValue(activeAbilityType, out var ability))
                return;

            AbilityData abilityDataBase = frame.FindAsset<AbilityData>(ability.AbilityData.Id);
            if (!(abilityDataBase is AttackAbilityData attackData))
                return;

            Shape2DConfig attackShape = attackData.GetCurrentAttackShape(frame, attacker);
            bool isFacingRight = GetIsFacingRight(frame, attacker);
            var shape = CreateAttackShapeWithDirection(frame, attackShape, isFacingRight);

            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettingsData.PlayerLayerMask,
                QueryOptions.HitKinematics | QueryOptions.HitDynamics | QueryOptions.HitTriggers);

            if (hits.Count > 0)
            {
                var hitList = frame.ResolveList(attackComponent->HitEntitiesThisAttack);

                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == attacker)
                        continue;

                    if (hitList.Contains(hit.Entity))
                        continue;

                    if (!frame.Unsafe.TryGetPointer<Transform2D>(hit.Entity, out var hitPlayerTransform))
                        continue;

                    hitList.Add(hit.Entity);

                    OnAttackHitTarget(frame, attacker, hit.Entity, transform->Position,
                        hitPlayerTransform->Position);
                }
            }
        }

        private void OnAttackHitTarget(Frame frame, EntityRef attacker, EntityRef target, FPVector2 attackerPos,
            FPVector2 targetPos)
        {
            AbilityInventory* abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(attacker);
            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            AbilityType activeAbilityType = abilityInventory->ActiveAbilityInfo.ActiveAbilityType;

            if (!dic.TryGetValue(activeAbilityType, out var ability))
                return;

            AttackAbilityData attackData = frame.FindAsset<AttackAbilityData>(ability.AbilityData.Id);
            if (attackData == null)
            {
                Debug.LogError("Attack data is null in OnAttackHitTarget");
                return;
            }
    
            AssetRef<KnockbackStatusEffectData> knockbackDataRef = attackData.GetCurrentKnockbackStatusEffectData(frame, attacker);
            if (!knockbackDataRef.Id.IsValid)
            {
                Debug.LogWarning("Knockback data is not configured for this attack");
                return;
            }

            KnockbackStatusEffectData knockbackData = frame.FindAsset<KnockbackStatusEffectData>(knockbackDataRef.Id);
            FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(frame, attacker, attackerPos, targetPos);
    
            // 根据目标实体的组件动态决定击退方式
            ApplyKnockbackToTarget(frame, target, knockbackData, knockbackDirection, knockbackDataRef);
        }
        
        private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData, 
            FPVector2 knockbackDirection, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
        {
            frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection, knockbackDataRef);
        }


        private bool GetIsFacingRight(Frame frame, EntityRef entityRef)
        {
            if (frame.Unsafe.TryGetPointer<MovementComponent>(entityRef, out var movement))
            {
                return movement->IsFacingRight;
            }

            return true;
        }

        private Shape2D CreateAttackShapeWithDirection(Frame frame, Shape2DConfig shapeConfig, bool isFacingRight)
        {
            Shape2DConfig adjustedConfig = new Shape2DConfig
            {
                ShapeType = shapeConfig.ShapeType,
                PolygonCollider = shapeConfig.PolygonCollider,
                CircleRadius = shapeConfig.CircleRadius,
                CapsuleSize = shapeConfig.CapsuleSize,
                EdgeExtent = shapeConfig.EdgeExtent,
                BoxExtents = shapeConfig.BoxExtents,
                PositionOffset = shapeConfig.PositionOffset,
                RotationOffset = shapeConfig.RotationOffset,
                UserTag = shapeConfig.UserTag,
                IsPersistent = shapeConfig.IsPersistent,
                CompoundShapes = shapeConfig.CompoundShapes
            };

            if (!isFacingRight)
            {
                adjustedConfig.PositionOffset.X = -adjustedConfig.PositionOffset.X;
            }

            return adjustedConfig.CreateShape(frame);
        }
    }
}
