# CharacterAnimationManager 扩展指南 / Extension Guide

## 概述 / Overview

`CharacterAnimationManager` 是一个抽象基类，为所有角色的动画管理提供统一的、可扩展的框架。通过继承此类，您可以快速为不同角色创建动画管理器，避免重复代码。

`CharacterAnimationManager` is an abstract base class that provides a unified, extensible framework for managing animations for all characters. By inheriting from this class, you can quickly create animation managers for different characters while avoiding code duplication.

## 为什么需要基类？ / Why Use a Base Class?

### 问题 / Problem

在原始实现中，每个角色都需要：
- 初始化 `AnimationStateManager`
- 手动实现 `PlayAnimation()`, `IsPlaying()` 等通用方法
- 大量重复代码

In the original implementation, each character needed to:
- Initialize `AnimationStateManager`
- Manually implement `PlayAnimation()`, `IsPlaying()`, and other common methods
- Lots of duplicate code

### 解决方案 / Solution

`CharacterAnimationManager` 基类提供：
- 统一的初始化流程
- 所有通用方法的默认实现
- 清晰的扩展点（抽象方法）
- 一致的API接口

`CharacterAnimationManager` base class provides:
- Unified initialization process
- Default implementation of all common methods
- Clear extension points (abstract methods)
- Consistent API interface

## 架构设计 / Architecture Design

```
CharacterAnimationManager (抽象基类 / Abstract Base Class)
    ├── 通用功能 / Common Features
    │   ├── PlayAnimation()
    │   ├── IsPlaying()
    │   ├── PlayDefaultState()
    │   └── ... 其他通用方法 / Other common methods
    │
    └── 扩展点 / Extension Points
        ├── RegisterAnimationStates() (抽象方法 / Abstract)
        └── OnAnimationManagerInitialized() (虚方法 / Virtual)

WarriorAnimationManager (Warrior实现 / Warrior Implementation)
    └── 实现特定于Warrior的动画注册
        Implements Warrior-specific animation registration

EnemyAnimationManager (敌人实现 / Enemy Implementation)
    └── 实现特定于Enemy的动画注册
        Implements Enemy-specific animation registration

YourCharacterAnimationManager (你的角色 / Your Character)
    └── 实现特定于你的角色的动画注册
        Implements your character-specific animation registration
```

## 创建新角色动画管理器 / Creating a New Character Animation Manager

### 步骤 1: 创建类并继承基类 / Step 1: Create Class and Inherit

```csharp
using Quantum.QuantumView;
using UnityEngine;

public class KnightAnimationManager : CharacterAnimationManager
{
    // 你的代码将在这里 / Your code goes here
}
```

### 步骤 2: 定义动画状态常量 / Step 2: Define Animation State Constants

```csharp
public class KnightAnimationManager : CharacterAnimationManager
{
    // 定义角色特有的动画状态
    // Define character-specific animation states
    public static class AnimationStates
    {
        // 基础动画 / Basic animations
        public const string Idle = "Idle";
        public const string Walk = "Walk";
        public const string Run = "Run";
        
        // 战斗动画 / Combat animations
        public const string SwordAttack = "SwordAttack";
        public const string ShieldBlock = "ShieldBlock";
        
        // 特殊动画 / Special animations
        public const string Roll = "Roll";
        public const string Parry = "Parry";
    }
    
    // ... 继续下一步
}
```

### 步骤 3: 实现 RegisterAnimationStates() / Step 3: Implement RegisterAnimationStates()

```csharp
protected override void RegisterAnimationStates()
{
    // 注册所有动画状态
    // Register all animation states
    
    // 基础动画 / Basic animations
    RegisterState(AnimationStates.Idle, isDefault: true);
    RegisterState(AnimationStates.Walk, crossfadeDuration: 0.15f);
    RegisterState(AnimationStates.Run, crossfadeDuration: 0.1f);
    
    // 战斗动画 / Combat animations
    RegisterState(AnimationStates.SwordAttack, crossfadeDuration: 0.05f);
    RegisterState(AnimationStates.ShieldBlock, crossfadeDuration: 0.1f);
    
    // 特殊动画 / Special animations
    RegisterState(AnimationStates.Roll, crossfadeDuration: 0.05f);
    RegisterState(AnimationStates.Parry, crossfadeDuration: 0.05f);
}
```

### 步骤 4 (可选): 添加便捷方法 / Step 4 (Optional): Add Convenience Methods

