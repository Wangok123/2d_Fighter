# Modular Character System (ECS-Style Ability Composition)

## 概述 (Overview)

本系统实现了类似守望先锋（Overwatch）在GDC演讲中介绍的ECS风格角色制作流程。通过将角色能力拆分为可复用的组件，可以大大简化新角色的创建过程。

This system implements an ECS-style character creation workflow similar to Overwatch's approach presented at GDC. By splitting character abilities into reusable components, creating new characters becomes much simpler.

## 核心理念 (Core Concepts)

### 守望先锋的方法 (Overwatch's Approach)
在GDC演讲中，暴雪团队分享了他们的角色制作流程：
- **第一个英雄**：需要创建大量功能组件（移动、攻击、防御等）
- **第二个英雄**：复用一些组件，创建少量新组件
- **后续英雄**：主要通过组合现有组件，很少需要新组件

At GDC, the Overwatch team shared their hero creation workflow:
- **First Hero**: Requires creating many functional components (movement, attacks, defense, etc.)
- **Second Hero**: Reuse some components, create a few new ones
- **Subsequent Heroes**: Mostly compose from existing components, rarely need new ones

### 我们的实现 (Our Implementation)
本系统将上述理念适配到Quantum引擎的确定性框架中：

This system adapts the above concept to Quantum's deterministic framework:

```
角色 (Character) = 组件集合 (Set of Components)

组件类型 (Component Types):
├── MovementAbilityComponent (移动能力)
├── AttackAbilityComponent (攻击能力)
├── DefenseAbilityComponent (防御能力)
└── SpecialAbilityComponent (特殊能力)
```

## 组件系统 (Component System)

### 1. AbilityComponentBase (能力组件基类)
所有能力组件的基类，定义了共同属性：
- AbilityId: 唯一标识符
- AbilityName: 显示名称
- Priority: 优先级（数值越高优先级越高）
- RequiredLevel: 需求等级
- Cooldown: 冷却时间
- EnergyCost: 能量消耗

All ability components inherit from this base class with common properties.

### 2. MovementAbilityComponent (移动能力组件)
定义角色的移动能力：

Defines character movement capabilities:

**能力类型 (Ability Types):**
- Walk: 基础移动
- Dash: 冲刺
- DoubleJump: 二段跳
- AirDash: 空中冲刺
- WallJump: 蹬墙跳
- Teleport: 瞬移
- Roll: 翻滚（带无敌帧）
- Slide: 滑行
- Glide: 滑翔
- Sprint: 疾跑

**配置参数 (Parameters):**
- SpeedMultiplier: 移动速度倍率
- CanUseInAir: 是否可空中使用
- MaxAirUses: 最大空中使用次数
- MovementDistance: 移动距离（用于冲刺等）
- MovementDuration: 持续时间
- GrantsInvincibility: 是否提供无敌帧

### 3. AttackAbilityComponent (攻击能力组件)
定义角色的攻击能力，替代原有的独立攻击配置：

Defines character attack capabilities, replacing individual attack configs:

**能力类型 (Ability Types):**
- LightMelee: 轻攻击
- HeavyMelee: 重攻击
- Projectile: 弹道攻击
- AreaOfEffect: 范围攻击
- Grab: 抓取/投掷
- ChargedShot: 蓄力射击
- RapidFire: 连射
- Uppercut: 上挑
- GroundPound: 下砸
- Counter: 反击

**配置参数 (Parameters):**
- BaseDamage: 基础伤害
- DamagePerLevel: 每级伤害加成
- AttackRange: 攻击范围
- StartupTime/ActiveTime/RecoveryTime: 攻击帧数据
- CanCombo: 是否可连击
- MaxComboCount: 最大连击数
- ComboDamageMultipliers: 连击伤害倍率
- CanCharge: 是否可蓄力
- MinChargeTime/MaxChargeTime: 蓄力时间范围
- IsProjectile: 是否为弹道
- ProjectileSpeed/ProjectileLifetime: 弹道参数

### 4. DefenseAbilityComponent (防御能力组件)
定义角色的防御能力：

Defines character defensive capabilities:

**能力类型 (Ability Types):**
- Block: 格挡
- Parry: 弹反
- Dodge: 闪避
- Shield: 护盾
- Counter: 反击
- Reflect: 反弹
- Invincibility: 无敌
- Armor: 护甲
- Barrier: 屏障
- Teleport: 闪现

**配置参数 (Parameters):**
- DamageReduction: 伤害减免
- DefenseDuration: 防御持续时间
- HasPerfectWindow: 是否有完美防御窗口
- PerfectWindowDuration: 完美防御窗口时长
- CanCounter: 是否可以反击
- CounterDamageMultiplier: 反击伤害倍率

### 5. SpecialAbilityComponent (特殊能力组件)
定义角色的特殊/大招能力：

