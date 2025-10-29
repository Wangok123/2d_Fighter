# 2d_Fighter
一个联机干架2d平台游戏

## 🆕 最新更新 (Latest Updates)

### ✨ 格斗游戏动画系统 (Fighting Game Animation System)

新增完整的格斗游戏动画系统，包括：

#### 1. 动画取消机制 (Cancel Mechanism)
- **优先级系统** - 控制哪些动画可以打断其他动画
- **取消窗口** - 支持连招系统（类似街霸）
- **取消策略** - 灵活的动画切换规则

#### 2. 投技系统 (Throw/Grapple System)
- **完整的投技机制** - 类似街霸的桑吉尔夫（Zangief）
- **抓取检测** - 基于范围和角度
- **投技破解** - 防守方可以破解投技
- **动画序列** - 起手 → 保持 → 执行 → 恢复

#### 3. 智能受击反应 (Smart Hit Reactions)
- **地面/空中受击** - 自动区分
- **击倒系统** - 强力攻击可击倒
- **受击优先级** - 可打断大多数动作

### 📖 快速开始

查看完整文档：
- 中文文档: `Md/Core/格斗游戏动画系统-取消机制与投技.md`
- 快速入门: `Client/Assets/Scripts/UnityCore/AnimationSystem/FIGHTING_GAME_SYSTEM_README.md`

示例代码：
- `FightingGameAnimationExample.cs` - 完整演示
- `ThrowableCharacterExample.cs` - 可被投技的角色示例
- `CancelMechanismValidation.cs` - 验证测试

### 🎮 使用示例

```csharp
// 基础使用
var animManager = GetComponent<WarriorAnimationManager>();

// 带取消机制的攻击
animManager.PlayAttack();  // 在取消窗口内可以连招

// 投技攻击
var throwComponent = GetComponent<ThrowAttackComponent>();
throwComponent.TryThrow();

// 智能受击
animManager.PlaySmartHurt(isGrounded);
```

---

## 项目介绍

一个基于Unity + Quantum引擎的2D格斗游戏平台。
