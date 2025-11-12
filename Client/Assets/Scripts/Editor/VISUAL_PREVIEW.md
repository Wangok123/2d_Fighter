# 自定义编辑器视觉效果示意 / Visual Preview of Custom Editors

## 注意 (Note)
由于环境限制，无法在实际的 Unity Editor 中截图。以下是基于代码实现的视觉效果模拟。
Due to environment limitations, actual Unity Editor screenshots cannot be taken. Below are visual simulations based on the code implementation.

---

## 1. StatusDataEditor - 角色状态配置

```
╔══════════════════════════════════════════════════════════════╗
║ 角色状态配置 (Character Status Configuration)                ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║ 生命值设置 (Health Settings)                                 ║
║   🔴 最大生命值 (Max Health)                                 ║
║   │  RawValue: [6553600          ]  ≈ 100.00                ║
║   │  角色满血时的生命值                                        ║
║   │                                                          ║
║   🔴 最小伤害值 (Minimum Damage)                             ║
║   │  RawValue: [65536            ]  ≈ 1.00                  ║
║   │  低于此值的伤害将被忽略                                    ║
║                                                              ║
║ 生命恢复设置 (Regeneration Settings)                         ║
║   🟢 恢复延迟 (Time Until Regen)                             ║
║   │  RawValue: [0                ]  ≈ 0.00s                 ║
║   │  受伤后多久开始恢复生命值（秒）                            ║
║   │                                                          ║
║   🟢 恢复速度 (Regen Rate)                                   ║
║   │  RawValue: [0                ]  ≈ 0.00                  ║
║   │  每秒恢复的生命值                                          ║
║                                                              ║
║ 重生设置 (Respawn Settings)                                  ║
║   🔵 重生时间 (Respawn Time)                                 ║
║   │  RawValue: [0                ]  ≈ 0.00s                 ║
║   │  死亡后重生所需时间（秒）                                  ║
║   │                                                          ║
║   🔵 无敌时间 (Invincible Time)                              ║
║   │  RawValue: [0                ]  ≈ 0.00s                 ║
║   │  重生后的无敌保护时间（秒）                                ║
║                                                              ║
║ 断线设置 (Disconnect Settings)                               ║
║   🟡 断线超时 (Time To Disconnect)                           ║
║   │  RawValue: [65536            ]  ≈ 1.00s                 ║
║   │  玩家断线后多久销毁角色实体（秒）                          ║
║                                                              ║
║ ℹ️ [运行时实时数值 (Runtime Values)]                          ║
║   Max Health: 100.0                                          ║
║   Respawn Time: 0.0                                          ║
║   Time Until Regen: 0.0                                      ║
║   ...                                                        ║
╚══════════════════════════════════════════════════════════════╝
```

---

## 2. GameSettingsDataEditor - 游戏设置配置

```
╔══════════════════════════════════════════════════════════════╗
║ 游戏设置配置 (Game Settings Configuration)                   ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║ 物理层设置 (Physics Layer Settings)                          ║
║   🟣 玩家层遮罩 (Player Layer Mask)                           ║
║   │  BitMask: [64                ]                          ║
║   │  定义哪些物理层会与玩家发生交互                            ║
║   │                                                          ║
║   │  已选择的层 (Selected Layers):                           ║
║   │    • Layer 6: Player                                    ║
║                                                              ║
║ ℹ️ [物理层遮罩用于控制玩家与哪些物理层的对象进行碰撞检测。]    ║
║    Layer Mask controls which physics layers the player       ║
║    can interact with.                                        ║
╚══════════════════════════════════════════════════════════════╝
```

---

## 3. AbilityDataEditor - 跳跃技能配置

