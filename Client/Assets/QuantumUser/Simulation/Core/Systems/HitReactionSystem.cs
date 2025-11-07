using Photon.Deterministic;

namespace Quantum
{
    public unsafe class HitReactionSystem : SystemMainThreadFilter<HitReactionSystem.Filter>,
        ISignalOnKnockbackApplied,
        ISignalOnStunApplied,
        ISignalOnHitstunApplied,
        ISignalOnDamageTaken,
        ISignalOnEntityDied
    {
        public struct Filter
        {
            public EntityRef Entity;
            public HitReactionComponent* HitReaction;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            HitReactionComponent* hitReaction = filter.HitReaction;
            
            // 早期退出优化：如果没有任何活跃状态，跳过更新
            if (!NeedsUpdate(frame, hitReaction))
                return;
            
            HitReactionConfig config = frame.FindAsset(filter.HitReaction->Config);
            
            if (filter.HitReaction->IsDead)
            {
                UpdateRespawn(frame, filter.Entity, filter.HitReaction, config);
                return;
            }

            UpdateHitstun(frame, filter.HitReaction);
            UpdateKnockback(frame, filter.Entity, filter.HitReaction, config);
            UpdateStun(frame, filter.Entity, filter.HitReaction);
            UpdateInvincibility(frame, filter.HitReaction);
            UpdateSuperArmor(frame, filter.Entity, filter.HitReaction, config);
            UpdateHealthRegen(frame, filter.HitReaction, config);
            UpdateConsecutiveHitCounter(frame, filter.HitReaction, config);
        }
        
        private bool NeedsUpdate(Frame frame, HitReactionComponent* hitReaction)
        {
            return hitReaction->IsDead
                   || hitReaction->IsHitstunned
                   || hitReaction->IsKnockedBack
                   || hitReaction->IsStunned
                   || hitReaction->InvincibilityTimer.IsRunning(frame)
                   || hitReaction->RegenDelayTimer.IsRunning(frame)
                   || hitReaction->ConsecutiveHitResetTimer.IsRunning(frame)
                   || hitReaction->CurrentHealth < hitReaction->MaxHealth
                   || (hitReaction->HasSuperArmor && hitReaction->SuperArmorValue < frame.FindAsset(hitReaction->Config).InitialSuperArmor);
        }

        private void UpdateHitstun(Frame frame, HitReactionComponent* hitReaction)
        {
            if (hitReaction->IsHitstunned && !hitReaction->HitstunTimer.IsRunning(frame))
            {
                hitReaction->IsHitstunned = false;
            }
        }

        private void UpdateKnockback(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            if (!hitReaction->IsKnockedBack)
                return;

            FP velocityMagnitude = hitReaction->KnockbackVelocity.Magnitude;
    
            if (velocityMagnitude > config.MinKnockbackThreshold)
            {
                FP decay = hitReaction->KnockbackDecay * frame.DeltaTime;
                hitReaction->KnockbackVelocity *= (FP._1 - decay);
        
                ApplyKnockbackMovement(frame, entity, hitReaction);
            }
            else
            {
                hitReaction->IsKnockedBack = false;
                hitReaction->KnockbackVelocity = FPVector2.Zero;
        
                // 清除物理速度
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
                {
                    physicsBody->Velocity = FPVector2.Zero;
                }
            }
        }

        private void ApplyKnockbackMovement(Frame frame, EntityRef entity, HitReactionComponent* hitReaction)
        {
            FPVector2 movement = hitReaction->KnockbackVelocity * frame.DeltaTime;
    
            // 优先尝试 KCC2D（玩家角色） 待做
    
            // 尝试 PhysicsBody2D（物理实体）
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
            {
                physicsBody->Velocity = hitReaction->KnockbackVelocity;
                return;
            }
    
            // 回退到直接修改 Transform2D（静态或简单实体）
            if (frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform))
            {
                transform->Position += movement;
            }
        }

        
        private void UpdateStun(Frame frame, EntityRef entity, HitReactionComponent* hitReaction)
        {
            if (hitReaction->StunTimer.IsRunning(frame))
            {
                hitReaction->IsStunned = true;
                
                if (frame.Unsafe.TryGetPointer<CharacterStatusComponent>(entity, out var status))
                {
                    status->IsIncapacitated = true;
                }
            }
            else if (hitReaction->IsStunned)
            {
                hitReaction->IsStunned = false;
                
                if (frame.Unsafe.TryGetPointer<CharacterStatusComponent>(entity, out var status))
                {
                    status->IsIncapacitated = false;
                }
            }
        }

