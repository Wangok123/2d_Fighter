# 属性计算器系统 (Attribute Calculator System)

## 概述

属性计算器系统使用策略模式，允许为不同的属性类型定义不同的计算逻辑，同时保持代码的可扩展性和可维护性。

## 核心接口

### IAttributeCalculator

```csharp
public interface IAttributeCalculator
{
    LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers);
}
```

所有计算器都必须实现此接口。

## 内置计算器

### 1. DefaultAttributeCalculator (默认计算器)

**公式:** `FinalValue = (BaseValue + ∑Add) × (1 + ∑Multiply) + ∑Final`

**适用场景:** 大部分通用属性（攻击力、防御力等）

```csharp
var attack = new GameAttribute(AttributeType.Attack, 100);
// 默认使用 DefaultAttributeCalculator
```

### 2. ClampedAttributeCalculator (限制范围计算器)

**功能:** 在默认计算后，限制结果在最小值和最大值之间

**适用场景:** 有硬性上下限的属性（如攻击速度）

```csharp
var calculator = new ClampedAttributeCalculator(
    minValue: new LATInt { Value = 100 },
    maxValue: new LATInt { Value = 20000 }
);
var attackSpeed = new GameAttribute(AttributeType.AttackSpeed, 1000, calculator);
```

### 3. SoftCapAttributeCalculator (软上限计算器)

**功能:** 超过软上限后，额外部分按比例递减

**适用场景:** 需要递减收益的属性（如移动速度）

```csharp
var calculator = new SoftCapAttributeCalculator(
    softCap: new LATInt { Value = 10000 },    // 软上限
    diminishRate: new LATInt { Value = 5000 } // 50% 递减率
);
var moveSpeed = new GameAttribute(AttributeType.MoveSpeed, 5000, calculator);
```

**示例计算:**
- 结果 ≤ 10000: 不变
- 结果 = 12000: 10000 + (2000 × 0.5) = 11000

### 4. PercentageAttributeCalculator (百分比计算器)

**功能:** 限制结果在 0% - 100% 之间

**适用场景:** 百分比属性（暴击率、抗性等）

```csharp
var critRate = new GameAttribute(AttributeType.CriticalRate, 1000);
// 自动使用 PercentageAttributeCalculator (通过工厂)
```

### 5. NonNegativeAttributeCalculator (非负计算器)

**功能:** 确保结果不为负数

**适用场景:** 不能为负的属性（生命值、护甲等）

```csharp
var hp = new GameAttribute(AttributeType.CurrentHP, 1000);
// 自动使用 NonNegativeAttributeCalculator (通过工厂)
```

## 使用方式

### 方式一: 默认行为（推荐）

```csharp
// 系统会根据 AttributeType 自动选择合适的计算器
var attack = new GameAttribute(AttributeType.Attack, 100);
var critRate = new GameAttribute(AttributeType.CriticalRate, 500);
```

### 方式二: 手动指定计算器

```csharp
var calculator = new ClampedAttributeCalculator(
    minValue: LATInt.Zero,
    maxValue: new LATInt { Value = 1000 }
);
var defense = new GameAttribute(AttributeType.Defense, 50, calculator);
```

### 方式三: 使用工厂方法

```csharp
var calculator = AttributeCalculatorFactory.CreateSoftCap(
    softCap: new LATInt { Value = 8000 },
    diminishRate: new LATInt { Value = 5000 }
);
var attribute = new GameAttribute(AttributeType.MoveSpeed, 5000, calculator);
```

### 方式四: 动态切换计算器

```csharp
var attribute = new GameAttribute(AttributeType.Attack, 100);

// 在某个特殊状态下切换计算器
attribute.SetCalculator(new SoftCapAttributeCalculator(
    softCap: new LATInt { Value = 5000 },
    diminishRate: new LATInt { Value = 7000 }
));
```

## 如何扩展：创建自定义计算器

### 示例 1: 创建简单的自定义计算器

```csharp
using System.Collections.Generic;
using LATMath;

public class MyCustomCalculator : IAttributeCalculator
{
    public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
    {
        // 自定义计算逻辑
        LATInt result = baseValue;
        
        foreach (var modifier in modifiers)
        {
            if (modifier.Type == ModifierType.Add)
                result += modifier.Value;
        }
        
        return result;
    }
}
```

### 示例 2: 组合现有计算器

```csharp
public class CompositeCalculator : IAttributeCalculator
{
    private readonly IAttributeCalculator _baseCalculator;
    
    public CompositeCalculator(IAttributeCalculator baseCalculator = null)
    {
        _baseCalculator = baseCalculator ?? new DefaultAttributeCalculator();
    }
    
    public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
    {
        // 先使用基础计算器
        var result = _baseCalculator.Calculate(baseValue, modifiers);
        
        // 再应用自定义逻辑
        result = ApplyCustomLogic(result);
        
        return result;
    }
    
    private LATInt ApplyCustomLogic(LATInt value)
    {
        // 你的自定义逻辑
        return value;
    }
}
```

