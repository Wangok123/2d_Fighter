using UnityEngine;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial class GrenadeProjectileData : ProjectileData
    {
        [Header("手榴弹设置")] [Tooltip("爆炸区域数据")] public AssetRef<SkillFieldData> ExplosionFieldData;

        [Header("投掷参数")] [Tooltip("投掷速度")] public FP ThrowSpeed = 10;

        [Tooltip("投掷弧度（初始向上速度）")] public FP ThrowArc = 5;

        [Header("引爆设置")] [Tooltip("爆炸触发类型")] public GrenadeDetonateType DetonateType = GrenadeDetonateType.Timer;

        [Tooltip("延迟引爆时间")] public FP DetonationDelay = 2;

        [Tooltip("是否允许空中碰撞触发")] public bool CanDetonateInAir = false;

        [Tooltip("是否在爆炸时生成力场")] public bool SpawnSkillFieldOnExplosion = true;

        public override void OnInitialize(Frame frame, EntityRef projectileEntity,
            ProjectileComponent* projectile, EntityRef owner)
        {
            projectile->Speed = ThrowSpeed;

            if (!frame.Unsafe.TryGetPointer<GrenadeRuntimeComponent>(projectileEntity, out var grenadeRuntime))
            {
                Log.Error("[Grenade Init] GrenadeRuntimeComponent not found!");
                return;
            }

            grenadeRuntime->TimeAlive = FP._0;
            grenadeRuntime->HasDetonated = false;

            if (!frame.Unsafe.TryGetPointer<PhysicsBody2D>(projectileEntity, out var body))
            {
                Log.Error("[Grenade Init] PhysicsBody2D not found!");
                return;
            }

            // 计算投掷方向
            FP horizontalSign = projectile->Direction.X >= FP._0 ? FP._1 : -FP._1;

            if (FPMath.Abs(projectile->Direction.X) < FP._0_01)
            {
                if (frame.Unsafe.TryGetPointer<Transform2D>(owner, out var ownerTransform))
                {
                    FP ownerRotation = ownerTransform->Rotation;
                    FP rotationRad = ownerRotation * FP.Deg2Rad;
                    FP cosRot = FPMath.Cos(rotationRad);
                    horizontalSign = cosRot >= FP._0 ? FP._1 : -FP._1;
                }
            }

            body->Velocity = new FPVector2(horizontalSign * ThrowSpeed, ThrowArc);
        }

        public override bool OnUpdateMovement(Frame frame, EntityRef projectileEntity,
            ProjectileComponent* projectile, Transform2D* transform)
        {
            if (!frame.Unsafe.TryGetPointer<GrenadeRuntimeComponent>(projectileEntity, out var grenadeRuntime))
                return false;

            if (!grenadeRuntime->HasDetonated)
            {
                grenadeRuntime->TimeAlive += frame.DeltaTime;

                if (DetonateType == GrenadeDetonateType.Timer &&
                    grenadeRuntime->TimeAlive >= DetonationDelay)
                {
                    TriggerExplosion(frame, projectileEntity, grenadeRuntime, transform->Position);
                    return false;
                }
            }

            return true;
        }

        public override bool OnHandleHit(Frame frame, EntityRef projectileEntity,
            ProjectileComponent* projectile, EntityRef target, FPVector2 hitPoint)
        {
            if (!frame.Unsafe.TryGetPointer<GrenadeRuntimeComponent>(projectileEntity, out var grenadeRuntime))
                return false;

            if (grenadeRuntime->HasDetonated)
                return true;

            if (CanDetonateInAir || DetonateType == GrenadeDetonateType.FirstContact)
            {
                Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(projectileEntity);
                TriggerExplosion(frame, projectileEntity, grenadeRuntime, transform->Position);
                return true;
            }

            return false;
        }

        private void TriggerExplosion(Frame frame, EntityRef projectileEntity,
            GrenadeRuntimeComponent* grenadeRuntime, FPVector2 explosionPosition)
        {
            grenadeRuntime->HasDetonated = true;

            if (SpawnSkillFieldOnExplosion && ExplosionFieldData.Id.IsValid)
            {
                ProjectileComponent* projectile = frame.Unsafe.GetPointer<ProjectileComponent>(projectileEntity);
                frame.Signals.SpawnSkillField(ExplosionFieldData, explosionPosition, projectile->Owner);
            }

            frame.Signals.DestroyProjectile(projectileEntity, ProjectileDestroyReason.HitTarget);
        }
    }
}