# KCC2D + AbilityEnable Integration Guide

## Overview

This guide explains how to use the integrated KCC2D system with the AbilityEnable component, allowing for dynamic runtime control of character abilities similar to the Sports Arena Brawler example.

## Key Components

### 1. KCCAbilityIntegration (Utility Class)

The `KCCAbilityIntegration` class provides helper methods to bridge the KCC2D system with the AbilityEnable component.

**Main Features:**
- Automatic ability-based settings override
- Runtime ability enable/disable
- Centralized ability checking

**Usage Example:**
```csharp
// Check if an ability is enabled
bool canDash = KCCAbilityIntegration.IsAbilityEnabled(frame, entity, AbilityId.MovementDash);

// Enable/disable an ability at runtime
KCCAbilityIntegration.SetAbilityEnabled(frame, entity, AbilityId.MovementDoubleJump, false);

// Get modified KCC settings based on enabled abilities
var settings = KCCAbilityIntegration.GetSettingsWithAbilityOverrides(frame, entity, kccConfig);
```

### 2. KCC2DConfig Extensibility Hooks

The KCC2DConfig now includes virtual methods for easy extension, following the Sports Arena Brawler pattern:

**Available Hooks:**
- `OnPreComputeState()` - Add custom state logic before default processing
- `OnPostComputeState()` - Add custom state logic after default processing
- `OnFilterInput()` - Filter or modify input before processing
- `OnIntegrateForces()` - Add custom forces or modify velocity
- `OnProcessJump()` - Add custom jump types or modify jump behavior

## How It Works

### Automatic AbilityEnable Integration

When you call `KCC2DConfig.Move()` without custom settings, it automatically:
1. Checks if the entity has an `AbilityEnable` component
2. If found, applies ability-based overrides to KCC2D settings
3. Processes movement with the modified settings

**Example:**
```csharp
// In MovementInputSystem
var config = frame.FindAsset(filter.KCC->Config);
config.Move(frame, filter.Entity, filter.Transform, filter.KCC);
// Automatically applies AbilityEnable overrides!
```

### Ability Mapping

The following abilities automatically control KCC2D features:

| AbilityId | KCC2D Setting |
|-----------|---------------|
| MovementDoubleJump | DoubleJumpEnabled |
| MovementDash | DashDuration (0 = disabled) |
| MovementWallJump | WallJumpEnabled |

## Creating Custom KCC States (Sports Arena Brawler Style)

To extend the KCC2D system with custom states similar to Sports Arena Brawler:

### Step 1: Create a Custom KCC2DConfig Class

```csharp
using Quantum;
using Photon.Deterministic;

public class ExtendedKCC2DConfig : KCC2DConfig
{
    [Header("Extended States")]
    public FP GlideGravityMultiplier = FP._0_25;
    public FP AirDashSpeed = 15;
    public FP AirDashDuration = FP._0_20;

    protected override bool OnPreComputeState()
    {
        // Check for custom states
        if (TryProcessGlideState())
        {
            return true; // Skip default processing
        }

        if (TryProcessAirDashState())
        {
            return true; // Skip default processing
        }

        return false; // Continue with default processing
    }

    private bool TryProcessGlideState()
    {
        // Check if glide ability is enabled
        if (!KCCAbilityIntegration.IsAbilityEnabled(_context.Frame, _context.Entity, AbilityId.MovementGlide))
        {
            return false;
        }

        // Check if gliding (custom input or condition)
        if (_context.KCC->State != KCCState.GROUNDED && 
            _context.KCC->Input.Jump.IsDown && 
            _context.KCC->KinematicVerticalSpeed < 0)
        {
            // Apply glide physics
            _context.KCC->KinematicVerticalSpeed *= GlideGravityMultiplier;
            // Could set a custom state here if you extend KCCState enum
            return false; // Allow default state processing to continue
        }

        return false;
    }

    private bool TryProcessAirDashState()
    {
        // Check if air dash ability is enabled
        if (!KCCAbilityIntegration.IsAbilityEnabled(_context.Frame, _context.Entity, AbilityId.MovementAirDash))
        {
            return false;
        }

        // Check for air dash input
        if (_context.KCC->Input.Dash.WasPressed && 
            _context.KCC->State != KCCState.GROUNDED &&
            _context.KCC->State != KCCState.DASHING)
        {
            // Execute air dash
            _context.KCC->SetState(_context.Frame, KCCState.DASHING, AirDashDuration);
            _context.KCC->KinematicHorizontalSpeed = _context.KCC->LastInputDirection * AirDashSpeed;
            return false; // Let default dash processing handle the rest
        }

        return false;
    }

    protected override void OnIntegrateForces()
    {
        // Add custom force processing
        // Example: Wind zones, conveyor belts, etc.
    }
}
```

### Step 2: Use the Custom Config

