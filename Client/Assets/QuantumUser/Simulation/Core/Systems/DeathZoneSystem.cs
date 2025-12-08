using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class DeathZoneSystem : SystemMainThreadFilter<DeathZoneSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public Transform2D* Transform;
            public RespawnComponent* RespawnComponent;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.RespawnComponent->IsDead)
            {
                return;
            }

            FPVector2 playerPosition = filter.Transform->Position;

            foreach (var (deathZoneEntity, deathZone) in frame.Unsafe.GetComponentBlockIterator<DeathZone>())
            {
                if (!deathZone->IsActive)
                {
                    continue;
                }

                if (frame.TryGet<Transform2D>(deathZoneEntity, out var deathZoneTransform) &&
                    frame.TryGet<PhysicsCollider2D>(deathZoneEntity, out var collider))
                {
                    if (IsInsideDeathZone(playerPosition, deathZoneTransform, collider))
                    {
                        HandlePlayerDeath(frame, filter.EntityRef, filter.RespawnComponent, deathZoneEntity);
                        break;
                    }
                }
            }
        }

        private bool IsInsideDeathZone(FPVector2 playerPosition, Transform2D deathZoneTransform, PhysicsCollider2D collider)
        {
            FPVector2 deathZonePosition = deathZoneTransform.Position;

            if (collider.Shape.Type == Shape2DType.Circle)
            {
                FPVector2 offset = playerPosition - deathZonePosition;
                FP radius = collider.Shape.Circle.Radius;
                return offset.SqrMagnitude <= radius * radius;
            }
            else if (collider.Shape.Type == Shape2DType.Box)
            {
                FPVector2 halfExtents = collider.Shape.Box.Extents;
                FPVector2 localPos = playerPosition - deathZonePosition;

                return FPMath.Abs(localPos.X) <= halfExtents.X &&
                       FPMath.Abs(localPos.Y) <= halfExtents.Y;
            }

            return false;
        }

        private void HandlePlayerDeath(Frame frame, EntityRef playerEntity, RespawnComponent* respawnComponent, EntityRef deathZoneEntity)
        {
            respawnComponent->IsDead = true;

            const int RESPAWN_DELAY_SECONDS = 5;
            respawnComponent->RespawnTimer.Start(RESPAWN_DELAY_SECONDS);

            frame.Signals.OnCharacterRespawn(playerEntity);
            
            frame.Events.OnPlayerDeathZoneDeath(playerEntity, frame.Get<Transform2D>(playerEntity).Position);
        }
    }
}
