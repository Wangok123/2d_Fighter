# Architecture Overview (架构总览)

## System Architecture Diagram (系统架构图)

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Game Entity (角色实体)                        │
│                                                                       │
│  ┌────────────────────┐      ┌─────────────────────┐                │
│  │  CharacterStatus   │      │  CharacterLevel     │                │
│  │  MovementData      │      │  AttackData         │                │
│  │  PlayerLink        │      │  CommandInputData   │                │
│  └────────────────────┘      └─────────────────────┘                │
│                                       │                              │
│                                       │ references                   │
│                                       ↓                              │
│                         ┌─────────────────────────┐                 │
│                         │ ModularCharacterConfig  │                 │
│                         │   (Character Builder)   │                 │
│                         └─────────────────────────┘                 │
│                                       │                              │
│                    ┌──────────────────┼──────────────────┐          │
│                    │                  │                  │          │
│                    ↓                  ↓                  ↓          │
│           ┌────────────────┐ ┌────────────────┐ ┌────────────────┐ │
│           │Movement Ability│ │Attack Ability  │ │Defense Ability │ │
│           │   Components   │ │   Components   │ │   Components   │ │
│           └────────────────┘ └────────────────┘ └────────────────┘ │
│                    │                  │                  │          │
│                    │                  ↓                  │          │
│                    │         ┌────────────────┐         │          │
│                    │         │Special Ability │         │          │
│                    │         │   Components   │         │          │
│                    │         └────────────────┘         │          │
│                    └──────────────────┬──────────────────┘          │
│                                       │                              │
│                                       │ processed by                 │
│                                       ↓                              │
│                         ┌─────────────────────────┐                 │
│                         │  ModularAbilitySystem   │                 │
│                         │  (Quantum System)       │                 │
│                         └─────────────────────────┘                 │
└─────────────────────────────────────────────────────────────────────┘
```

## Component Composition (组件组合)

### Traditional Approach (传统方法)

```
Character 1 (Warrior)
├── WalkCode.cs         [NEW CODE]
├── JumpCode.cs         [NEW CODE]
├── LightAttackCode.cs  [NEW CODE]
├── HeavyAttackCode.cs  [NEW CODE]
└── BlockCode.cs        [NEW CODE]

Character 2 (Ninja)
├── WalkCode.cs         [COPY-PASTE]
├── JumpCode.cs         [COPY-PASTE]
├── DashCode.cs         [NEW CODE]
├── LightKickCode.cs    [NEW CODE]
└── ParryCode.cs        [NEW CODE]

Total: 10 implementations (50% duplicated code)
```

### Modular Approach (模块化方法)

```
Component Library (组件库):
├── Walk_Movement.asset
├── Jump_Movement.asset
├── Dash_Movement.asset
├── LightAttack_Attack.asset
├── HeavyAttack_Attack.asset
├── LightKick_Attack.asset
├── Block_Defense.asset
└── Parry_Defense.asset

Character 1 (Warrior) - Composition:
├── Walk_Movement        [REFERENCE]
├── Jump_Movement        [REFERENCE]
├── LightAttack_Attack   [REFERENCE]
├── HeavyAttack_Attack   [REFERENCE]
└── Block_Defense        [REFERENCE]

Character 2 (Ninja) - Composition:
├── Walk_Movement        [REUSE!]
├── Jump_Movement        [REUSE!]
├── Dash_Movement        [REFERENCE]
├── LightKick_Attack     [REFERENCE]
└── Parry_Defense        [REFERENCE]

Total: 8 components, 2 reused (25% immediate reuse, grows over time)
```

## Data Flow (数据流)

```
┌──────────────┐
│  User Input  │
│  (Keyboard)  │
└──────┬───────┘
       │
       ↓
┌──────────────────────┐
│  Input System        │
│  (SimpleInput2D)     │
└──────┬───────────────┘
       │
       ↓
