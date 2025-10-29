# Fighting Game Animation System - Quick Start
# 格斗游戏动画系统 - 快速入门

## What's New? / 新增功能

This update adds a complete fighting game animation system with:
本次更新新增了完整的格斗游戏动画系统，包括：

### 1. ✨ Animation Cancel Mechanism / 动画取消机制
- **Animation Priority System** - Controls which animations can interrupt others
- **动画优先级系统** - 控制哪些动画可以打断其他动画
- **Cancel Windows** - Precise timing for combo cancels (like in Street Fighter)
- **取消窗口** - 精确的连招取消时机（类似街霸）
- **Cancel Policies** - Flexible rules for when animations can be cancelled
- **取消策略** - 灵活的动画取消规则

### 2. 🤼 Throw/Grapple Attack System / 投技系统
- **Complete throw mechanics** - Like Zangief from Street Fighter
- **完整的投技机制** - 类似街霸的桑吉尔夫
- **Grab detection** - Range and angle-based target detection
- **抓取检测** - 基于范围和角度的目标检测
- **Throw break system** - Defenders can break throws within a window
- **投技破解** - 防守方可以在窗口期内破解投技
- **Animation sequence** - Start → Hold → Execute → Recovery
- **动画序列** - 起手 → 保持 → 执行 → 恢复

### 3. 💥 Smart Hit Reactions / 智能受击反应
- **Ground vs Air hits** - Separate animations based on character state
- **地面与空中受击** - 根据角色状态使用不同动画
- **Hit priority** - Hits can interrupt most actions
- **受击优先级** - 受击可以打断大多数动作
- **Knockdown system** - Powerful hits knock down opponents
- **击倒系统** - 强力攻击可以击倒对手

## Quick Start / 快速开始

### For Warrior Character / Warrior角色使用

The Warrior character now has full support for the cancel system:
Warrior角色现在完全支持取消系统：

```csharp
using UnityCore.AnimationSystem;

// Get the animation manager
var animManager = GetComponent<WarriorAnimationManager>();

// Basic attacks with cancel support
animManager.PlayAttack();  // Can be cancelled during cancel windows

// Throw attacks
animManager.PlayThrowStart();

// Smart hit reactions
bool isGrounded = true;
animManager.PlaySmartHurt(isGrounded);  // Automatically chooses ground/air hit
```

### Add Throw System / 添加投技系统

```csharp
// 1. Add ThrowAttackComponent to your character
// 在角色上添加ThrowAttackComponent组件

// 2. Configure in Inspector / 在Inspector中配置：
//    - Grab Range: 1.5
//    - Grab Angle: 45
//    - Throw Damage: 50
//    - Knockback Force: 10

// 3. Use in code / 代码中使用：
var throwComponent = GetComponent<ThrowAttackComponent>();

if (Input.GetKeyDown(KeyCode.T))
{
    bool success = throwComponent.TryThrow();
    if (success)
    {
        Debug.Log("Throw initiated!");
    }
}
```

### Make Character Throwable / 让角色可被投技

```csharp
public class MyCharacter : MonoBehaviour, IThrowable, IHealthSystem
{
    public bool CanBeGrabbed()
    {
        // Return true if character can be grabbed
        return !isInvincible && !isDead;
    }

    public void OnGrabbed(GameObject attacker)
    {
        Debug.Log("I was grabbed!");
    }

    public void OnThrowBroken()
    {
        Debug.Log("I broke the throw!");
    }

    public void OnReleased()
    {
        Debug.Log("Released from throw");
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }
}
```

## Examples / 示例

### Example 1: Simple Combo System / 简单连招系统

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.F))
    {
        // Try to attack - will check cancel rules
        if (_animManager.CanPlayAnimation("Attack"))
        {
            _animManager.PlayAttack();
            Debug.Log("Combo!");
        }
        else
        {
            Debug.Log("Cannot cancel current animation");
        }
    }
}
```

### Example 2: Throw Attack / 投技攻击

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.T))
    {
        // Try to grab opponent
        bool success = _throwComponent.TryThrow();
        
        if (success)
        {
            Debug.Log("Grabbed opponent!");
        }
    }
}
```

### Example 3: Smart Hit Reaction / 智能受击

```csharp
public void OnHit(float damage, bool isGrounded)
{
    // Automatically choose correct hit animation
    _animManager.PlaySmartHurt(isGrounded);
    
    // Or manually specify
    if (isGrounded)
        _animManager.PlayHurtGround();
    else
        _animManager.PlayHurtAir();
}
```

