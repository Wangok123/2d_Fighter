# 属性计算器快速上手指南

## 5 分钟快速入门

### 1️⃣ 基础使用（0 行额外代码）

```csharp
// 创建属性 - 系统自动选择合适的计算器
var attack = new GameAttribute(AttributeType.Attack, 100);
var critRate = new GameAttribute(AttributeType.CriticalRate, 500);
var moveSpeed = new GameAttribute(AttributeType.MoveSpeed, 5000);

// 添加修饰符
attack.AddModifier(new AttributeModifier(50, ModifierType.Add, ModifierSource.Equipment));
attack.AddModifier(new AttributeModifier(3000, ModifierType.Multiply, ModifierSource.Buff));

// 获取最终值
var finalAttack = attack.FinalValue.Value; // (100 + 50) × 1.3 = 195
```

**向后兼容:** ✅ 现有代码无需修改！

---

### 2️⃣ 自定义计算器（3 行代码）

```csharp
// 创建限制范围的攻击速度
var calculator = new ClampedAttributeCalculator(
    minValue: new LATInt { Value = 100 },
    maxValue: new LATInt { Value = 20000 }
);

var attackSpeed = new GameAttribute(AttributeType.AttackSpeed, 1000, calculator);
attackSpeed.AddModifier(new AttributeModifier(25000, ModifierType.Add, ModifierSource.Buff));

var final = attackSpeed.FinalValue.Value; // 限制在 20000
```

---

### 3️⃣ 工厂方法（推荐）

```csharp
// 方式 1: 自动选择
var attribute = new GameAttribute(AttributeType.CriticalRate, 1000);
// ✅ 工厂自动分配 PercentageCalculator (0-100% 限制)

// 方式 2: 使用工厂创建特定计算器
var softCapCalc = AttributeCalculatorFactory.CreateSoftCap(
    softCap: new LATInt { Value = 8000 },
    diminishRate: new LATInt { Value = 5000 }
);
var speed = new GameAttribute(AttributeType.MoveSpeed, 5000, softCapCalc);
```

---

### 4️⃣ 动态切换（1 行代码）

```csharp
var attribute = new GameAttribute(AttributeType.Attack, 100);

// 进入特殊状态，切换计算器
attribute.SetCalculator(new ClampedAttributeCalculator(
    minValue: LATInt.Zero,
    maxValue: new LATInt { Value = 500 }
));
```

---

## 常见场景

### 场景 1: 暴击率限制 0-100%

```csharp
var critRate = new GameAttribute(AttributeType.CriticalRate, 2000);
// 自动限制在 0-10000 (0-100%)

critRate.AddModifier(new AttributeModifier(9000, ModifierType.Add, ModifierSource.Equipment));
// FinalValue = 10000 (100%)，不会超过
```

### 场景 2: 移动速度软上限

```csharp
var moveSpeed = new GameAttribute(AttributeType.MoveSpeed, 5000);
// 自动应用软上限 10000，超过部分打五折

moveSpeed.AddModifier(new AttributeModifier(8000, ModifierType.Add, ModifierSource.Buff));
// 计算: 5000 + 8000 = 13000
// 软上限: 10000 + (3000 × 0.5) = 11500
```

### 场景 3: 生命值不能为负

```csharp
var hp = new GameAttribute(AttributeType.CurrentHP, 500);
// 自动限制最小值为 0

hp.AddModifier(new AttributeModifier(-800, ModifierType.Add, ModifierSource.Debuff));
// FinalValue = 0 (不会变成负数)
```

---

## 创建自定义计算器（5 分钟）

### 步骤 1: 创建计算器类

```csharp
using System.Collections.Generic;
using LATMath;
using UnityCore.GameModule.Battle.Attributes.Calculators;

namespace YourNamespace
{
    public class MyCustomCalculator : IAttributeCalculator
    {
        private readonly IAttributeCalculator _baseCalculator;
        
        public MyCustomCalculator()
        {
            _baseCalculator = new DefaultAttributeCalculator();
        }
        
        public LATInt Calculate(LATInt baseValue, IReadOnlyList<AttributeModifier> modifiers)
        {
            // 使用默认计算
            var result = _baseCalculator.Calculate(baseValue, modifiers);
            
            // 你的自定义逻辑
            // 例如：每100点加成递减10%
            if (result > new LATInt { Value = 100 })
            {
                var excess = result - new LATInt { Value = 100 };
                result = new LATInt { Value = 100 } + excess * new LATInt { Value = 9000 } / LATInt.One;
            }
            
            return result;
        }
    }
}
```

