# 攻击系统重构总结 (Quantum格式 - 可配置优先级) / Attack System Refactoring Summary (Quantum Format - Configurable Priority)

## 中文总结

### 问题
NormalAttackSystem中使用了分权重的方式处理特殊攻击、重攻击、轻攻击，这样写有什么劣势，如何改进？

### 回答

#### 原实现的劣势：

1. **隐式优先级** - 攻击优先级隐藏在代码执行顺序中
2. **代码组织差** - 所有攻击逻辑混在Update方法中，没有清晰的分区
3. **优先级固定** - 优先级硬编码在代码中，无法灵活配置
4. **缺少文档** - 没有清楚说明优先级系统或设计意图

#### 解决方案：可配置优先级系统

通过在AttackConfig中添加优先级配置，使系统更加灵活：

**1. 在AttackConfig中添加优先级字段**
```csharp
[Header("Attack Priority Settings")]
[Tooltip("Priority for special moves (higher value = higher priority)")]
public int SpecialMovePriority = 100;

[Tooltip("Priority for heavy attack (higher value = higher priority)")]
public int HeavyAttackPriority = 50;

[Tooltip("Priority for light attack (higher value = higher priority)")]
public int LightAttackPriority = 10;
```

**2. 基于配置的优先级处理**
```csharp
/// <summary>
/// Process attacks in order of their configured priority values.
/// Higher priority values are processed first.
/// </summary>
private void ProcessAttacksByPriority(Frame frame, ref Filter filter, 
                                      SimpleInput2D input, AttackConfig config)
{
    // 根据配置的优先级值处理攻击
    // 从最高优先级到最低优先级依次检查和执行
}
```

**3. 清晰的代码组织**
- 使用分隔符标注每个攻击系统
- 在注释中说明优先级可通过配置调整
- 每个方法都有详细注释

#### 优势：

1. ✅ **可配置优先级** - 通过AttackConfig设置，无需修改代码
2. ✅ **灵活性强** - 可以为不同角色设置不同的攻击优先级
3. ✅ **清晰的文档** - 在代码和配置中都明确标注优先级
4. ✅ **更好的组织** - 代码分区清晰，易于维护
5. ✅ **Quantum兼容** - 完全遵循确定性模拟模式
6. ✅ **无破坏性变更** - 相同行为，相同API

---

## English Summary

### Question
The original question asked about the disadvantages of the weight-based (priority-based) approach used in NormalAttackSystem for handling special attacks, heavy attacks, and light attacks, and how to improve it.

### Answer

#### Disadvantages of Original Implementation:

1. **Implicit Priority** - Attack priority is hidden in code execution order
2. **Poor Code Organization** - All attack logic mixed in Update method without clear sections
3. **Fixed Priority** - Priority is hard-coded, cannot be configured flexibly
4. **Lack of Documentation** - No clear explanation of priority system or design intent

#### Solution: Configurable Priority System

By adding priority configuration to AttackConfig, the system becomes more flexible:

**1. Priority Fields in AttackConfig**
```csharp
[Header("Attack Priority Settings")]
[Tooltip("Priority for special moves (higher value = higher priority)")]
public int SpecialMovePriority = 100;

[Tooltip("Priority for heavy attack (higher value = higher priority)")]
public int HeavyAttackPriority = 50;

[Tooltip("Priority for light attack (higher value = higher priority)")]
public int LightAttackPriority = 10;
```

**2. Config-Based Priority Processing**
```csharp
/// <summary>
/// Process attacks in order of their configured priority values.
/// Higher priority values are processed first.
/// </summary>
private void ProcessAttacksByPriority(Frame frame, ref Filter filter, 
                                      SimpleInput2D input, AttackConfig config)
{
    // Process attacks based on configured priority values
    // Check and execute from highest to lowest priority
}
```

**3. Clear Code Organization**
- Use separators to mark each attack system
- Comments indicate priority is configurable
- Detailed comments for each method

#### Benefits:

