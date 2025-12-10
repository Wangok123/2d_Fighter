using Photon.Deterministic;
using System;
using Quantum.Core.Utils;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public class SkillActionConfig
    {
        [Tooltip("动作类型")] public SkillActionType ActionType = SkillActionType.None;

        [Tooltip("触发时间")] public FP TriggerTime = FP._0;

        [Header("Apply Velocity Settings")] [Tooltip("应用速度")]
        public FPVector2 Velocity = FPVector2.Zero;

        [Header("Spawn Hitbox Settings")] [Tooltip("持续时间")]
        public FP Duration = FP._0_10;

        [Tooltip("打击框形状")] public Shape2DConfig HitboxShape;

        [Tooltip("击退数据")] public AssetRef<KnockbackStatusEffectData> KnockbackData;

        [Header("Spawn Projectile Settings")] [Tooltip("弹道数据")]
        public AssetRef<ProjectileData> ProjectileData;

        [Tooltip("生成偏移")] public FPVector2 SpawnOffset = FPVector2.Zero;

        [Header("Apply Status Effect Settings")] [Tooltip("状态效果数据")]
        public StatusEffect StatusEffectData;

        [Header("Play Animation Settings")] [Tooltip("动画名称")]
        public string AnimationName = "";

        [Header("Spawn VFX Settings")] [Tooltip("VFX预制体名称")]
        public string VFXPrefabName = "";

        [Tooltip("VFX位置偏移")] public FPVector2 VFXOffset = FPVector2.Zero;
    }

    public unsafe class SkillData : AssetObject
    {
        [Header("Basic Settings")] [Tooltip("技能名称")]
        public string SkillName = "New Skill";

        [Tooltip("技能持续时间")] public FP TotalDuration = FP._1;

        [Tooltip("冷却时间")] public FP Cooldown = FP._2;

        [Header("Skill Flags")] [Tooltip("技能特性标记")]
        public SkillFlags Flags;

        [Header("Phase Durations")] [Tooltip("启动阶段时长")]
        public FP StartupDuration = FP._0_10;

        [Tooltip("激活阶段时长")] public FP ActiveDuration = FP._0_50;

        [Tooltip("恢复阶段时长")] public FP RecoveryDuration = FP._0_33;

        [Header("Skill Actions")] [Tooltip("技能动作列表")]
        public SkillActionConfig[] Actions;

        [Header("Downward Force Settings (if ApplyDownwardForce flag is set)")] [Tooltip("下压加速度")]
        public FP DownwardAcceleration;

        [Tooltip("最大下落速度")] public FP MaxDownwardSpeed;

        [Header("Landing Shockwave Settings (if LandingShockwave flag is set)")] [Tooltip("着陆冲击波形状")]
        public Shape2DConfig ShockwaveShape;

        [Tooltip("着陆冲击波击退数据")] public AssetRef<KnockbackStatusEffectData> ShockwaveKnockbackData;

        public virtual bool CanActivate(Frame frame, EntityRef entityRef)
        {
            // 使用辅助类检查运动控制器
            if (!MovementControllerHelper.HasMovementController(frame, entityRef))
                return false;

            // 使用辅助类检查地面状态
            bool isGrounded = MovementControllerHelper.IsGrounded(frame, entityRef);

            if (Flags.HasFlag(SkillFlags.GroundedOnly) && !isGrounded)
                return false;

            if (Flags.HasFlag(SkillFlags.AirOnly) && isGrounded)
                return false;

            return true;
        }

        public virtual void OnSkillStarted(Frame frame, EntityRef entityRef)
        {
        }

        public virtual void OnPhaseChanged(Frame frame, EntityRef entityRef, SkillPhase newPhase)
        {
        }

        public virtual void OnSkillCompleted(Frame frame, EntityRef entityRef)
        {
        }

        public virtual void ApplyDownwardForce(Frame frame, EntityRef entityRef)
        {
            // 检查是否在地面（使用辅助类）
            if (MovementControllerHelper.IsGrounded(frame, entityRef))
                return;

            // 获取当前垂直速度
            FP currentDownwardSpeed = -MovementControllerHelper.GetVerticalVelocity(frame, entityRef);
            FP newDownwardSpeed = currentDownwardSpeed + DownwardAcceleration * frame.DeltaTime;
            newDownwardSpeed = FPMath.Min(newDownwardSpeed, MaxDownwardSpeed);

            // 设置新的垂直速度
            MovementControllerHelper.SetVerticalVelocity(frame, entityRef, -newDownwardSpeed);
        }

        public virtual void SpawnLandingShockwave(Frame frame, EntityRef entityRef, Transform2D* transform,
            MovementComponent* movement)
        {
#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[SkillData] SpawnLandingShockwave called for {SkillName}");
#endif

            if (!ShockwaveKnockbackData.Id.IsValid)
            {
#if DEBUG || UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"[SkillData] ShockwaveKnockbackData is invalid!");
#endif
                return;
            }

            GameSettingsData gameSettings = GameSettingsHelper.Get(frame);

            Shape2DConfig adjustedConfig = new Shape2DConfig
            {
                ShapeType = ShockwaveShape.ShapeType,
                PolygonCollider = ShockwaveShape.PolygonCollider,
                CircleRadius = ShockwaveShape.CircleRadius,
                CapsuleSize = ShockwaveShape.CapsuleSize,
                EdgeExtent = ShockwaveShape.EdgeExtent,
                BoxExtents = ShockwaveShape.BoxExtents,
                PositionOffset = ShockwaveShape.PositionOffset,
                RotationOffset = ShockwaveShape.RotationOffset,
                UserTag = ShockwaveShape.UserTag,
                IsPersistent = ShockwaveShape.IsPersistent,
                CompoundShapes = ShockwaveShape.CompoundShapes
            };

            if (!movement->IsFacingRight)
            {
                adjustedConfig.PositionOffset.X = -adjustedConfig.PositionOffset.X;
            }

            Shape2D shape = adjustedConfig.CreateShape(frame);
            HitCollection hits = frame.Physics2D.OverlapShape(*transform, shape, gameSettings.PlayerLayerMask,
                QueryOptions.HitDynamics | QueryOptions.HitKinematics | QueryOptions.HitTriggers);

#if DEBUG || UNITY_EDITOR
            UnityEngine.Debug.Log($"[SkillData] Shockwave hit {hits.Count} entities");
#endif

            if (hits.Count > 0)
            {
                KnockbackStatusEffectData knockbackData =
                    frame.FindAsset<KnockbackStatusEffectData>(ShockwaveKnockbackData.Id);

                for (int i = 0; i < hits.Count; i++)
                {
                    Hit hit = hits[i];

                    if (hit.Entity == entityRef)
                        continue;

                    if (!frame.Has<KnockbackComponent>(hit.Entity))
                        continue;

#if DEBUG || UNITY_EDITOR
                    UnityEngine.Debug.Log($"[SkillData] Applying knockback to entity {hit.Entity}");
#endif

                    Transform2D* targetTransform = frame.Unsafe.GetPointer<Transform2D>(hit.Entity);

                    FPVector2 knockbackDirection = knockbackData.GetKnockbackDirection(
                        frame,
                        movement->IsFacingRight,
                        transform->Position,
                        targetTransform->Position
                    );

                    ApplyKnockbackToTarget(frame, hit.Entity, knockbackData, knockbackDirection, ShockwaveKnockbackData);
                }
            }
        }

        private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData, 
            FPVector2 knockbackDirection, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
        {
            frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection, knockbackDataRef);
        }
    }
}