1. Create a new `ExtendedKCC2DConfig` asset in Unity
2. Configure the extended properties
3. Reference it in your KCC2D component

## Runtime Ability Control Examples

### Example 1: Power-Up System

```csharp
public unsafe class PowerUpSystem : SystemMainThreadFilter<PowerUpSystem.Filter>
{
    public struct Filter
    {
        public EntityRef Entity;
        public AbilityEnable* AbilityEnable;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
        // When player picks up a power-up, enable double jump
        if (SomeCondition())
        {
            KCCAbilityIntegration.SetAbilityEnabled(
                filter.AbilityEnable, 
                AbilityId.MovementDoubleJump, 
                true
            );
        }
    }
}
```

### Example 2: Debuff System

```csharp
public unsafe class DebuffSystem : SystemMainThreadFilter<DebuffSystem.Filter>
{
    public struct Filter
    {
        public EntityRef Entity;
        public AbilityEnable* AbilityEnable;
        public CharacterStatus* Status;
    }

    public override void Update(Frame frame, ref Filter filter)
    {
        // When stunned, disable all abilities
        if (filter.Status->IsStunned)
        {
            KCCAbilityIntegration.DisableAllAbilities(filter.AbilityEnable);
        }
        else
        {
            KCCAbilityIntegration.EnableAllAbilities(filter.AbilityEnable);
        }
    }
}
```

### Example 3: Level-Based Unlocks

```csharp
public unsafe class AbilityUnlockSystem : SystemSignalsOnly, ISignalOnLevelUp
{
    public void OnLevelUp(Frame frame, EntityRef entity, int newLevel)
    {
        if (!frame.Unsafe.TryGetPointer<AbilityEnable>(entity, out var abilityEnable))
        {
            return;
        }

        // Unlock abilities based on level
        switch (newLevel)
        {
            case 2:
                KCCAbilityIntegration.SetAbilityEnabled(
                    abilityEnable, 
                    AbilityId.MovementDash, 
                    true
                );
                break;
            case 5:
                KCCAbilityIntegration.SetAbilityEnabled(
                    abilityEnable, 
                    AbilityId.MovementDoubleJump, 
                    true
                );
                break;
            case 10:
                KCCAbilityIntegration.SetAbilityEnabled(
                    abilityEnable, 
                    AbilityId.SpecialUltimate, 
                    true
                );
                break;
        }
    }
}
```

## Input Filtering

The KCC2D system now automatically filters input based on enabled abilities. For example:

- If `MovementDashEnabled = false`, dash input is ignored
- This happens in the `OnFilterInput()` hook

You can extend this for custom abilities:

```csharp
protected override SimpleInput2D OnFilterInput(SimpleInput2D input)
{
    // Call base implementation first
    input = base.OnFilterInput(input);

    // Add custom filtering
    if (!KCCAbilityIntegration.IsAbilityEnabled(_context.Frame, _context.Entity, AbilityId.CustomAbility))
    {
        // Filter custom input
        input.CustomButton = default;
    }

    return input;
}
```

## Best Practices

1. **Initialization**: Set up AbilityEnable component on character spawn
   ```csharp
   var abilityEnable = frame.Unsafe.GetPointer<AbilityEnable>(entity);
   KCCAbilityIntegration.EnableAllAbilities(abilityEnable);
   // Or selectively enable based on character config
   ```

2. **Performance**: The ability checks use simple boolean comparisons - very fast!

3. **Modularity**: Keep ability logic in separate systems (like Sports Arena Brawler)
   - Movement abilities → MovementInputSystem
   - Attack abilities → AbilityInputSystem
   - Special abilities → SpecialAbilitySystem

4. **Extensibility**: Prefer virtual hooks over modifying base classes
   - Easier to maintain
   - Cleaner separation of concerns
   - Similar to Sports Arena Brawler's architecture

## Signal-Based Extensions (Advanced)

For even better modularity (like Sports Arena Brawler), you can use signals:

```csharp
// In Ability.qtn, add:
signal OnAbilityStateChanged(EntityRef entity, AbilityId abilityId, bool enabled);

// In your custom config:
protected override void OnPostComputeState()
{
    // Detect state changes and fire signals
    if (StateChangedToGliding())
    {
        _context.Frame.Signals.OnAbilityStateChanged(
            _context.Entity, 
            AbilityId.MovementGlide, 
            true
        );
    }
}
```

## Summary

The integrated KCC2D + AbilityEnable system provides:
- ✅ Runtime ability control
- ✅ Easy extension through virtual hooks
- ✅ Automatic settings override
- ✅ Clean, modular architecture
- ✅ Similar to Sports Arena Brawler's flexibility

This makes it easy to create diverse characters with different ability sets, implement progression systems, and add custom movement states - all while maintaining clean, maintainable code.
