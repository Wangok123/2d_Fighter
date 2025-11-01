# Example Character Configurations (示例角色配置)

This file demonstrates how to create characters using the modular ability component system.

## Reusable Ability Components (可复用的能力组件)

These ability components can be shared across multiple characters.

### Movement Abilities (移动能力)

#### 1. Basic Walk (基础移动)
```yaml
Type: MovementAbilityComponent
AbilityId: movement_walk
AbilityName: Basic Walk
MovementType: Walk
SpeedMultiplier: 1.0
CanUseInAir: false
Priority: 5
UnlockedByDefault: true
RequiredLevel: 0
```

#### 2. Double Jump (二段跳)
```yaml
Type: MovementAbilityComponent
AbilityId: movement_double_jump
AbilityName: Double Jump
MovementType: DoubleJump
MaxAirUses: 1
CanUseInAir: true
Priority: 8
UnlockedByDefault: false
RequiredLevel: 5
Cooldown: 0.5
```

#### 3. Dash (冲刺)
```yaml
Type: MovementAbilityComponent
AbilityId: movement_dash
AbilityName: Dash
MovementType: Dash
MovementDistance: 3.0
MovementDuration: 0.25
GrantsInvincibility: true
CanUseInAir: false
Priority: 15
UnlockedByDefault: false
RequiredLevel: 5
Cooldown: 1.0
```

#### 4. Air Dash (空中冲刺)
```yaml
Type: MovementAbilityComponent
AbilityId: movement_air_dash
AbilityName: Air Dash
MovementType: AirDash
MovementDistance: 2.5
MovementDuration: 0.2
MaxAirUses: 1
CanUseInAir: true
Priority: 15
UnlockedByDefault: false
RequiredLevel: 10
Cooldown: 2.0
```

### Attack Abilities (攻击能力)

#### 1. Light Punch (轻拳)
```yaml
Type: AttackAbilityComponent
AbilityId: attack_light_punch
AbilityName: Light Punch
AttackType: LightMelee
BaseDamage: 10
DamagePerLevel: 0.5
AttackRange: 1.0
AttackWidth: 0.5
StartupTime: 0.1
ActiveTime: 0.2
RecoveryTime: 0.3
CanCombo: true
MaxComboCount: 3
ComboWindow: 0.5
ComboDamageMultipliers: [1.0, 1.25, 1.5]
Priority: 10
Cooldown: 0.25
UnlockedByDefault: true
```

#### 2. Heavy Punch (重拳)
```yaml
Type: AttackAbilityComponent
AbilityId: attack_heavy_punch
AbilityName: Heavy Punch
AttackType: HeavyMelee
BaseDamage: 25
DamagePerLevel: 1.0
AttackRange: 1.2
AttackWidth: 0.7
StartupTime: 0.3
ActiveTime: 0.3
RecoveryTime: 0.5
CanCharge: true
MinChargeTime: 0.5
MaxChargeTime: 2.0
FullChargeDamageMultiplier: 2.0
Priority: 50
Cooldown: 0.5
UnlockedByDefault: true
```

#### 3. Light Kick (轻腿)
```yaml
Type: AttackAbilityComponent
AbilityId: attack_light_kick
AbilityName: Light Kick
AttackType: LightMelee
BaseDamage: 12
DamagePerLevel: 0.6
AttackRange: 1.3
AttackWidth: 0.6
StartupTime: 0.15
ActiveTime: 0.2
RecoveryTime: 0.35
CanCombo: true
MaxComboCount: 2
ComboWindow: 0.6
ComboDamageMultipliers: [1.0, 1.5]
Priority: 12
Cooldown: 0.3
UnlockedByDefault: true
```

#### 4. Magic Bolt (魔法弹)
```yaml
Type: AttackAbilityComponent
AbilityId: attack_magic_bolt
AbilityName: Magic Bolt
AttackType: Projectile
BaseDamage: 15
DamagePerLevel: 0.8
IsProjectile: true
ProjectileSpeed: 10
ProjectileLifetime: 2.0
ProjectileCount: 1
AttackRange: 10.0
StartupTime: 0.2
ActiveTime: 0.1
RecoveryTime: 0.4
Priority: 20
Cooldown: 0.4
EnergyCost: 10
UnlockedByDefault: true
```

### Defense Abilities (防御能力)

#### 1. Basic Block (基础格挡)
```yaml
Type: DefenseAbilityComponent
AbilityId: defense_block
AbilityName: Basic Block
DefenseType: Block
DamageReduction: 0.5
DefenseDuration: 1.0
CanMoveWhileDefending: false
Priority: 30
UnlockedByDefault: true
```