Defines character special/ultimate abilities:

**能力类型 (Ability Types):**
- Ultimate: 终极技能
- Transformation: 变身
- Summon: 召唤
- AreaDamage: 大范围伤害
- Healing: 治疗
- Buff: 增益
- Debuff: 减益
- TimeManipulation: 时间操控
- Deployable: 可部署物
- CommandGrab: 指令投
- ScreenClear: 清屏
- Resurrection: 复活

**配置参数 (Parameters):**
- InputSequence: 输入序列（用于格斗游戏式指令）
- Damage: 伤害
- HealingAmount: 治疗量
- EffectDuration: 效果持续时间
- IsUltimate: 是否为大招
- RequiredCharge: 需要的充能值
- IsTransformation: 是否为变身技能
- IsSummon: 是否为召唤技能

## ModularCharacterConfig (模块化角色配置)

这是新的角色配置容器，替代了原有的 `CharacterAttackConfig`：

This is the new character configuration container, replacing the old `CharacterAttackConfig`:

```csharp
public class ModularCharacterConfig : AssetObject
{
    // 角色身份 (Character Identity)
    public int CharacterId;
    public string CharacterName;
    public string Description;
    
    // 能力组合 (Ability Composition)
    public AssetRef<MovementAbilityComponent>[] MovementAbilities;
    public AssetRef<AttackAbilityComponent>[] AttackAbilities;
    public AssetRef<DefenseAbilityComponent>[] DefenseAbilities;
    public AssetRef<SpecialAbilityComponent>[] SpecialAbilities;
    
    // 被动特性 (Passive Traits)
    public PassiveTraits PassiveTraits;
    
    // 升级解锁 (Ability Unlocks)
    public AbilityUnlock[] AbilityUnlocks;
    
    // 向后兼容 (Backward Compatibility)
    public AssetRef<CharacterAttackConfig> LegacyAttackConfig;
}
```

## 使用工作流 (Usage Workflow)

### 创建第一个角色 (Creating First Character)

1. **创建基础能力组件 (Create Base Ability Components)**
   ```
   创建资源 (Create Assets):
   - Walk_Movement (基础移动)
   - DoubleJump_Movement (二段跳)
   - LightPunch_Attack (轻拳)
   - HeavyPunch_Attack (重拳)
   - Block_Defense (格挡)
   - Fireball_Special (火球术)
   ```

2. **组合角色 (Compose Character)**
   ```
   创建 ModularCharacterConfig:
   - CharacterId: 1
   - CharacterName: "战士 (Warrior)"
   - MovementAbilities: [Walk_Movement, DoubleJump_Movement]
   - AttackAbilities: [LightPunch_Attack, HeavyPunch_Attack]
   - DefenseAbilities: [Block_Defense]
   - SpecialAbilities: [Fireball_Special]
   ```

### 创建第二个角色 (Creating Second Character)

1. **复用现有组件 (Reuse Existing Components)**
   ```
   复用 (Reuse):
   - Walk_Movement (基础移动 - 已存在)
   - Block_Defense (格挡 - 已存在)
   ```

2. **创建新组件 (Create New Components)**
   ```
   新建 (Create New):
   - Dash_Movement (冲刺 - 新)
   - LightKick_Attack (轻腿 - 新)
   - Shuriken_Special (手里剑 - 新)
   ```

3. **组合新角色 (Compose New Character)**
   ```
   创建 ModularCharacterConfig:
   - CharacterId: 2
   - CharacterName: "忍者 (Ninja)"
   - MovementAbilities: [Walk_Movement (复用), Dash_Movement (新)]
   - AttackAbilities: [LightKick_Attack (新)]
   - DefenseAbilities: [Block_Defense (复用)]
   - SpecialAbilities: [Shuriken_Special (新)]
   ```

### 创建第三个及后续角色 (Creating Third+ Characters)

大部分情况下只需要组合现有组件！

In most cases, just compose existing components!

```
创建 ModularCharacterConfig:
- CharacterId: 3
- CharacterName: "法师 (Mage)"
- MovementAbilities: [Walk_Movement (复用), Teleport_Movement (新)]
- AttackAbilities: [MagicBolt_Attack (新)]
- DefenseAbilities: [Barrier_Defense (新)]
- SpecialAbilities: [Fireball_Special (复用!), Meteor_Special (新)]
```

## 优势 (Advantages)

### 1. 组件复用 (Component Reuse)
- 基础移动能力可以被多个角色使用
- 通用攻击模式可以跨角色共享
- 减少重复代码

Basic movement abilities can be used by multiple characters, reducing duplicate code.

### 2. 快速迭代 (Fast Iteration)
- 创建新角色主要是"拼接"工作
- 不需要写新代码（除非需要全新的能力类型）
- 设计师可以独立创建角色

