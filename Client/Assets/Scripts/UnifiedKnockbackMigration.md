# 统一击退系统迁移完成报告

## 已完成的修改

### 1. 统一击退应用接口

已将以下9个文件中的击退应用逻辑统一为使用 `OnKnockbackApplied` Signal：

#### 数据层（Assets）
- ✅ `/Assets/QuantumUser/Simulation/Core/Assets/SkillField/SkillFieldData.cs`
- ✅ `/Assets/QuantumUser/Simulation/Core/Assets/SkillField/PushFieldData.cs`
- ✅ `/Assets/QuantumUser/Simulation/Core/Assets/SkillField/VortexFieldData.cs`
- ✅ `/Assets/QuantumUser/Simulation/Core/Assets/Skill/SkillData.cs`

#### 系统层（Systems）
- ✅ `/Assets/QuantumUser/Simulation/Core/Systems/AttackSystem.cs`
- ✅ `/Assets/QuantumUser/Simulation/Core/Systems/CommandInputSystem.cs`
- ✅ `/Assets/QuantumUser/Simulation/Core/Systems/ProjectileSystem.cs`
- ✅ `/Assets/QuantumUser/Simulation/Core/Systems/SkillFieldSystem.cs`
- ✅ `/Assets/QuantumUser/Simulation/Core/Systems/SkillSystem.cs`

### 2. 修改内容

**之前（多种模式，耦合运动控制器）：**
```csharp
private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData, 
    FPVector2 knockbackDirection, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
{
    if (frame.Has<PhysicsBody2D>(target))
    {
        FPVector2 knockbackVelocity = knockbackDirection * knockbackData.KnockbackForce;
        frame.Signals.OnKnockbackPhysic2DApplied(target, knockbackVelocity);
        return;
    }

    if (frame.Has<CharacterController2D>(target))
    {
        frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection, knockbackDataRef);
        return;
    }

#if DEBUG || UNITY_EDITOR
    UnityEngine.Debug.LogWarning($"Target entity has no supported controller");
#endif
}
```

**之后（统一接口，解耦运动控制器）：**
```csharp
private void ApplyKnockbackToTarget(Frame frame, EntityRef target, KnockbackStatusEffectData knockbackData, 
    FPVector2 knockbackDirection, AssetRef<KnockbackStatusEffectData> knockbackDataRef)
{
    frame.Signals.OnKnockbackApplied(target, knockbackData.KnockBackDuration, knockbackDirection, knockbackDataRef);
}
```

## 优势对比

### 代码简化
- **之前**: 每个文件约20行击退应用代码
- **之后**: 每个文件仅3行击退应用代码
- **减少代码量**: 约153行（9个文件 × 17行）

### 维护性提升
- **集中化**: 所有击退逻辑集中在 `UnifiedKnockbackSystem` 中
- **单一职责**: 击退应用点只负责发送信号，不关心底层实现
- **易于调试**: 击退问题只需在一个地方排查

### 扩展性增强
- **新控制器支持**: 只需在 `UnifiedKnockbackSystem` 中添加，无需修改9个文件
- **行为定制**: 可以轻松为不同控制器定制不同的击退行为
- **向后兼容**: 所有现有代码无需修改即可工作

## 下一步操作

### 必须完成的步骤

#### 1. 创建统一击退系统
创建文件：`/Assets/Scripts/UnifiedKnockbackSystem.cs`