┌──────────────────────┐
│ ModularAbilitySystem │ ←─── Reads: ModularCharacterConfig
│                      │ ←─── Reads: Ability Components
│  • Check Priority    │ ←─── Reads: CharacterLevel
│  • Check Unlocks     │ ←─── Reads: AttackData
│  • Match Input       │
│  • Execute Ability   │
└──────┬───────────────┘
       │
       ↓
┌──────────────────────┐
│  Game Logic          │
│  • Apply Damage      │
│  • Update Cooldowns  │
│  • Fire Events       │
└──────┬───────────────┘
       │
       ↓
┌──────────────────────┐
│  Visual/Audio        │
│  Feedback            │
└──────────────────────┘
```

## Priority Processing (优先级处理)

```
Frame N:
│
├─ Collect All Available Abilities
│  ├─ Special Abilities (Priority: 100-199)
│  ├─ Defense Abilities (Priority: 30-99)
│  ├─ Attack Abilities (Priority: 10-99)
│  └─ Movement Abilities (Priority: 0-9)
│
├─ Filter by Input Match
│  └─ Only abilities with matching input
│
├─ Filter by Unlock Status
│  └─ Only abilities unlocked at current level
│
├─ Filter by Cooldown
│  └─ Only abilities not on cooldown
│
├─ Sort by Priority (Highest First)
│  └─ [Special(150), Heavy(50), Light(10)]
│
└─ Execute First Match
   └─ Special(150) executes → Stop processing
```

## Ability Lifecycle (能力生命周期)

```
┌─────────────────────┐
│  Create Ability     │  Designer creates MovementAbilityComponent
│  Component Asset    │  in Unity Editor
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Add to Character   │  Reference ability in ModularCharacterConfig
│  Config             │
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Link to Entity     │  Entity prototype references character config
│  Prototype          │
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Entity Spawned     │  Character spawns in game
│  in Game            │
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  System Loads       │  ModularAbilitySystem reads config
│  Config             │
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Each Frame:        │
│  • Check Input      │  System processes abilities
│  • Check Unlocks    │  based on input and state
│  • Execute Ability  │
└──────────┬──────────┘
           │
           ↓ (Ability Executed)
           │
┌──────────┴──────────┐
│  Apply Cooldown     │  Set cooldown timer
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Wait for Cooldown  │  Ability unusable during cooldown
└──────────┬──────────┘
           │
           └─────→ (Back to frame processing)
```

## Reusability Flow (复用流程)

### Creating Characters Over Time (随时间创建角色)

```
Time →

Character 1 (Month 1):
Create: [Walk, Jump, LightAttack, HeavyAttack, Block, Fireball, Uppercut]
Reuse:  []
Total Components in Library: 7

Character 2 (Month 2):
Create: [Dash, LightKick, Parry]
Reuse:  [Walk, Jump]
Total Components in Library: 10 (+3)

Character 3 (Month 3):
Create: [MagicBolt, Barrier, Meteor]
Reuse:  [Walk, Fireball]
Total Components in Library: 13 (+3)

Character 4 (Month 4):
Create: []
Reuse:  [Walk, HeavyAttack, Block, Barrier, Fireball]
Total Components in Library: 13 (+0) ← Pure composition!

Character 5 (Month 5):
Create: [AirDash, RapidFire]
Reuse:  [Walk, Jump, Dash, LightAttack, Parry]
Total Components in Library: 15 (+2)
```

### Reuse Rate Over Time (随时间的复用率)

```
100% │                                    ┌──────
     │                               ┌────┘
 80% │                          ┌────┘
     │                     ┌────┘
 60% │                ┌────┘
     │           ┌────┘
 40% │      ┌────┘
     │ ┌────┘
 20% ├─┘
     │
  0% └─────┬─────┬─────┬─────┬─────┬─────┬─────
          C1    C2    C3    C4    C5    C6    C7
          
     Reuse Rate = (Reused Components / Total Components) * 100
