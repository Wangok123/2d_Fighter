# 重击蓄力与搓招系统文档

## 概述

本次更新在原有的攻击系统基础上，新增了以下功能：

1. **重击蓄力系统** - 长按HP键可以蓄力，蓄力时间越长伤害越高
2. **搓招系统** - 类似街头霸王的指令输入系统，支持方向+按键组合技

## 重击蓄力系统

### 工作原理

- 按住HP键开始蓄力
- 蓄力时间从0到最大值（默认2秒）
- 释放HP键触发蓄力重击
- 伤害倍率根据蓄力时间线性增长

### 配置参数

在 `AttackConfig` 资产中：

```csharp
[Header("Heavy Charge Settings")]
public FP MaxChargeTime = FP._2;              // 最大蓄力时间（秒）
public FP MinChargeTime = FP._0_50;           // 最小蓄力时间（低于此值无加成）
public FP FullChargeDamageMultiplier = FP._2; // 满蓄力伤害倍率（2.0x）
```

### 伤害计算

- 蓄力等级 = (实际蓄力时间 - 最小蓄力时间) / (最大蓄力时间 - 最小蓄力时间)
- 伤害倍率 = 1.0 + 蓄力等级 × (满蓄力倍率 - 1.0)
- 最终伤害 = 基础重击伤害 × 伤害倍率

### 示例

假设配置：
- 基础重击伤害 = 25
- 最小蓄力时间 = 0.5秒
- 最大蓄力时间 = 2秒
- 满蓄力倍率 = 2.0x

蓄力时间对应伤害：
- 0秒: 25 (无蓄力)
- 0.5秒: 25 (刚达到最小蓄力时间)
- 1.25秒: 37.5 (50%蓄力，1.5x倍率)
- 2秒: 50 (满蓄力，2.0x倍率)

## 搓招系统（指令输入系统）

### 工作原理

系统会持续记录玩家的输入序列（方向+按键），并与配置的特殊招式进行匹配。

### 输入编码

使用数字键盘风格的方向编码（相对于角色朝向）：

```
7  8  9     ↖  ↑  ↗
4  5  6  =  ←  ·  →
1  2  3     ↙  ↓  ↘
```

CommandInput 枚举：
- `Down` = 1 (↓)
- `DownRight` = 2 (↘)
- `Right` = 3 (→)
- `UpRight` = 4 (↗)
- `Up` = 5 (↑)
- `UpLeft` = 6 (↖)
- `Left` = 7 (←)
- `DownLeft` = 8 (↙)
- `LP` = 9 (轻攻击)
- `HP` = 10 (重攻击)
- `Dash` = 11 (冲刺)
- `Jump` = 12 (跳跃)

### 创建特殊招式

1. 创建 `SpecialMoveConfig` 资产：

```csharp
public class SpecialMoveConfig : AssetObject
{
    public int MoveId;                    // 招式唯一ID
    public string MoveName;               // 招式名称（如"波动拳"）
    public int[] InputSequence;           // 输入序列
    public FP Damage = 30;                // 伤害
    public FP Cooldown = FP._1;           // 冷却时间
    public int RequiredLevel = 0;         // 需求等级
}
```

2. 配置输入序列示例：

**波动拳（Hadouken）**: ↓ ↘ → + LP
```csharp
InputSequence = new int[] { 1, 2, 3, 9 };  // Down, DownRight, Right, LP
```

**升龙拳（Shoryuken）**: → ↓ ↘ + HP
```csharp
InputSequence = new int[] { 3, 1, 2, 10 };  // Right, Down, DownRight, HP
```

**旋风腿（Tatsumaki）**: ↓ ↙ ← + LP
```csharp
InputSequence = new int[] { 1, 8, 7, 9 };  // Down, DownLeft, Left, LP
```

3. 将特殊招式添加到角色的 `AttackData` 组件：

在Unity编辑器中，在角色实体的 `AttackData` 组件上配置 `SpecialMoves` 数组，添加创建的特殊招式配置资产。

### 输入缓冲与匹配

- 系统维护一个输入缓冲区（默认最多8个输入）
- 输入在一定时间窗口内有效（默认0.5秒）
- 每帧检查缓冲区是否匹配任何特殊招式
- 匹配成功后执行招式并清空缓冲区

### 配置参数

