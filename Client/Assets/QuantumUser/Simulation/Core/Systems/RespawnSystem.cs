using Photon.Deterministic;
using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class RespawnSystem : SystemMainThreadFilter<RespawnSystem.Filter>,
        ISignalOnComponentAdded<RespawnPoint>,
        ISignalOnCharacterRespawn
    {
        public struct Filter 
        {
            public EntityRef EntityRef;
            public Transform2D* Transform;
            public RespawnComponent* RespawnComponent;
        }

        public void OnAdded(Frame frame, EntityRef entity, RespawnPoint* component)
        {
            var spawnPlaces = frame.Unsafe.GetPointerSingleton<SpawnPlaces>();
    
            if (frame.TryResolveList(spawnPlaces->Spawners, out var spawners) == false)
            {
                frame.AllocateList(out spawnPlaces->Spawners);
                spawners = frame.ResolveList(spawnPlaces->Spawners);
            }
    
            spawners.Add(entity);
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.RespawnComponent->IsDead)
            {
                return;
            }

            filter.RespawnComponent->RespawnTimer.Tick(frame.DeltaTime);

            if (filter.RespawnComponent->RespawnTimer.IsDone)
            {
                frame.Signals.OnCharacterRespawn(filter.EntityRef);
            }
        }

        public void OnCharacterRespawn(Frame frame, EntityRef character)
        {
            if (!frame.Unsafe.TryGetPointer<Transform2D>(character, out var transform) ||
                !frame.Unsafe.TryGetPointer<RespawnComponent>(character, out var respawnComponent))
            {
                return;
            }

            FPVector2 spawnPosition = GetSpawnPosition(frame, character);

            transform->Teleport(frame, spawnPosition);

            ResetMovementState(frame, character);
            ResetStatusEffects(frame, character);
            ResetAttackState(frame, character);
            ResetAbilityState(frame, character);
            ResetPhysicsState(frame, character);

            respawnComponent->IsDead = false;
            respawnComponent->RespawnTimer.Reset();

            frame.Events.OnPlayerRespawned(character, spawnPosition);
        }

        private void ResetMovementState(Frame frame, EntityRef character)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(character, out var kcc))
            {
                kcc->ResetVelocity();
            }
        }

        private void ResetStatusEffects(Frame frame, EntityRef character)
        {
            if (frame.TryGet<CharacterStatusComponent>(character, out var statusComponent))
            {
                statusComponent.KnockbackStatusEffect.DurationTimer.Reset();
                statusComponent.KnockbackStatusEffect.KnockbackVelocity = FPVector2.Zero;
            }
        }

        private void ResetAttackState(Frame frame, EntityRef character)
        {
            if (frame.Unsafe.TryGetPointer<AttackComponent>(character, out var attackComponent))
            {
                attackComponent->ResetToDefault(frame, character);
            }

            if (frame.Has<ComboAttackRuntimeComponent>(character))
            {
                var comboRuntime = frame.Unsafe.GetPointer<ComboAttackRuntimeComponent>(character);
                comboRuntime->ResetToDefault();
            }

            if (frame.Has<ChargeAttackRuntimeComponent>(character))
            {
                var chargeRuntime = frame.Unsafe.GetPointer<ChargeAttackRuntimeComponent>(character);
                chargeRuntime->ResetToDefault();
            }
        }

        private void ResetAbilityState(Frame frame, EntityRef character)
        {
            if (frame.Unsafe.TryGetPointer<AbilityInventory>(character, out var abilityInventory))
            {
                abilityInventory->ResetActiveAbility();
            }
        }

        private void ResetPhysicsState(Frame frame, EntityRef character)
        {
            if (frame.TryGet<PhysicsCollider2D>(character, out var collider))
            {
                collider.IsTrigger = false;
            }
        }

        private FPVector2 GetSpawnPosition(Frame frame, EntityRef character)
        {
            CharacterTeam playerTeam = CharacterTeam.None;

            if (frame.TryGet<PlayerSpawner>(character, out var spawner))
            {
                playerTeam = spawner.PlayerTeam;
            }
            
            var spawnPlaces = frame.Unsafe.GetPointerSingleton<SpawnPlaces>();
            
            if (!frame.TryResolveList(spawnPlaces->Spawners, out var spawners))
            {
                Log.Error("[RespawnSystem] Cannot resolve spawners list!");
                return FPVector2.Zero;
            }

            for (int i = 0; i < spawners.Count; i++)
            {
                var spawnEntity = spawners[i];

                if (!frame.Exists(spawnEntity))
                {
                    continue;
                }

                if (frame.TryGet<RespawnPoint>(spawnEntity, out var respawnPoint) &&
                    frame.TryGet<Transform2D>(spawnEntity, out var spawnTransform))
                {
                    if (respawnPoint.Team == playerTeam || respawnPoint.Team == CharacterTeam.Neutral)
                    {
                        return spawnTransform.Position;
                    }
                }
            }

            if (spawners.Count > 0)
            {
                int randomIndex = frame.RNG->Next(0, spawners.Count);
                var fallbackEntity = spawners[randomIndex];

                if (frame.Exists(fallbackEntity) && 
                    frame.TryGet<Transform2D>(fallbackEntity, out var fallbackTransform))
                {
                    return fallbackTransform.Position;
                }

                Log.Error($"[RespawnSystem] Fallback spawn point is invalid!");
            }
            
            Log.Error("[RespawnSystem] No valid spawn points available!");
            return FPVector2.Zero;
        }
    }
}
