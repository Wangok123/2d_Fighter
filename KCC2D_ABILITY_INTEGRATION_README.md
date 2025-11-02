# KCC2D + AbilityEnable Integration

## 概述 / Overview

本次修改实现了 Quantum 引擎下 KCC2D 系统与 AbilityEnable 组件的深度集成，并提供了类似 Sports Arena Brawler 官方案例的可扩展性。

This modification implements deep integration between the KCC2D system and the AbilityEnable component under the Quantum engine, providing extensibility similar to the official Sports Arena Brawler sample.

## 主要功能 / Key Features

### 1. 自动能力控制 / Automatic Ability Control

KCC2D 系统现在会自动检查 AbilityEnable 组件，并根据启用的能力动态调整行为：
- ✅ 二段跳 (Double Jump) - 通过 `MovementDoubleJumpEnabled` 控制
- ✅ 冲刺 (Dash) - 通过 `MovementDashEnabled` 控制
- ✅ 墙跳 (Wall Jump) - 通过 `MovementWallJumpEnabled` 控制
- ✅ 空中冲刺 (Air Dash) - 通过 `MovementAirDashEnabled` 控制
- ✅ 滑翔 (Glide) - 通过 `MovementGlideEnabled` 控制

The KCC2D system now automatically checks the AbilityEnable component and dynamically adjusts behavior based on enabled abilities.

### 2. 可扩展性钩子 / Extensibility Hooks

KCC2DConfig 现在提供虚方法，可以轻松扩展自定义状态和行为：

KCC2DConfig now provides virtual methods for easy extension of custom states and behaviors:

```csharp
protected virtual bool OnPreComputeState()      // 状态计算前处理
protected virtual void OnPostComputeState()     // 状态计算后处理
protected virtual SimpleInput2D OnFilterInput() // 输入过滤
protected virtual void OnIntegrateForces()      // 自定义力整合
protected virtual bool OnProcessJump()          // 自定义跳跃处理
```

### 3. 工具类 / Utility Classes

#### KCCAbilityIntegration
提供便捷的能力检查和控制方法 / Provides convenient ability checking and control methods:

```csharp
// 检查能力是否启用 / Check if ability is enabled
bool canDash = KCCAbilityIntegration.IsAbilityEnabled(frame, entity, AbilityId.MovementDash);

// 启用/禁用能力 / Enable/disable ability
KCCAbilityIntegration.SetAbilityEnabled(frame, entity, AbilityId.MovementDoubleJump, true);

// 启用所有能力 / Enable all abilities
KCCAbilityIntegration.EnableAllAbilities(abilityEnable);

// 禁用所有能力（如眩晕状态）/ Disable all abilities (e.g., stunned state)
KCCAbilityIntegration.DisableAllAbilities(abilityEnable);
```

## 文件结构 / File Structure

```
Client/Assets/Photon/QuantumAddons/KCC2D/
├── KCC2DConfig.cs                      # 修改：添加 AbilityEnable 集成和扩展钩子
├── KCCUtils.cs                         # 修改：添加辅助方法
├── KCCAbilityIntegration.cs            # 新增：能力集成工具类
├── ExtendedKCC2DConfig.cs              # 新增：扩展实现示例
└── KCC_ABILITY_INTEGRATION_GUIDE.md    # 新增：详细使用指南

Client/Assets/QuantumUser/Simulation/Core/Systems/
├── AbilityInputSystem.cs               # 修改：添加 AbilityEnable 检查
└── AbilityControlExampleSystem.cs      # 新增：使用示例
```

## 使用方法 / Usage

### 基础使用 / Basic Usage

1. **确保实体有 AbilityEnable 组件** / Ensure entity has AbilityEnable component:
   ```csharp
   // 在实体原型中添加 AbilityEnable 组件
   // Add AbilityEnable component in entity prototype
   ```

2. **KCC2D 会自动应用能力设置** / KCC2D automatically applies ability settings:
   ```csharp
   // MovementInputSystem 中已自动集成
   // Already integrated in MovementInputSystem
   var config = frame.FindAsset(filter.KCC->Config);
   config.Move(frame, filter.Entity, filter.Transform, filter.KCC);
   // AbilityEnable 设置会自动应用！/ AbilityEnable settings applied automatically!
   ```

### 扩展自定义状态 / Extending with Custom States

参考 `ExtendedKCC2DConfig.cs` 创建自定义配置 / See `ExtendedKCC2DConfig.cs` for custom configuration:

```csharp
public class MyCustomKCC2DConfig : KCC2DConfig
{
    protected override bool OnPreComputeState()
    {
        // 添加自定义状态逻辑 / Add custom state logic
        if (SomeCustomCondition())
        {
            // 处理自定义状态 / Handle custom state
            return true; // 跳过默认处理 / Skip default processing
        }
        return false;
    }
}
```

### 运行时能力控制 / Runtime Ability Control

```csharp
// 示例：等级系统解锁能力 / Example: Level-based ability unlocking
if (level >= 3)
{
    KCCAbilityIntegration.SetAbilityEnabled(
        frame, entity, 
        AbilityId.MovementDoubleJump, 
        true
    );
}

// 示例：眩晕状态禁用所有能力 / Example: Stun disables all abilities
if (isStunned)
{
    KCCAbilityIntegration.DisableAllAbilities(abilityEnable);
}
```

