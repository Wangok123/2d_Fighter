# Quantum动画系统使用指南
# Quantum Animation System Guide

## 概述 / Overview

本系统基于Quantum引擎的ECS架构重新设计，所有动画逻辑运行在Quantum确定性模拟层，Unity层仅负责表现。

This system is redesigned based on Quantum engine's ECS architecture, all animation logic runs in Quantum deterministic simulation layer, Unity layer is only responsible for presentation.

## 架构 / Architecture

```
┌─────────────────────────────────────────────────────┐
│             Quantum Simulation Layer                │
│         (Deterministic, Networked)                  │
├─────────────────────────────────────────────────────┤
│  Components (.qtn files):                           │
│  - AnimationState      - 动画状态                    │
│  - AnimationConfig     - 动画配置引用                │
│  - ThrowState          - 投技状态                    │
│  - HitReaction         - 受击反应                    │
│  - ComboData           - 连招数据                    │
├─────────────────────────────────────────────────────┤
│  Systems (确定性逻辑):                               │
│  - AnimationSystem     - 动画状态管理                │
│  - ThrowSystem         - 投技逻辑                    │
│  - HitReactionSystem   - 受击逻辑                    │
│  - ComboSystem         - 连招逻辑                    │
│  - CombatInputSystem   - 战斗输入处理                │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│              Unity View Layer                       │
│         (Presentation Only)                         │
├─────────────────────────────────────────────────────┤
│  - QuantumAnimationView   - 同步Quantum状态到Animator │
└─────────────────────────────────────────────────────┘
```

## 核心优势 / Core Advantages

### 1. 确定性同步 / Deterministic Sync
- 所有动画逻辑在Quantum模拟层运行
- 保证所有客户端动画状态完全一致
- 支持回滚和预测

### 2. 网络友好 / Network Friendly
- 动画状态自动同步，无需额外网络代码
- 减少网络流量，只同步状态哈希
- 天然支持延迟和丢包

### 3. 易于调试 / Easy to Debug
- 可以重放确定性模拟
- 清晰的状态机逻辑
- 组件化设计便于追踪

## 文件说明 / File Descriptions

### Quantum Simulation Layer

#### 1. AnimationComponents.qtn
定义所有动画相关的组件：
- `AnimationState` - 存储当前动画状态、优先级、过渡信息
- `AnimationConfig` - 引用动画配置资源
- `ThrowState` - 投技状态机数据
- `HitReaction` - 受击反应数据（硬直、击退等）
- `ComboData` - 连招计数和窗口

#### 2. AnimationConfigData.cs
动画配置资源，包含：
- `AnimationStateData` - 单个动画状态定义（名称、优先级、取消策略）
- `CancelWindowData` - 取消窗口定义
- `CanCancel()` - 判断是否可以取消当前动画的逻辑

#### 3. Systems (系统)

**AnimationSystem.cs**
- 管理动画状态切换
- 检查优先级和取消规则
- 提供 `TryPlayAnimation()`, `CanPlayAnimation()`, `ForcePlayAnimation()` 等API

**ThrowSystem.cs**
- 处理投技攻击逻辑
- 检测抓取目标
- 投技破解机制
- 伤害和击退计算

**HitReactionSystem.cs**
- 处理受击反应
- 智能选择地面/空中受击动画
- 硬直和击退效果
- 死亡处理

**ComboSystem.cs**
- 连招计数和窗口管理
- 自动重置连招
- 提供连招查询API

**CombatInputSystem.cs**
- 处理战斗相关输入
- 调用各个System的API
- 输入到游戏逻辑的桥梁

### Unity View Layer

#### QuantumAnimationView.cs
- 从Quantum同步动画状态到Unity Animator
- 使用哈希值播放动画
- 提供调试信息显示

## 使用方法 / Usage

### 1. 创建动画配置资源 (AnimationConfigData)

在Unity中创建AnimationConfigData资源：