```csharp
using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class UnifiedKnockbackSystem : SystemMainThreadFilter<UnifiedKnockbackSystem.Filter>, ISignalOnKnockbackApplied
    {
        public struct Filter
        {
            public EntityRef EntityRef;
            public Transform2D* Transform;
            public KnockbackComponent* Knockback;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.Knockback->StatusEffect.DurationTimer.IsRunning)
            {
                return;
            }

            FPVector2 lastRelativePosition = GetKnockbackRelativePosition(frame, filter.Knockback);
            filter.Knockback->StatusEffect.DurationTimer.Tick(frame.DeltaTime);
            FPVector2 newRelativePosition = GetKnockbackRelativePosition(frame, filter.Knockback);

            FPVector2 knockbackMovement = newRelativePosition - lastRelativePosition;
            FPVector2 knockbackVelocity = knockbackMovement / frame.DeltaTime;

            filter.Knockback->StatusEffect.KnockbackVelocity = knockbackVelocity;

            ApplyKnockbackByControllerType(frame, filter.EntityRef, filter.Transform, knockbackMovement, knockbackVelocity);

            if (!filter.Knockback->StatusEffect.DurationTimer.IsRunning)
            {
                OnKnockbackEnd(frame, filter.EntityRef);
            }
        }

        public void OnKnockbackApplied(Frame frame, EntityRef target, FP duration, FPVector2 direction, AssetRef<KnockbackStatusEffectData> statusEffectData)
        {
            if (!frame.Has<KnockbackComponent>(target))
            {
                frame.Add<KnockbackComponent>(target);
            }

            KnockbackComponent* knockback = frame.Unsafe.GetPointer<KnockbackComponent>(target);
            
            knockback->StatusEffect.DurationTimer.Start(duration);
            knockback->StatusEffect.KnockbackDirection = direction.Normalized;
            knockback->StatusEffect.StatusEffectData = statusEffectData;
            knockback->StatusEffect.KnockbackVelocity = FPVector2.Zero;

            DetermineApplicationMode(frame, target, knockback);
        }

        private void DetermineApplicationMode(Frame frame, EntityRef target, KnockbackComponent* knockback)
        {
            if (frame.Has<KCC2D>(target))
            {
                knockback->ApplicationMode = KnockbackApplicationMode.KCC2D;
            }
            else if (frame.Has<CharacterController2D>(target))
            {
                knockback->ApplicationMode = KnockbackApplicationMode.CharacterController;
            }
            else if (frame.Has<PhysicsBody2D>(target))
            {
                knockback->ApplicationMode = KnockbackApplicationMode.Physics2D;
            }
        }

        private void ApplyKnockbackByControllerType(Frame frame, EntityRef entity, Transform2D* transform, 
            FPVector2 movement, FPVector2 velocity)
        {
            KnockbackComponent* knockback = frame.Unsafe.GetPointer<KnockbackComponent>(entity);

            switch (knockback->ApplicationMode)
            {
                case KnockbackApplicationMode.KCC2D:
                    ApplyToKCC2D(frame, entity, velocity);
                    break;

                case KnockbackApplicationMode.CharacterController:
                    ApplyToCharacterController(frame, entity, transform, movement, velocity);
                    break;

                case KnockbackApplicationMode.Physics2D:
                    ApplyToPhysics2D(frame, entity, velocity);
                    break;
            }
        }

        private void ApplyToKCC2D(Frame frame, EntityRef entity, FPVector2 velocity)
        {
            if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
            {
                kcc->DynamicVelocity = velocity;
            }
        }

        private void ApplyToCharacterController(Frame frame, EntityRef entity, Transform2D* transform, 
            FPVector2 movement, FPVector2 velocity)
        {
            transform->Position += movement;
            
            if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc))
            {
                cc->Velocity = velocity;
            }
        }

        private void ApplyToPhysics2D(Frame frame, EntityRef entity, FPVector2 velocity)
        {
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(entity, out var physicsBody))
            {
                physicsBody->Velocity = velocity;
            }
        }

        private void OnKnockbackEnd(Frame frame, EntityRef entity)
        {
            KnockbackComponent* knockback = frame.Unsafe.GetPointer<KnockbackComponent>(entity);

            switch (knockback->ApplicationMode)
            {
                case KnockbackApplicationMode.KCC2D:
                    if (frame.Unsafe.TryGetPointer<KCC2D>(entity, out var kcc))
                    {
                        kcc->DynamicVelocity = FPVector2.Zero;
                    }
                    break;

                case KnockbackApplicationMode.CharacterController:
                    if (frame.Unsafe.TryGetPointer<CharacterController2D>(entity, out var cc))
                    {
                        cc->Velocity = FPVector2.Zero;
                    }
                    break;
            }
        }

        private FPVector2 GetKnockbackRelativePosition(Frame frame, KnockbackComponent* knockback)
        {
            KnockbackStatusEffectData data = frame.FindAsset<KnockbackStatusEffectData>(knockback->StatusEffect.StatusEffectData.Id);

            FP normalizedTime = knockback->StatusEffect.DurationTimer.NormalizedTime;
            FP normalizedPositionX = data.KnockbackCurveX.Evaluate(normalizedTime);
            FP normalizedPositionY = data.KnockbackCurveY.Evaluate(normalizedTime);

            FPVector2 relativePosition = new FPVector2(
                knockback->StatusEffect.KnockbackDirection.X * data.KnockbackDistanceX * normalizedPositionX,
                knockback->StatusEffect.KnockbackDirection.Y * data.KnockbackDistanceY * normalizedPositionY
            );

            return relativePosition;
        }
    }
}
```

#### 2. 更新DSL定义
修改 `/Assets/QuantumUser/Simulation/Core/DSL/StatusEffect.qtn`：

```
enum StatusEffectType
{
    Knockback
}

enum KnockbackApplicationMode
{
    CharacterController,
    Physics2D,
    KCC2D
}

struct KnockbackStatusEffect : StatusEffect
{
    FrameTimer DurationTimer;
    FPVector2 KnockbackDirection;
    FPVector2 KnockbackVelocity;
    asset_ref<KnockbackStatusEffectData> StatusEffectData;
}

component KnockbackComponent
{
    KnockbackStatusEffect StatusEffect;
    KnockbackApplicationMode ApplicationMode;
}

signal OnKnockbackApplied(entity_ref target, FP duration, FPVector2 direction, asset_ref<KnockbackStatusEffectData> statusEffectData);
```