```csharp
// 为常用动画添加便捷方法
// Add convenience methods for common animations
public void PlayIdle() => PlayAnimation(AnimationStates.Idle);
public void PlayWalk() => PlayAnimation(AnimationStates.Walk);
public void PlayRun() => PlayAnimation(AnimationStates.Run);
public void PlaySwordAttack() => PlayAnimation(AnimationStates.SwordAttack);
public void PlayShieldBlock() => PlayAnimation(AnimationStates.ShieldBlock);
public void PlayRoll() => PlayAnimation(AnimationStates.Roll);
public void PlayParry() => PlayAnimation(AnimationStates.Parry);
```

### 步骤 5 (可选): 重写初始化完成回调 / Step 5 (Optional): Override Initialization Callback

```csharp
protected override void OnAnimationManagerInitialized()
{
    // 在动画管理器初始化完成后执行额外逻辑
    // Execute additional logic after animation manager is initialized
    Debug.Log("Knight Animation Manager initialized");
    
    // 可以在这里执行：
    // You can do things like:
    // - 设置初始动画速度
    // - Set initial animation speed
    // - 注册事件监听器
    // - Register event listeners
    // - 其他初始化逻辑
    // - Other initialization logic
}
```

## 完整示例 / Complete Example

以下是一个完整的角色动画管理器示例：

Here's a complete example of a character animation manager:

```csharp
using Quantum.QuantumView;
using UnityEngine;

public class ArcherAnimationManager : CharacterAnimationManager
{
    public static class AnimationStates
    {
        // Movement
        public const string Idle = "Idle";
        public const string Walk = "Walk";
        public const string Run = "Run";
        public const string Jump = "Jump";
        
        // Combat
        public const string DrawBow = "DrawBow";
        public const string AimBow = "AimBow";
        public const string ShootArrow = "ShootArrow";
        public const string MeleeAttack = "MeleeAttack";
        
        // Defense
        public const string Dodge = "Dodge";
        public const string Hurt = "Hurt";
        public const string Death = "Death";
    }
    
    protected override void RegisterAnimationStates()
    {
        // Movement animations
        RegisterState(AnimationStates.Idle, isDefault: true);
        RegisterState(AnimationStates.Walk, crossfadeDuration: 0.2f);
        RegisterState(AnimationStates.Run, crossfadeDuration: 0.15f);
        RegisterState(AnimationStates.Jump, crossfadeDuration: 0.1f);
        
        // Combat animations
        RegisterState(AnimationStates.DrawBow, crossfadeDuration: 0.1f);
        RegisterState(AnimationStates.AimBow, crossfadeDuration: 0.15f);
        RegisterState(AnimationStates.ShootArrow, crossfadeDuration: 0.05f);
        RegisterState(AnimationStates.MeleeAttack, crossfadeDuration: 0.05f);
        
        // Defense animations
        RegisterState(AnimationStates.Dodge, crossfadeDuration: 0.05f);
        RegisterState(AnimationStates.Hurt, crossfadeDuration: 0.05f);
        RegisterState(AnimationStates.Death, crossfadeDuration: 0.1f);
    }
    
    protected override void OnAnimationManagerInitialized()
    {
        Debug.Log("Archer Animation Manager initialized");
    }
    
    // Convenience methods
    public void PlayIdle() => PlayAnimation(AnimationStates.Idle);
    public void PlayWalk() => PlayAnimation(AnimationStates.Walk);
    public void PlayRun() => PlayAnimation(AnimationStates.Run);
    public void PlayJump() => PlayAnimation(AnimationStates.Jump);
    public void PlayDrawBow() => PlayAnimation(AnimationStates.DrawBow);
    public void PlayAimBow() => PlayAnimation(AnimationStates.AimBow);
    public void PlayShootArrow() => PlayAnimation(AnimationStates.ShootArrow);
    public void PlayMeleeAttack() => PlayAnimation(AnimationStates.MeleeAttack);
    public void PlayDodge() => PlayAnimation(AnimationStates.Dodge);
    public void PlayHurt() => PlayAnimation(AnimationStates.Hurt);
    public void PlayDeath() => PlayAnimation(AnimationStates.Death);
}
```

## 使用方式 / Usage

### 在Unity中使用 / Using in Unity

1. 将组件添加到角色GameObject
   Add the component to your character GameObject

2. 组件会自动初始化
   The component will initialize automatically

3. 在其他脚本中调用
   Call from other scripts:

```csharp
public class PlayerController : MonoBehaviour
{
    private ArcherAnimationManager animManager;
    
    void Start()
    {
        animManager = GetComponent<ArcherAnimationManager>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animManager.PlayShootArrow();
        }
        
        if (isRunning)
            animManager.PlayRun();
        else
            animManager.PlayIdle();
    }
}
```

## 基类提供的方法 / Methods Provided by Base Class

所有继承自 `CharacterAnimationManager` 的类自动获得以下方法：

All classes inheriting from `CharacterAnimationManager` automatically get these methods:

### 动画播放 / Animation Playback

```csharp
// 播放动画（带过渡）
PlayAnimation(string stateName, bool forceReplay = false)

// 立即播放（无过渡）
PlayAnimationImmediate(string stateName)

// 播放默认状态
PlayDefaultState()
```

### 状态查询 / State Queries

```csharp
// 检查是否在播放指定动画
bool IsPlaying(string stateName)

// 检查当前动画是否完成
bool IsCurrentAnimationFinished()

// 获取当前动画归一化时间
float GetAnimationNormalizedTime(int layer = 0)
```

### 速度控制 / Speed Control

```csharp
// 设置动画速度
SetAnimationSpeed(float speed)
```

### 访问器 / Accessors

```csharp
// 获取状态管理器
AnimationStateManager StateManager { get; }

// 获取当前动画名称
string CurrentAnimation { get; }

// 获取Animator组件
Animator Animator { get; }
```

## 辅助方法 / Helper Methods

基类还提供了一些保护级别的辅助方法供子类使用：

The base class also provides some protected helper methods for subclasses:

```csharp
// 注册单个状态
protected void RegisterState(string stateName, int layer = 0, 
    float crossfadeDuration = 0.1f, bool isDefault = false)

// 批量注册状态（使用默认参数）
protected void RegisterStates(params string[] stateNames)
```

## 最佳实践 / Best Practices

1. **使用常量类** - 始终为动画状态定义常量类
   **Use Constants Class** - Always define a constants class for animation states

2. **分组注册** - 按功能分组注册动画（移动、战斗、特殊等）
   **Group Registration** - Register animations by functionality (movement, combat, special, etc.)

3. **合理设置过渡时间** - 根据动画类型设置合适的crossfadeDuration
   **Set Appropriate Transition Times** - Set crossfadeDuration based on animation type

4. **添加便捷方法** - 为常用动画添加便捷方法提高代码可读性
   **Add Convenience Methods** - Add convenience methods for frequently used animations

5. **保持一致性** - 不同角色使用相似的命名约定
   **Maintain Consistency** - Use similar naming conventions across different characters

## 与 AnimationStateController 的对比 / Comparison with AnimationStateController

| 特性 / Feature | CharacterAnimationManager | AnimationStateController |
|----------------|---------------------------|--------------------------|
| 用途 / Purpose | 特定角色的动画管理 / Character-specific | 通用动画管理 / Generic |
| 扩展方式 / Extension | 继承 / Inheritance | 配置 / Configuration |
| 灵活性 / Flexibility | 高（代码定制）/ High (code) | 中（配置文件）/ Medium (config) |
| 类型安全 / Type Safety | ✓ 常量定义 / Constants | ✗ 字符串 / Strings |
| 适用场景 / Use Case | 需要便捷方法和定制逻辑 / Need convenience methods | 快速原型和简单场景 / Quick prototyping |

## 总结 / Summary

`CharacterAnimationManager` 基类通过提供可扩展的框架，大大简化了为不同角色创建动画管理器的过程。它减少了重复代码，提供了一致的API，并允许您专注于角色特定的动画逻辑。

The `CharacterAnimationManager` base class greatly simplifies the process of creating animation managers for different characters by providing an extensible framework. It reduces code duplication, provides a consistent API, and allows you to focus on character-specific animation logic.

**关键优势 / Key Advantages:**
- 减少80%的样板代码 / Reduces 80% of boilerplate code
- 统一的接口 / Unified interface
- 易于维护 / Easy to maintain
- 快速创建新角色 / Quick creation of new characters
- 类型安全 / Type safe

## 示例文件 / Example Files

参考以下示例了解更多：
Refer to these examples for more details:

- `WarriorAnimationManager.cs` - 24个动画状态的完整示例 / Complete example with 24 states
- `EnemyAnimationManager.cs` - 简单敌人的示例 / Simple enemy example
