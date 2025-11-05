# 2D Fighter - 功能特性文档

## 项目概述

2D Fighter 是一个基于 Unity 和 Photon Quantum 的多人在线 2D 平台格斗游戏，采用确定性物理引擎和 ECS 架构，支持流畅的联机对战体验。

## 核心技术栈

### 客户端技术
- **游戏引擎**: Unity 2022.3 LTS
- **物理引擎**: Photon Quantum (确定性物理)
- **架构模式**: ECS (Entity Component System)
- **资源管理**: YooAsset (热更新支持)
- **配置系统**: Luban

### 服务器技术
- **网络层**: 自研 LAT 服务器 + KCP 协议
- **协议**: Protocol Buffers
- **语言**: C# (.NET 6.0+)

## 主要功能模块

### 1. 模块化角色系统 (Modular Character System)

基于 ECS 风格的角色能力组合系统，灵感来自守望先锋的角色创建工作流。

#### 核心特性
- ✅ **能力组件化**: 将角色能力拆分为可复用的独立组件
- ✅ **快速角色组合**: 通过组合现有能力快速创建新角色
- ✅ **代码复用**: 大幅减少重复代码，提高开发效率
- ✅ **向后兼容**: 与现有系统保持兼容

#### 能力类型

**移动能力 (Movement Abilities)**
- 基础移动 (Movement)
- 跳跃 (Jump)
- 二段跳 (Double Jump)
- 冲刺 (Dash)
- 空中冲刺 (Air Dash)
- 蹬墙跳 (Wall Jump)
- 滑墙 (Wall Slide)
- 滑翔 (Glide)

**攻击能力 (Attack Abilities)**
- 轻攻击 (Light Attack)
- 重攻击 (Heavy Attack)
- 远程攻击 (Ranged Attack)
- 范围攻击 (Area Attack)

**防御能力 (Defense Abilities)**
- 格挡 (Block)
- 招架 (Parry)
- 闪避 (Dodge)
- 护盾 (Shield)

**特殊能力 (Special Abilities)**
- 终极技能 (Ultimate)
- 变身 (Transformation)
- 召唤 (Summon)

#### 技术实现
- **配置资产**: `ModularCharacterConfig` - 组合角色的所有能力
- **能力组件**: 
  - `MovementAbilityComponent` - 移动能力基类
  - `AttackAbilityComponent` - 攻击能力基类
  - `DefenseAbilityComponent` - 防御能力基类
  - `SpecialAbilityComponent` - 特殊能力基类
- **能力数据**: `AbilityData` - 具体能力的参数配置

### 2. 战斗系统

#### 核心系统
- **MovementInputSystem**: 处理角色移动输入和KCC运动
- **AbilityInputSystem**: 处理能力输入和执行
- **AbilitySystem**: 管理能力的生命周期和状态
- **CommandInputSystem**: 处理复杂的组合技输入序列

#### 战斗特性
- ✅ **连招系统**: 支持多段连击，连招计数和重置
- ✅ **蓄力攻击**: 重攻击可蓄力增强伤害
- ✅ **组合技**: 通过输入序列触发特殊招式
- ✅ **能力冷却**: 每个能力独立的冷却时间管理
- ✅ **技能缓冲**: 输入缓冲系统，提升操作流畅性
- ✅ **伤害计算**: 基于能力配置的伤害系统

#### 战斗组件
- **AttackData**: 攻击数据组件（连招、冷却、蓄力等）
- **CharacterStatus**: 角色状态（生命值、死亡、无敌等）
- **AbilityInventory**: 能力清单，管理角色所有能力
- **AbilityEnable**: 能力启用状态，控制哪些能力可用

### 3. 角色状态管理

#### 状态组件
- **CharacterStatus**: 
  - 当前生命值
  - 死亡状态
  - 无力化状态
  - 重生计时器
  - 回复计时器
  - 无敌计时器
  - 断线计时器

- **CharacterLevel**: 
  - 当前等级
  - 支持等级升级事件

#### 角色数据
- **StatusData**: 角色基础属性配置
- **PlayerMovementData**: 移动参数配置

### 4. 能力解锁系统 (Level-Up System)

- **LevelUpSystem**: 管理角色等级和能力解锁
- 能力可以设置解锁等级要求
- 支持运行时动态解锁/锁定能力
- 等级提升时触发 `LevelUp` 事件

### 5. 输入系统

#### 输入处理
- **SimpleInput2D**: 2D方向输入（移动、瞄准）
- **CommandInputData**: 组合技输入缓冲
- 支持多帧输入缓冲
- 输入序列匹配和超时管理

#### 输入特性
- ✅ 精确的帧级输入采集
- ✅ 输入缓冲减少操作失误
- ✅ 组合技输入序列识别
- ✅ 输入超时自动清理

### 6. 网络系统

#### 确定性同步 (Photon Quantum)
- ✅ 完全确定性的游戏逻辑
- ✅ 帧同步机制
- ✅ 客户端预测 + 服务器验证
- ✅ 回滚和重演机制