        private void UpdateInvincibility(Frame frame, HitReactionComponent* hitReaction)
        {
            if (!hitReaction->InvincibilityTimer.IsRunning(frame) && 
                hitReaction->InvincibilityTimer.ElapsedSeconds(frame) != FP._0)
            {
                hitReaction->InvincibilityTimer = FrameTimer.None;
            }
        }

        private void UpdateSuperArmor(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            if (!config.HasSuperArmor)
                return;

            if (hitReaction->HasSuperArmor)
            {
                if (hitReaction->SuperArmorValue < config.InitialSuperArmor)
                {
                    if (!hitReaction->RegenDelayTimer.IsRunning(frame))
                    {
                        hitReaction->SuperArmorValue += config.SuperArmorRegenRate * frame.DeltaTime;
                        hitReaction->SuperArmorValue = FPMath.Min(hitReaction->SuperArmorValue, config.InitialSuperArmor);
                    }
                }
            }
            else if (hitReaction->SuperArmorValue <= 0)
            {
                hitReaction->HasSuperArmor = false;
            }
        }

        private void UpdateHealthRegen(Frame frame, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            if (!config.EnableHealthRegen || hitReaction->IsDead)
                return;

            if (hitReaction->CurrentHealth >= hitReaction->MaxHealth)
                return;

            if (hitReaction->RegenDelayTimer.IsRunning(frame))
                return;

            hitReaction->CurrentHealth += config.HealthRegenRate * frame.DeltaTime;
            hitReaction->CurrentHealth = FPMath.Min(hitReaction->CurrentHealth, hitReaction->MaxHealth);
        }

        private void UpdateConsecutiveHitCounter(Frame frame, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            if (hitReaction->ConsecutiveHitCount > 0 && !hitReaction->ConsecutiveHitResetTimer.IsRunning(frame))
            {
                hitReaction->ConsecutiveHitCount = 0;
            }
        }

        private void UpdateRespawn(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            if (!config.CanRespawn)
                return;

            if (!hitReaction->RespawnTimer.IsRunning(frame) && hitReaction->RespawnTimer.ElapsedSeconds(frame) > FP._0)
            {
                RespawnEntity(frame, entity, hitReaction, config);
            }
        }

        private void RespawnEntity(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            hitReaction->CurrentHealth = hitReaction->MaxHealth;
            hitReaction->IsDead = false;
            hitReaction->IsHitstunned = false;
            hitReaction->IsKnockedBack = false;
            hitReaction->IsStunned = false;
            hitReaction->KnockbackVelocity = FPVector2.Zero;
            hitReaction->ConsecutiveHitCount = 0;
            hitReaction->InvincibilityTimer = FrameTimer.FromSeconds(frame, config.RespawnInvincibilityDuration);
            hitReaction->RespawnTimer = FrameTimer.None;
            
            if (config.HasSuperArmor)
            {
                hitReaction->SuperArmorValue = config.InitialSuperArmor;
                hitReaction->HasSuperArmor = true;
            }

            if (frame.Unsafe.TryGetPointer<Transform2D>(entity, out var transform))
            {
                transform->Position = hitReaction->RespawnPosition;
            }

            if (frame.Unsafe.TryGetPointer<CharacterStatusComponent>(entity, out var status))
            {
                status->IsIncapacitated = false;
            }

            frame.Signals.OnEntityRespawned(entity);
            frame.Events.OnPlayerRespawn(entity, hitReaction->RespawnPosition);
        }

