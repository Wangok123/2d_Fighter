# NormalAttackSystem 移除迁移指南
# Migration Guide: NormalAttackSystem Removal

## 变更概述 (Change Summary)

根据Quantum引擎的组织架构和模块化设计原则，`NormalAttackSystem` 已被移除。所有攻击功能（轻攻击、重攻击、特殊技能）现在统一由 `ModularAbilitySystem` 处理。

Following Quantum's architectural principles and modular design, `NormalAttackSystem` has been removed. All attack functionality (light attacks, heavy attacks, special moves) is now unified under `ModularAbilitySystem`.

## 为什么移除 (Why Remove)

1. **功能重复 (Functional Redundancy)**
   - `NormalAttackSystem` 和 `ModularAbilitySystem` 都处理攻击逻辑
   - 轻攻击和重攻击本质上是能力(Ability)的一种
   - 维护两个系统增加了代码复杂度

2. **符合Quantum架构 (Aligns with Quantum Architecture)**
   - Quantum推荐使用模块化、可组合的设计
   - 统一的能力系统更容易扩展和维护
   - 减少系统间的耦合

3. **简化开发流程 (Simplifies Development)**
   - 新角色只需要配置 `ModularCharacterConfig`
   - 所有能力统一管理，一致的开发体验
   - 减少学习曲线

## 主要变更 (Key Changes)

### 1. 组件定义更新 (Component Definition Update)

**文件:** `Character.qtn`

**变更前:**
```qtn
component AttackData
{
    asset_ref<CharacterAttackConfig> AttackConfig;
    // ... other fields
}
```

**变更后:**
```qtn
component AttackData
{
    asset_ref<CharacterAttackConfig> AttackConfig;
    asset_ref<ModularCharacterConfig> ModularConfig;  // 新增
    // ... other fields
}
```

### 2. 系统文件移除 (System File Removed)

- ❌ `NormalAttackSystem.cs` - 已删除
- ❌ `NormalAttackSystem.cs.meta` - 已删除

### 3. ModularAbilitySystem 增强 (ModularAbilitySystem Enhanced)

**文件:** `ModularAbilitySystem.cs`

`GetModularConfig` 方法现在从 `AttackData` 组件读取配置：

```csharp
private ModularCharacterConfig GetModularConfig(Frame frame, ref Filter filter)
{
    // 从 AttackData 组件读取 ModularConfig 引用
    if (filter.AttackData->ModularConfig.Id.IsValid)
    {
        return frame.FindAsset(filter.AttackData->ModularConfig);
    }
    
    // 没有模块化配置，使用传统系统
    return null;
}
```

### 4. MovementSystem 更新 (MovementSystem Updated)

**文件:** `MovementSystem.cs`

现在支持两种配置方式：
- 优先使用 `ModularCharacterConfig`（如果存在）
- 向后兼容 `CharacterAttackConfig`

新增方法：
- `GetModifiedSettingsFromModularConfig()` - 从模块化配置获取设置
- `IsAbilityUnlocked()` - 检查能力解锁状态

## 向后兼容性 (Backward Compatibility)

### ✅ 现有角色继续工作 (Existing Characters Continue Working)

如果你的角色只配置了 `CharacterAttackConfig`：
- 角色仍然可以正常工作
- `ModularAbilitySystem` 会检测到没有 `ModularConfig`，直接返回
- 不会产生错误或警告

### ⚠️ 建议迁移 (Recommended Migration)

虽然向后兼容，但建议迁移到新系统以获得完整功能：

1. **轻攻击和重攻击** 需要使用 `ModularCharacterConfig` 才能生效
2. 新系统提供更多能力类型和自定义选项
3. 更容易维护和扩展

## 迁移步骤 (Migration Steps)

### 选项 1: 保持现有角色不变 (Option 1: Keep Existing Characters)

如果现有角色已经满足需求：
```
不需要做任何改动！
No changes needed!
```

现有的 `CharacterAttackConfig` 继续被 `CommandInputSystem` 使用（用于输入缓冲配置）。

### 选项 2: 迁移到模块化系统 (Option 2: Migrate to Modular System)

#### 步骤 1: 创建攻击能力组件 (Create Attack Ability Components)

在Unity中创建 `AttackAbilityComponent` 资源：

**轻攻击示例:**
```
Assets/QuantumUser/Resources/Abilities/Attack/LightAttack.asset
- AbilityName: "轻攻击"
- AttackType: LightMelee
- Priority: 20
- BaseDamage: 10
- CanCombo: true
- MaxComboCount: 3
- ComboDamageMultipliers: [1.0, 1.2, 1.5]
```

**重攻击示例:**
```
Assets/QuantumUser/Resources/Abilities/Attack/HeavyAttack.asset
- AbilityName: "重攻击"
- AttackType: HeavyMelee
- Priority: 50
- BaseDamage: 25
- CanCharge: true
- MinChargeTime: 0.5
- MaxChargeTime: 2.0
- FullChargeDamageMultiplier: 2.0
```

