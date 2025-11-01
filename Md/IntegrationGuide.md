# Integration Guide (集成指南)

This guide explains how to integrate the modular ability component system into your workflow.

## Quick Start (快速开始)

### Option 1: Start Fresh with New Characters (新角色直接使用)

If you're creating a new character, use the modular system from the start:

1. **Create Ability Components in Unity Editor**
   
   Navigate to: `Assets/QuantumUser/Resources/Abilities/`
   
   Create folders:
   ```
   Abilities/
   ├── Movement/
   ├── Attack/
   ├── Defense/
   └── Special/
   ```

2. **Create Movement Ability**
   
   Right-click in `Movement/` folder → Create → Quantum → Abilities → Movement Ability Component
   
   Configure:
   - Ability ID: `movement_walk`
   - Ability Name: `Basic Walk`
   - Movement Type: `Walk`
   - Speed Multiplier: `1.0`
   - Priority: `5`

3. **Create Attack Ability**
   
   Right-click in `Attack/` folder → Create → Quantum → Abilities → Attack Ability Component
   
   Configure:
   - Ability ID: `attack_light`
   - Ability Name: `Light Attack`
   - Attack Type: `LightMelee`
   - Base Damage: `10`
   - Can Combo: `true`
   - Priority: `10`

4. **Create Character Config**
   
   Right-click in `Characters/` folder → Create → Quantum → Modular Character Config
   
   Configure:
   - Character ID: `1`
   - Character Name: `Your Character Name`
   - Drag your ability components into their respective arrays

5. **Link to Entity Prototype**
   
   In your character's entity prototype:
   - Reference the ModularCharacterConfig (this would require extending Character.qtn)

### Option 2: Migrate Existing Character (迁移现有角色)

If you have existing characters using `CharacterAttackConfig`:

1. **Keep Existing Config Working**
   
   Don't delete or modify your existing `CharacterAttackConfig` yet. The system supports both.

2. **Create Equivalent Ability Components**
   
   For each attack in your legacy config, create a corresponding ability component:
   
   ```
   LightAttackConfig → AttackAbilityComponent (type: LightMelee)
   HeavyAttackConfig → AttackAbilityComponent (type: HeavyMelee)
   SpecialMoveConfig → SpecialAbilityComponent
   ```

3. **Create ModularCharacterConfig**
   
   Create a new `ModularCharacterConfig` and populate it with your new components.

4. **Test Both Systems**
   
   Keep the legacy config reference in `ModularCharacterConfig.LegacyAttackConfig` for fallback.

5. **Switch Over**
   
   Once tested, you can remove the legacy config reference.

## System Integration (系统集成)

### Enabling the Modular System (启用模块化系统)

The `ModularAbilitySystem` is designed to coexist with `NormalAttackSystem`.

#### Current Behavior:
- If entity has no `ModularCharacterConfig`, `NormalAttackSystem` handles it (legacy behavior)
- If entity has `ModularCharacterConfig`, `ModularAbilitySystem` handles it

#### To Enable on Entity:

You need to extend the `Character.qtn` component to include a reference to `ModularCharacterConfig`:

```qtn
component AttackData
{
    asset_ref<CharacterAttackConfig> AttackConfig;
    asset_ref<ModularCharacterConfig> ModularConfig;  // ADD THIS LINE
    [ExcludeFromPrototype] Int32 ComboCounter;
    [ExcludeFromPrototype] FrameTimer ComboResetTimer;
    [ExcludeFromPrototype] FrameTimer AttackCooldown;
    [ExcludeFromPrototype] bool IsAttacking;
    [ExcludeFromPrototype] FP HeavyChargeTime;
    [ExcludeFromPrototype] bool IsChargingHeavy;
}
```

Then update `ModularAbilitySystem.GetModularConfig()`:

```csharp
private ModularCharacterConfig GetModularConfig(Frame frame, ref Filter filter)
{
    if (filter.AttackData->ModularConfig.Id.IsValid)
    {
        return frame.FindAsset(filter.AttackData->ModularConfig);
    }
    return null;
}
```

### Priority Handling (优先级处理)

The system processes abilities by priority (highest first):

**Recommended Priority Ranges:**
```
Special Abilities:  100-199
Defense Abilities:   30-99
Heavy Attacks:       50-99
Light Attacks:       10-29
Movement:             0-9
```

Higher priority abilities are checked first. Once an ability executes, no other abilities execute that frame.

### Input Mapping (输入映射)

The system maps inputs to ability types:

