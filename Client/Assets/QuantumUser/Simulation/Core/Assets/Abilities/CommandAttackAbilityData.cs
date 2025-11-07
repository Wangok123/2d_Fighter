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
        [Tooltip("无敌帧时间")]
        public FP InvincibilityDuration = FP._0_10;
        
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

        public override unsafe bool TryActivateAbility(Frame frame, EntityRef entityRef, PlayerLink* playerLink, AbilityType abilityType, ref Ability ability)
        {
            bool activated = base.TryActivateAbility(frame, entityRef, playerLink, abilityType, ref ability);
            
            if (activated)
            {
                CommandInputData* commandInputData = frame.Unsafe.GetPointer<CommandInputData>(entityRef);
                if (commandInputData != null)
                {
                    commandInputData->InputBufferIndex = 0;
                    commandInputData->InputBufferSize = 0;
                }
            }

            return activated;
        }

        protected override void OnAttackActivate(Frame frame, EntityRef entityRef, Ability* ability)
        {
            if (InvincibilityDuration > 0)
            {
                SetInvincibility(frame, entityRef, InvincibilityDuration);
            }
            
            if (SpawnsProjectile)
            {
                SpawnProjectiles(frame, entityRef);
            }
            
            frame.Events.SpecialMovePerformed(entityRef, 0, CalculateDamage(frame, entityRef));
            
            if (!SpawnsProjectile)
            {
                base.OnAttackActivate(frame, entityRef, ability);
            }
        }


        protected virtual void SetInvincibility(Frame frame, EntityRef entityRef, FP duration)
        {
            if (frame.Unsafe.TryGetPointer<CharacterStatusComponent>(entityRef, out var characterStatus))
            {
                //characterStatus->InvincibleTimer = FrameTimer.FromSeconds(frame, duration);
            }
        }

        protected virtual void SpawnProjectiles(Frame frame, EntityRef entityRef)
        {
            if (!frame.Unsafe.TryGetPointer<Transform2D>(entityRef, out var transform))
                return;

            MovementComponent* movementData = frame.Unsafe.GetPointer<MovementComponent>(entityRef);
            FPVector2 direction = movementData->IsFacingRight ? FPVector2.Right : FPVector2.Left;
            FP damage = CalculateDamage(frame, entityRef);
            
            for (int i = 0; i < ProjectileCount; i++)
            {
                // frame.Events.ProjectileSpawnRequested(
                //     entityRef, 
                //     transform->Position, 
                //     direction * ProjectileSpeed, 
                //     damage, 
                //     ProjectileLifetime
                // );
            }
        }

        protected override FP CalculateDamage(Frame frame, EntityRef entityRef)
        {
            FP baseDamage = base.CalculateDamage(frame, entityRef);
            return baseDamage * SpecialDamageMultiplier;
        }
    }
}