#### 步骤 2: 创建 ModularCharacterConfig

在Unity中创建 `ModularCharacterConfig` 资源：

```
Assets/QuantumUser/Resources/Characters/Warrior_ModularConfig.asset
- CharacterId: 1
- CharacterName: "战士"
- AttackAbilities: 
  - [0]: LightAttack
  - [1]: HeavyAttack
- SpecialAbilities:
  - [0]: YourSpecialMove (if any)
```

#### 步骤 3: 更新实体原型 (Update Entity Prototype)

在角色的实体原型中：

**变更前:**
```
AttackData:
  AttackConfig: YourCharacterAttackConfig
```

**变更后:**
```
AttackData:
  AttackConfig: YourCharacterAttackConfig (保留用于输入缓冲)
  ModularConfig: Warrior_ModularConfig (新增)
```

#### 步骤 4: 测试 (Test)

在Unity中运行游戏并测试：
- ✅ 轻攻击 (LP按钮)
- ✅ 重攻击 (HP按钮 - 按住蓄力)
- ✅ 连招系统
- ✅ 特殊技能

## 使用 LegacyConfigConverter 工具 (Using LegacyConfigConverter)

系统提供了辅助转换工具：

```csharp
// 转换轻攻击配置
var lightAbility = LegacyConfigConverter.ConvertLightAttack(lightConfig);

// 转换重攻击配置
var heavyAbility = LegacyConfigConverter.ConvertHeavyAttack(heavyConfig);

// 转换特殊技能
var specialAbility = LegacyConfigConverter.ConvertSpecialMove(specialConfig);

// 完整转换角色配置
var modularConfig = LegacyConfigConverter.ConvertToModularConfig(
    legacyConfig, 
    characterId: 1, 
    characterName: "MyCharacter"
);
```

**注意:** 转换后的组件需要在Unity中保存为资源文件。

## 常见问题 (FAQ)

### Q: 我的角色突然不能攻击了？
**A:** 检查是否配置了 `ModularConfig`。如果没有，`ModularAbilitySystem` 会跳过处理。确保在 `AttackData` 组件中引用了正确的 `ModularCharacterConfig`。

### Q: 可以同时使用两种配置吗？
**A:** 可以！`AttackData` 可以同时有 `AttackConfig` 和 `ModularConfig`：
- `AttackConfig` 用于 `CommandInputSystem`（输入缓冲设置）
- `ModularConfig` 用于 `ModularAbilitySystem`（攻击执行）

### Q: 如何调试攻击不生效的问题？
**A:** 检查以下几点：
1. `ModularConfig` 是否正确引用
2. `AttackAbilities` 数组是否有元素
3. 能力的 `Priority` 是否合理
4. 检查控制台日志中的 "Modular Attack" 消息

### Q: 性能影响如何？
**A:** 性能影响微乎其微：
- 移除了一个系统，理论上性能更好
- `ModularAbilitySystem` 只在有 `ModularConfig` 时执行
- 使用了优先级排序，但列表很小

## 后续工作 (Next Steps)

1. **为新角色使用模块化系统**
   - 创建可复用的能力组件库
   - 通过组合快速创建新角色

2. **逐步迁移现有角色**
   - 不急于一次性迁移所有角色
   - 可以根据需要逐个迁移

3. **扩展能力系统**
   - 添加新的能力类型
   - 实现更复杂的能力组合

4. **清理旧代码（可选）**
   - 如果所有角色都迁移完成
   - 可以考虑移除 `CharacterAttackConfig` 相关的旧配置

## 技术细节 (Technical Details)

### 系统执行顺序 (System Execution Order)

由于Quantum的系统是并行执行的，需要确保：
- `ModularAbilitySystem` 在每帧更新
- 系统会自动处理输入和攻击逻辑
- 无需手动管理系统顺序

### 确定性保证 (Determinism Guarantee)

所有改动保持Quantum的确定性：
- 使用 `FP` 类型进行计算
- 所有配置继承自 `AssetObject`
- 无随机数或浮点运算

### 网络同步 (Network Synchronization)

- 所有攻击数据存储在 `AttackData` 组件中
- 自动网络同步
- 无需额外配置

## 参考资料 (References)

- [模块化角色系统详解](./ModularCharacterSystem.md)
- [示例角色配置](./ExampleCharacters.md)
- Quantum文档: https://doc.photonengine.com/quantum

## 支持 (Support)

如有问题，请查看：
1. 项目文档: `/Md` 目录
2. 示例资源: `Assets/QuantumUser/Resources/`
3. 源代码注释: 系统文件中的详细注释

---

**最后更新:** 2025-11-02  
**版本:** 1.0
