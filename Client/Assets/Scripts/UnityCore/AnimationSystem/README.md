# Animation State Management System

## Quick Start / 快速开始

This system solves the problem of managing 24+ animation states in the Warrior Animator by using code instead of manual transitions in Unity.

此系统通过使用代码而不是Unity中的手动转换来解决管理Warrior Animator中24个以上动画状态的问题。

## Files Created / 创建的文件

### Core System / 核心系统
- `Client/Assets/Scripts/Core/AnimationSystem/AnimationStateManager.cs` - Main manager class
- `Client/Assets/Scripts/Core/AnimationSystem/AnimationStateConfig.cs` - ScriptableObject configuration
- `Client/Assets/Scripts/Core/AnimationSystem/AnimationStateController.cs` - MonoBehaviour component

### Helper Classes / 辅助类 ⭐ NEW
- `Client/Assets/Scripts/Core/AnimationSystem/AnimatorParameterHelper.cs` - Parameter management (SetBool, SetTrigger, etc.)
- `Client/Assets/Scripts/Core/AnimationSystem/AnimatorLayerHelper.cs` - Layer weight management
- `Client/Assets/Scripts/Core/AnimationSystem/AnimatorExtendedHelper.cs` - IK, state queries, advanced features

### Extensible Base Class / 可扩展基类
- `Client/Assets/Scripts/Quantum/QuantumView/CharacterAnimationManager.cs` - Extensible base class for character animations

### Character Implementations / 角色实现
- `Client/Assets/Scripts/Quantum/QuantumView/WarriorAnimationManager.cs` - Warrior-specific implementation
- `Client/Assets/Scripts/Quantum/QuantumView/EnemyAnimationManager.cs` - Enemy example implementation

### Examples / 示例
- `Client/Assets/Scripts/Examples/AnimationSystemExample.cs` - Usage example

### Documentation / 文档
- `Md/Core/动画状态管理系统.md` - Full documentation (Chinese & English)

## Usage for Warrior / Warrior角色使用方法

### Option 1: Use WarriorAnimationManager (Recommended)

```csharp
// Add WarriorAnimationManager component to your Warrior GameObject
// 将WarriorAnimationManager组件添加到Warrior GameObject

using Quantum.QuantumView;

public class PlayerController : MonoBehaviour
{
    private WarriorAnimationManager _animManager;

    void Start()
    {
        _animManager = GetComponent<WarriorAnimationManager>();
    }

    void Update()
    {
        // Simple method calls
        if (isRunning)
            _animManager.PlayRun();
        else
            _animManager.PlayIdle();
        
        if (Input.GetKeyDown(KeyCode.Space))
            _animManager.PlayJump();
    }
}
```

### Option 2: Use AnimationStateManager directly

```csharp
using Core.AnimationSystem;

public class CustomController : MonoBehaviour
{
    private AnimationStateManager _stateManager;

    void Start()
    {
        var animator = GetComponent<Animator>();
        _stateManager = new AnimationStateManager(animator);
        
        // Register states
        _stateManager.RegisterState("Idle", isDefault: true);
        _stateManager.RegisterState("Run");
        _stateManager.RegisterState("Jump");
        
        _stateManager.PlayDefaultState();
    }
}
```

## Option 3: Create Your Own Character Animation Manager (Extensible) ⭐ NEW / 创建自定义角色动画管理器（可扩展）

The system now includes `CharacterAnimationManager` base class for easy extensibility:
系统现在包含`CharacterAnimationManager`基类，便于扩展：

```csharp
using Quantum.QuantumView;
using UnityEngine;

// 1. Inherit from CharacterAnimationManager
// 1. 继承CharacterAnimationManager
public class MyCharacterAnimationManager : CharacterAnimationManager
{
    // 2. Define animation state constants
    // 2. 定义动画状态常量
    public static class AnimationStates
    {
        public const string Idle = "Idle";
        public const string Walk = "Walk";
        public const string Attack = "Attack";
        // ... add more states
    }
    
    // 3. Implement RegisterAnimationStates method
    // 3. 实现RegisterAnimationStates方法
    protected override void RegisterAnimationStates()
    {
        RegisterState(AnimationStates.Idle, isDefault: true);
        RegisterState(AnimationStates.Walk, crossfadeDuration: 0.15f);
        RegisterState(AnimationStates.Attack, crossfadeDuration: 0.05f);
        // ... register more states
    }
    
    // 4. Optional: Add convenience methods
    // 4. 可选：添加便捷方法
    public void PlayIdle() => PlayAnimation(AnimationStates.Idle);
    public void PlayWalk() => PlayAnimation(AnimationStates.Walk);
    public void PlayAttack() => PlayAnimation(AnimationStates.Attack);
}
```

