# Quantum动画系统快速参考
# Quantum Animation System Quick Reference

## 文件结构 / File Structure

```
Client/Assets/QuantumUser/
├── Simulation/Core/
│   ├── DSL/
│   │   └── AnimationComponents.qtn          # 组件定义
│   ├── Systems/
│   │   ├── AnimationSystem.cs               # 动画状态管理
│   │   ├── ThrowSystem.cs                   # 投技系统
│   │   ├── HitReactionSystem.cs             # 受击系统
│   │   ├── ComboSystem.cs                   # 连招系统
│   │   └── CombatInputSystem.cs             # 战斗输入
│   └── AnimationConfigData.cs               # 配置资源
└── View/
    └── QuantumAnimationView.cs              # Unity表现层
```

## 核心API快速参考 / Core API Quick Reference

### AnimationSystem

```csharp
// 尝试播放动画（会检查取消规则）
bool success = AnimationSystem.TryPlayAnimation(frame, entity, stateHash);

// 检查是否可以播放
bool canPlay = AnimationSystem.CanPlayAnimation(frame, entity, stateHash);

// 强制播放（忽略所有规则）
AnimationSystem.ForcePlayAnimation(frame, entity, stateHash);
```

### ThrowSystem

```csharp
// 尝试投技
bool success = ThrowSystem.TryThrow(frame, attacker, grabRange, grabAngle);

// 执行投技（造成伤害）
ThrowSystem.ExecuteThrow(frame, attacker, damage, knockbackVelocity);

// 完成投技
ThrowSystem.CompleteThrow(frame, attacker);

// 破解投技
bool broken = ThrowSystem.TryBreakThrow(frame, attacker, breakWindow);
```

### HitReactionSystem

```csharp
// 应用受击（智能选择地面/空中）
HitReactionSystem.ApplyHit(frame, target, damage, knockback, hitStunDuration, isGrounded);

// 应用击倒
HitReactionSystem.ApplyKnockdown(frame, target, damage, knockback);
```

### ComboSystem

```csharp
// 尝试攻击（会管理连招）
bool success = ComboSystem.TryAttack(frame, entity, attackHash);

// 获取当前连招数
int combo = ComboSystem.GetComboCount(frame, entity);

// 重置连招
ComboSystem.ResetCombo(frame, entity);
```

## 组件数据结构 / Component Data Structures

### AnimationState
```csharp
component AnimationState
{
    Int32 CurrentStateHash;      // 当前动画哈希
    Int32 Priority;              // 优先级
    FPAnimationTime StartTime;   // 开始时间
    Boolean IsTransitioning;     // 是否在过渡
    FP TransitionDuration;       // 过渡时间
}
```

### ThrowState
```csharp
component ThrowState
{
    Int32 State;                 // 投技状态
    EntityRef GrabbedTarget;     // 被抓取的目标
    FPAnimationTime ThrowStartTime;
    Boolean IsExecuting;
}
```

### HitReaction
```csharp
component HitReaction
{
    Boolean IsGrounded;          // 是否在地面
    FrameTimer HitStunTimer;     // 硬直计时器
    FPVector2 KnockbackVelocity; // 击退速度
}
```

### ComboData
```csharp
component ComboData
{
    Int32 ComboCount;            // 连招计数
    FrameTimer ComboWindowTimer; // 连招窗口
    Int32 LastAttackHash;        // 上次攻击
}
```

## 配置示例 / Configuration Example

### AnimationConfigData 配置

```csharp
States = new AnimationStateData[]
{
    // Idle - 最低优先级，随时可取消
    new AnimationStateData
    {
        StateName = "Idle",
        StateHash = Animator.StringToHash("Idle"),
        Priority = 0,
        CancelPolicy = 1, // AlwaysCancellable
        CrossfadeDuration = FP._0_10
    },
    
    // Attack - 中等优先级，窗口内可取消（连招）
    new AnimationStateData
    {
        StateName = "Attack",
        StateHash = Animator.StringToHash("Attack"),
        Priority = 30,
        CancelPolicy = 2, // CancellableInWindow
        CrossfadeDuration = FP._0_05,
        CancelWindows = new CancelWindowData[]
        {
            new CancelWindowData
            {
                StartTime = FP._0_40,  // 40%
                EndTime = FP._0_70,    // 70%
                AllowedTargetHashes = new int[] 
                { 
                    Animator.StringToHash("Attack"),
                    Animator.StringToHash("Skill") 
                }
            }
        }
    },
    
    // Throw - 高优先级，起手阶段可被打断
    new AnimationStateData
    {
        StateName = "Throw_Start",
        StateHash = Animator.StringToHash("Throw_Start"),
        Priority = 50,
        CancelPolicy = 2, // CancellableInWindow
        CrossfadeDuration = FP._0_05,
        CancelWindows = new CancelWindowData[]
        {
            new CancelWindowData
            {
                StartTime = FP._0,     // 0%
                EndTime = FP._0_20,    // 20%
                AllowedTargetHashes = null  // 允许任何打断
            }
        }
    },
    
    // Hit - 很高优先级，只能被更高优先级打断
    new AnimationStateData
    {
        StateName = "Hurt_Ground",
        StateHash = Animator.StringToHash("Hurt_Ground"),
        Priority = 60,
        CancelPolicy = 4, // OnlyByHigherPriority
        CrossfadeDuration = FP._0_05
    },
    
    // Death - 最高优先级，不可取消
    new AnimationStateData
    {
        StateName = "Death",
        StateHash = Animator.StringToHash("Death"),
        Priority = 100,
        CancelPolicy = 0, // NonCancellable
        CrossfadeDuration = FP._0_05
    }
};

DefaultStateHash = Animator.StringToHash("Idle");
```