**删除旧Signal**：
```
// ❌ 删除这一行
signal OnKnockbackPhysic2DApplied(entity_ref target, FPVector2 knockbackVelocity);
```

#### 3. 创建组件扩展
创建文件：`/Assets/Scripts/KnockbackComponent.Partial.cs`

```csharp
namespace Quantum
{
    public partial struct KnockbackComponent
    {
        public bool IsKnockedBack => StatusEffect.DurationTimer.IsRunning;
        
        public FP KnockbackProgress => StatusEffect.DurationTimer.NormalizedTime;
        
        public FP RemainingKnockbackTime(Frame frame) => StatusEffect.DurationTimer.RemainingTime(frame);
    }
}
```

#### 4. 删除旧系统
删除文件（建议先备份）：
- ❌ `/Assets/QuantumUser/Simulation/Core/Systems/StatusEffect/KnockbackStatusEffectSystem.cs`

#### 5. 更新CharacterStatusComponent
修改 `/Assets/QuantumUser/Simulation/Core/DSL/Character.qtn`：

```
component CharacterStatusComponent
{
    asset_ref<HitReactionData> HitReactionData;
    Boolean IsSuperArmored;
    // ❌ 删除这一行：KnockbackStatusEffect KnockbackStatusEffect;
}
```

修改 `/Assets/QuantumUser/Simulation/Core/Extensions/CharacterStatusComponent.Partial.cs`：

```csharp
namespace Quantum
{
    public partial struct CharacterStatusComponent
    {
        public bool IsRespawning => false;
        
        public bool IsKnockedBack(Frame frame, EntityRef entity)
        {
            if (frame.Unsafe.TryGetPointer<KnockbackComponent>(entity, out var knockback))
            {
                return knockback->IsKnockedBack;
            }
            return false;
        }
        
        public bool IsIncapacitated(Frame frame, EntityRef entity)
        {
            return IsKnockedBack(frame, entity);
        }
    }
}
```

#### 6. 更新Character2DSystem
修改 `/Assets/QuantumUser/Simulation/Core/Systems/Character2DSystem.cs`：

```csharp
public override void Update(Frame frame, ref Filter filter)
{
    bool isKnockedBack = frame.Has<KnockbackComponent>(filter.EntityRef) && 
                         frame.Get<KnockbackComponent>(filter.EntityRef).IsKnockedBack;
    
    if (isKnockedBack)
    {
        filter.KCC->Velocity = FPVector2.Lerp(filter.KCC->Velocity, FPVector2.Zero, FP._10 * frame.DeltaTime);
    }
    
    FPVector2 movementDirection;
    
    if (isKnockedBack || filter.CharacterStatus->IsIncapacitated(frame, filter.EntityRef))
    {
        movementDirection = FPVector2.Zero;
    }
    else
    {
        movementDirection = GetMovementDirection(frame, filter.EntityRef);
        
        if (movementDirection.SqrMagnitude > FP._1)
        {
            movementDirection = movementDirection.Normalized;
        }
    }
    
    if (!filter.CharacterStatus->IsRespawning)
    {
        filter.KCC->Move(frame, filter.EntityRef, movementDirection);
    }
}
```

#### 7. 添加系统到配置
在 `SystemsConfig` 中：
1. 添加 `UnifiedKnockbackSystem`
2. 移除 `KnockbackStatusEffectSystem`
3. 确保 `UnifiedKnockbackSystem` 在角色移动系统之前执行

#### 8. 为实体添加KnockbackComponent
为所有需要击退的实体Prefab添加 `KnockbackComponent`：
- 玩家
- 敌人
- 其他可击退对象

## 测试清单

- [ ] 玩家被击退效果正常
- [ ] 敌人被击退效果正常
- [ ] CharacterController2D击退正常
- [ ] Physics2D击退正常
- [ ] KCC2D击退正常（新功能）
- [ ] 技能击退正常
- [ ] 投射物击退正常
- [ ] 技能场击退正常
- [ ] 攻击击退正常
- [ ] 击退结束后速度归零

## 注意事项

1. **备份**: 修改前请备份项目
2. **测试**: 逐步测试每个修改
3. **编译**: DSL修改后需要重新编译Quantum代码
4. **版本控制**: 建议在新分支进行迁移

## 成功标志

所有测试通过后，你将拥有：
- ✅ 统一的击退接口
- ✅ 支持3种运动控制器（CharacterController2D、Physics2D、KCC2D）
- ✅ 更简洁的代码
- ✅ 更好的可维护性
- ✅ 更强的扩展性