```csharp
// 在Unity Editor中
// Create > Quantum > Animation Config Data

// 配置动画状态
States = new AnimationStateData[]
{
    new AnimationStateData
    {
        StateName = "Idle",
        StateHash = Animator.StringToHash("Idle"),
        Priority = 0, // AnimationPriority.Idle
        CancelPolicy = 1, // AlwaysCancellable
        CrossfadeDuration = FP._0 + FP._0_10,
        CancelWindows = null
    },
    new AnimationStateData
    {
        StateName = "Attack",
        StateHash = Animator.StringToHash("Attack"),
        Priority = 30, // AnimationPriority.Attack
        CancelPolicy = 2, // CancellableInWindow
        CrossfadeDuration = FP._0 + FP._0_05,
        CancelWindows = new CancelWindowData[]
        {
            new CancelWindowData
            {
                StartTime = FP._0 + FP._0_40,
                EndTime = FP._0 + FP._0_70,
                AllowedTargetHashes = new int[] 
                { 
                    Animator.StringToHash("Attack"),
                    Animator.StringToHash("Skill") 
                }
            }
        }
    },
    // ... 更多状态
};
```

### 2. 设置角色实体原型 (Entity Prototype)

在Quantum Entity Prototype中添加组件：
- AnimationState
- AnimationConfig (引用上面创建的AnimationConfigData)
- ThrowState
- HitReaction
- ComboData

### 3. 在System中使用API

```csharp
// 播放动画
AnimationSystem.TryPlayAnimation(frame, entity, attackHash);

// 检查是否可以播放
if (AnimationSystem.CanPlayAnimation(frame, entity, attackHash))
{
    // 可以播放
}

// 强制播放（忽略所有规则）
AnimationSystem.ForcePlayAnimation(frame, entity, deathHash);

// 执行投技
ThrowSystem.TryThrow(frame, attackerEntity, grabRange, grabAngle);

// 应用受击
HitReactionSystem.ApplyHit(frame, targetEntity, damage, knockback, hitStunDuration, isGrounded);

// 连招攻击
ComboSystem.TryAttack(frame, entity, attackHash);
```

### 4. Unity View层同步

在角色GameObject上添加 `QuantumAnimationView` 组件：
- 引用Animator
- 启用DebugInfo查看状态

系统会自动同步Quantum的动画状态到Unity Animator。

## 动画状态哈希值 / Animation State Hashes

**重要：** 使用 `Animator.StringToHash()` 生成哈希值并保持一致：

```csharp
// 在配置资源中
StateHash = Animator.StringToHash("Attack");

// 在System代码中使用相同的哈希
private const int ATTACK_HASH = -1001; // 这应该与Animator.StringToHash("Attack")相同

// 建议：创建一个集中管理的常量类
public static class AnimationHashes
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int ThrowStart = Animator.StringToHash("Throw_Start");
    // ... 等等
}
```

## 优先级配置建议 / Priority Configuration Recommendations

```csharp
public enum AnimationPriority
{
    Idle = 0,
    Movement = 10,
    Jump = 20,
    Attack = 30,
    Skill = 40,
    Throw = 50,
    Hit = 60,
    Knockdown = 70,
    Death = 100
}
```

## 取消策略说明 / Cancel Policy Descriptions

| 值 | 策略 | 说明 | 适用场景 |
|----|------|------|----------|
| 0 | NonCancellable | 完全不可取消 | 死亡、投技执行 |
| 1 | AlwaysCancellable | 随时可取消 | Idle、Run |
| 2 | CancellableInWindow | 窗口内可取消 | 攻击（连招） |
| 3 | CancellableOnEnd | 接近结束可取消 | 技能恢复 |
| 4 | OnlyByHigherPriority | 仅高优先级可打断 | 受击、投技抓取 |

## 示例场景 / Example Scenarios

### 场景1：实现三段连招

