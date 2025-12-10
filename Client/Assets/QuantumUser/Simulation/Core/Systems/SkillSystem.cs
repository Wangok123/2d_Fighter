using Photon.Deterministic;
using Quantum.Core.Utils;
using Quantum.Physics2D;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class SkillSystem : SystemMainThreadFilter<SkillSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public SkillComponent* SkillComponent;
            public Transform2D* Transform;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.SkillComponent->CurrentSkill.Id.IsValid)
                return;

            SkillData skillData = frame.FindAsset<SkillData>(filter.SkillComponent->CurrentSkill.Id);
            if (skillData == null)
                return;

            filter.SkillComponent->ElapsedTime += frame.DeltaTime;
            filter.SkillComponent->PhaseTimer += frame.DeltaTime;

            UpdateSkillPhase(frame, ref filter, skillData);
            ExecuteSkillActions(frame, ref filter, skillData);
            ApplySkillFlags(frame, ref filter, skillData);
        }

        private void UpdateSkillPhase(Frame frame, ref Filter filter, SkillData skillData)
        {
            SkillPhase currentPhase = filter.SkillComponent->Phase;
            SkillPhase nextPhase = currentPhase;
            FP phaseDuration = FP._0;

            switch (currentPhase)
            {
                case SkillPhase.Startup:
                    phaseDuration = skillData.StartupDuration;
                    nextPhase = SkillPhase.Active;
                    break;

                case SkillPhase.Active:
                    phaseDuration = skillData.ActiveDuration;
                    nextPhase = SkillPhase.Recovery;
                    break;

                case SkillPhase.Recovery:
                    phaseDuration = skillData.RecoveryDuration;
                    nextPhase = SkillPhase.None;
                    break;
            }

            if (filter.SkillComponent->PhaseTimer >= phaseDuration)
            {
                if (nextPhase == SkillPhase.None)
                {
                    CompleteSkill(frame, ref filter, skillData);
                }
                else
                {
                    ChangePhase(frame, ref filter, skillData, nextPhase);
                }
            }
        }

        private void ChangePhase(Frame frame, ref Filter filter, SkillData skillData, SkillPhase newPhase)
        {
            filter.SkillComponent->Phase = newPhase;
            filter.SkillComponent->PhaseTimer = FP._0;

            skillData.OnPhaseChanged(frame, filter.Entity, newPhase);

            frame.Signals.OnSkillPhaseChanged(filter.Entity, newPhase);
            frame.Events.SkillPhaseChanged(filter.Entity, newPhase);
        }

        private void CompleteSkill(Frame frame, ref Filter filter, SkillData skillData)
        {
            skillData.OnSkillCompleted(frame, filter.Entity);

            if (filter.SkillComponent->HitEntities.Ptr != default)
            {
                frame.FreeList(filter.SkillComponent->HitEntities);
                filter.SkillComponent->HitEntities = default;
            }

            filter.SkillComponent->CurrentSkill = default;
            filter.SkillComponent->Phase = SkillPhase.None;
            filter.SkillComponent->ElapsedTime = FP._0;
            filter.SkillComponent->ActionIndex = 0;

            frame.Signals.OnSkillCompleted(filter.Entity);
            frame.Events.SkillCompleted(filter.Entity);
        }

        private void ExecuteSkillActions(Frame frame, ref Filter filter, SkillData skillData)
        {
            if (skillData.Actions == null || skillData.Actions.Length == 0)
                return;

            for (int i = filter.SkillComponent->ActionIndex; i < skillData.Actions.Length; i++)
            {
                SkillActionConfig action = skillData.Actions[i];

                if (filter.SkillComponent->ElapsedTime >= action.TriggerTime)
                {
                    ExecuteAction(frame, ref filter, skillData, action);
                    filter.SkillComponent->ActionIndex = i + 1;
                }
                else
                {
                    break;
                }
            }
        }

        private void ExecuteAction(Frame frame, ref Filter filter, SkillData skillData, SkillActionConfig action)
        {
            switch (action.ActionType)
            {
                case SkillActionType.ApplyVelocity:
                    ApplyVelocity(frame, ref filter, action.Velocity);
                    break;

                case SkillActionType.SpawnHitbox:
                    SpawnHitbox(frame, ref filter, action);
                    break;

                case SkillActionType.SpawnProjectile:
                    SpawnProjectile(frame, ref filter, action);
                    break;

                case SkillActionType.ApplyStatusEffect:
                    break;
            }
        }

        private void ApplyVelocity(Frame frame, ref Filter filter, FPVector2 velocity)
        {
            if (!frame.Unsafe.TryGetPointer<MovementComponent>(filter.Entity, out var movement))
                return;

            FPVector2 adjustedVelocity = new FPVector2(
                velocity.X * (movement->IsFacingRight ? FP._1 : -FP._1),
                velocity.Y
            );

            // 使用辅助类设置速度（兼容两种控制器）
            MovementControllerHelper.SetVelocity(frame, filter.Entity, adjustedVelocity);
        }

        private void SpawnHitbox(Frame frame, ref Filter filter, SkillActionConfig action)
        {
            if (!frame.Unsafe.TryGetPointer<MovementComponent>(filter.Entity, out var movement))
                return;

            GameSettingsData gameSettings = GameSettingsHelper.Get(frame);

            Shape2DConfig adjustedConfig = new Shape2DConfig
            {
                ShapeType = action.HitboxShape.ShapeType,
                PolygonCollider = action.HitboxShape.PolygonCollider,
                CircleRadius = action.HitboxShape.CircleRadius,
                CapsuleSize = action.HitboxShape.CapsuleSize,
                EdgeExtent = action.HitboxShape.EdgeExtent,
                BoxExtents = action.HitboxShape.BoxExtents,
                PositionOffset = action.HitboxShape.PositionOffset,
                RotationOffset = action.HitboxShape.RotationOffset,
                UserTag = action.HitboxShape.UserTag,
                IsPersistent = action.HitboxShape.IsPersistent,
                CompoundShapes = action.HitboxShape.CompoundShapes
            };

            if (!movement->IsFacingRight)
            {
                adjustedConfig.PositionOffset.X = -adjustedConfig.PositionOffset.X;
            }

            Shape2D shape = adjustedConfig.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*filter.Transform, shape, gameSettings.PlayerLayerMask,
                QueryOptions.HitDynamics | QueryOptions.HitKinematics | QueryOptions.HitTriggers);

            if (hits.Count > 0)
            {
                var hitList = frame.ResolveList(filter.SkillComponent->HitEntities);

                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == filter.Entity)
                        continue;

                    if (hitList.Contains(hit.Entity))
                        continue;

                    hitList.Add(hit.Entity);

                    ApplyHitEffect(frame, ref filter, hit.Entity, action, movement->IsFacingRight);
                }
            }
        }

        private void SpawnProjectile(Frame frame, ref Filter filter, SkillActionConfig action)
        {
            if (!action.ProjectileData.Id.IsValid)
                return;

            if (!frame.Unsafe.TryGetPointer<MovementComponent>(filter.Entity, out var movement))
                return;

            FPVector2 direction = movement->IsFacingRight ? FPVector2.Right : FPVector2.Left;
            FPVector2 spawnOffset = new FPVector2(
                action.SpawnOffset.X * (movement->IsFacingRight ? FP._1 : -FP._1),
                action.SpawnOffset.Y
            );
            FPVector2 spawnPosition = filter.Transform->Position + spawnOffset;

            frame.Signals.SpawnProjectile(action.ProjectileData, spawnPosition, direction, filter.Entity);
        }

        private void ApplyHitEffect(Frame frame, ref Filter filter, EntityRef target, SkillActionConfig action,
            bool isFacingRight)
        {
            if (!action.KnockbackData.Id.IsValid)
                return;

            KnockbackStatusEffectData knockbackData =
                frame.FindAsset<KnockbackStatusEffectData>(action.KnockbackData.Id);
            if (knockbackData == null)
                return;

            Transform2D* attackerTransform = filter.Transform;
            Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(target);

            FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(
                frame,
                isFacingRight,
                attackerTransform->Position,
                targetTransform->Position
            );

            ApplyKnockbackToTarget(frame, target, knockbackData, knockbackDirection, action.KnockbackData);
        }

        private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData,
            FPVector2 knockbackDirection, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
        {
            frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection,
                knockbackDataRef);
        }

        private void ApplySkillFlags(Frame frame, ref Filter filter, SkillData skillData)
        {
            if (!frame.Unsafe.TryGetPointer<MovementComponent>(filter.Entity, out var movement))
                return;

            // 应用下压力和着陆检测（使用辅助类）
            if (skillData.Flags.HasFlag(SkillFlags.ApplyDownwardForce))
            {
                bool isGrounded = MovementControllerHelper.IsGrounded(frame, filter.Entity);

                // 在空中时应用下压力
                if (!isGrounded)
                {
                    skillData.ApplyDownwardForce(frame, filter.Entity);
                }

                // 检测刚刚着陆
                if (isGrounded && skillData.Flags.HasFlag(SkillFlags.LandingShockwave))
                {
                    // 只在第一次着陆时触发
                    if (!filter.SkillComponent->HasTriggeredLanding)
                    {
                        skillData.SpawnLandingShockwave(frame, filter.Entity, filter.Transform, movement);

                        frame.Signals.OnSkillLanded(filter.Entity, filter.SkillComponent->CurrentSkill);
                        frame.Events.SkillLanded(filter.Entity, filter.Transform->Position);

                        // 标记已触发
                        filter.SkillComponent->HasTriggeredLanding = true;

                        CompleteSkill(frame, ref filter, skillData);
                        return;
                    }
                }
            }

            // 应用霸体状态
            if (skillData.Flags.HasFlag(SkillFlags.SuperArmor))
            {
            }
        }
    }
}