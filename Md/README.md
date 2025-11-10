# 2D Fighter 项目文档

本目录包含 2D Fighter 项目的技术文档。

## 📚 文档列表

### 核心系统文档

- **[模块化角色系统详解](./ModularCharacterSystem.md)** - ECS 风格的角色能力组件系统
- **[角色配置示例](./ExampleCharacters.md)** - 模块化角色配置案例和复用统计
- **[系统集成指南](./IntegrationGuide.md)** - 模块化系统集成步骤详解
- **[系统架构总览](./Architecture.md)** - 模块化系统架构图和数据流
- **[模块化角色系统简介](./模块化角色系统-简介.md)** - 中文版系统简介
- **[性能分析报告](./性能分析报告.md)** ⭐ - 项目性能分析和优化建议

### Core 模块文档

位于 [Core/](./Core/) 目录：

- **动画状态管理系统** - 简化管理大量动画状态
- **动画系统迁移指南** - 从现有 Animator 迁移指南
- **动画状态机问题解决方案** - 常见问题和解决方法

### 游戏系统文档

位于 [Systems/](./Systems/) 目录：

- 战斗系统文档
- 其他游戏系统文档

### PublicLib 文档

位于 [PublicLib/](./PublicLib/) 目录：

- 公共工具库说明
- Timer 工具
- FixedNum 工具

## 🎯 快速导航

### 我要创建新角色
1. 阅读 [模块化角色系统详解](./ModularCharacterSystem.md)
2. 参考 [角色配置示例](./ExampleCharacters.md)
3. 按照 [系统集成指南](./IntegrationGuide.md) 操作

### 我要了解系统架构
- 查看 [系统架构总览](./Architecture.md)
- 查看根目录的 [ARCHITECTURE_CHANGES.md](../ARCHITECTURE_CHANGES.md)
- 查看根目录的 [架构重构总结.md](../架构重构总结.md)

### 我要了解所有功能
- 查看根目录的 [FEATURES.md](../FEATURES.md) ⭐

### 我要了解性能和优化
- 查看 [性能分析报告](./性能分析报告.md) ⭐

## 📁 目录结构

```
Md/
├── README.md                        # 本文件
├── ModularCharacterSystem.md       # 模块化角色系统（英文）
├── ExampleCharacters.md            # 角色示例
├── IntegrationGuide.md             # 集成指南
├── Architecture.md                 # 架构总览
├── 模块化角色系统-简介.md           # 模块化系统简介（中文）
├── 性能分析报告.md                  # 性能分析和优化建议 ⭐
├── Core/                            # 核心模块文档
│   ├── 概述.md
│   ├── 动画状态管理系统.md
│   ├── 动画状态管理系统-迁移指南.md
│   └── 动画状态机问题解决方案.md
├── PublicLib/                       # 公共库文档
│   ├── 概述.md
│   ├── Timer/
│   └── FixedNum/
├── Systems/                         # 游戏系统文档
│   └── README.md
└── images/                          # 文档图片资源
```

## 🔄 文档更新

文档会随项目开发持续更新。

## 📞 联系方式

- GitHub: [Wangok123](https://github.com/Wangok123)
- 项目地址: https://github.com/Wangok123/2d_Fighter

---

**最后更新**: 2024-11-10
