# Animation State Management System

## Quick Start / 快速开始

This system solves the problem of managing 24+ animation states in the Warrior Animator by using code instead of manual transitions in Unity.

此系统通过使用代码而不是Unity中的手动转换来解决管理Warrior Animator中24个以上动画状态的问题。

## Files Created / 创建的文件

### Core System / 核心系统
- `Client/Assets/Scripts/Core/AnimationSystem/AnimationStateManager.cs` - Main manager class
- `Client/Assets/Scripts/Core/AnimationSystem/AnimationStateConfig.cs` - ScriptableObject configuration
- `Client/Assets/Scripts/Core/AnimationSystem/AnimationStateController.cs` - MonoBehaviour component

### Warrior Implementation / Warrior实现
- `Client/Assets/Scripts/Quantum/QuantumView/WarriorAnimationManager.cs` - Warrior-specific implementation

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

## Key Features / 主要特性

1. **No Manual Transitions** - No need to connect states in Unity Animator
   **无需手动转换** - 无需在Unity Animator中连接状态

2. **Easy to Extend** - Add new animations by calling RegisterState()
   **易于扩展** - 通过调用RegisterState()添加新动画

3. **Type Safety** - Use constants to avoid typos
   **类型安全** - 使用常量避免拼写错误

4. **High Performance** - Hash-based lookups instead of string comparisons (~5x faster)
   **高性能** - 基于哈希查找而非字符串比较（约5倍性能提升）

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