```

## System Comparison (系统对比)

### Legacy System (旧系统)

```
Pros:
✅ Simple and straightforward
✅ All code in one place
✅ Easy to understand for single character

Cons:
❌ Code duplication across characters
❌ Hard to maintain consistency
❌ Each character needs full implementation
❌ Changes require editing multiple files
```

### Modular System (模块化系统)

```
Pros:
✅ High component reuse
✅ Easy to create new characters
✅ Consistent behavior across characters
✅ Changes propagate to all users
✅ Designer-friendly (no code needed)
✅ Scales well with character count

Cons:
⚠️ Initial setup requires more components
⚠️ Requires understanding component composition
⚠️ More files to manage in project
```

## Integration Points (集成点)

```
┌────────────────────────────────────────────────────────────┐
│                    Quantum Framework                        │
├────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────────┐        ┌──────────────────┐         │
│  │ Character.qtn    │        │ NormalAttack     │         │
│  │                  │        │ System           │         │
│  │ • AttackData     │◄───────│ (Legacy)         │         │
│  │   - LegacyConfig │        │                  │         │
│  │   - ModularConfig│◄───┐   └──────────────────┘         │
│  └──────────────────┘    │                                 │
│                          │   ┌──────────────────┐         │
│  ┌──────────────────┐    └───│ ModularAbility   │         │
│  │ Ability          │◄───────│ System           │         │
│  │ Components       │        │ (New)            │         │
│  └──────────────────┘        └──────────────────┘         │
│                                                             │
└────────────────────────────────────────────────────────────┘
```

## File Structure (文件结构)

```
Client/Assets/QuantumUser/
│
├── Simulation/
│   ├── Core/
│   │   ├── Assets/
│   │   │   ├── Abilities/                    [NEW]
│   │   │   │   ├── AbilityComponentBase.cs
│   │   │   │   ├── MovementAbilityComponent.cs
│   │   │   │   ├── AttackAbilityComponent.cs
│   │   │   │   ├── DefenseAbilityComponent.cs
│   │   │   │   └── SpecialAbilityComponent.cs
│   │   │   ├── ModularCharacterConfig.cs     [NEW]
│   │   │   ├── LegacyConfigConverter.cs      [NEW]
│   │   │   ├── AttackConfig.cs               [EXISTING]
│   │   │   ├── LightAttackConfig.cs          [EXISTING]
│   │   │   ├── HeavyAttackConfig.cs          [EXISTING]
│   │   │   └── CharacterAttackConfig.cs      [EXISTING]
│   │   │
│   │   ├── Systems/
│   │   │   ├── ModularAbilitySystem.cs       [NEW]
│   │   │   ├── NormalAttackSystem.cs         [EXISTING]
│   │   │   ├── MovementSystem.cs             [EXISTING]
│   │   │   └── CommandInputSystem.cs         [EXISTING]
│   │   │
│   │   └── DSL/
│   │       ├── Character.qtn                  [MODIFY TO ADD ModularConfig]
│   │       └── Ability.qtn                    [EXISTING]
│   │
│   └── Generated/                             [AUTO-GENERATED]
│
└── Resources/
    ├── Abilities/                             [NEW - DESIGNER CREATES]
    │   ├── Movement/
    │   ├── Attack/
    │   ├── Defense/
    │   └── Special/
    │
    └── Characters/                            [NEW - DESIGNER CREATES]
        ├── Warrior_ModularConfig.asset
        ├── Ninja_ModularConfig.asset
        └── Mage_ModularConfig.asset
```

## Summary (总结)

The modular ability component system provides:

1. **Component Reuse** - Create once, use many times
2. **Rapid Iteration** - New characters by composition
3. **Maintainability** - Changes affect all users
4. **Scalability** - Library grows, effort decreases
5. **Designer-Friendly** - No coding required for new characters
6. **Backward Compatible** - Works with existing system

This achieves the **Overwatch workflow** described in their GDC presentation! 🎮