**Current Mappings:**
```csharp
Light Attack (LP button) → AttackAbilityType.LightMelee
Heavy Attack (HP button) → AttackAbilityType.HeavyMelee
Block (Block button)     → DefenseAbilityType.Block
Special (Input Sequence) → SpecialAbilityComponent.InputSequence
```

**To Add Custom Mappings:**

Modify `ModularAbilitySystem.ShouldExecuteAttackAbility()`:

```csharp
private bool ShouldExecuteAttackAbility(SimpleInput2D input, AttackAbilityComponent ability)
{
    switch (ability.AttackType)
    {
        case AttackAbilityType.LightMelee:
            return input.LP.WasPressed;
        case AttackAbilityType.HeavyMelee:
            return input.HP.WasPressed || input.HP.IsDown;
        case AttackAbilityType.Projectile:
            return input.LK.WasPressed;  // Add your custom mapping
        // ... more mappings
        default:
            return false;
    }
}
```

## Creating Custom Ability Types (创建自定义能力类型)

### Adding New Ability Category (新增能力类别)

If you need a completely new type of ability (e.g., Utility abilities):

1. **Create New Component Class**

```csharp
// File: UtilityAbilityComponent.cs
using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    public class UtilityAbilityComponent : AbilityComponentBase
    {
        [Header("Utility Type")]
        public UtilityAbilityType UtilityType;
        
        [Header("Utility Properties")]
        public FP EffectDuration;
        public FP EffectRange;
        
        public UtilityAbilityComponent()
        {
            AbilityId = "utility_basic";
            AbilityName = "Basic Utility";
            Priority = 40;
        }
    }
    
    public enum UtilityAbilityType
    {
        Heal,
        Scout,
        Teleporter,
        Trap
    }
}
```

2. **Add to ModularCharacterConfig**

```csharp
public class ModularCharacterConfig : AssetObject
{
    // ... existing code ...
    
    [Header("Utility Abilities")]
    public AssetRef<UtilityAbilityComponent>[] UtilityAbilities;
}
```

3. **Process in ModularAbilitySystem**

```csharp
// In ProcessAbilitiesByPriority():

// Add utility abilities
if (config.UtilityAbilities != null)
{
    foreach (var abilityRef in config.UtilityAbilities)
    {
        var ability = frame.FindAsset(abilityRef);
        if (ability != null && IsAbilityUnlocked(filter.Level, ability))
        {
            if (ShouldExecuteUtilityAbility(input, ability))
            {
                abilitiesToProcess.Add((ability.Priority, () => ExecuteUtilityAbility(frame, ref filter, ability)));
            }
        }
    }
}
```

### Adding New Ability Instance Type (新增能力实例类型)

To add a new attack type to existing `AttackAbilityComponent`:

1. **Extend the Enum**

```csharp
public enum AttackAbilityType
{
    LightMelee,
    HeavyMelee,
    Projectile,
    AreaOfEffect,
    Grab,
    ChargedShot,
    RapidFire,
    Uppercut,
    GroundPound,
    Counter,
    Sweep,        // NEW: Add your new type
    Launcher,     // NEW: Add your new type
}
```

2. **Handle Input Mapping**

```csharp
private bool ShouldExecuteAttackAbility(SimpleInput2D input, AttackAbilityComponent ability)
{
    switch (ability.AttackType)
    {
        case AttackAbilityType.LightMelee:
            return input.LP.WasPressed;
        case AttackAbilityType.Sweep:
            return input.Down.IsDown && input.LK.WasPressed;  // Down + Light Kick
        case AttackAbilityType.Launcher:
            return input.Up.IsDown && input.HP.WasPressed;    // Up + Heavy Punch
        // ... other cases
    }
}
```

3. **Create Assets in Unity**

Just create a new `AttackAbilityComponent` asset and set its type to the new enum value!

## Level-Based Unlocks (等级解锁)

### Configuring Unlocks (配置解锁)

In `ModularCharacterConfig`, set up the unlock schedule:

```csharp
AbilityUnlocks = new AbilityUnlock[]
{
    new AbilityUnlock
    {
        UnlockLevel = 5,
        AbilityId = "movement_double_jump",
        UnlockDescription = "Unlock Double Jump"
    },
    new AbilityUnlock
    {
        UnlockLevel = 10,
        AbilityId = "special_ultimate",
        UnlockDescription = "Unlock Ultimate Ability"
    }
};
```

### How Unlocks Work (解锁机制)