#### 自定义服务器 (LAT Server)
- ✅ 基于 KCP 的可靠 UDP 传输
- ✅ 低延迟网络通信
- ✅ 登录服务 (LoginService)
- ✅ 匹配服务 (MatchService)
- ✅ 战斗服务 (BattleService)

#### 网络协议
- Protocol Buffers 消息格式
- 登录协议 (Login.proto)
- 匹配协议 (Match.proto)
- 战斗协议 (Battle.proto)

### 7. 对象池系统

高效的对象复用系统，减少 GC 压力。

#### 功能
- ✅ 自动对象创建和回收
- ✅ 预热池容量
- ✅ 池容量限制
- ✅ 泛型支持

#### 实现
- `ObjectPoolManager` - 对象池管理器
- `IObjectPool` - 对象池接口
- `ObjectBase` - 可池化对象基类

### 8. 事件系统

#### Quantum 事件
- **LevelUp**: 等级提升事件
- **AttackPerformed**: 攻击执行事件
- **SpecialMovePerformed**: 特殊招式执行事件
- **AbilityActivated**: 能力激活事件
- **AbilityCancelled**: 能力取消事件
- **AbilityEnded**: 能力结束事件

#### 信号系统
- **CheckAbilityEnabled**: 检查能力是否启用
- **OnCooldownsReset**: 冷却重置信号
- **OnActiveAbilityStopped**: 活动能力停止信号

### 9. 状态效果系统 (Status Effects)

支持各种临时状态效果和能力修改。

#### 状态效果类型
- 击退效果 (KnockbackStatusEffect)
- 特殊移动能力效果
- 临时属性修改
- Buff/Debuff 系统基础

#### 状态效果数据
- `WallJumpAbilityData` - 蹬墙跳数据
- `DoubleJumpAbilityData` - 二段跳数据
- `WallSlideAbilityData` - 滑墙数据
- `JumpAbilityData` - 跳跃数据
- `DashAbilityData` - 冲刺数据
- `KnockbackStatusEffectData` - 击退效果数据

### 10. 工具和实用功能

#### 核心工具
- **BinaryTools**: 二进制序列化工具
  - BinaryWriter/BinaryReader
  - BufferWriter/BufferReader
  - 高性能数据转换

- **Utils**: 通用工具集
  - ListPool - 列表对象池
  - ArrayUtil - 数组工具
  - MathCommon - 数学工具
  - WeightedRandom - 加权随机

#### 状态机系统
- `StateMachine` - 通用状态机
- `IStateNode` - 状态节点接口
- 支持状态转换和管理

## 架构重构历史

### 最新重构 (详见 ARCHITECTURE_CHANGES.md)

#### 消除配置冗余
- 移除了 `AttackData` 组件中的 `AttackConfig` 引用
- `ModularCharacterConfig` 作为唯一配置源
- 简化角色配置工作流

#### 系统重组
- 将大型系统拆分为小型专注系统
- `MovementSystem` → `MovementInputSystem`
- `ModularAbilitySystem` → `AbilityInputSystem`
- 更好的性能和可维护性

#### 为信号系统做准备
- 系统架构已为信号驱动优化
- 未来可轻松转换为 `SystemSignalsOnly`
- 更响应式的事件驱动架构

## 开发工作流

### 创建新角色的步骤
1. 创建 `ModularCharacterConfig` 资产
2. 从现有能力组件中选择和组合
3. 根据需要创建新的能力组件
4. 配置能力参数和解锁条件
5. 在实体原型上引用配置

### 添加新能力的步骤
1. 继承相应的能力组件基类
2. 实现能力逻辑（通过 AbilityData）
3. 在 `AbilityType` 枚举中添加新类型
4. 在系统中添加能力处理逻辑
5. 配置到角色的 `ModularCharacterConfig`

## 性能优化

### 已实施的优化
- ✅ 对象池减少 GC 分配
- ✅ 小型专注系统提升缓存效率
- ✅ 确定性物理避免不必要的同步
- ✅ 能力缓冲减少重复计算
- ✅ 组件复用减少代码量

### 未来优化方向
- 信号驱动系统（减少 Update 调用）
- 更细粒度的系统拆分
- 数据导向设计优化

## 文档资源

### 核心文档
- **ARCHITECTURE_CHANGES.md** - 架构变更详细说明
- **REFACTORING_NOTES.md** - 重构笔记和迁移指南
- **架构重构总结.md** - 中文版架构总结

### 模块化系统文档
- **Md/ModularCharacterSystem.md** - 模块化角色系统详解
- **Md/ExampleCharacters.md** - 角色配置示例
- **Md/IntegrationGuide.md** - 系统集成指南
- **Md/Architecture.md** - 系统架构总览

### 系统文档
- **Md/Core/** - 核心系统文档
  - 动画状态管理系统
  - 动画系统迁移指南
  - 等...

## 版本信息

- **Unity 版本**: 2022.3 LTS 或更高
- **Photon Quantum**: 集成在项目中
- **.NET 版本**: .NET Standard 2.1 (客户端), .NET 6.0+ (服务器)

## 联系方式

- **开发者**: Wangok123
- **GitHub**: https://github.com/Wangok123/2d_Fighter
- **项目类型**: 个人开发项目

---

**最后更新**: 2024-11-05