## 与 Sports Arena Brawler 的相似性 / Similarities to Sports Arena Brawler

1. **模块化能力系统** / Modular Ability System
   - 能力可以动态启用/禁用 / Abilities can be dynamically enabled/disabled
   - 通过组件组合构建角色 / Build characters through component composition

2. **可扩展的状态机** / Extensible State Machine
   - 虚方法钩子支持自定义状态 / Virtual method hooks for custom states
   - 清晰的扩展点 / Clear extension points

3. **信号驱动架构** / Signal-Driven Architecture
   - 支持 SystemSignalsOnly 模式 / Supports SystemSignalsOnly pattern
   - 通过信号进行系统间通信 / Inter-system communication via signals

## 示例场景 / Example Scenarios

### 场景 1：角色升级系统 / Scenario 1: Character Progression

```csharp
// 1级：基础移动和轻攻击 / Level 1: Basic movement and light attack
// 2级：解锁冲刺 / Level 2: Unlock dash
// 3级：解锁二段跳 / Level 3: Unlock double jump
// 5级：解锁空中冲刺 / Level 5: Unlock air dash
// 10级：解锁必杀技 / Level 10: Unlock ultimate

参见 AbilityControlExampleSystem.cs
See AbilityControlExampleSystem.cs
```

### 场景 2：临时能力增强 / Scenario 2: Temporary Ability Boosts

```csharp
// 拾取道具后临时启用能力 / Temporarily enable abilities after picking up items
// 例如：飞行道具启用滑翔 / Example: Flight item enables glide
```

### 场景 3：状态效果系统 / Scenario 3: Status Effect System

```csharp
// 眩晕：禁用所有能力 / Stun: Disable all abilities
// 沉默：禁用特殊技能 / Silence: Disable special abilities
// 减速：禁用冲刺和跳跃 / Slow: Disable dash and jump
```

## 性能考虑 / Performance Considerations

- ✅ 能力检查使用简单的布尔比较，非常高效 / Ability checks use simple boolean comparisons - very efficient
- ✅ 仅在需要时才应用设置覆盖 / Settings overrides only applied when needed
- ✅ 虚方法调用开销极小 / Virtual method call overhead is minimal
- ✅ 兼容 Quantum 的确定性模型 / Compatible with Quantum's deterministic model

## 测试建议 / Testing Recommendations

1. **测试基础移动能力** / Test basic movement abilities:
   - 二段跳 / Double jump
   - 冲刺 / Dash
   - 墙跳 / Wall jump

2. **测试运行时能力切换** / Test runtime ability toggling:
   - 启用/禁用能力后的行为 / Behavior after enabling/disabling abilities
   - 状态转换的正确性 / Correctness of state transitions

3. **测试自定义扩展** / Test custom extensions:
   - 使用 ExtendedKCC2DConfig / Using ExtendedKCC2DConfig
   - 自定义状态处理 / Custom state handling

## 常见问题 / FAQ

**Q: 如何添加新的自定义能力？**
**Q: How to add new custom abilities?**

A: 
1. 在 Ability.qtn 中添加新的 AbilityId / Add new AbilityId in Ability.qtn
2. 在 AbilityEnable 组件中添加对应的布尔字段 / Add corresponding boolean field in AbilityEnable
3. 在 KCCAbilityIntegration 中添加映射 / Add mapping in KCCAbilityIntegration
4. 创建自定义 KCC2DConfig 实现逻辑 / Create custom KCC2DConfig to implement logic

**Q: 能力检查的性能如何？**
**Q: What about performance of ability checks?**

A: 非常高效！只是简单的布尔值检查，没有复杂的查找或计算。
A: Very efficient! Just simple boolean checks, no complex lookups or calculations.

**Q: 如何像 Sports Arena Brawler 那样添加自定义状态？**
**Q: How to add custom states like Sports Arena Brawler?**

A: 继承 KCC2DConfig 并重写虚方法钩子。参见 ExtendedKCC2DConfig.cs 示例。
A: Extend KCC2DConfig and override virtual method hooks. See ExtendedKCC2DConfig.cs example.

## 相关文档 / Related Documentation

- [KCC_ABILITY_INTEGRATION_GUIDE.md](./Client/Assets/Photon/QuantumAddons/KCC2D/KCC_ABILITY_INTEGRATION_GUIDE.md) - 详细集成指南 / Detailed integration guide
- [ExtendedKCC2DConfig.cs](./Client/Assets/Photon/QuantumAddons/KCC2D/ExtendedKCC2DConfig.cs) - 扩展实现示例 / Extension implementation example
- [AbilityControlExampleSystem.cs](./Client/Assets/QuantumUser/Simulation/Core/Systems/AbilityControlExampleSystem.cs) - 使用示例 / Usage examples

## 下一步 / Next Steps

1. 在 Unity 编辑器中测试修改 / Test modifications in Unity Editor
2. 创建自定义 KCC2DConfig 资源 / Create custom KCC2DConfig assets
3. 根据游戏需求配置能力解锁 / Configure ability unlocks based on game requirements
4. 实现特定于游戏的扩展状态 / Implement game-specific extended states

## 贡献 / Contributing

如有问题或建议，请提交 Issue 或 Pull Request。
For issues or suggestions, please submit an Issue or Pull Request.