#### 2. Perfect Parry (完美弹反)
```yaml
Type: DefenseAbilityComponent
AbilityId: defense_parry
AbilityName: Perfect Parry
DefenseType: Parry
DamageReduction: 1.0
HasPerfectWindow: true
PerfectWindowDuration: 0.1
PerfectWindowAtStart: true
CanCounter: true
CounterDamageMultiplier: 1.5
CounterWindowDuration: 0.5
Priority: 40
Cooldown: 3.0
UnlockedByDefault: false
RequiredLevel: 8
```

#### 3. Magic Barrier (魔法屏障)
```yaml
Type: DefenseAbilityComponent
AbilityId: defense_barrier
AbilityName: Magic Barrier
DefenseType: Barrier
DamageReduction: 0.8
DefenseDuration: 3.0
ContinuousCost: true
EnergyCostPerSecond: 15
Priority: 35
Cooldown: 5.0
EnergyCost: 30
UnlockedByDefault: false
RequiredLevel: 10
```

### Special Abilities (特殊能力)

#### 1. Hadouken (波动拳)
```yaml
Type: SpecialAbilityComponent
AbilityId: special_hadouken
AbilityName: Hadouken
SpecialType: Projectile
InputSequence: [Down, DownRight, Right, LP]  # 下, 右下, 右, 轻拳
Damage: 30
EffectRange: 8.0
GroundOnly: true
CanActivateAnytime: false
Priority: 100
Cooldown: 2.0
EnergyCost: 25
UnlockedByDefault: false
RequiredLevel: 5
```

#### 2. Shoryuken (升龙拳)
```yaml
Type: SpecialAbilityComponent
AbilityId: special_shoryuken
AbilityName: Shoryuken
SpecialType: Uppercut
InputSequence: [Right, Down, DownRight, HP]  # 右, 下, 右下, 重拳
Damage: 40
EffectDuration: 0.8
GroundOnly: true
Priority: 110
Cooldown: 3.0
EnergyCost: 35
UnlockedByDefault: false
RequiredLevel: 8
```

#### 3. Meteor Strike (陨石打击)
```yaml
Type: SpecialAbilityComponent
AbilityId: special_meteor
AbilityName: Meteor Strike
SpecialType: AreaDamage
Damage: 50
EffectRange: 4.0
EffectDuration: 1.0
IsUltimate: true
RequiredCharge: 100
ChargePerDamage: 0.5
ChargePerDamageTaken: 1.0
Priority: 150
Cooldown: 1.0
UnlockedByDefault: false
RequiredLevel: 15
```

#### 4. Healing Aura (治疗光环)
```yaml
Type: SpecialAbilityComponent
AbilityId: special_heal
AbilityName: Healing Aura
SpecialType: Healing
HealingAmount: 50
EffectRange: 5.0
EffectDuration: 3.0
Priority: 80
Cooldown: 10.0
EnergyCost: 50
UnlockedByDefault: false
RequiredLevel: 12
```

## Character Configurations (角色配置)

### Character 1: Warrior (战士)
First character - needs to create many components.

```yaml
Type: ModularCharacterConfig
CharacterId: 1
CharacterName: Warrior
Description: A balanced fighter with strong melee attacks

MovementAbilities:
  - movement_walk
  - movement_double_jump (unlocked at level 5)
  - movement_dash (unlocked at level 5)

AttackAbilities:
  - attack_light_punch
  - attack_heavy_punch

DefenseAbilities:
  - defense_block

SpecialAbilities:
  - special_hadouken (unlocked at level 5)
  - special_shoryuken (unlocked at level 8)

PassiveTraits:
  HealthMultiplier: 1.0
  SpeedMultiplier: 1.0
  DamageMultiplier: 1.0
  DefenseMultiplier: 1.0
  HealthRegeneration: 0.5
  EnergyRegeneration: 2.0

AbilityUnlocks:
  - Level: 5, AbilityId: movement_double_jump, Description: "Unlock Double Jump"
  - Level: 5, AbilityId: movement_dash, Description: "Unlock Dash"
  - Level: 5, AbilityId: special_hadouken, Description: "Unlock Hadouken"
  - Level: 8, AbilityId: special_shoryuken, Description: "Unlock Shoryuken"
```

**Components Created:** 9 new components
**Reused:** 0

---

### Character 2: Ninja (忍者)
Second character - reuses some components, creates a few new ones.

