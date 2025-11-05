using Photon.Deterministic;
using System;
using UnityEngine;

namespace Quantum
{
    [Serializable]
    public unsafe partial class CommandAttackAbilityData : AttackAbilityData
    {
        [Header("Command Input Settings")]
        [Tooltip("指令输入序列")]
        public int[] InputSequence = new int[] { };
        
        [Tooltip("输入序列必须在此时间内完成")]
        public FP InputTimeWindow = FP._0_50;
        
        [Tooltip("是否需要精确输入")]
        public bool RequireStrictInput = false;
        
        [Header("Special Move Properties")]
        [Tooltip("是否是特殊技")]
        public bool IsSpecialMove = true;
        
        [Tooltip("能量消耗")]
        public FP EnergyCost = 25;
        
        [Tooltip("伤害倍率")]
        public FP SpecialDamageMultiplier = FP._2;
        
        [Header("Invincibility")]
        [Tooltip("无敌帧")]
        public FP InvincibilityFrames = FP._0_10;
        
        [Tooltip("是否在整个攻击过程中都无敌")]
        public bool FullInvincibility = false;
        
        [Header("Projectile Settings")]
        [Tooltip("是否生成投射物")]
        public bool SpawnsProjectile = false;
        
        [Tooltip("投射物速度")]
        public FP ProjectileSpeed = 10;
        
        [Tooltip("投射物存活时间")]
        public FP ProjectileLifetime = FP._2;
        
        [Tooltip("投射物数量")]
        public int ProjectileCount = 1;
        
        [Tooltip("投射物形状")]
        public Shape2DConfig ProjectileShape;
        
        [Header("Advanced Properties")]
        [Tooltip("是否有超级取消")]
        public bool AllowSuperCancel = false;
        
        [Tooltip("超级取消时间窗口")]
        public FP SuperCancelWindow = FP._0_20;

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerStatus* playerStatus, ref Ability ability)
        {
            if (IsSpecialMove && EnergyCost > 0)
            {
                if (frame.TryGet<Energy>(entityRef, out var energy))
                {
                    if (energy.CurrentEnergy < EnergyCost)
                    {
                        return false;
                    }
                }
            }

            bool activated = base.TryActivateAbility(frame, entityRef, playerStatus, ref ability);
            
            if (activated)
            {
                if (IsSpecialMove && EnergyCost > 0 && frame.Unsafe.TryGetPointer<Energy>(entityRef, out var energy))
                {
                    energy->CurrentEnergy -= EnergyCost;
                }
                
                frame.Signals.OnClearInputBuffer(entityRef);
            }

            return activated;
        }

        protected override void OnAttackActivate(Frame frame, EntityRef entityRef, ref Ability ability)
        {
            if (InvincibilityFrames > 0)
            {
                SetInvincibility(frame, entityRef, true, InvincibilityFrames);
            }
            
            if (SpawnsProjectile)
            {
                SpawnProjectiles(frame, entityRef);
            }
            
            frame.Events.SpecialMovePerformed(entityRef, 0, CalculateDamage(frame, entityRef));
            
            if (!SpawnsProjectile)
            {
                base.OnAttackActivate(frame, entityRef, ref ability);
            }
        }

        protected virtual void SetInvincibility(Frame frame, EntityRef entityRef, bool isInvincible, FP duration)
        {
            if (frame.Unsafe.TryGetPointer<CharacterState>(entityRef, out var characterState))
            {
                characterState->IsInvincible = isInvincible ? 1 : 0;
                
                if (duration > 0)
                {
                    characterState->InvincibleTimer = CountdownTimer.FromSeconds(frame, duration);
                }
            }
        }

        protected virtual void SpawnProjectiles(Frame frame, EntityRef entityRef)
        {
            if (!frame.Unsafe.TryGetPointer<Transform2D>(entityRef, out var transform))
                return;

            var movementData = frame.Unsafe.GetPointer<MovementData>(entityRef);
            
            for (int i = 0; i < ProjectileCount; i++)
            {
                EntityRef projectile = frame.Create();
                
                if (frame.Unsafe.TryGetPointer<Transform2D>(projectile, out var projectileTransform))
                {
                    projectileTransform->Position = transform->Position;
                    projectileTransform->Rotation = transform->Rotation;
                }
                
                if (frame.Unsafe.TryGetPointer<Projectile>(projectile, out var projectileData))
                {
                    FP direction = movementData->IsFacingRight ? FP._1 : -FP._1;
                    projectileData->Velocity = new FPVector2(direction * ProjectileSpeed, FP._0);
                    projectileData->LifetimeTimer = CountdownTimer.FromSeconds(frame, ProjectileLifetime);
                    projectileData->Damage = CalculateDamage(frame, entityRef);
                    projectileData->Owner = entityRef;
                }
            }
        }

        protected override FP CalculateDamage(Frame frame, EntityRef entityRef)
        {
            FP baseDamage = base.CalculateDamage(frame, entityRef);
            return baseDamage * SpecialDamageMultiplier;
        }
    }
}