1. ✅ **Configurable Priority** - Set via AttackConfig, no code changes needed
2. ✅ **Highly Flexible** - Different characters can have different attack priorities
3. ✅ **Clear Documentation** - Priority explicitly marked in code and config
4. ✅ **Better Organization** - Code sections are clear and maintainable
5. ✅ **Quantum Compatible** - Fully follows deterministic simulation patterns
6. ✅ **No Breaking Changes** - Same behavior, same API

---

## 代码对比 / Code Comparison

### 改进前 / Before:
```csharp
public override void Update(Frame frame, ref Filter filter)
{
    // 优先级硬编码
    if (TryExecuteSpecialMove(...)) return;
    ProcessHeavyCharging(...);
    if (input.LP.WasPressed) ProcessLightAttack(...);
}
```

### 改进后 / After:
```csharp
/// Attack priorities configurable via AttackConfig:
/// - SpecialMovePriority (default: 100)
/// - HeavyAttackPriority (default: 50)
/// - LightAttackPriority (default: 10)
public override void Update(Frame frame, ref Filter filter)
{
    UpdateAttackTimers(frame, ref filter);
    
    // Process attacks based on configured priority
    ProcessAttacksByPriority(frame, ref filter, input, attackConfig);
}

private void ProcessAttacksByPriority(...)
{
    // Dynamically process based on priority values
    for (int currentPriority = maxPriority; currentPriority >= 0; currentPriority--)
    {
        if (specialPriority == currentPriority) { ... }
        if (heavyPriority == currentPriority) { ... }
        if (lightPriority == currentPriority) { ... }
    }
}
```

---

## 配置示例 / Configuration Examples

### 默认配置 / Default Configuration
```
SpecialMovePriority = 100  (最高 / Highest)
HeavyAttackPriority = 50   (中等 / Medium)
LightAttackPriority = 10   (最低 / Lowest)
```

### 自定义配置示例 / Custom Configuration Examples

**示例1：重击优先级更高的角色**
```
SpecialMovePriority = 100
HeavyAttackPriority = 90   // 提高重击优先级
LightAttackPriority = 10
```

**示例2：所有攻击同等优先级**
```
SpecialMovePriority = 50
HeavyAttackPriority = 50
LightAttackPriority = 50
```

**示例3：轻攻击优先的快速角色**
```
SpecialMovePriority = 100
HeavyAttackPriority = 30
LightAttackPriority = 60   // 提高轻攻击优先级
```

---

## 改进内容总结 / Summary of Improvements

| 方面 / Aspect | 改进前 / Before | 改进后 / After |
|---------------|----------------|----------------|
| 优先级配置 / Priority Config | 硬编码 / Hard-coded | 可配置 / Configurable |
| 灵活性 / Flexibility | 固定 / Fixed | 高度灵活 / Highly flexible |
| 文档 / Documentation | 缺少 / Lacking | 完整详细 / Complete |
| 代码组织 / Code Organization | 一般 / Average | 清晰分区 / Clear sections |
| Quantum兼容 / Quantum Compatible | ✅ | ✅ |
| 确定性 / Deterministic | ✅ | ✅ |
| 破坏性变更 / Breaking Changes | - | 无 / None ✅ |

---

## 使用指南 / Usage Guide

### 如何调整攻击优先级 / How to Adjust Attack Priority

1. 在Unity编辑器中打开AttackConfig资产
2. 找到"Attack Priority Settings"部分
3. 调整优先级数值：
   - 数值越大，优先级越高
   - 相同数值表示同等优先级
4. 保存配置即可生效

### 添加新攻击类型 / Adding New Attack Types

1. 在AttackConfig中添加新的优先级字段
2. 在ProcessAttacksByPriority中添加检查逻辑
3. 创建对应的处理方法
4. 更新文档说明

---

## 结论 / Conclusion

**中文：**
通过在AttackConfig中添加可配置的优先级字段，系统变得更加灵活和数据驱动。设计师可以在Unity编辑器中轻松调整不同角色的攻击优先级，无需修改代码。这种方法既保持了Quantum引擎的兼容性，又提供了更好的可配置性。

**English:**
By adding configurable priority fields to AttackConfig, the system becomes more flexible and data-driven. Designers can easily adjust attack priorities for different characters in the Unity editor without code changes. This approach maintains Quantum engine compatibility while providing better configurability.
