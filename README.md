# ⚔️ 2D Fighter

> 基于 Photon Quantum 的多人在线 2D 平台格斗游戏

[![Unity](https://img.shields.io/badge/Unity-2022.3.62f2-black.svg?style=flat&logo=unity)](https://unity.com/)
[![Photon Quantum](https://img.shields.io/badge/Photon-Quantum-blue.svg?style=flat)](https://www.photonengine.com/quantum)
[![C#](https://img.shields.io/badge/Language-C%23-239120.svg?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg?style=flat&logo=.net)](https://dotnet.microsoft.com/)
[![KCP](https://img.shields.io/badge/Network-KCP-green.svg?style=flat)](https://github.com/skywind3000/kcp)
[![Protobuf](https://img.shields.io/badge/Protocol-Protobuf-red.svg?style=flat)](https://developers.google.com/protocol-buffers)

一个采用确定性物理引擎和 ECS 架构的联机格斗游戏，支持流畅的多人实时对战体验。项目包含自研 LAT 游戏框架、自研 LAT 服务器，以及完整的客户端-服务器双端实现。

## ✨ 项目特色

- 🎯 **确定性同步** - 基于 Photon Quantum 的帧同步机制，保证公平对战
- 🧩 **模块化角色系统** - ECS 风格的能力组件化设计，灵感来自守望先锋
- 🚀 **高性能网络** - 自研 LAT 服务器 + KCP 协议，低延迟通信
- 🎮 **丰富的能力系统** - 移动、攻击、防御、特殊技能四大类能力
- ⚡ **优化的架构** - 对象池、事件系统、状态机等高效模块

## 🎮 核心功能

### 模块化角色系统 (Modular Character System)

借鉴守望先锋的角色创建工作流，实现了 ECS 风格的能力组件系统：

- ✅ **能力组件化** - 将角色能力拆分为可复用的独立组件
- ✅ **快速组合** - 通过组合现有能力快速创建新角色  
- ✅ **代码复用** - 大幅减少重复代码，提高开发效率
- ✅ **向后兼容** - 与现有系统保持兼容

#### 能力类型

| 类别 | 能力 |
|------|------|
| 🏃 **移动** | 移动、跳跃、二段跳、冲刺、空中冲刺、蹬墙跳、滑墙、滑翔 |
| ⚔️ **攻击** | 轻攻击、重攻击、远程攻击、范围攻击 |
| 🛡️ **防御** | 格挡、招架、闪避、护盾 |
| ⭐ **特殊** | 终极技能、变身、召唤 |

### 战斗系统

- 🎯 **连招系统** - 支持多段连击和连招计数
- ⚡ **蓄力攻击** - 重攻击可蓄力增强伤害
- 🎪 **组合技** - 通过输入序列触发特殊招式
- ⏱️ **技能冷却** - 独立的冷却时间管理
- 🎮 **输入缓冲** - 提升操作流畅性的输入缓冲系统

## 🏗️ 技术架构

本项目采用**客户端-服务器分离架构**，使用 Photon Quantum 提供确定性物理模拟，结合自研 LAT 框架和服务器，实现高性能、低延迟的联机格斗体验。

### 🖥️ 客户端技术栈

#### 核心引擎与框架
| 技术 | 版本 | 用途 |
|------|------|------|
| **Unity** | 2022.3.62f2 LTS | 游戏引擎 |
| **Photon Quantum** | 最新版 | 确定性物理引擎、帧同步 |
| **LAT 框架** | 自研 | 游戏核心框架（GameEntry、组件系统） |
| **.NET Standard** | 2.1 | 脚本运行时 |

#### Unity 插件与包管理

**资源管理**
- **YooAsset** (2.3.2-preview) - 资源热更新、AssetBundle 管理
- **Addressables** (1.22.3) - Unity 原生资源系统

**配置与数据**
- **Luban** - 配置表生成工具，支持 Excel → C# 类型安全代码
- **Google.Protobuf** (3.30.2) - 网络通信协议

**游戏功能**
- **Unity Input System** (1.14.0) - 新版输入系统
- **Cinemachine** (2.10.3) - 摄像机控制
- **TextMeshPro** (3.0.7) - 高质量文本渲染
- **Timeline** (1.7.7) - 动画和事件序列
- **Feature 2D** (2.0.1) - 2D 游戏工具集

**开发工具**
- **ParrelSync** - 多客户端同步测试
- **Unity Test Framework** (1.1.33) - 单元测试

#### LAT 游戏框架（自研）

LAT (龙傲天) 是为本项目开发的完整游戏框架，提供模块化架构：

**核心模块** (`Scripts/Core/`)
- **EventSystem** - 事件总线，松耦合模块通信
- **ObjectPool** - Unity GameObject 对象池
- **ReferencePool** - C# 对象池，减少 GC 压力
- **StateMachine** - 通用状态机框架
- **BinaryTools** - 二进制序列化工具
- **CustomDataStruct** - 自定义数据结构
  - `LatLinkedList` - 高性能链表
  - `MultiDictionary` - 多键字典
  - `Digraph` - 有向图
  - `UnionFind` - 并查集

**Unity 层模块** (`Scripts/UnityCore/`)
- **GameModule** - 游戏模块管理器
  - Battle - 战斗系统
  - Lobby - 大厅系统
  - Match - 匹配系统
  - HeroSelect - 角色选择
  - Loading - 加载管理
  - Playing - 游戏进行中状态
  - GameFlow - 游戏流程控制
  - GM - 调试/作弊系统
  - Coroutine - 协程管理
- **AnimationSystem** - 动画状态管理
  - WarriorAnimationManager
  - CharacterAnimationManager
- **Input** - 输入处理
  - UIInputModule - UI 输入
  - PlayerInputModule - 玩家输入
- **Network** - 网络通信层（与 LatServer 对接）
- **ResourceSystem** - 资源加载管理
- **SaveSystem** - 存档系统
- **SceneManagement** - 场景管理
- **UI Framework** - UI 界面框架
- **Audio** - 音频管理

**Quantum 集成** (`Scripts/Quantum/`)
- **CustomViewContext** - 自定义视图上下文
- **PlayersManager** - 玩家管理器
- **QuantumSmoothTransform** - 平滑位置同步
- **LocalQuantumInputPoller** - 本地输入采集
- **PlayerViewController** - 玩家视图控制器
- **Debugger** - 可视化调试工具
  - ProjectileDebugger - 投射物调试
  - AttackRangeDebugger - 攻击范围可视化
  - SkillFieldDebugger - 技能范围调试
  - DeathZoneDebugger - 死亡区域调试
  - RespawnPointDebugger - 重生点调试

#### Quantum 游戏逻辑层

**核心系统** (`QuantumUser/Simulation/Core/Systems/`)
- **MovementInputSystem** - 移动输入处理
- **AbilityInputSystem** - 技能输入处理
- **AbilitySystem** - 技能生命周期管理
- **CommandInputSystem** - 连招输入识别
- **LevelUpSystem** - 等级与技能解锁系统

**资产定义** (`Core/Assets/`)
- 角色配置（Stats、Abilities）
- 技能配置（Damage、Cooldown、Range）
- 地图配置（Platforms、Spawn Points）

**DSL 系统** - 领域特定语言，用于技能和能力定义

---

### 🖧 服务器端技术栈

#### 核心架构

**LatServer** - 自研游戏服务器 (.NET 8.0)

```
LatServer/
├── LatServer/          # 业务逻辑层
├── LatNet/             # 网络传输层（KCP）
└── LatProtocol/        # 协议定义层（Protobuf）
```

#### LatServer 业务逻辑层

**框架结构** (`LatServer/Core/`)

**服务层** (`Service/`)
- **NetService** - 网络服务管理，KCP 连接池
- **ConfigService** - 配置表加载（Luban Tables）
- **TimerService** - 定时器调度系统
- **CacheService** - 内存缓存（玩家数据、房间状态）

**游戏系统** (`System/`)
- **LoginSys** - 用户认证与登录
- **MatchSys** - 匹配算法（ELO 排位、快速匹配）
- **RoomSys** - 战斗房间管理
  - `PVPRoom` - PVP 房间实例
  - `Fsm/` - 房间状态机
    - RoomStateSelecting → 角色选择
    - RoomStateConfirm → 准备确认
    - RoomStateLoading → 加载等待
    - RoomStateFighting → 战斗中
    - RoomStateEnd → 结算
- **EventSys** - 事件总线架构

**消息处理** (`MessageHandleService/`)
- **Dispatcher** - 消息路由分发
- **Handler/** - 协议处理器
  - LoginHandler - 处理登录请求
  - MatchHandler - 处理匹配请求
  - SendSelectHandler - 角色选择
  - SendConfirmHandler - 准备确认
  - SendLoadProgressHandler - 加载进度
  - SendLoadFinishHandler - 加载完成
- **MessageHandlerService** - 消息处理管道

**工具模块** (`Tools/`)
- **LubanLib** - Luban 配置反序列化库

**初始化流程**
```
ServerRoot (单例)
  ├─> MessageHandlerService  (注册消息处理器)
  ├─> CacheService            (初始化缓存)
  ├─> NetService              (启动网络监听)
  ├─> TimerService            (启动定时器)
  ├─> CfgService              (加载配置表)
  ├─> LoginSystem             (用户认证系统)
  ├─> MatchSystem             (匹配系统)
  └─> RoomSystem              (房间系统)
```

#### LatNet 网络层

**KCP 可靠 UDP 传输**
- **KCPNet\<T\>** - 泛型 KCP 服务器/客户端实现
- **KCPSession** - 单连接会话管理
  - 心跳检测（Heartbeat）
  - 消息分包与重组
  - 可靠传输保证
- **KCPHandle** - 连接处理器
- **KCPTool** - 工具函数

**技术特性**
- 基于 UDP 的可靠传输
- 自动重传（ARQ）
- 拥塞控制
- 快速重传算法
- 低延迟优化（适合实时对战）

**依赖**
- `Kcp.dll` - KCP 核心库

#### LatProtocol 协议层

**Protobuf 消息定义** (`Protocol/*.cs`)
- **Common.cs** - 通用 DTO
  - UserDto - 用户信息
  - HeroDto - 英雄信息
  - BattleHeroDto - 战斗角色信息
- **Login.cs** - 登录协议
  - LoginRequest
  - LoginResponse
- **Match.cs** - 匹配协议
  - StartMatchRequest
  - MatchSuccessNotify
- **Battle.cs** - 战斗协议
  - BattleOperationNotify
  - BattleResultNotify
- **Operation.cs** - 操作协议
  - PlayerInputNotify
  - SkillCastNotify

**协议管理**
- **ProtocolManager** - 协议注册中心
- **ProtocolProcessor** - 消息序列化/反序列化
- **ProtobufHelper** - Protobuf 工具类

**配置与常量**
- **ServerConfig** - 服务器配置（端口、超时等）
- **ErrorCodeID** - 标准错误码定义
- **ProtocolID** - 消息类型 ID 枚举

**依赖**
- **Google.Protobuf** (3.30.2)

---

### 🔗 共享组件

#### 协议定义 (`Protocal/Proto/`)

**Protobuf 协议文件**
```protobuf
common.proto      # 通用数据传输对象
login.proto       # 认证与登录协议
match.proto       # 匹配系统协议
battle.proto      # 战斗系统协议
operation.proto   # 玩家操作协议
```

**编译流程**
- 使用 `protoc.exe` 编译 .proto 文件
- 生成客户端代码 → `Client/Assets/Gen/`
- 生成服务器代码 → `Server/LatServer/LatProtocol/Protocol/`

#### 配置系统 (`Public/`)

**Luban 代码生成工具** (`Public/Luban/`)
- **Luban.dll** - 核心代码生成引擎
- **支持格式** - Excel、JSON、XML、Lua、Yaml
- **输出语言** - C#、Java、Go、Lua、TypeScript、Python 等
- **特性** - 类型检查、多语言、数据验证

**配置表** (`Public/CfgTables/`)
| 配置表 | 说明 |
|--------|------|
| **unit.xlsx** | 角色基础属性（HP、移动速度、攻击力） |
| **skill.xlsx** | 技能定义（伤害、冷却时间、范围） |
| **map.xlsx** | 地图配置（平台、重生点、边界） |
| **target.xlsx** | 目标选择规则 |
| **reward.xlsx** | 奖励表（经验、金币、道具） |
| **Defines/** | 枚举和常量定义 |

**生成代码**
- 客户端：`Client/Assets/Scripts/Gen/Tables.cs`
  - `TbUnit` - 角色表
  - `TbSkill` - 技能表
  - `TbMap` - 地图表
- 服务器：`Server/LatServer/Tools/LubanLib/`

**代码生成脚本**
```bash
gen.bat         # 生成客户端配置
gen_server.bat  # 生成服务器配置
```

#### 数据流架构

```
┌─────────────────────────────────────────────────────────┐
│  Client (Unity)                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Player Input → LocalQuantumInputPoller           │  │
│  │       ↓                                           │  │
│  │ Quantum Simulation (Deterministic Physics)       │  │
│  │       ↓                                           │  │
│  │ LAT Network Module                               │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         │  KCP (UDP Reliable)   │
         │  Protobuf Messages    │
         └───────────┬───────────┘
                     │
┌────────────────────┴────────────────────────────────────┐
│  Server (LatServer .NET 8.0)                            │
│  ┌──────────────────────────────────────────────────┐  │
│  │ LatNet (KCP Layer)                               │  │
│  │       ↓                                           │  │
│  │ MessageHandlerService (Dispatcher)               │  │
│  │       ↓                                           │  │
│  │ Game Systems                                     │  │
│  │   ├─ LoginSystem                                 │  │
│  │   ├─ MatchSystem                                 │  │
│  │   └─ RoomSystem (FSM)                           │  │
│  │       ↓                                           │  │
│  │ Services (Config, Cache, Timer)                  │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
         ↓
    Synchronized State
         ↓
    Back to Clients
```

---

### 🔧 技术特性总览

| 特性 | 技术方案 | 说明 |
|------|----------|------|
| **网络同步** | Photon Quantum 帧同步 | 确定性物理，客户端预测 + 服务器验证 |
| **网络传输** | KCP (UDP) + Protobuf | 低延迟可靠传输，高效序列化 |
| **架构模式** | ECS + 模块化组件 | Entity-Component-System + 能力组件化 |
| **内存管理** | 双层对象池 | ObjectPool (GameObject) + ReferencePool (C# 对象) |
| **资源管理** | YooAsset + Addressables | 支持热更新、分包加载 |
| **配置系统** | Luban | Excel → 类型安全 C# 代码，数据驱动设计 |
| **状态管理** | 有限状态机 | 战斗系统、角色 AI、房间流程 |
| **事件系统** | 事件总线 | 松耦合模块通信，支持优先级 |
| **并发编程** | Task + CancellationToken | 异步 I/O，可取消任务 |
| **输入系统** | Unity Input System + 帧同步 | 支持多设备输入，确定性回放 |

## 📁 项目结构

```
2d_Fighter/
├── Client/                          # Unity 客户端项目
│   ├── Assets/
│   │   ├── Scripts/                 # 游戏脚本
│   │   │   ├── Core/                # LAT 核心模块
│   │   │   │   ├── EventSystem/     # 事件总线
│   │   │   │   ├── ObjectPool/      # GameObject 对象池
│   │   │   │   ├── ReferencePool/   # C# 对象池
│   │   │   │   ├── StateMachine/    # 状态机框架
│   │   │   │   ├── BinaryTools/     # 序列化工具
│   │   │   │   └── CustomDataStruct/# 自定义数据结构
│   │   │   ├── UnityCore/           # Unity 层扩展
│   │   │   │   ├── GameModule/      # 游戏模块（Battle、Lobby、Match 等）
│   │   │   │   ├── AnimationSystem/ # 动画管理
│   │   │   │   ├── Input/           # 输入处理
│   │   │   │   ├── Network/         # 网络通信
│   │   │   │   ├── ResourceSystem/  # 资源加载
│   │   │   │   ├── UI/              # UI 框架
│   │   │   │   └── ...              # 其他系统模块
│   │   │   ├── Quantum/             # Quantum 集成
│   │   │   │   ├── Base/            # 基础组件
│   │   │   │   ├── Debugger/        # 可视化调试工具
│   │   │   │   └── Input/           # 输入采集器
│   │   │   └── Gen/                 # 生成代码
│   │   │       ├── Tables.cs        # Luban 配置表类
│   │   │       ├── latcfg/          # 配置表数据（TbUnit、TbSkill 等）
│   │   │       └── latEnum/         # 枚举定义
│   │   ├── QuantumUser/             # Quantum 游戏逻辑
│   │   │   ├── Simulation/          # 模拟层（确定性逻辑）
│   │   │   │   ├── Core/            # 核心系统
│   │   │   │   │   ├── Systems/     # ECS 系统
│   │   │   │   │   ├── Assets/      # 资产配置
│   │   │   │   │   └── DSL/         # 领域特定语言
│   │   │   │   └── Generated/       # Quantum 生成代码
│   │   │   └── View/                # 视图层（表现逻辑）
│   │   ├── Photon/                  # Photon SDK
│   │   │   ├── Quantum/             # Quantum 引擎核心
│   │   │   ├── PhotonRealtime/      # Photon Realtime
│   │   │   └── QuantumMenu/         # Quantum UI 菜单
│   │   ├── ThirdParty/              # 第三方库
│   │   │   └── BigCat/              # BigCat 工具集
│   │   │       ├── Wjybxx.Commons.Core/      # 通用工具库
│   │   │       ├── Wjybxx.Commons.Concurrent/# 并发工具
│   │   │       ├── Wjybxx.BTree.Core/        # 行为树核心
│   │   │       ├── Wjybxx.Dson.Core/         # DSON 序列化
│   │   │       └── ...                       # 其他组件
│   │   ├── Configs/                 # 配置文件
│   │   ├── Art/                     # 美术资源
│   │   └── ...                      # 其他资源
│   ├── Packages/                    # Unity 包配置
│   │   └── manifest.json            # 包依赖定义
│   └── ProjectSettings/             # 项目设置
│
├── Server/                          # 服务器项目
│   └── LatServer/                   # LatServer 解决方案
│       ├── LatServer.sln            # Visual Studio 解决方案
│       ├── LatServer/               # 业务逻辑层
│       │   ├── Core/
│       │   │   ├── Service/         # 服务层
│       │   │   │   ├── NetService.cs         # 网络服务
│       │   │   │   ├── ConfigService.cs      # 配置服务
│       │   │   │   ├── TimerService.cs       # 定时器服务
│       │   │   │   └── CacheService.cs       # 缓存服务
│       │   │   ├── System/          # 游戏系统
│       │   │   │   ├── LoginSys/             # 登录系统
│       │   │   │   ├── MatchSys/             # 匹配系统
│       │   │   │   ├── RoomSys/              # 房间系统
│       │   │   │   │   ├── PVPRoom.cs        # PVP 房间
│       │   │   │   │   └── Fsm/              # 房间状态机
│       │   │   │   └── EventSys/             # 事件系统
│       │   │   └── MessageHandleService/     # 消息处理
│       │   │       ├── Dispatcher.cs         # 消息分发
│       │   │       └── Handler/              # 协议处理器
│       │   ├── Tools/
│       │   │   └── LubanLib/        # Luban 配置库
│       │   └── ServerRoot.cs        # 服务器入口
│       ├── LatNet/                  # 网络层（KCP）
│       │   ├── KCPNet.cs            # KCP 服务器/客户端
│       │   ├── KCPSession.cs        # 会话管理
│       │   ├── KCPHandle.cs         # 连接处理
│       │   └── KCPTool.cs           # 工具函数
│       └── LatProtocol/             # 协议层
│           ├── Protocol/            # Protobuf 生成代码
│           │   ├── Common.cs        # 通用 DTO
│           │   ├── Login.cs         # 登录协议
│           │   ├── Match.cs         # 匹配协议
│           │   ├── Battle.cs        # 战斗协议
│           │   └── Operation.cs     # 操作协议
│           ├── ProtocolManager.cs   # 协议管理器
│           ├── ServerConfig.cs      # 服务器配置
│           └── ErrorCodeID.cs       # 错误码定义
│
├── Protocal/                        # 协议定义
│   └── Proto/                       # Protobuf 源文件
│       ├── common.proto             # 通用消息
│       ├── login.proto              # 登录协议
│       ├── match.proto              # 匹配协议
│       ├── battle.proto             # 战斗协议
│       └── operation.proto          # 操作协议
│
├── Public/                          # 公共资源
│   ├── CfgTables/                   # 配置表（Excel）
│   │   ├── unit.xlsx                # 角色配置
│   │   ├── skill.xlsx               # 技能配置
│   │   ├── map.xlsx                 # 地图配置
│   │   ├── target.xlsx              # 目标规则
│   │   ├── reward.xlsx              # 奖励配置
│   │   └── Defines/                 # 枚举定义
│   ├── Luban/                       # Luban 工具
│   │   ├── Luban.dll                # 代码生成器
│   │   └── ...                      # 模板文件
│   ├── LogLib/                      # 日志库
│   │   ├── Kcp.dll                  # KCP 核心库
│   │   ├── LATTimer.dll             # 定时器库
│   │   └── LogLib.dll               # 日志工具
│   ├── gen.bat                      # 客户端配置生成脚本
│   └── gen_server.bat               # 服务器配置生成脚本
│
├── CommonTools/                     # 通用工具
├── GenerateDatas/                   # 生成数据
├── Md/                              # 文档目录
│   ├── ModularCharacterSystem.md    # 模块化角色系统详解
│   ├── ExampleCharacters.md         # 角色配置示例
│   ├── IntegrationGuide.md          # 系统集成指南
│   ├── Architecture.md              # 架构总览
│   └── 性能分析报告.md              # 性能分析
├── Md_Imgs/                         # 文档图片
├── FEATURES.md                      # 功能特性文档 ⭐
├── ARCHITECTURE_CHANGES.md          # 架构变更说明
├── REFACTORING_NOTES.md             # 重构笔记
├── 架构重构总结.md                  # 架构总结（中文）
└── README.md                        # 本文件
```

## 🚀 快速开始

### 📋 环境要求

#### 客户端开发环境
- **Unity** 2022.3.62f2 或更高版本（推荐使用 LTS 版本）
- **操作系统** Windows 10/11、macOS、Linux
- **.NET Standard** 2.1（Unity 自带）
- **磁盘空间** 至少 10GB
- **内存** 8GB RAM（推荐 16GB）

#### 服务器开发环境
- **.NET SDK** 8.0 或更高版本
- **操作系统** Windows 10/11、Linux（推荐 Ubuntu 20.04+）
- **IDE** Visual Studio 2022、Rider 或 VS Code
- **端口** 默认需要开放 UDP 端口（可在 ServerConfig 配置）

#### 开发工具（可选）
- **Git** 版本控制
- **Protoc** Protocol Buffers 编译器（用于协议修改）
- **Luban** 配置表生成工具（已包含在 Public/Luban）

---

### 🎬 启动步骤

#### 1. 克隆仓库

```bash
git clone https://github.com/Wangok123/2d_Fighter.git
cd 2d_Fighter
```

#### 2. 启动服务器（可选）

如果需要测试联机功能，首先启动 LatServer：

```bash
cd Server/LatServer/LatServer
dotnet restore          # 恢复 NuGet 包
dotnet build            # 编译项目
dotnet run              # 启动服务器
```

**预期输出：**
```
[INFO] NetService started on port 9527
[INFO] ConfigService loaded 5 tables
[INFO] LoginSystem initialized
[INFO] MatchSystem initialized
[INFO] RoomSystem initialized
[INFO] Server is ready!
```

**服务器配置**  
编辑 `Server/LatServer/LatServer/ServerConfig.cs` 修改：
- 监听端口
- 心跳间隔
- 匹配超时时间
- 房间最大人数

#### 3. 打开 Unity 客户端

1. **启动 Unity Hub**
2. **添加项目**
   - 点击 "Add" → "Add project from disk"
   - 选择 `2d_Fighter/Client` 目录
3. **等待导入**
   - Unity 会自动导入资源和编译脚本
   - 首次导入可能需要 5-15 分钟
4. **配置网络**（可选）
   - 如果启动了本地服务器，检查 `Assets/Configs/NetworkConfig` 中的服务器地址
   - 默认连接到 `127.0.0.1:9527`

#### 4. 运行游戏

**单人测试（本地模式）**
- 打开场景 `Assets/Scenes/MainScene`
- 点击 Unity 编辑器顶部的 **Play** 按钮
- 游戏将以单人模式运行（使用 Quantum 本地模拟）

**多人测试（联机模式）**
- 确保 LatServer 正在运行
- 方式 1：使用 ParrelSync 克隆编辑器
  ```
  Unity 菜单 → ParrelSync → Clones Manager → Create new clone
  ```
- 方式 2：构建独立客户端
  ```
  File → Build Settings → Build
  ```
- 在两个客户端中分别登录不同账号，进行匹配对战

---

### 🔧 配置系统

#### 修改游戏配置

配置表位于 `Public/CfgTables/`，使用 Excel 编辑：

1. **编辑配置表**
   - 打开 `unit.xlsx` 修改角色属性
   - 打开 `skill.xlsx` 修改技能数据
   - 打开 `map.xlsx` 修改地图配置

2. **生成代码**
   ```bash
   cd Public
   gen.bat          # 生成客户端配置代码
   gen_server.bat   # 生成服务器配置代码
   ```

3. **重启项目**
   - Unity 会自动重新编译
   - 服务器需要重新构建和启动

#### 修改网络协议

协议定义位于 `Protocal/Proto/`：

1. **编辑 .proto 文件**
   ```protobuf
   // 示例：添加新消息
   message NewFeatureRequest {
     int32 player_id = 1;
     string feature_name = 2;
   }
   ```

2. **编译协议**
   ```bash
   cd Protocal
   protoc --csharp_out=../Client/Assets/Gen/ Proto/*.proto
   protoc --csharp_out=../Server/LatServer/LatProtocol/Protocol/ Proto/*.proto
   ```

3. **注册消息处理**
   - 客户端：在 `NetworkModule` 中注册
   - 服务器：在 `MessageHandlerService` 中添加 Handler

---

### 🐛 常见问题

<details>
<summary><b>Unity 编译错误：找不到 Photon 或 Quantum 命名空间</b></summary>

**解决方案：**
1. 确保 Unity 版本为 2022.3.62f2 或更高
2. 等待所有包导入完成（查看右下角进度条）
3. 如果仍有问题，删除 `Library` 文件夹并重新打开项目
</details>

<details>
<summary><b>服务器启动失败：端口被占用</b></summary>

**解决方案：**
1. 检查端口是否被其他程序占用
   ```bash
   # Windows
   netstat -ano | findstr 9527
   # Linux/Mac
   lsof -i :9527
   ```
2. 修改 `ServerConfig.cs` 中的端口号
3. 或终止占用端口的进程
</details>

<details>
<summary><b>客户端无法连接服务器</b></summary>

**解决方案：**
1. 确认 LatServer 正在运行
2. 检查防火墙是否阻止了 UDP 端口
3. 验证客户端配置的服务器地址和端口
4. 查看服务器日志，确认没有启动错误
</details>

<details>
<summary><b>Luban 配置生成失败</b></summary>

**解决方案：**
1. 确认 Excel 文件格式正确（第一行为字段名，第二行为类型）
2. 检查 `luban.conf` 配置文件路径
3. 确保 .NET 8.0 运行时已安装
4. 查看 `Public/output.log` 获取详细错误信息
</details>

<details>
<summary><b>Quantum 模拟不确定性问题</b></summary>

**解决方案：**
1. 确保所有游戏逻辑在 `QuantumUser/Simulation` 中实现
2. 不要在 Quantum 代码中使用 `UnityEngine.Random` 或 `System.Random`
3. 使用 `RNGSession` 进行随机数生成
4. 避免使用浮点数运算，使用 `FP`（定点数）类型
</details>

---

### 📦 构建发布

#### 客户端构建

```bash
# Unity 菜单
File → Build Settings
  → 选择目标平台（Windows、Mac、Linux）
  → Build
```

**优化建议：**
- 启用 IL2CPP 编译（更好的性能）
- 启用代码剥离（Managed Stripping Level: High）
- 压缩资源（Compression: LZ4）

#### 服务器构建

```bash
cd Server/LatServer/LatServer
dotnet publish -c Release -r win-x64 --self-contained true    # Windows
dotnet publish -c Release -r linux-x64 --self-contained true  # Linux
```

输出目录：`bin/Release/net8.0/<runtime>/publish/`

**部署建议：**
- 使用 systemd（Linux）或 Windows Service 管理服务器进程
- 配置自动重启策略
- 使用 Nginx 进行负载均衡（多服务器部署）
- 监控服务器性能和日志

## 📚 文档

- **[功能特性文档](FEATURES.md)** - 所有功能的详细说明 ⭐
- **[架构变更说明](ARCHITECTURE_CHANGES.md)** - 系统架构演进历史
- **[重构笔记](REFACTORING_NOTES.md)** - 代码重构和迁移指南
- **[架构重构总结](架构重构总结.md)** - 中文版架构总结
- **[性能分析报告](Md/性能分析报告.md)** ⭐ - 项目性能分析和优化建议

### 模块化系统
- [模块化角色系统详解](Md/ModularCharacterSystem.md)
- [角色配置示例](Md/ExampleCharacters.md)
- [系统集成指南](Md/IntegrationGuide.md)
- [系统架构总览](Md/Architecture.md)

## 🎯 开发路线图

- [x] 基础角色移动系统
- [x] 模块化能力组件系统
- [x] 战斗系统（攻击、连招）
- [x] 能力解锁和等级系统
- [x] 确定性网络同步
- [ ] 完整的角色平衡调整
- [ ] 更多角色和能力
- [ ] 游戏模式扩展
- [ ] UI/UX 优化

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

本项目为个人开发项目，详见 LICENSE 文件。

## 👤 作者

**Wangok123**

- GitHub: [@Wangok123](https://github.com/Wangok123)
- Project: [2d_Fighter](https://github.com/Wangok123/2d_Fighter)

## 🌟 致谢

本项目受到以下框架和项目的启发：
- [Photon Quantum](https://www.photonengine.com/quantum) - 确定性游戏引擎
- [GameFramework](https://github.com/EllanJiang/GameFramework) - 游戏框架设计思想
- [ET](https://github.com/egametang/ET) - ECS 架构和事件系统
- Overwatch GDC Presentation - 模块化角色系统设计理念

---

<div align="center">
  Made with ❤️ by Wangok123
</div>
