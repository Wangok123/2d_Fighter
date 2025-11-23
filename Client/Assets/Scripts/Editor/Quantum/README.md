# Quantum AssetObject 自定义编辑器

本目录包含了为 Quantum AssetObject 类创建的自定义编辑器，用于改善 Inspector 界面的可用性。

## 📁 文件结构

```
/Assets/Scripts/Editor/Quantum/
├── /Abilities                          # 技能相关编辑器
│   ├── ChargeAttackAbilityDataEditor.cs
│   └── JumpAbilityDataEditor.cs
├── /HitReaction                        # 击退/打击反应编辑器
│   └── KnockbackStatusEffectDataEditor.cs
├── /Projectiles                        # 弹道相关编辑器
│   ├── ProjectileDataEditor.cs         # 基类编辑器
│   ├── ArcProjectileDataEditor.cs
│   └── BoomerangProjectileDataEditor.cs
├── /SkillFields                        # 技能场相关编辑器
│   ├── DamageFieldDataEditor.cs
│   ├── PushFieldDataEditor.cs
│   ├── HealFieldDataEditor.cs
│   ├── SlowFieldDataEditor.cs
│   ├── VortexFieldDataEditor.cs
│   └── DelayedExplosionFieldDataEditor.cs
└── README.md                           # 本文件
```

## 🎯 编辑器功能说明

### HitReaction

#### KnockbackStatusEffectDataEditor
- **目标类**: `KnockbackStatusEffectData`
- **优化点**:
  - 根据 `KnockbackApplicationMode` 显示不同的字段组
    - `Physics2D`: 只显示 `KnockbackForce`
    - `CharacterController`: 显示持续时间、曲线、距离等完整设置
  - 根据 `KnockbackType` 条件显示 `FixedKnockbackDirection`

### Abilities

#### ChargeAttackAbilityDataEditor
- **目标类**: `ChargeAttackAbilityData`
- **优化点**:
  - `ScaleKnockbackWithCharge = true` 时显示击退缩放倍率
  - `ScaleAttackRangeWithCharge = true` 时显示攻击范围缩放倍率

#### JumpAbilityDataEditor
- **目标类**: `JumpAbilityData`
- **优化点**:
  - `AllowVariableHeight = true` 时显示最小跳跃高度百分比
  - 添加了提示信息说明可变跳跃高度的作用

### Projectiles

#### ProjectileDataEditor
- **目标类**: `ProjectileData` (基类)
- **优化点**:
  - `PierceTargets = true` 时显示最大穿透数量
  - 提供了 `DrawCustomInspector()` 虚方法供子类扩展

#### ArcProjectileDataEditor
- **目标类**: `ArcProjectileData`
- **优化点**:
  - `EnableGroundClamp = true` 时显示最低高度限制

#### BoomerangProjectileDataEditor
- **目标类**: `BoomerangProjectileData`
- **优化点**:
  - `UseKCC = true` 时显示 KCC 配置
  - `RotateWhileForward = true` 时显示旋转速度

### SkillFields

#### DamageFieldDataEditor
- **目标类**: `DamageFieldData`
- **优化点**:
  - `ApplyDOT = true` 时显示 DOT 持续时间和伤害
  - `ApplyKnockback = true` 时显示击退配置

#### PushFieldDataEditor
- **目标类**: `PushFieldData`
- **优化点**:
  - `Direction = CustomDirection` 时显示自定义方向向量
  - `FalloffWithDistance = true` 时显示最大影响距离

#### HealFieldDataEditor
- **目标类**: `HealFieldData`
- **优化点**:
  - `HealByPercentage = true` 时显示百分比治疗相关字段
  - `HealByPercentage = false` 时显示固定治疗量
  - `GrantShield = true` 时显示护盾相关字段

#### SlowFieldDataEditor
- **目标类**: `SlowFieldData`
- **优化点**:
  - `StackableSlows = true` 时显示叠加层数和额外减速
  - `ShowSlowEffect = true` 时显示减速特效类型

#### VortexFieldDataEditor
- **目标类**: `VortexFieldData`
- **优化点**:
  - `DealDamage = true` 时显示每 Tick 伤害
  - `StunInCore = true` 时显示眩晕持续时间

#### DelayedExplosionFieldDataEditor
- **目标类**: `DelayedExplosionFieldData`
- **优化点**:
  - `DamageFalloff = true` 时显示中心伤害倍率
  - 添加了帮助信息说明伤害衰减计算方式

## 💡 使用说明

1. 这些编辑器会自动应用到对应的 AssetObject 上
2. 在 Inspector 中选择任何对应类型的资源时，会看到优化后的界面
3. 只有相关的字段会根据条件显示，提高了可读性和易用性

## 🔧 扩展指南

如果需要为其他 AssetObject 创建自定义编辑器：

1. 在对应的子文件夹中创建新的编辑器脚本
2. 使用 `[CustomEditor(typeof(YourAssetObjectType))]` 特性
3. 在 `OnEnable()` 中查找需要的序列化属性
4. 在 `OnInspectorGUI()` 中根据条件绘制字段
5. 使用 `EditorGUI.indentLevel` 来表示层级关系
6. 使用 `EditorGUILayout.HelpBox()` 添加提示信息

## 📝 编码规范

- 使用中文标签（`new GUIContent("中文描述")`）
- 条件字段使用缩进表示从属关系
- 使用分组和空行分隔不同的设置区域
- 保持与项目的编码风格一致