### 示例 3: 条件计算器

```csharp
public class ConditionalCalculator : IAttributeCalculator
{
    private readonly IAttributeCalculator _baseCalculator;
    private readonly System.Func<bool> _condition;
    private readonly LATInt _bonusWhenTrue;
    
    public ConditionalCalculator(
        System.Func<bool> condition, 
        LATInt bonusWhenTrue, 
        IAttributeCalculator baseCalculator = null)
    {
        _condition = condition;
        _bonusWhenTrue = bonusWhenTrue;
        _baseCalculator = baseCalculator ?? new DefaultAttributeCalculator();
    }
    
    public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
    {
        var result = _baseCalculator.Calculate(baseValue, modifiers);
        
        // 根据条件添加额外加成
        if (_condition != null && _condition.Invoke())
        {
            result += _bonusWhenTrue;
        }
        
        return result;
    }
}

// 使用示例
var lowHealthBonus = new ConditionalCalculator(
    condition: () => unit.CurrentHP < unit.MaxHP / 2,
    bonusWhenTrue: new LATInt { Value = 500 }
);
var attack = new GameAttribute(AttributeType.Attack, 100, lowHealthBonus);
```

## 工厂模式配置

### 修改 AttributeCalculatorFactory

```csharp
public static class AttributeCalculatorFactory
{
    public static IAttributeCalculator GetCalculator(AttributeType attributeType)
    {
        return attributeType switch
        {
            AttributeType.Attack => new DefaultAttributeCalculator(),
            AttributeType.CriticalRate => new PercentageAttributeCalculator(),
            AttributeType.MoveSpeed => new SoftCapAttributeCalculator(...),
            
            // 添加你的自定义映射
            AttributeType.MyCustomAttribute => new MyCustomCalculator(),
            
            _ => new DefaultAttributeCalculator()
        };
    }
}
```

## 最佳实践

### 1. 优先使用组合而非继承

```csharp
// ✅ 好的做法
public class MyCalculator : IAttributeCalculator
{
    private readonly IAttributeCalculator _baseCalculator;
    
    public MyCalculator(IAttributeCalculator baseCalculator = null)
    {
        _baseCalculator = baseCalculator ?? new DefaultAttributeCalculator();
    }
    
    public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
    {
        var result = _baseCalculator.Calculate(baseValue, modifiers);
        // 添加额外逻辑
        return result;
    }
}

// ❌ 不推荐的做法
public class MyCalculator : DefaultAttributeCalculator
{
    // ...
}
```

### 2. 保持计算器无状态

```csharp
// ✅ 好的做法 - 无状态
public class StatelessCalculator : IAttributeCalculator
{
    private readonly LATInt _config; // 只读配置
    
    public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
    {
        // 基于输入计算，不依赖内部状态
        return baseValue + _config;
    }
}

// ❌ 不推荐的做法 - 有状态
public class StatefulCalculator : IAttributeCalculator
{
    private int _callCount; // 可变状态
    
    public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
    {
        _callCount++; // 修改状态
        return baseValue;
    }
}
```

### 3. 使用工厂集中管理

```csharp
// ✅ 好的做法
var attribute = new GameAttribute(AttributeType.Attack, 100);
// 工厂自动选择合适的计算器

// ❌ 不推荐的做法（除非有特殊需求）
var calculator = new DefaultAttributeCalculator();
var attribute = new GameAttribute(AttributeType.Attack, 100, calculator);
```

## 向后兼容性

现有代码无需修改，继续按原样工作：

```csharp
// 旧代码依然有效
var attack = new GameAttribute(AttributeType.Attack, 100);
attack.AddModifier(new AttributeModifier(50, ModifierType.Add, ModifierSource.Equipment));
var finalValue = attack.FinalValue; // 正常工作
```

## 性能考虑

1. **计算器重用**: DefaultCalculator 等常用计算器在工厂中是单例
2. **延迟计算**: FinalValue 使用脏标记，只在需要时计算
3. **无状态设计**: 计算器可安全共享，节省内存

## 调试技巧

```csharp
// 获取当前修饰符列表
var modifiers = attribute.GetModifiers();
foreach (var mod in modifiers)
{
    Debug.Log($"Modifier: {mod.Type}, Value: {mod.Value.Value}");
}

// 测试计算器
var calculator = AttributeCalculatorFactory.GetCalculator(AttributeType.Attack);
var result = calculator.Calculate(
    new LATInt { Value = 100 }, 
    new List<AttributeModifier> { ... }
);
```