```
╔══════════════════════════════════════════════════════════════╗
║ 跳跃技能配置 (Jump Ability Configuration)                    ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║ 时机设置 (Timing Settings)                                   ║
║   🟠 输入缓冲时间 (Input Buffer)                             ║
║   │  RawValue: [0                ]  ≈ 0.00s                 ║
║   │  按键输入后的容错时间（秒）                                ║
║   │                                                          ║
║   🟠 延迟时间 (Delay)                                         ║
║   │  RawValue: [0                ]  ≈ 0.00s                 ║
║   │  激活后到生效的延迟（秒）                                  ║
║   │                                                          ║
║   🟠 持续时间 (Duration)                                      ║
║   │  RawValue: [16384            ]  ≈ 0.25s                 ║
║   │  技能持续时间（秒）                                        ║
║   │                                                          ║
║   🟠 冷却时间 (Cooldown)                                      ║
║   │  RawValue: [0                ]  ≈ 0.00s                 ║
║   │  技能冷却时间（秒）                                        ║
║                                                              ║
║ 方向设置 (Direction Settings)                                ║
║   🔵 施放方向类型: [FacingDirection    ▼]                     ║
║   │  技能施放时的朝向判定方式                                  ║
║   │                                                          ║
║   🔵 面向施放方向: [✓]                                        ║
║   │  是否在施放时面向施放方向                                  ║
║   │                                                          ║
║   🔵 保持速度: [  ]                                           ║
║   │  施放时是否保持当前移动速度                                ║
║                                                              ║
║ 优先级设置 (Priority Settings)                               ║
║   🔴 优先级: [Normal              ▼]                         ║
║   │  技能的优先级等级                                          ║
║   │                                                          ║
║   🔴 可被高优先级打断: [✓]                                    ║
║   │  是否可以被更高优先级技能打断                              ║
║   │                                                          ║
║   🔴 可打断低优先级: [✓]                                      ║
║   │  是否可以打断更低优先级技能                                ║
║                                                              ║
║ 特殊属性 (Special Properties)                                ║
║   🟠 JumpImpulse                                             ║
║   │  RawValue: [655360           ]  ≈ 10.00                 ║
║   │                                                          ║
║   🟠 JumpHeightMultiplier                                    ║
║   │  RawValue: [98304            ]  ≈ 1.50                  ║
║   │                                                          ║
║   🟠 AllowVariableHeight: [✓]                                ║
║   │                                                          ║
║   🟠 MinJumpHeightPercent                                    ║
║   │  RawValue: [65536            ]  ≈ 1.00                  ║
╚══════════════════════════════════════════════════════════════╝
```

---

## 4. ProjectileDataEditor - 手榴弹弹道配置

```
╔══════════════════════════════════════════════════════════════╗
║ 手榴弹弹道配置 (Grenade Projectile Configuration)            ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║ 基础设置 (Basic Settings)                                    ║
║   🔵 生命周期 (Lifetime)                                      ║
║   │  RawValue: [196608           ]  ≈ 3.00                  ║
║   │  弹道存在的时间（秒）                                      ║
║   │                                                          ║
║   🔵 视觉原型 (Visual Prototype)                              ║
║   │  [None (EntityPrototype)     ]                          ║
║   │  弹道的视觉表现预制体                                      ║
║                                                              ║
║ 伤害设置 (Damage Settings)                                   ║
║   🔴 基础伤害 (Base Damage)                                   ║
║   │  RawValue: [655360           ]  ≈ 10.00                 ║
║   │  弹道造成的基础伤害值                                      ║
║   │                                                          ║
║   🔴 受击硬直时间 (Hitstun Duration)                          ║
║   │  RawValue: [16384            ]  ≈ 0.25                  ║
║   │  目标被击中后的硬直时间（秒）                              ║
║                                                              ║
║ 击退设置 (Knockback Settings)                                ║
║   🟠 击退力度 (Knockback Force)                               ║
║   │  RawValue: [327680           ]  ≈ 5.00                  ║
║   │  击退效果的力度大小                                        ║
║   │                                                          ║
║   🟠 击退类型 (Knockback Type)                                ║
║   │  [AwayFromSource            ▼]                          ║
║   │  击退方向的计算方式                                        ║
║                                                              ║
║ 碰撞设置 (Collision Settings)                                ║
║   🟢 碰撞形状 (Collision Shape)                               ║
║   │  [Circle (Shape2DConfig)     ]                          ║
║   │  弹道的碰撞检测形状                                        ║
║   │                                                          ║
║   🟢 碰撞层 (Collision Layer)                                 ║
║   │  BitMask: [64                ]                          ║
║   │  弹道所在的碰撞层                                          ║
║   │                                                          ║
║   🟢 穿透目标 (Pierce Targets): [  ]                          ║
║   │  是否可以穿透目标继续前进                                  ║
║                                                              ║
║ ℹ️ [击退类型说明 (Knockback Type Description):]               ║
║    • AwayFromSource: 远离弹道位置                             ║
║    • AwayFromAttacker: 远离发射者位置                         ║
║    • ProjectileDirection: 沿弹道方向                          ║
║    • Up: 向上击飞                                             ║
║    • Fixed: 使用固定方向                                      ║
╚══════════════════════════════════════════════════════════════╝
```

---

## 5. SkillFieldDataEditor - 伤害场配置