**Benefits of using CharacterAnimationManager base class:**
- Reusable code for all character types
- Consistent API across different characters
- Less boilerplate code
- Easy to create new character animation managers

**使用CharacterAnimationManager基类的好处：**
- 所有角色类型可重用代码
- 不同角色间一致的API
- 更少的样板代码
- 轻松创建新的角色动画管理器

## Key Features / 主要特性

1. **No Manual Transitions** - No need to connect states in Unity Animator
   **无需手动转换** - 无需在Unity Animator中连接状态

2. **Easy to Extend** - Add new animations by calling RegisterState()
   **易于扩展** - 通过调用RegisterState()添加新动画

3. **Type Safety** - Use constants to avoid typos
   **类型安全** - 使用常量避免拼写错误

4. **High Performance** - Hash-based lookups instead of string comparisons (~5x faster)
   **高性能** - 基于哈希查找而非字符串比较（约5倍性能提升）

5. **Extensible Base Class** - CharacterAnimationManager provides reusable foundation for all characters
   **可扩展基类** - CharacterAnimationManager为所有角色提供可重用基础

6. **Rich Helper Classes** - AnimatorParameterHelper, LayerHelper, ExtendedHelper for advanced features ⭐ NEW
   **丰富的辅助类** - AnimatorParameterHelper、LayerHelper、ExtendedHelper提供高级功能 ⭐ 新增

## Helper Classes Usage / 辅助类使用方法 ⭐ NEW

The system now includes powerful helper classes for advanced Animator features:

### AnimatorParameterHelper - 参数管理

```csharp
// Access via AnimationStateManager
var paramHelper = stateManager.ParameterHelper;

// Set parameters (with hash caching for performance)
paramHelper.SetBool("isGrounded", true);
paramHelper.SetInt("weaponType", 2);
paramHelper.SetFloat("speed", 5.5f);
paramHelper.SetTrigger("attack");

// Get parameter values
bool isGrounded = paramHelper.GetBool("isGrounded");
int weaponType = paramHelper.GetInt("weaponType");
float speed = paramHelper.GetFloat("speed");

// Batch operations
paramHelper.SetBools(new Dictionary<string, bool> {
    { "isGrounded", true },
    { "isRunning", false }
});

// Reset all triggers
paramHelper.ResetAllTriggers();
```

### AnimatorLayerHelper - 层级管理

```csharp
var layerHelper = stateManager.LayerHelper;

// Set layer weight
layerHelper.SetLayerWeight("UpperBody", 1.0f);
layerHelper.SetLayerWeight(1, 0.5f); // By index

// Smooth transitions
layerHelper.FadeInLayer(1, 2.0f, Time.deltaTime);
layerHelper.FadeOutLayer(1, 2.0f, Time.deltaTime);

// Enable/Disable layers
layerHelper.EnableLayer(1);
layerHelper.DisableLayer(2);
```

### AnimatorExtendedHelper - 高级功能

```csharp
var extendedHelper = stateManager.ExtendedHelper;

// State queries
bool isInAttack = extendedHelper.IsInState("Attack");
bool isTransitioning = extendedHelper.IsInTransition();
float progress = extendedHelper.GetNormalizedTime();

// IK control
extendedHelper.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
extendedHelper.SetIKPosition(AvatarIKGoal.RightHand, targetPosition);

// Look At
extendedHelper.SetLookAtWeight(1.0f);
extendedHelper.SetLookAtPosition(enemyPosition);

// Speed control
extendedHelper.Pause();
extendedHelper.Resume();
extendedHelper.SetSpeed(2.0f);
```

## Warrior Animation States / Warrior动画状态

The WarriorAnimationManager includes all 24 animation states:
WarriorAnimationManager包含所有24个动画状态：

- Idle, Run, Jump, Fall
- Attack, Dash Attack, Dash
- Crouch, Slide
- Hurt, Death
- Wall Slide, Edge Grab, Edge Idle
- Ladder
- And variants with/without effects

## Benefits / 优势

### Before (使用前)
- 24 animation states = hundreds of manual transitions
- Difficult to maintain and debug
- Hard to add new animations
- Easy to make mistakes

### After (使用后)
- No manual transitions needed
- Simple code-based animation control
- Easy to add new states: just call RegisterState()
- Type-safe with constants
- Better debugging with logs and breakpoints

## See Also / 另请参阅

For complete documentation, see:
完整文档请参阅：
- `Md/Core/动画状态管理系统.md`