```csharp
// 在AnimationConfigData中配置
new AnimationStateData
{
    StateName = "Attack",
    StateHash = Animator.StringToHash("Attack"),
    Priority = 30,
    CancelPolicy = 2, // CancellableInWindow
    CancelWindows = new CancelWindowData[]
    {
        new CancelWindowData
        {
            StartTime = FP._0_40, // 40%
            EndTime = FP._0_70,   // 70%
            AllowedTargetHashes = new int[] { Animator.StringToHash("Attack") }
        }
    }
}

// 在System中
public override void Update(Frame frame, ref Filter filter)
{
    var input = frame.GetPlayerInput(filter.PlayerLink->Player);
    
    if (input->Jump.WasPressed)
    {
        bool success = ComboSystem.TryAttack(frame, filter.Entity, ATTACK_HASH);
        if (success)
        {
            int combo = ComboSystem.GetComboCount(frame, filter.Entity);
            Log.Info($"Combo: {combo}"); // 会显示1, 2, 3...
        }
    }
}
```

### 场景2：投技攻击

```csharp
// 在System中
if (input->Down.IsDown && input->Jump.WasPressed)
{
    bool success = ThrowSystem.TryThrow(
        frame, 
        filter.Entity, 
        grabRange: FP._1_50,  // 1.5米
        grabAngle: 45          // 45度
    );
    
    if (success)
    {
        // 投技成功，之后可以在动画事件中调用ExecuteThrow
    }
}

// 在动画事件回调中
ThrowSystem.ExecuteThrow(
    frame, 
    attackerEntity, 
    damage: 50, 
    knockbackVelocity: new FPVector2(5, 3)
);

// 完成投技
ThrowSystem.CompleteThrow(frame, attackerEntity);
```

### 场景3：智能受击

```csharp
// 检测是否在地面
bool isGrounded = filter.KCC->Grounded;

// 应用受击
HitReactionSystem.ApplyHit(
    frame,
    targetEntity,
    damage: 10,
    knockback: new FPVector2(2, 1),
    hitStunDuration: FP._0_30, // 0.3秒
    isGrounded: isGrounded
);

// 系统会自动选择正确的受击动画（地面/空中）
```

## 调试技巧 / Debugging Tips

### 1. 启用View层调试信息
在QuantumAnimationView上勾选 "Show Debug Info"，会在场景中显示：
- 当前动画哈希
- 优先级
- 归一化时间
- 是否在过渡

### 2. 使用Quantum Inspector
在Quantum Debugger中可以实时查看：
- AnimationState组件的值
- ThrowState状态
- ComboData连招计数

### 3. 日志输出
在System中添加日志：
```csharp
Log.Info($"Playing animation: {stateHash}, Priority: {priority}");
```

## 性能考虑 / Performance Considerations

1. **哈希值缓存**
   - 使用静态常量存储哈希值
   - 避免运行时调用StringToHash

2. **组件查询**
   - 使用Filter减少TryGetPointer调用
   - 只在需要时查询组件

3. **取消窗口**
   - 保持窗口数量≤3
   - 简化窗口检查逻辑

## 迁移指南 / Migration Guide

从之前的Unity-only实现迁移到Quantum实现：

### 之前 (Unity-only)
```csharp
// Unity层
var animManager = GetComponent<WarriorAnimationManager>();
animManager.PlayAttack();
```

### 现在 (Quantum)
```csharp
// Quantum System
AnimationSystem.TryPlayAnimation(frame, entity, ATTACK_HASH);

// Unity View层自动同步，无需手动调用
```

## 常见问题 / FAQ

**Q: 为什么要用Quantum而不是Unity动画系统？**
A: Quantum保证所有客户端状态一致，支持网络同步和回滚，这对多人格斗游戏至关重要。

**Q: 动画哈希值如何管理？**
A: 建议创建一个AnimationHashes静态类集中管理所有哈希值。

**Q: 如何处理动画事件？**
A: 在Quantum System中使用定时器或检查动画进度来触发逻辑，而不是依赖Unity的AnimationEvent。

**Q: 性能如何？**
A: Quantum System运行在确定性模拟中，性能开销很小。View层只是播放动画，开销与普通Unity动画相同。

## 总结 / Summary

本系统将动画逻辑完全迁移到Quantum确定性层，实现了：
- ✅ 网络同步的动画系统
- ✅ 确定性的优先级和取消机制
- ✅ 完整的投技系统
- ✅ 智能受击反应
- ✅ 连招系统
- ✅ Unity View层自动同步

这是一个生产级的解决方案，适用于需要精确同步的多人格斗游戏。
