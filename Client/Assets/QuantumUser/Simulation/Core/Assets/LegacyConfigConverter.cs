using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    /// <summary>
    /// Utility class to help migrate from legacy CharacterAttackConfig to ModularCharacterConfig.
    /// This provides helper methods to convert existing configs to the new modular system.
    /// </summary>
    public static class LegacyConfigConverter
    {
        /// <summary>
        /// Checks if a character is using the new modular system or legacy system
        /// </summary>
        public static bool IsUsingModularSystem(CharacterAttackConfig legacyConfig)
        {
            // A character uses modular system if it has no legacy attack config set
            return legacyConfig == null;
        }

        /// <summary>
        /// Convert a LightAttackConfig to an AttackAbilityComponent
        /// This helps migrate existing attack configs to the new system
        /// </summary>
        public static AttackAbilityComponent ConvertLightAttack(LightAttackConfig lightConfig)
        {
            if (lightConfig == null) return null;

            var component = ScriptableObject.CreateInstance<AttackAbilityComponent>();
            component.AbilityId = AbilityId.AttackLight;
            component.AbilityName = "Light Attack (Converted)";
            component.AttackType = AttackAbilityType.LightMelee;
            
            // Copy base properties
            component.Priority = lightConfig.Priority;
            component.BaseDamage = lightConfig.Damage;
            component.Cooldown = lightConfig.Cooldown;
            
            // Copy combo properties
            component.CanCombo = true;
            component.MaxComboCount = lightConfig.MaxComboCount;
            component.ComboWindow = lightConfig.ComboWindow;
            component.ComboDamageMultipliers = lightConfig.ComboDamageMultipliers;
            
            return component;
        }

        /// <summary>
        /// Convert a HeavyAttackConfig to an AttackAbilityComponent
        /// </summary>
        public static AttackAbilityComponent ConvertHeavyAttack(HeavyAttackConfig heavyConfig)
        {
            if (heavyConfig == null) return null;

            var component = ScriptableObject.CreateInstance<AttackAbilityComponent>();
            component.AbilityId = AbilityId.AttackHeavy;
            component.AbilityName = "Heavy Attack (Converted)";
            component.AttackType = AttackAbilityType.HeavyMelee;
            
            // Copy base properties
            component.Priority = heavyConfig.Priority;
            component.BaseDamage = heavyConfig.Damage;
            component.Cooldown = heavyConfig.Cooldown;
            
            // Copy charge properties
            component.CanCharge = true;
            component.MinChargeTime = heavyConfig.MinChargeTime;
            component.MaxChargeTime = heavyConfig.MaxChargeTime;
            component.FullChargeDamageMultiplier = heavyConfig.FullChargeDamageMultiplier;
            
            return component;
        }

        /// <summary>
        /// Convert a SpecialMoveConfig to a SpecialAbilityComponent
        /// </summary>
        public static SpecialAbilityComponent ConvertSpecialMove(SpecialMoveConfig specialConfig)
        {
            if (specialConfig == null) return null;

            var component = ScriptableObject.CreateInstance<SpecialAbilityComponent>();
            
            // Map legacy MoveId to the Special abilities range (300-399 in AbilityId enum)
            // Using SpecialUltimate as the base value for clarity
            // Clamp to ensure valid range (300-399)
            int specialIdValue = (int)AbilityId.SpecialUltimate + specialConfig.MoveId;
            if (specialIdValue > 399)
            {
                Log.Warn($"Special ability MoveId {specialConfig.MoveId} exceeds valid range, clamping to 399");
                specialIdValue = 399;
            }
            component.AbilityId = (AbilityId)specialIdValue;
            
            component.AbilityName = specialConfig.MoveName;
            component.SpecialType = SpecialAbilityType.Combo; // Default type
            
            // Copy properties
            component.Damage = specialConfig.Damage;
            component.Cooldown = specialConfig.Cooldown;
            component.RequiredLevel = specialConfig.RequiredLevel;
            component.InputSequence = specialConfig.InputSequence;
            component.Priority = 100; // Default special priority
            
            return component;
        }

        /// <summary>
        /// Create a ModularCharacterConfig from a legacy CharacterAttackConfig
        /// This is useful for one-time conversion of existing characters
        /// </summary>
        public static ModularCharacterConfig ConvertToModularConfig(CharacterAttackConfig legacyConfig, int characterId, string characterName)
        {
            if (legacyConfig == null) return null;

            var modularConfig = ScriptableObject.CreateInstance<ModularCharacterConfig>();
            modularConfig.CharacterId = characterId;
            modularConfig.CharacterName = characterName;
            modularConfig.Description = $"Converted from legacy config";
            
            // Convert attack configs to attack ability components
            var attackAbilities = new System.Collections.Generic.List<AssetRef<AttackAbilityComponent>>();
            
            // Note: These would need to be saved as assets first
            // This is just a demonstration of the conversion logic
            
            // Store reference to legacy config for backward compatibility
            modularConfig.LegacyAttackConfig = new AssetRef<CharacterAttackConfig>();
            
            // Set up default passive traits
            modularConfig.PassiveTraits = new PassiveTraits
            {
                HealthMultiplier = FP._1,
                SpeedMultiplier = FP._1,
                DamageMultiplier = FP._1,
                DefenseMultiplier = FP._1,
                HealthRegeneration = FP._0_50,
                EnergyRegeneration = FP._2
            };
            
            // Set up ability unlocks based on legacy config
            var unlocks = new System.Collections.Generic.List<AbilityUnlock>();
            
            if (legacyConfig.DoubleJumpUnlockLevel > 0)
            {
                unlocks.Add(new AbilityUnlock
                {
                    UnlockLevel = legacyConfig.DoubleJumpUnlockLevel,
                    AbilityId = AbilityId.MovementDoubleJump,
                    UnlockDescription = "Unlock Double Jump"
                });
            }
            
            if (legacyConfig.DashUnlockLevel > 0)
            {
                unlocks.Add(new AbilityUnlock
                {
                    UnlockLevel = legacyConfig.DashUnlockLevel,
                    AbilityId = AbilityId.MovementDash,
                    UnlockDescription = "Unlock Dash"
                });
            }
            
            modularConfig.AbilityUnlocks = unlocks.ToArray();
            
            return modularConfig;
        }
    }
}