## Unity设置步骤 / Unity Setup Steps

### 1. 创建AnimationConfigData资源
1. Right Click in Project > Create > Quantum > Asset Object
2. 选择 AnimationConfigData
3. 配置所有动画状态

### 2. 设置Entity Prototype
1. 在Quantum Entity Prototype上添加组件：
   - AnimationState
   - AnimationConfig（引用上面的资源）
   - ThrowState
   - HitReaction
   - ComboData

### 3. 添加View组件
1. 在角色GameObject上添加 QuantumAnimationView
2. 引用Animator组件
3. 勾选Show Debug Info（可选）

## 常用模式 / Common Patterns

### 模式1：连招攻击
```csharp
public override void Update(Frame frame, ref Filter filter)
{
    var input = frame.GetPlayerInput(filter.PlayerLink->Player);
    
    if (input->Jump.WasPressed)
    {
        bool success = ComboSystem.TryAttack(frame, filter.Entity, ATTACK_HASH);
        if (success)
        {
            int combo = ComboSystem.GetComboCount(frame, filter.Entity);
            // 连招成功！
        }
    }
}
```

### 模式2：投技 + 破解
```csharp
// 攻击方
if (throwInput)
{
    ThrowSystem.TryThrow(frame, attacker, FP._1_50, 45);
}

// 防守方
if (breakInput)
{
    ThrowSystem.TryBreakThrow(frame, attacker, FP._0_30);
}
```

### 模式3：受击反应
```csharp
// 检测碰撞后
bool isGrounded = filter.KCC->Grounded;
HitReactionSystem.ApplyHit(
    frame, 
    target, 
    damage: 10, 
    knockback: new FPVector2(2, 1),
    hitStunDuration: FP._0_30,
    isGrounded: isGrounded
);
```

## 优先级参考 / Priority Reference

```
0   - Idle           (空闲)
10  - Movement       (移动)
20  - Jump           (跳跃)
30  - Attack         (攻击)
40  - Skill          (技能)
50  - Throw          (投技)
60  - Hit            (受击)
70  - Knockdown      (击倒)
100 - Death          (死亡)
```

## 取消策略参考 / Cancel Policy Reference

```
0 - NonCancellable        (完全不可取消)
1 - AlwaysCancellable     (随时可取消)
2 - CancellableInWindow   (窗口内可取消)
3 - CancellableOnEnd      (接近结束可取消)
4 - OnlyByHigherPriority  (仅高优先级可打断)
```

## 调试命令 / Debug Commands

在QuantumAnimationView中启用DebugInfo后，场景中会显示：
- State Hash: 当前动画哈希
- Priority: 优先级
- Normalized Time: 归一化时间
- Transitioning: 是否在过渡

在Quantum Debugger中可以查看：
- 所有组件的实时值
- System的执行情况
- 帧回放和预测

## 性能提示 / Performance Tips

1. **缓存哈希值** - 使用static readonly int存储
2. **减少TryGetPointer** - 使用System Filter
3. **限制取消窗口数量** - 建议≤3个
4. **批量处理** - 在同一帧内处理多个实体

## 下一步 / Next Steps

1. 在Unity中创建AnimationConfigData资源
2. 配置所有角色的动画状态
3. 在Entity Prototype上添加必要组件
4. 在角色上添加QuantumAnimationView
5. 在CombatInputSystem中实现游戏逻辑
6. 测试网络同步效果

---

更详细的文档请参考：`Md/Core/Quantum动画系统使用指南.md`
