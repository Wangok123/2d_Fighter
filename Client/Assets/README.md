# 🎮 Client 项目

基于 Unity 2022.3 + Quantum 的多人在线游戏客户端项目。

## 📋 项目信息

- **Unity 版本**：2022.3
- **渲染管线**：Built-in (2D)
- **网络方案**：Quantum + KCPNet
- **资源管理**：YooAsset 2.3.2
- **配置系统**：Luban
- **消息协议**：Protobuf

## 🚀 快速开始

### 环境要求

- Unity 2022.3.x
- .NET Standard 2.1
- Git

### 项目结构

```
Client/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # 核心框架（纯C#）
│   │   ├── UnityCore/         # Unity层功能
│   │   ├── QuantumUser/       # Quantum游戏逻辑
│   │   └── Gen/               # 自动生成代码
│   ├── AssetPackage/          # 打包资源
│   └── Scenes/                # Unity场景
├── Pages/                     # 项目规范文档（Bezi Pages）
└── README.md                  # 本文件
```

### 初次运行

1. 克隆项目
2. 使用 Unity 2022.3 打开项目
3. 打开场景 `Assets/Scenes/EntryScene.unity`
4. 点击 Play 运行

## 📚 开发规范

完整的开发规范文档位于 `/Pages` 目录，在 Bezi 中查看。

### 核心规范

- **[框架架构规范](Pages/框架架构规范%20(Framework%20Architecture).md)** - Component/Module 架构、生命周期管理
- **[项目结构规范](Pages/项目结构规范%20(Project%20Structure).md)** - 目录组织、资源分类
- **[编码规范](Pages/编码规范%20(Coding%20Guidelines).md)** - 命名、代码风格

### 系统规范

- **[事件系统规范](Pages/事件系统规范%20(Event%20System%20Guidelines).md)** - 事件定义、订阅、发布
- **[资源加载规范](Pages/资源加载规范%20(Resource%20Loading%20Guidelines).md)** - YooAsset 使用、资源管理
- **[UI开发规范](Pages/UI开发规范%20(UI%20Development%20Guidelines).md)** - UI 生命周期、事件绑定
- **[网络协议规范](Pages/网络协议规范%20(Network%20Protocol%20Guidelines).md)** - Protobuf 消息、网络通信
- **[配置系统规范](Pages/配置系统规范%20(Configuration%20System%20Guidelines).md)** - Luban 配置管理

### Quantum 规范

- **[Quantum开发规范](Pages/Quantum开发规范%20(Quantum%20Development).md)** - 面向数据编程、System/Signal/Event

### 📖 完整索引

查看 **[项目规范总览](Pages/📖%20项目规范总览%20(Project%20Rules%20Overview).md)** 获取所有规范的导航和快速参考。

## 🏗️ 核心架构

### 全局访问入口

项目使用 `Game` 静态门面提供统一访问：

```csharp
// UI
Game.UI.OpenUI("LoginWnd", userData);

// 事件
Game.Event.Subscribe<BattleStartEventArgs>(OnBattleStart);
Game.Event.Fire(this, new BattleStartEventArgs());

// 资源
Game.YooAsset.LoadGameObjectAsync("Prefabs/Player", OnLoaded);

// 网络
Game.Network.SendMsg(MessageID.LoginRequest, request);

// 配置
var cfg = Game.Config.Tables.TbUnit.Get(unitId);
```

### Component & Module 架构

- **Component**（Unity层）：继承 `LatComponent`，负责 MonoBehaviour 集成
- **Module**（逻辑层）：继承 `CoreModule`，负责业务逻辑实现

```csharp
// 访问 Component
Game.UI.OpenUI("LoginWnd");

// 访问 Module
var manager = GameModuleManager.GetModule<GameFlowManager>();
```

## 🔧 常用工具

### 编辑器菜单

- `Tools/生成配置` - 生成 Luban 客户端配置
- `Tools/生成服务器配置` - 生成 Luban 服务器配置

### 代码生成

- **Protobuf**：`/Assets/Scripts/ProtoMessage/` - 网络消息
- **Luban**：`/Assets/Scripts/Gen/latcfg/` - 配置数据

## ⚠️ 重要约定

### ✅ 必须遵守

1. **通过 `Game` 访问组件**，禁止直接 `FindObjectOfType`
2. **Component 继承 `LatComponent`**，调用 `base.Awake()`
3. **Module 继承 `CoreModule`**，实现 `Update()` 和 `Shutdown()`
4. **UI 继承 `UIFormLogic`**，在 `OnClose` 中解绑事件
5. **使用 YooAsset 加载资源**，禁止 `Resources.Load`

### ❌ 禁止事项

1. ❌ 禁止使用单例模式（框架已提供统一管理）
2. ❌ 禁止在 Component 中编写复杂业务逻辑
3. ❌ 禁止在 Module 中持有 MonoBehaviour 引用
4. ❌ 禁止忘记取消事件订阅（会导致内存泄漏）
5. ❌ 禁止在 Quantum 中使用面向对象思维

## 🐛 常见问题

### YooAsset 未初始化

**问题**：加载资源失败，提示 YooAsset 未初始化

**解决**：
```csharp
yield return new WaitUntil(() => Game.YooAsset != null && Game.YooAsset.IsInit);
```

### 配置读取为空

**问题**：`Game.Config.Tables` 为 null

**解决**：确保在 `BaseComponent` 初始化完成后再访问配置

### 事件订阅泄漏

**问题**：UI 关闭后仍收到事件

**解决**：在 `OnClose` 中调用 `Game.Event.Unsubscribe`

## 📦 依赖包

### 核心依赖

- **YooAsset** 2.3.2 - 资源管理
- **Luban** - 配置系统
- **TextMeshPro** 3.0.6 - 文本渲染
- **Input System** 1.7.0 - 输入管理

### 自定义包

- **wjybxx.commons.core** - 工具库
- **wjybxx.btree.core** - 行为树

## 👥 团队协作

### 分支管理

- `main` - 主分支（稳定版本）
- `dev` - 开发分支
- `feature/*` - 功能分支

### 提交规范

```
feat: 新增XX功能
fix: 修复XX问题
docs: 更新XX文档
refactor: 重构XX模块
perf: 优化XX性能
```

### Code Review 检查点

- [ ] 是否遵循框架架构规范
- [ ] 是否遵循编码规范
- [ ] 是否有内存泄漏（事件订阅）
- [ ] 是否有性能问题
- [ ] 是否有充分的错误处理

## 📞 联系方式

- **技术负责人**：[联系方式]
- **文档维护**：[联系方式]
- **问题反馈**：[Issue 地址]

## 📄 许可证

[项目许可证信息]

---

**最后更新**：2024年

**文档版本**：v1.0
