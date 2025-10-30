# 战斗系统和技能解锁功能

## 新增功能概述

本次更新为2D平台动作游戏添加了以下功能：

### 1. 攻击系统
- **轻攻击 (Light Attack)**: 按下 LP 键触发
  - 具有3段连击效果
  - 每段连击伤害递增: 1x → 1.25x → 1.5x
  - 连击窗口时间可配置 (默认1秒)
  - 攻击冷却时间可配置 (默认0.25秒)

- **重攻击 (Heavy Attack)**: 按下 HP 键触发
  - 单次高伤害攻击
  - 会重置连击计数
  - 攻击冷却时间可配置 (默认0.5秒)

### 2. 等级系统
- 角色等级追踪 (CharacterLevel组件)
- 支持手动升级或通过游戏逻辑自动升级

### 3. 技能解锁系统
- **二段跳 (Double Jump)**: 5级解锁
- **冲刺 (Dash)**: 5级解锁
- 低于等级要求时，相应技能会被禁用

## 组件说明

### CharacterLevel
```
component CharacterLevel
{
    Int32 CurrentLevel;  // 当前等级
}
```

### AttackData
```
component AttackData
{
    asset_ref<AttackConfig> AttackConfig;  // 攻击配置资产引用
    Int32 ComboCounter;                     // 连击计数器
    FrameTimer ComboResetTimer;             // 连击重置计时器
    FrameTimer AttackCooldown;              // 攻击冷却计时器
    bool IsAttacking;                       // 是否正在攻击
}
```

## 资产配置

### AttackConfig
在Unity编辑器中创建 AttackConfig 资产来配置攻击参数：

**轻攻击设置**
- `LightAttackDamage`: 轻攻击基础伤害 (默认: 10)
- `LightAttackCooldown`: 轻攻击冷却时间 (默认: 0.25秒)
- `ComboWindow`: 连击窗口时间 (默认: 1秒)
- `MaxComboCount`: 最大连击次数 (默认: 3)
- `ComboDamageMultipliers`: 连击伤害倍率数组 (默认: [1.0, 1.25, 1.5])

**重攻击设置**
- `HeavyAttackDamage`: 重攻击伤害 (默认: 25)
- `HeavyAttackCooldown`: 重攻击冷却时间 (默认: 0.5秒)

**解锁设置**
- `DoubleJumpUnlockLevel`: 二段跳解锁等级 (默认: 5)
- `DashUnlockLevel`: 冲刺解锁等级 (默认: 5)

## 使用方法

### 设置角色
1. 在角色实体原型(Entity Prototype)上添加以下组件：
   - `CharacterLevel` - 设置初始等级
   - `AttackData` - 配置 AttackConfig 资产引用

### 升级角色
使用以下方法手动升级角色：

```csharp
// 升级指定等级数
LevelUpSystem.LevelUpCharacter(frame, entity, levelsToAdd: 1);

// 直接设置等级
LevelUpSystem.SetCharacterLevel(frame, entity, newLevel: 5);
```

### 监听事件

**LevelUp 事件**
```csharp
public void OnEventLevelUp(EventLevelUp evt)
{
    Log.Info($"角色升级! 旧等级: {evt.OldLevel}, 新等级: {evt.NewLevel}");
}
```

**AttackPerformed 事件**
```csharp
public void OnEventAttackPerformed(EventAttackPerformed evt)
{
    if (evt.IsHeavyAttack)
    {
        Log.Info($"重攻击! 伤害: {evt.Damage}");
    }
    else
    {
        Log.Info($"轻攻击! 连击: {evt.ComboCount}, 伤害: {evt.Damage}");
    }
}
```

## 系统说明

### AbilitySystem
处理攻击输入和连击逻辑：
- 监听 LP/HP 按键输入
- 管理连击计数器
- 计算连击伤害
- 触发攻击事件

### MovementSystem
处理技能解锁过滤：
- 检查角色等级
- 根据等级禁用未解锁技能
- 过滤冲刺输入
- 动态修改二段跳设置

### LevelUpSystem
管理角色等级：
- 提供升级API
- 触发升级事件
- 记录等级变化

## 测试建议

1. **测试轻攻击连击**
   - 连续按LP键3次，观察伤害递增
   - 等待1秒后按LP，连击应重置

2. **测试重攻击**
   - 按HP键触发重攻击
   - 确认连击被重置

3. **测试技能解锁**
   - 低等级 (< 5): 按冲刺/跳跃键，技能应被禁用
   - 5级及以上: 冲刺和二段跳应正常工作

4. **测试升级**
   - 使用 LevelUpSystem 升级角色
   - 验证升级事件触发
   - 确认技能在5级时解锁

## 注意事项

- 所有战斗逻辑在 Quantum 确定性框架中运行
- 伤害计算仅在模拟层完成，未实现实际的碰撞检测和伤害应用
- 需要在实际游戏中添加攻击命中检测和伤害应用逻辑
- 建议在角色生成时初始化 CharacterLevel 和 AttackData 组件