Creating new characters is mainly "assembly" work without writing new code (unless completely new ability types are needed).

### 3. 易于平衡 (Easy Balancing)
- 修改一个能力组件影响所有使用它的角色
- 或者为特定角色创建该能力的变体
- 清晰的参数调整点

Modifying an ability component affects all characters using it, or create variants for specific characters.

### 4. 向后兼容 (Backward Compatible)
- 保留了 `LegacyAttackConfig` 字段
- 现有角色继续工作
- 可以逐步迁移

Existing characters continue working, gradual migration possible.

### 5. 符合Quantum (Quantum Compatible)
- 所有组件继承自 `AssetObject`
- 使用 `FP` 类型保持确定性
- 与 Quantum 的 ECS 架构协调工作

All components inherit from `AssetObject` and use `FP` types for determinism.

## 系统架构 (System Architecture)

```
ModularAbilitySystem (新系统 New System)
├── 读取 ModularCharacterConfig
├── 收集所有能力组件
├── 按优先级排序
├── 检查输入匹配
├── 检查解锁状态
└── 执行能力

NormalAttackSystem (现有系统 Existing System - 仍然工作)
├── 读取 CharacterAttackConfig
└── 处理传统攻击逻辑
```

两个系统可以共存，逐步迁移！

Both systems coexist, allowing gradual migration!

## 扩展性 (Extensibility)

### 添加新的能力类型 (Adding New Ability Types)

如果需要全新的能力类别：

If you need a completely new ability category:

1. 创建新的组件类 (Create new component class)
   ```csharp
   public class UtilityAbilityComponent : AbilityComponentBase
   {
       // 例如: 治疗、侦察、传送点等
       // e.g., healing, scouting, teleport points, etc.
   }
   ```

2. 添加到 ModularCharacterConfig
   ```csharp
   public AssetRef<UtilityAbilityComponent>[] UtilityAbilities;
   ```

3. 在 ModularAbilitySystem 中处理
   ```csharp
   if (config.UtilityAbilities != null)
   {
       // Process utility abilities
   }
   ```

### 添加新的能力实例 (Adding New Ability Instances)

只需在Unity编辑器中创建新的ScriptableObject资源！

Simply create new ScriptableObject assets in Unity editor!

## 迁移指南 (Migration Guide)

### 从旧系统迁移 (Migrating from Old System)

1. **保持现有配置工作 (Keep Existing Configs Working)**
   - 不要删除 `CharacterAttackConfig`
   - 继续使用 `NormalAttackSystem`

2. **为新角色使用新系统 (Use New System for New Characters)**
   - 创建 `ModularCharacterConfig`
   - 组合能力组件
   - 启用 `ModularAbilitySystem`

3. **逐步迁移现有角色 (Gradually Migrate Existing Characters)**
   - 将攻击配置转换为攻击能力组件
   - 创建对应的 `ModularCharacterConfig`
   - 测试验证
   - 移除旧配置

## 最佳实践 (Best Practices)

1. **命名约定 (Naming Convention)**
   ```
   [能力描述]_[类型]
   Walk_Movement
   LightPunch_Attack
   Fireball_Special
   ```

2. **组件粒度 (Component Granularity)**
   - 保持能力组件专注单一功能
   - 通过组合实现复杂行为
   - 避免"万能"组件

3. **优先级管理 (Priority Management)**
   ```
   Special Abilities: 100+
   Heavy Attacks: 50-99
   Defense: 30-49
   Light Attacks: 10-29
   Movement: 0-9
   ```

4. **文档化 (Documentation)**
   - 为每个能力组件写清楚的描述
   - 记录参数的作用
   - 标注哪些角色使用了该组件

## 示例配置 (Example Configurations)

详见项目中的示例资源：
See example assets in project:

```
Assets/QuantumUser/Resources/Characters/Examples/
├── Warrior_ModularConfig.asset
├── Ninja_ModularConfig.asset
└── Mage_ModularConfig.asset

Assets/QuantumUser/Resources/Abilities/Movement/
├── Walk_Movement.asset
├── Dash_Movement.asset
└── DoubleJump_Movement.asset

Assets/QuantumUser/Resources/Abilities/Attack/
├── LightPunch_Attack.asset
├── HeavyPunch_Attack.asset
└── Fireball_Attack.asset
```

## 总结 (Summary)

本系统成功将守望先锋的ECS风格工作流适配到了Quantum引擎：
- ✅ 第一个角色需要创建多个组件
- ✅ 后续角色主要通过组合现有组件
- ✅ 大大减少了新角色的开发时间
- ✅ 保持了Quantum的确定性要求
- ✅ 与现有系统兼容

This system successfully adapts Overwatch's ECS-style workflow to the Quantum engine, greatly reducing development time for new characters while maintaining compatibility with existing systems.
