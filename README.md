# ⚔️ 2D Fighter

> 基于 Photon Quantum 的多人在线 2D 平台格斗游戏

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black.svg?style=flat&logo=unity)](https://unity.com/)
[![Photon Quantum](https://img.shields.io/badge/Photon-Quantum-blue.svg?style=flat)](https://www.photonengine.com/quantum)
[![C#](https://img.shields.io/badge/Language-C%23-239120.svg?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-6.0+-512BD4.svg?style=flat&logo=.net)](https://dotnet.microsoft.com/)

一个采用确定性物理引擎和 ECS 架构的联机格斗游戏，支持流畅的多人实时对战体验。

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

### 客户端
- **游戏引擎**: Unity 2022.3 LTS
- **物理引擎**: Photon Quantum (确定性物理)
- **架构模式**: ECS (Entity Component System) + 自研 LAT 框架
- **资源管理**: YooAsset (支持热更新)
- **配置系统**: Luban
- **对象池**: 双层对象池 (ObjectPool + ReferencePool)

### LAT 框架 (自研)
LAT (龙傲天) 是本项目自研的游戏框架，提供完整的游戏开发基础设施：
- **GameEntry 管理器** - 统一的组件注册和访问
- **LatComponent 系统** - 模块化组件架构
- **ReferencePool** - C# 对象池，减少 GC 压力
- **ObjectPool** - Unity 对象池，高效复用 GameObject
- **事件系统** - 松耦合的模块通信
- **动画系统** - 动画状态管理
- **输入系统** - 统一的输入处理
- **UI 系统** - 界面管理框架

### 服务器
- **网络层**: 自研 LAT 服务器
- **传输协议**: KCP (基于 UDP 的可靠传输)
- **消息格式**: Protocol Buffers
- **开发语言**: C# (.NET 6.0+)

### 核心游戏系统 (Quantum)
- **MovementInputSystem** - 移动输入处理
- **AbilityInputSystem** - 能力输入处理
- **AbilitySystem** - 能力生命周期管理
- **CommandInputSystem** - 组合技输入识别
- **LevelUpSystem** - 等级和能力解锁系统

## 📁 项目结构

```
2d_Fighter/
├── Client/                      # Unity 客户端
│   ├── Assets/
│   │   ├── Scripts/            # 游戏脚本
│   │   ├── QuantumUser/        # Quantum 游戏逻辑
│   │   └── Photon/             # Photon SDK
│   └── Packages/               # Unity 包
├── Server/                      # 自研服务器
│   └── LatServer/
│       ├── LatNet/             # 网络层 (KCP)
│       ├── LatProtocol/        # 协议层
│       └── LatServer/          # 业务逻辑
├── Protocal/                    # Protobuf 协议定义
├── Public/                      # 公共资源
│   ├── CfgTables/              # 配置表
│   └── Luban/                  # 配置工具
├── FEATURES.md                  # 详细功能文档 ⭐
├── ARCHITECTURE_CHANGES.md      # 架构变更说明
└── README.md                    # 本文件
```

## 🚀 快速开始

### 环境要求

**客户端**
- Unity 2022.3 LTS 或更高版本
- .NET Standard 2.1

**服务器**
- .NET 6.0 SDK 或更高版本
- 支持平台：Windows、Linux

### 启动项目

1. **克隆仓库**
   ```bash
   git clone https://github.com/Wangok123/2d_Fighter.git
   cd 2d_Fighter
   ```

2. **启动服务器**（可选）
   ```bash
   cd Server/LatServer/LatServer
   dotnet run
   ```

3. **打开 Unity 项目**
   - 使用 Unity Hub 打开 `Client` 目录
   - 等待项目导入和编译完成
   - 点击 Play 按钮运行游戏

## 📚 文档

- **[功能特性文档](FEATURES.md)** - 所有功能的详细说明 ⭐
- **[架构变更说明](ARCHITECTURE_CHANGES.md)** - 系统架构演进历史
- **[重构笔记](REFACTORING_NOTES.md)** - 代码重构和迁移指南
- **[架构重构总结](架构重构总结.md)** - 中文版架构总结

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