        public void OnKnockbackApplied(Frame frame, EntityRef target, FP duration, FPVector2 knockbackVelocity)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            HitReactionConfig config = frame.FindAsset(hitReaction->Config);

            if (hitReaction->IsDead || IsInvincible(frame ,hitReaction) || !config.CanBeKnockedBack)
                return;

            if (hitReaction->HasSuperArmor && config.SuperArmorPreventKnockback)
                return;

            hitReaction->IsKnockedBack = true;
            hitReaction->KnockbackVelocity = knockbackVelocity;
            hitReaction->KnockbackDecay = config.KnockbackDecayRate;
            
            ApplyHitstun(frame, target, hitReaction, config, duration, HitType.Medium);

            if (frame.Unsafe.TryGetPointer<MovementComponent>(target, out var movementData))
            {
                FP horizontalDirection = knockbackVelocity.X;
                if (FPMath.Abs(horizontalDirection) > FP._0_10)
                {
                    movementData->IsFacingRight = horizontalDirection < 0;
                }
            }

            frame.Events.OnPlayerKnockedBack(target, knockbackVelocity.Normalized, knockbackVelocity.Magnitude);
        }

        public void OnStunApplied(Frame frame, EntityRef target, FP duration)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            HitReactionConfig config = frame.FindAsset(hitReaction->Config);

            if (hitReaction->IsDead || IsInvincible(frame,hitReaction) || !config.CanBeStunned)
                return;

            if (hitReaction->HasSuperArmor && config.SuperArmorPreventHitstun)
                return;

            hitReaction->StunTimer = FrameTimer.FromSeconds(frame, duration);
            hitReaction->IsStunned = true;
            
            ApplyHitstun(frame, target, hitReaction, config, duration, HitType.Heavy);
        }

        public void OnHitstunApplied(Frame frame, EntityRef target, FP duration)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            HitReactionConfig config = frame.FindAsset(hitReaction->Config);

            if (hitReaction->IsDead || IsInvincible(frame,hitReaction))
                return;

            ApplyHitstun(frame, target, hitReaction, config, duration, HitType.Light);
        }

        public void OnDamageTaken(Frame frame, EntityRef target, EntityRef attacker, FP damage, HitType hitType)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            HitReactionConfig config = frame.FindAsset(hitReaction->Config);

            if (hitReaction->IsDead || IsInvincible(frame,hitReaction))
                return;

            FP finalDamage = CalculateFinalDamage(frame, hitReaction, config, damage);

            if (finalDamage < config.MinimumDamage)
                return;

            if (hitReaction->HasSuperArmor)
            {
                hitReaction->SuperArmorValue -= finalDamage;
                if (hitReaction->SuperArmorValue <= 0)
                {
                    hitReaction->HasSuperArmor = false;
                    frame.Signals.OnSuperArmorBroken(target);
                }
                else
                {
                    frame.Events.OnSuperArmorTriggered(target, hitReaction->SuperArmorValue);
                    return;
                }
            }

            hitReaction->CurrentHealth -= finalDamage;
            
            ResetHealthRegen(frame, hitReaction, config);
            IncrementConsecutiveHitCounter(frame, hitReaction, config);

            bool isCounterHit = hitReaction->IsHitstunned || hitReaction->IsStunned;

            if (frame.Unsafe.TryGetPointer<Transform2D>(target, out var targetTransform) &&
                frame.Unsafe.TryGetPointer<Transform2D>(attacker, out var attackerTransform))
            {
                FPVector2 hitDirection = (targetTransform->Position - attackerTransform->Position).Normalized;
                frame.Events.OnPlayerHit(target, attacker, finalDamage, hitDirection, hitType, isCounterHit);
            }

            if (hitReaction->CurrentHealth <= 0)
            {
                HandleDeath(frame, target, attacker, hitReaction, config);
            }
        }

        public void OnEntityDied(Frame frame, EntityRef target, EntityRef killer)
        {
            if (!frame.Unsafe.TryGetPointer<HitReactionComponent>(target, out var hitReaction))
                return;

            HitReactionConfig config = frame.FindAsset(hitReaction->Config);
            HandleDeath(frame, target, killer, hitReaction, config);
        }

        private void ApplyHitstun(Frame frame, EntityRef entity, HitReactionComponent* hitReaction, 
            HitReactionConfig config, FP duration, HitType hitType)
        {
            if (config.SuperArmorPreventHitstun && hitReaction->HasSuperArmor)
                return;

            FP multiplier = GetHitstunMultiplier(config, hitType);
            FP finalDuration = duration * multiplier;

            hitReaction->IsHitstunned = true;
            hitReaction->HitstunTimer = FrameTimer.FromSeconds(frame, finalDuration);
            
            if (config.HitInterruptsActions && frame.Unsafe.TryGetPointer<AbilityInventory>(entity, out var abilityInventory))
            {
                if (abilityInventory->HasActiveAbility)
                {
                    frame.Signals.OnActiveAbilityStopped(entity);
                }
            }
        }

        private FP GetHitstunMultiplier(HitReactionConfig config, HitType hitType)
        {
            switch (hitType)
            {
                case HitType.Light:
                    return config.LightHitStunMultiplier;
                case HitType.Medium:
                    return config.MediumHitStunMultiplier;
                case HitType.Heavy:
                case HitType.Launch:
                    return config.HeavyHitStunMultiplier;
                default:
                    return FP._1;
            }
        }

        private FP CalculateFinalDamage(Frame frame, HitReactionComponent* hitReaction, HitReactionConfig config, FP baseDamage)
        {
            FP damage = baseDamage;
            
            damage *= (FP._1 - config.BaseDamageReduction);
            
            if (hitReaction->ConsecutiveHitCount > 0)
            {
                FP reductionCount = FPMath.Min(hitReaction->ConsecutiveHitCount, config.MaxConsecutiveHitReduction);
                FP reductionMultiplier = FP._1 - (config.ConsecutiveHitDamageReduction * reductionCount);
                damage *= FPMath.Max(reductionMultiplier, FP._0_10);
            }

            return damage;
        }

        private void HandleDeath(Frame frame, EntityRef target, EntityRef killer, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            hitReaction->IsDead = true;
            hitReaction->CurrentHealth = 0;
            hitReaction->IsHitstunned = false;
            hitReaction->IsKnockedBack = false;
            hitReaction->IsStunned = false;
            hitReaction->KnockbackVelocity = FPVector2.Zero;
            hitReaction->ConsecutiveHitCount = 0;
            
            if (frame.Unsafe.TryGetPointer<CharacterStatusComponent>(target, out var status))
            {
                status->IsIncapacitated = true;
            }

            if (frame.Unsafe.TryGetPointer<Transform2D>(target, out var transform))
            {
                hitReaction->RespawnPosition = transform->Position;
            }

            if (config.CanRespawn && config.RespawnTime > 0)
            {
                hitReaction->RespawnTimer = FrameTimer.FromSeconds(frame, config.RespawnTime);
            }

            frame.Events.OnPlayerDied(target, killer);
        }

        private void ResetHealthRegen(Frame frame, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            if (config.EnableHealthRegen && config.RegenDelayAfterHit > 0)
            {
                hitReaction->RegenDelayTimer = FrameTimer.FromSeconds(frame, config.RegenDelayAfterHit);
            }
        }

        private void IncrementConsecutiveHitCounter(Frame frame, HitReactionComponent* hitReaction, HitReactionConfig config)
        {
            hitReaction->ConsecutiveHitCount++;
            hitReaction->ConsecutiveHitResetTimer = FrameTimer.FromSeconds(frame, config.ConsecutiveHitResetTime);
        }

        private bool IsInvincible(Frame frame ,HitReactionComponent* hitReaction)
        {
            return hitReaction->InvincibilityTimer.IsRunning(frame);
        }
    }
}