```
╔══════════════════════════════════════════════════════════════╗
║ 伤害场配置 (Damage Field Configuration)                      ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║ 基础设置 (Basic Settings)                                    ║
║   🔵 持续时间 (Duration)                                      ║
║   │  RawValue: [327680           ]  ≈ 5.00                  ║
║   │  技能场持续存在的时间（秒）                                ║
║   │                                                          ║
║   🔵 Tick间隔 (Tick Interval)                                ║
║   │  RawValue: [32768            ]  ≈ 0.50                  ║
║   │  效果触发的时间间隔（秒）                                  ║
║   │                                                          ║
║   🔵 视觉原型 (Visual Prototype)                              ║
║   │  [None (EntityPrototype)     ]                          ║
║   │  技能场的视觉表现预制体                                    ║
║                                                              ║
║ 效果范围 (Effect Range)                                      ║
║   🟢 范围形状 (Effect Area)                                   ║
║   │  [Circle (Shape2DConfig)     ]                          ║
║   │  技能场影响的区域形状                                      ║
║   │                                                          ║
║   🟢 影响层 (Target Layer)                                    ║
║   │  BitMask: [64                ]                          ║
║   │  技能场能够影响的物理层                                    ║
║   │                                                          ║
║   🟢 影响友军 (Affect Allies): [  ]                           ║
║   │  是否对友方单位产生效果                                    ║
║   │                                                          ║
║   🟢 影响敌人 (Affect Enemies): [✓]                           ║
║   │  是否对敌方单位产生效果                                    ║
║                                                              ║
║ 伤害设置 (Damage Settings)                                   ║
║   🔴 每次Tick伤害 (Damage Per Tick)                           ║
║   │  RawValue: [327680           ]  ≈ 5.00                  ║
║   │  每个Tick周期造成的伤害值                                 ║
║   │                                                          ║
║   🔴 受击硬直时间 (Hitstun Duration)                          ║
║   │  RawValue: [6554             ]  ≈ 0.10                  ║
║   │  目标被击中后的硬直时间（秒）                              ║
║                                                              ║
║ 击退设置 (Knockback Settings)                                ║
║   🟠 击退力度 (Knockback Force)                               ║
║   │  RawValue: [196608           ]  ≈ 3.00                  ║
║   │  击退效果的力度大小                                        ║
║   │                                                          ║
║   🟠 击退方向 (Knockback Direction)                           ║
║   │  X: [0                       ]                          ║
║   │  Y: [65536                   ]                          ║
║   │  预览: (0.00, 1.00)                                      ║
║   │  目标被击退的方向向量                                      ║
║                                                              ║
║ ℹ️ [技能场说明 (Skill Field Description):]                    ║
║    技能场是一个持续存在的区域效果，会在指定的Tick间隔内        ║
║    对进入范围的目标产生影响。                                  ║
║                                                              ║
║    • Duration: 技能场存在的总时间                             ║
║    • TickInterval: 效果触发的频率（如每0.5秒触发一次）         ║
║    • DamagePerTick: 每次触发时造成的伤害                      ║
║                                                              ║
║    可以通过 AffectAllies 和 AffectEnemies 控制影响的          ║
║    目标类型。                                                 ║
╚══════════════════════════════════════════════════════════════╝
```

---

## 关键视觉元素说明 (Key Visual Elements)

### 1. 颜色编码 (Color Coding)
```
🔴 红色 = 伤害、生命值相关
🟢 绿色 = 移动、恢复、效果范围
🔵 蓝色 = 基础设置、核心配置
🟠 橙色 = 时机、击退相关
🟡 黄色 = 特殊状态、断线设置
🟣 紫色 = UI、物理层、额外功能
```

### 2. 实时数值预览 (Real-time Value Preview)
```
RawValue: [6553600] ≈ 100.00
          ^^^^^^^^   ^^^^^^^^^
          原始值      实际数值
```

### 3. 分组结构 (Section Structure)
```
分组标题 (Section Title)
  字段名称 (Field Name)
  │  [输入框或下拉框]  ≈ 预览值
  │  工具提示说明
  │
  下一个字段...
```

### 4. 信息框 (Info Boxes)
```
ℹ️ [标题]
   详细说明文本
   可以多行显示
```

### 5. 列表显示 (List Display)
```
已选择的层 (Selected Layers):
  • Layer 6: Player
  • Layer 7: Enemy
```

---

## 对比效果 (Comparison)

### 原始 Unity Inspector (Before)
- 单调的灰白色界面
- 所有字段平铺无分组
- 原始数值难以理解
- 纯英文显示
- 缺少说明信息

### 自定义编辑器 (After)
- 彩色的分组标题
- 有逻辑的组织结构
- 实时数值预览
- 中英文双语
- 丰富的提示和说明

---

**注**: 实际的 Unity Inspector 效果会更加美观，包含 Unity 原生的 GUI 样式和交互效果。
**Note**: The actual Unity Inspector will look even better with Unity's native GUI styles and interactive features.