### 步骤 2: 使用自定义计算器

```csharp
var attribute = new GameAttribute(AttributeType.Attack, 50, new MyCustomCalculator());
```

### 步骤 3: 注册到工厂（可选）

在 `AttributeCalculatorFactory.cs` 中添加：

```csharp
public static IAttributeCalculator GetCalculator(AttributeType attributeType)
{
    return attributeType switch
    {
        // ...现有映射
        
        AttributeType.YourCustomType => new MyCustomCalculator(),
        
        _ => DefaultCalculator
    };
}
```

---

## 内置计算器速查表

| 计算器类型 | 用途 | 示例 |
|-----------|------|------|
| `DefaultAttributeCalculator` | 标准计算 | `new GameAttribute(type, value)` |
| `ClampedAttributeCalculator` | 硬性上下限 | 攻击速度 100-20000 |
| `SoftCapAttributeCalculator` | 软上限递减 | 移动速度超过 10000 后递减 |
| `PercentageAttributeCalculator` | 0-100% 限制 | 暴击率、抗性 |
| `NonNegativeAttributeCalculator` | 不能为负 | 生命值、护甲 |

---

## 调试技巧

### 打印属性详情

```csharp
Debug.Log(attribute.ToString());
// 输出: Attack: Base=100, Final=195, Modifiers=2
```

### 查看所有修饰符

```csharp
var modifiers = attribute.GetModifiers();
foreach (var mod in modifiers)
{
    Debug.Log($"{mod.Type}: {mod.Value.Value}, Source: {mod.Source}");
}
```

### 测试计算器逻辑

```csharp
var calculator = new ClampedAttributeCalculator(LATInt.Zero, new LATInt { Value = 100 });

var testModifiers = new List<AttributeModifier>
{
    new AttributeModifier(50, ModifierType.Add, ModifierSource.Equipment),
    new AttributeModifier(100, ModifierType.Add, ModifierSource.Buff)
};

var result = calculator.Calculate(new LATInt { Value = 10 }, testModifiers);
Debug.Log($"Result: {result.Value}"); // 100 (被限制)
```

---

## 常见问题 FAQ

### Q: 现有代码需要改吗？
**A:** 不需要！完全向后兼容。

### Q: 如何禁用自动计算器选择？
**A:** 手动传入计算器参数：
```csharp
var attr = new GameAttribute(type, value, AttributeCalculatorFactory.GetDefault());
```

### Q: 计算器可以组合吗？
**A:** 可以！大部分计算器接受 baseCalculator 参数：
```csharp
var composite = new ClampedAttributeCalculator(
    min, max,
    baseCalculator: new SoftCapAttributeCalculator(cap, rate)
);
```

### Q: 性能如何？
**A:** 优秀！
- 脏标记延迟计算
- 常用计算器单例共享
- 无状态设计无GC压力

### Q: 如何测试我的计算器？
**A:** 参考 `AttributeCalculatorTests.cs`：
```csharp
AttributeCalculatorTests.RunAllTests();
```

---

## 下一步

- 📖 阅读详细文档: `README_AttributeCalculator.md`
- 💡 查看示例代码: `Examples/AttributeCalculatorExample.cs`
- 🧪 运行单元测试: `Tests/AttributeCalculatorTests.cs`
- 🔧 创建自定义计算器: `Calculators/CustomCalculatorExample.cs`

---

## 技术支持

遇到问题？检查以下内容：
1. ✅ 确保实现了 `IAttributeCalculator` 接口
2. ✅ 检查计算器是否正确组合
3. ✅ 运行单元测试验证功能
4. ✅ 查看完整文档了解更多细节