在 `AttackConfig` 资产中：

```csharp
[Header("Command Input Settings")]
public FP CommandInputWindow = FP._0_50;   // 指令输入有效时间窗口（秒）
public int MaxInputBufferSize = 8;         // 最大输入缓冲区大小
```

## 组件说明

### CommandInputData

用于跟踪玩家的输入序列：

```
component CommandInputData
{
    Int32 InputBufferSize;           // 当前缓冲区大小
    array<Int32>[8] InputBuffer;     // 输入缓冲区
    Int32 InputBufferIndex;          // 当前索引
    FrameTimer InputExpiryTimer;     // 输入过期计时器
}
```

### AttackData（更新）

新增字段：
- `SpecialMoves` - 特殊招式配置数组
- `HeavyChargeTime` - 当前重击蓄力时间
- `IsChargingHeavy` - 是否正在蓄力

## 系统说明

### CommandInputSystem

负责追踪和记录玩家输入：
- 每帧读取输入并转换为指令编码
- 添加到输入缓冲区
- 自动处理输入过期
- 提供匹配和清空缓冲区的静态方法

### AbilitySystem（更新）

增强功能：
1. **特殊招式检查** - 优先检查是否有特殊招式匹配
2. **重击蓄力处理** - 追踪HP键按压时间
3. **蓄力释放** - 计算伤害并触发攻击

## 事件

### AttackPerformed（更新）

新增 `ChargeLevel` 字段：
```csharp
event AttackPerformed
{
    EntityRef Attacker;
    Boolean IsHeavyAttack;
    Int32 ComboCount;
    FP Damage;
    FP ChargeLevel;  // 新增：蓄力等级（0-1）
}
```

### SpecialMovePerformed（新增）

```csharp
event SpecialMovePerformed
{
    EntityRef Attacker;
    Int32 MoveId;     // 招式ID
    FP Damage;        // 造成的伤害
}
```

## 使用示例

### 监听事件

```csharp
public void OnEventAttackPerformed(EventAttackPerformed evt)
{
    if (evt.IsHeavyAttack && evt.ChargeLevel > 0)
    {
        Log.Info($"蓄力重击! 蓄力等级: {evt.ChargeLevel * 100}%, 伤害: {evt.Damage}");
    }
}

public void OnEventSpecialMovePerformed(EventSpecialMovePerformed evt)
{
    Log.Info($"特殊招式 ID:{evt.MoveId}, 伤害: {evt.Damage}");
}
```

### 测试蓄力系统

1. 按住HP键1秒钟
2. 释放HP键
3. 观察伤害倍率（应该有50%左右的加成）

### 测试搓招系统

假设配置了波动拳（↓ ↘ → + LP）：

1. 快速输入：按下 → 松开 → 按住↘ → 松开 → 按住→ → 松开 → 按LP
2. 系统应该识别并触发波动拳
3. 观察 SpecialMovePerformed 事件

## 扩展性

### 添加新的特殊招式

1. 创建新的 `SpecialMoveConfig` 资产
2. 配置输入序列和属性
3. 添加到角色的 `AttackData.SpecialMoves` 数组
4. 无需修改代码即可添加无限种招式

### 高级用法

- 可以为不同角色配置不同的特殊招式集
- 可以通过等级限制解锁招式
- 可以配置不同的伤害、冷却时间
- 输入序列可以任意组合方向和按键

## 注意事项

1. **输入精度**: 搓招需要在有效时间窗口内完成（默认0.5秒）
2. **优先级**: 特殊招式 > 蓄力重击 > 普通轻攻击
3. **方向相对性**: 所有方向输入都相对于角色朝向（自动转换）
4. **蓄力取消**: 按住HP期间受到攻击或死亡会取消蓄力
5. **冷却时间**: 特殊招式有独立的冷却时间，在冷却期间无法使用

## 配置建议

### 平衡性建议

- 蓄力重击伤害倍率：1.5x - 2.5x
- 特殊招式伤害：轻攻击的2-3倍
- 特殊招式冷却：1-3秒
- 输入窗口时间：0.3-0.7秒（太短难操作，太长容易误触发）

### 性能建议

- 输入缓冲区大小：6-10个输入
- 特殊招式数量：每个角色4-8个
- 输入序列长度：3-6个输入（太长难记忆）