1. Abilities with `UnlockedByDefault = true` are available immediately
2. Abilities with `RequiredLevel > 0` become available at that level
3. `AbilityUnlocks` array provides UI descriptions for unlock events
4. System checks both `UnlockedByDefault` and `RequiredLevel` before executing abilities

## Performance Considerations (性能考虑)

### Asset References (资源引用)

- All ability components are `AssetObject` references
- Quantum handles asset loading efficiently
- No runtime instantiation needed
- Assets are loaded once and reused

### Priority Sorting (优先级排序)

- Abilities are sorted by priority each frame
- For better performance, keep ability arrays small
- Use priority ranges to group similar abilities
- Consider caching sorted list if needed

### Determinism (确定性)

All components use Quantum's `FP` (Fixed Point) types:
- ✅ `FP` for all numeric values
- ✅ `FrameTimer` for time tracking
- ✅ Deterministic enums and arrays
- ❌ No `float` or `double` types
- ❌ No `Unity` time or random

## Debugging (调试)

### Logging (日志)

The system includes debug logging:

```csharp
Log.Debug($"Modular Attack: {ability.AbilityName} - Type: {ability.AttackType}, Damage: {damage}");
```

Enable in Quantum settings to see ability execution.

### Common Issues (常见问题)

**Q: Abilities not executing?**
- Check `RequiredLevel` and `UnlockedByDefault`
- Verify input mapping matches ability type
- Ensure ability priority is set correctly
- Check if cooldown is active

**Q: Multiple abilities executing?**
- Check priority values - only highest priority should execute
- Verify input mapping doesn't overlap

**Q: Legacy config not working?**
- Both systems can coexist
- `NormalAttackSystem` handles legacy configs
- `ModularAbilitySystem` only handles entities with `ModularCharacterConfig`

## Best Practices (最佳实践)

### 1. Organization (组织结构)

```
Assets/QuantumUser/Resources/
├── Abilities/
│   ├── Movement/
│   │   ├── Walk_Movement.asset
│   │   ├── Dash_Movement.asset
│   │   └── DoubleJump_Movement.asset
│   ├── Attack/
│   │   ├── LightPunch_Attack.asset
│   │   └── HeavyPunch_Attack.asset
│   ├── Defense/
│   │   └── Block_Defense.asset
│   └── Special/
│       └── Fireball_Special.asset
└── Characters/
    ├── Warrior_ModularConfig.asset
    ├── Ninja_ModularConfig.asset
    └── Mage_ModularConfig.asset
```

### 2. Naming Convention (命名规范)

- Abilities: `[Description]_[Category]` (e.g., `Fireball_Special`)
- Characters: `[Name]_ModularConfig` (e.g., `Warrior_ModularConfig`)
- Use clear, descriptive names
- Avoid abbreviations unless standard

### 3. Reusability (可复用性)

- Create generic abilities first (Walk, Block, Light Attack)
- Create character-specific abilities only when needed
- Consider creating variants instead of duplicates
- Document which characters use which abilities

### 4. Testing (测试)

- Test each ability component independently
- Test character compositions thoroughly
- Verify unlock progression works correctly
- Test edge cases (cooldowns, combos, etc.)

### 5. Documentation (文档记录)

- Fill in `Description` field for all abilities
- Document special interactions
- Keep track of ability reuse in a spreadsheet
- Update character configs when abilities change

## Migration Checklist (迁移检查清单)

When migrating an existing character:

- [ ] Create equivalent ability components for all attacks
- [ ] Create movement ability components
- [ ] Create defense ability components
- [ ] Create special ability components
- [ ] Create `ModularCharacterConfig` asset
- [ ] Configure passive traits
- [ ] Set up ability unlocks
- [ ] Add reference to entity prototype (requires Character.qtn update)
- [ ] Test all abilities work correctly
- [ ] Test combo system
- [ ] Test charging mechanics
- [ ] Test special moves with input sequences
- [ ] Test level unlocks
- [ ] Verify compatibility with existing systems
- [ ] Remove legacy config reference (optional)

## Next Steps (下一步)

1. Review the [complete documentation](ModularCharacterSystem.md)
2. Check out [example character configs](ExampleCharacters.md)
3. Try creating a simple test character
4. Migrate one existing character as a pilot
5. Create new characters using the modular system

## Support (支持)

For questions or issues:
1. Check the main documentation: `Md/ModularCharacterSystem.md`
2. Review example configs: `Md/ExampleCharacters.md`
3. Examine existing ability component implementations
4. Review the `ModularAbilitySystem` source code

Happy character creating! 🎮