```yaml
Type: ModularCharacterConfig
CharacterId: 2
CharacterName: Ninja
Description: Fast and agile fighter with aerial mobility

MovementAbilities:
  - movement_walk (REUSED!)
  - movement_double_jump (REUSED!)
  - movement_air_dash (NEW - created for this character)

AttackAbilities:
  - attack_light_kick (NEW - created for this character)

DefenseAbilities:
  - defense_parry (NEW - created for this character)

SpecialAbilities:
  - special_shoryuken (REUSED!)

PassiveTraits:
  HealthMultiplier: 0.8
  SpeedMultiplier: 1.3
  DamageMultiplier: 0.9
  DefenseMultiplier: 0.7
  HealthRegeneration: 0.3
  EnergyRegeneration: 3.0

AbilityUnlocks:
  - Level: 3, AbilityId: movement_double_jump, Description: "Unlock Double Jump"
  - Level: 5, AbilityId: movement_air_dash, Description: "Unlock Air Dash"
  - Level: 8, AbilityId: defense_parry, Description: "Unlock Perfect Parry"
  - Level: 10, AbilityId: special_shoryuken, Description: "Unlock Shoryuken"
```

**Components Created:** 3 new components
**Reused:** 3 components
**Improvement:** 66% reuse!

---

### Character 3: Mage (法师)
Third character - mostly composing from existing components!

```yaml
Type: ModularCharacterConfig
CharacterId: 3
CharacterName: Mage
Description: Ranged spellcaster with powerful magic

MovementAbilities:
  - movement_walk (REUSED!)

AttackAbilities:
  - attack_magic_bolt (NEW - but simple projectile)

DefenseAbilities:
  - defense_barrier (NEW - but similar to block)

SpecialAbilities:
  - special_hadouken (REUSED! - just reskinned as fireball)
  - special_meteor (NEW - ultimate ability)
  - special_heal (NEW - support ability)

PassiveTraits:
  HealthMultiplier: 0.7
  SpeedMultiplier: 0.9
  DamageMultiplier: 1.3
  DefenseMultiplier: 0.6
  HealthRegeneration: 0.2
  EnergyRegeneration: 4.0

AbilityUnlocks:
  - Level: 5, AbilityId: special_hadouken, Description: "Unlock Fireball"
  - Level: 10, AbilityId: defense_barrier, Description: "Unlock Magic Barrier"
  - Level: 12, AbilityId: special_heal, Description: "Unlock Healing Aura"
  - Level: 15, AbilityId: special_meteor, Description: "Unlock Meteor Strike"
```

**Components Created:** 4 new components
**Reused:** 2 components
**Improvement:** 33% reuse (but many unique abilities for variety)

---

### Character 4: Tank (坦克)
Fourth character - high reuse!

```yaml
Type: ModularCharacterConfig
CharacterId: 4
CharacterName: Tank
Description: High defense, slow movement, absorbs damage

MovementAbilities:
  - movement_walk (REUSED!)

AttackAbilities:
  - attack_heavy_punch (REUSED!)

DefenseAbilities:
  - defense_block (REUSED!)
  - defense_barrier (REUSED!)

SpecialAbilities:
  - special_heal (REUSED!)

PassiveTraits:
  HealthMultiplier: 1.5
  SpeedMultiplier: 0.7
  DamageMultiplier: 1.1
  DefenseMultiplier: 1.4
  HealthRegeneration: 1.0
  EnergyRegeneration: 1.5

AbilityUnlocks:
  - Level: 5, AbilityId: defense_barrier, Description: "Unlock Barrier"
  - Level: 10, AbilityId: special_heal, Description: "Unlock Self-Heal"
```

**Components Created:** 0 new components!
**Reused:** 5 components
**Improvement:** 100% reuse! 🎉

---

## Summary (总结)

### Component Reuse Statistics

| Character | New Components | Reused Components | Reuse Rate |
|-----------|---------------|-------------------|------------|
| Warrior   | 9             | 0                 | 0%         |
| Ninja     | 3             | 3                 | 50%        |
| Mage      | 4             | 2                 | 33%        |
| Tank      | 0             | 5                 | 100%       |

### Total Component Library Growth

After 4 characters:
- Total unique components created: 16
- Average components per character: 4
- Average reuse rate (excluding first): 61%

### Overwatch-Style Workflow Achieved! ✅

- ✅ First hero requires creating many components
- ✅ Subsequent heroes reuse more and more components
- ✅ Eventually, new heroes can be created by pure assembly
- ✅ Maintains flexibility for unique abilities
- ✅ Drastically reduces development time

### Creating the 5th, 6th, 7th... Characters

As the component library grows, creating new characters becomes faster:
- Pick movement style from library
- Pick attack style from library
- Pick defense style from library
- Add 1-2 unique special abilities (optional)
- Adjust passive traits
- Configure unlock progression
- Done! 🚀
