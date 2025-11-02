# Refactoring: Modular Character System Optimization

## Overview
This refactoring addresses redundancy and performance issues in the movement and ability systems by:
1. Removing the redundant `AttackConfig` reference from `AttackData` component
2. Making `ModularCharacterConfig` the single source of character configuration
3. Reorganizing systems for better code clarity and maintainability

## Key Changes

### 1. Component Simplification

**AttackData Component (Character.qtn)**
- **Removed**: `asset_ref<CharacterAttackConfig> AttackConfig` (redundant)
- **Kept**: `asset_ref<ModularCharacterConfig> ModularConfig` (single source of truth)
- All character configuration now flows through `ModularCharacterConfig`

### 2. Configuration Consolidation

**ModularCharacterConfig**
- **Added**: Command input settings (`CommandInputWindow`, `MaxInputBufferSize`)
- Now contains all settings previously split between `AttackConfig` and `ModularConfig`
- Single asset manages all character abilities, unlocks, and configuration

### 3. System Reorganization

**New Focused Systems:**

1. **MovementInputSystem** (replaces MovementSystem)
   - Handles player input processing and KCC movement
   - Applies ability unlock filtering
   - Manages facing direction updates
   - Uses only `ModularCharacterConfig` for configuration

2. **AbilityInputSystem** (replaces ModularAbilitySystem)
   - Processes all ability inputs (attack, special)
   - Priority-based ability execution
   - Timer management for combos and cooldowns
   - Uses only `ModularCharacterConfig` for configuration

3. **CommandInputSystem** (updated)
   - Now uses `ModularCharacterConfig` instead of `CharacterAttackConfig`
   - Tracks input sequences for special moves
   - Simplified configuration access

**Legacy Systems (Deprecated):**
- `MovementSystem.Legacy.cs` - kept for backward compatibility
- `ModularAbilitySystem.Legacy.cs` - kept for backward compatibility
- Both updated to use only `ModularCharacterConfig`

## Benefits

### 1. Reduced Redundancy
- Single configuration source (`ModularCharacterConfig`) instead of dual references
- Eliminates confusion about which config to use
- Easier to maintain and extend

### 2. Better Code Organization
- Systems have clear, focused responsibilities
- Easier to understand and modify
- Each system does one thing well

### 3. Improved Maintainability
- New abilities can be added to `ModularCharacterConfig` without touching multiple assets
- Simpler upgrade path for existing characters
- Clear separation between runtime state (components) and configuration (assets)

### 4. Performance Considerations
- Fewer asset lookups (single config instead of multiple)
- More cache-friendly data access patterns
- Systems remain as `SystemMainThreadFilter` for Quantum determinism

## Migration Guide

### For Existing Characters

1. **Update Entity Prototypes**:
   ```
   - Remove AttackConfig reference from AttackData component
   - Ensure ModularConfig is set on AttackData component
   ```

2. **Update ModularCharacterConfig Assets**:
   ```
   - Add CommandInputWindow value (default: 0.5)
   - Add MaxInputBufferSize value (default: 8)
   ```

3. **System Configuration**:
   ```
   - Remove or disable legacy systems (MovementSystem.Legacy, ModularAbilitySystem.Legacy)
   - Ensure new systems are active (MovementInputSystem, AbilityInputSystem)
   ```

### For New Characters

Simply create a `ModularCharacterConfig` asset with:
- Character identity (ID, name, description)
- Movement, attack, defense, and special abilities
- Passive traits
- Ability unlocks (if using progression system)
- Command input settings

Then reference it from the entity's `AttackData` component.

## Code Examples

### Before (Legacy Approach)
```csharp
// Had to check both configs
var attackConfig = frame.FindAsset(filter.AttackData->AttackConfig);
var modularConfig = frame.FindAsset(filter.AttackData->ModularConfig);

// Confusion about which to use
bool unlocked = level->CurrentLevel >= attackConfig.DashUnlockLevel; // Legacy
// vs
bool unlocked = IsAbilityUnlocked(level, modularConfig, AbilityId.MovementDash); // Modular
```

### After (Refactored Approach)
```csharp
// Single config source
var modularConfig = frame.FindAsset(filter.AttackData->ModularConfig);

// Clear unlock checking
bool unlocked = IsAbilityUnlocked(level, modularConfig, AbilityId.MovementDash);
```

## Testing

After migration, verify:
1. Movement abilities (walk, jump, dash, double jump) work correctly
2. Attack abilities (light, heavy) execute properly
3. Special moves trigger on correct input sequences
4. Ability unlocks function as expected
5. Combo system operates correctly
6. No null reference errors for config lookups

## Future Enhancements

This refactoring enables:
1. **Signal-based systems**: Foundation for converting to `SystemSignalsOnly` pattern
2. **Better ability composition**: Easier to mix and match abilities
3. **Runtime ability modification**: Dynamic ability unlocking/locking
4. **Data-driven character creation**: More designer-friendly workflow

## Notes

- Legacy systems are retained for backward compatibility
- All new development should use `MovementInputSystem` and `AbilityInputSystem`
- The refactoring maintains Quantum's deterministic execution model
- No gameplay changes - only internal architecture improvements