## Animation Priority Reference / 动画优先级参考

```
Death (100)        ← Highest - Cannot be interrupted
    ↑
Knockdown (70)     ← Very high
    ↑
Hit (60)           ← High - Can interrupt most actions
    ↑
Throw (50)         ← High
    ↑
Skill (40)         ← Medium-high
    ↑
Attack (30)        ← Medium
    ↑
Jump (20)          ← Medium-low
    ↑
Movement (10)      ← Low
    ↑
Idle (0)           ← Lowest - Can be interrupted by anything
```

## Files Added / 新增文件

### Core System / 核心系统
- `AnimationPriority.cs` - Priority and cancel policy definitions
- `ThrowAttackComponent.cs` - Complete throw attack system
- Updated `AnimationStateManager.cs` - Cancel mechanism support
- Updated `CharacterAnimationManager.cs` - New helper methods
- Updated `WarriorAnimationManager.cs` - All animations with priorities

### Examples / 示例
- `FightingGameAnimationExample.cs` - Complete demo with UI
- `ThrowableCharacterExample.cs` - Example throwable character

### Documentation / 文档
- `格斗游戏动画系统-取消机制与投技.md` - Full documentation (CN/EN)

## Testing the System / 测试系统

Run `FightingGameAnimationExample` scene to test:
运行`FightingGameAnimationExample`场景进行测试：

**Keyboard Controls:**
- `1-2`: Idle/Run
- `Space`: Jump
- `F`: Attack (can combo)
- `G`: Dash Attack
- `T`: Throw
- `H`: Take Hit (smart)
- `K`: Knockdown
- `Y`: Test priority
- `U`: Force play
- `Q`: Show status
- `C`: Toggle ground state

## Best Practices / 最佳实践

### ✅ DO / 建议

1. **Use CanPlayAnimation() before playing** - Check if animation can be played
   ```csharp
   if (_animManager.CanPlayAnimation("Attack"))
       _animManager.PlayAttack();
   ```

2. **Set appropriate priorities** - Follow the priority hierarchy
   ```csharp
   RegisterState("Attack", priority: AnimationPriority.Attack);
   ```

3. **Use cancel windows for combos** - Define precise timing
   ```csharp
   cancelWindows: new[] {
       new CancelWindow(0.4f, 0.7f, new[] { "Attack", "Skill" })
   }
   ```

4. **Subscribe to throw events** - Handle throw lifecycle
   ```csharp
   _throwComponent.OnThrowStarted += OnThrowStarted;
   ```

### ❌ DON'T / 避免

1. **Don't ignore cancel rules unnecessarily** - Only use `ignoreCancelRules` for forced state changes
2. **Don't set too many cancel windows** - Keep it simple (max 3)
3. **Don't make everything NonCancellable** - It breaks the flow
4. **Don't forget to implement IThrowable** - Required for throw targets

## Common Issues / 常见问题

### Q: My attack is being interrupted by movement?
**A:** Check priorities. Attacks should have higher priority than movement.

### Q: Combos not working?
**A:** Make sure you defined cancel windows in RegisterState.

### Q: Throw not working?
**A:** Check:
1. Target has IThrowable interface
2. Target is in range and angle
3. Target's CanBeGrabbed() returns true

### Q: How to make super armor (can't be interrupted)?
**A:** Set high priority and NonCancellable policy:
```csharp
RegisterState("SuperAttack", 
    priority: AnimationPriority.Skill,
    cancelPolicy: AnimationCancelPolicy.NonCancellable);
```

## Performance Notes / 性能说明

- Uses hash-based lookups (5x faster than strings)
- Cancel checks are O(1) for priority comparison
- Cancel window checks are O(n) where n = window count (keep it small)
- Throw detection uses Physics2D (optimize with layers)

## What's Next? / 下一步

This system provides the foundation for:
- Advanced combo systems
- Counter attack mechanics
- Special move cancels
- Chain combos
- Custom fighting game mechanics

## Support / 支持

For detailed documentation, see:
详细文档请查看：
- `Md/Core/格斗游戏动画系统-取消机制与投技.md`

For questions or issues, refer to the example code:
问题或疑问请参考示例代码：
- `FightingGameAnimationExample.cs`
- `ThrowableCharacterExample.cs`

---

**Enjoy building your fighting game! / 祝您游戏开发顺利！** 🥊
