# 数据类编辑器扩展文档 / Data Class Editor Extensions Documentation

## 概述 / Overview

本文档说明了为项目中的 ScriptableObject 数据类创建的自定义编辑器扩展。这些编辑器提供了丰富的可视化界面，使配置数据的编辑更加直观和高效。

This document describes the custom editor extensions created for ScriptableObject data classes in the project. These editors provide rich visualization interfaces to make editing configuration data more intuitive and efficient.

## 版本信息 / Version

- **版本 / Version**: 2.1
- **最后更新 / Last Updated**: 2025-11-11
- **新增编辑器 / New Editors**: 4

---

## 新增的数据类编辑器 / New Data Class Editors

### 1. AttackDefinitionEditor.cs

**用途 / Purpose**: 为 `AttackDefinition` 提供战斗攻击配置的可视化编辑器

**适用类 / Applicable Classes**:
- `Combat.AttackDefinition`

**主要功能 / Key Features**:

#### 📊 伤害可视化 / Damage Visualization
- **伤害范围条** / Damage Range Bar
  - 渐变色显示从最小到最大伤害
  - 颜色编码：绿色（低）→ 黄色（中）→ 橙色（高）→ 红色（极高）
  - 显示平均伤害值
  
#### ⏱️ 冷却时间 / Cooldown Time
- 时间值的颜色编码显示
- 进度条可视化（0-10秒范围）
- 实时秒数显示

#### 🎯 攻击范围 / Attack Range
- 自动分类：近战（<2m）、中距离（2-5m）、远程（>5m）
- 颜色编码的范围类型指示器
- 范围值实时显示

#### 💥 暴击系统 / Critical Hit System
- 暴击倍率可视化（×1.0 - ×5.0）
- 暴击概率进度条（0-100%）
- 自动计算暴击平均伤害
- 期望伤害计算

#### 📈 性能分析 / Performance Analysis
- DPS（每秒伤害）自动计算
- 期望伤害显示
- 输出评估：高/中/低
- 综合性能评级

**使用示例 / Usage Example**:
```
选择任何 AttackDefinition ScriptableObject 资产，
Inspector 面板会自动显示增强的可视化界面。
```

---

### 2. AnimationStateConfigEditor.cs

**用途 / Purpose**: 为 `AnimationStateConfig` 提供动画状态管理的可视化编辑器

**适用类 / Applicable Classes**:
- `UnityCore.AnimationSystem.AnimationStateConfig`

**主要功能 / Key Features**:

#### 🎬 自动生成 / Auto-Generation
- 从 Animator Controller 自动生成动画状态
- 一键提取所有 Animation Clip
- 自动设置默认参数

#### ⭐ 默认状态管理 / Default State Management
- 默认状态验证和高亮显示
- 自动检查默认状态是否存在
- 绿色星标标记默认状态

#### 🎭 状态卡片 / State Cards
- 每个动画状态独立可折叠卡片
- 彩色图层徽章显示
- 状态名称、层级、过渡时间
- 快速删除按钮

#### ⏱️ 过渡时间可视化 / Crossfade Visualization
- 颜色编码的过渡时间
- 进度条显示
- 平均过渡时间统计

#### 📊 统计信息 / Statistics
- 总状态数
- 使用的层级数
- 平均过渡时间
- 配置完整性检查

**使用示例 / Usage Example**:
```csharp
1. 创建或选择 AnimationStateConfig 资产
2. 拖入 Animator Controller
3. 点击"自动生成"按钮
4. 设置默认状态名称
5. 调整各状态的参数
```

---

### 3. PlayerCfgSOEditor.cs

**用途 / Purpose**: 为 `PlayerCfgSO` 提供玩家配置的全面可视化编辑器

**适用类 / Applicable Classes**:
- `Configs.PlayerCfgSO`

**主要功能 / Key Features**:

#### 🏃 移动设置 / Movement Settings
- **移动速度可视化** / Move Speed Visualization
  - 速度评估：快速/中等/缓慢
  - 颜色编码的速度条
  - 速度进度条（0-15 m/s）

- **跳跃系统** / Jump System
  - 跳跃力度显示
  - 预计跳跃高度计算（基于物理公式）
  - 空中速度倍率可视化
  - 空中实际速度计算

#### ⚡ 冲刺设置 / Dash Settings
- **冲刺可视化** / Dash Visualization
  - 冲刺距离计算和显示
  - 箭头图形显示冲刺轨迹
  - 绿色起点标记
  - 距离数值标注

- **冷却管理** / Cooldown Management
  - 冷却时间进度条
  - 颜色编码的时间值

#### 🧗 蹬墙/滑墙设置 / Wall Slide Settings
- **滑墙速度** / Slide Speed
  - 速度倍率可视化
  - 推荐值提示（0.5-0.8）

- **蹬墙跳可视化** / Wall Jump Visualization
  - 2D 力度方向图
  - 红色横向分量
  - 绿色纵向分量
  - 黄色合力箭头
  - 力度数值显示

#### ⚔️ 攻击设置 / Attack Settings
- **连招系统** / Combo System
  - 连招重置时间显示
  - 连招段数统计
  
- **攻击位移** / Attack Movement
  - 每段攻击的位移向量
  - 方向箭头显示（↑↓←→）
  - 位移大小和颜色编码

#### 📈 配置总结 / Configuration Summary
- 基础移动速度
- 冲刺速度及倍率
- 单次冲刺距离
- 连招段数
- **机动性评估**: 极高/高/中等/低

**使用示例 / Usage Example**:
```
1. 调整移动速度和跳跃力度
2. 观察预计跳跃高度的实时变化
3. 配置冲刺参数，查看冲刺距离可视化
4. 设置蹬墙跳力度，查看力度方向图
5. 配置攻击位移序列
6. 查看配置总结的机动性评估
```

---

### 4. GameSceneSOEditor.cs

**用途 / Purpose**: 为 `GameSceneSO` 及其子类提供场景配置的可视化编辑器

**适用类 / Applicable Classes**:
- `UnityCore.SceneManagement.GameSceneSO`
- `UnityCore.SceneManagement.YooAssetSceneSO`
- `UnityCore.SceneManagement.UISceneSO`
- `UnityCore.SceneManagement.PersistentManagersSO`

**主要功能 / Key Features**:

#### 🎬 类型识别 / Type Recognition
- **自动识别场景类型** / Auto Type Detection
  - YooAsset 场景：📦 蓝色主题
  - UI 场景：🎨 紫色主题
  - 持久场景：🔒 橙色主题
  - 基础场景：🎬 默认主题

#### 🎯 场景引用管理 / Scene Reference Management
- Addressable Asset 引用显示
- 场景配置状态徽章
- Asset GUID 显示
- 配置状态验证（已配置/未配置）

#### 🔑 GUID 管理 / GUID Management
- 唯一标识符显示
- 一键复制 GUID
- 自动生成说明
- 可选择文本字段

#### 📖 类型信息 / Type Information
- 脚本类型显示
- 基类信息
- 资产名称
- **类型说明**：每种场景类型的详细描述

#### ⚡ 快速操作 / Quick Actions
- 定位资产按钮
- 打开场景按钮（带状态检查）
- 快速导航功能

**场景类型说明 / Scene Type Descriptions**:

| 类型 | 说明 |
|------|------|
| **YooAssetSceneSO** | 通过 YooAsset 资源管理系统加载的场景，支持热更新和异步加载 |
| **UISceneSO** | 管理 UI 相关的场景，通常包含 Canvas、EventSystem 等 UI 组件 |
| **PersistentManagersSO** | 在游戏运行期间始终保持加载的场景，包含全局管理器等持久化对象 |
| **GameSceneSO** | 基础游戏场景配置，使用 Addressable Assets 系统进行资源管理 |

**使用示例 / Usage Example**:
```
1. 创建或选择场景配置资产
2. 拖入场景的 AssetReference
3. 查看场景配置状态徽章
4. 复制 GUID 用于代码引用
5. 使用快速操作定位或打开场景
```

---

## 统一设计模式 / Unified Design Patterns

所有编辑器都遵循以下统一设计模式：

### 🎨 颜色编码系统 / Color Coding System

#### 时序颜色 / Timing Colors
```
< 0.1秒  → 红色   (极短/可能有问题)
0.1-1秒  → 绿色   (合理范围)
1-5秒    → 黄色   (中等长度)
> 5秒    → 橙色   (较长)
```

#### 数值颜色 / Value Colors
```
低值     → 绿色
中值     → 黄色
高值     → 橙色
极高值   → 红色
```

#### 状态颜色 / Status Colors
```
有效     → 绿色   ✓
警告     → 黄色   ⚠
错误     → 红色   ✗
禁用     → 灰色   -
```

### 📊 可视化元素 / Visualization Elements

1. **进度条 / Progress Bars**
   - 用于显示百分比、时间、距离等
   - 带标签的颜色填充
   - 背景、边框、文本

2. **徽章 / Badges**
   - 用于状态标识
   - 彩色背景，白色文本
   - 紧凑的信息显示

3. **渐变条 / Gradient Bars**
   - 用于范围显示
   - 从一个颜色过渡到另一个颜色
   - 支持多段渐变

4. **方向图 / Direction Diagrams**
   - 用于矢量显示
   - 箭头图形
   - 分量分解

5. **信息面板 / Info Panels**
   - 彩色背景盒子
   - 分组相关信息
   - 可折叠的章节

### 🔧 使用 CustomEditorStyles

所有编辑器都使用 `CustomEditorStyles` 类来保持视觉一致性：

```csharp
// 绘制标题
CustomEditorStyles.DrawHeader("配置名称", CustomEditorStyles.Icons.Config);

// 绘制子标题
CustomEditorStyles.DrawSubHeader("子章节", CustomEditorStyles.Icons.Info);

// 绘制彩色数值
Color color = CustomEditorStyles.GetTimingColor(timeValue);
CustomEditorStyles.DrawColoredValue($"{timeValue:F2}s", color);

// 绘制进度条
Rect rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
CustomEditorStyles.DrawProgressBar(rect, progress, color, label);

// 绘制徽章
CustomEditorStyles.DrawBadge(rect, "状态", backgroundColor);

// 彩色盒子
CustomEditorStyles.BeginColoredBox(color);
// ... 内容 ...
CustomEditorStyles.EndColoredBox();
```

---

## 文件组织 / File Organization

```
Assets/Scripts/Editor/
├── CustomEditorStyles.cs                   # 统一样式和颜色库
│
├── [Ability Editors - 已有]
├── AbilityDataEditor.cs
├── AttackAbilityDataEditor.cs
├── ChargeAttackAbilityDataEditor.cs
├── CommandAttackAbilityDataEditor.cs
├── ProjectileDataEditor.cs
├── HitReactionDataEditor.cs
│
├── [Property Drawers - 已有]
├── ComboStepConfigDrawer.cs
├── Shape2DConfigDrawer.cs
├── KnockbackCurveProfileDrawer.cs
│
├── [新增的数据类编辑器]
├── AttackDefinitionEditor.cs               # 攻击定义编辑器
├── AnimationStateConfigEditor.cs           # 动画状态配置编辑器
├── PlayerCfgSOEditor.cs                    # 玩家配置编辑器
├── GameSceneSOEditor.cs                    # 场景配置编辑器
│
└── [文档]
    ├── README_EDITOR_OPTIMIZATION.md       # 编辑器优化文档
    ├── VISUAL_ENHANCEMENTS_SUMMARY.md      # 可视化增强总结
    └── DATA_EDITOR_EXTENSIONS.md           # 本文档
```

---

## 开发指南 / Development Guidelines

### 创建新编辑器的步骤 / Steps to Create New Editor

1. **继承正确的基类** / Inherit Correct Base Class
   ```csharp
   [CustomEditor(typeof(YourDataClass), true)]
   public class YourDataEditor : Editor
   ```

2. **使用 CustomEditorStyles** / Use CustomEditorStyles
   - 统一的颜色和样式
   - 标准的绘制方法
   - 一致的视觉效果

3. **组织内容** / Organize Content
   - 使用折叠组分隔章节
   - 相关字段放在一起
   - 提供清晰的标题和图标

4. **添加可视化** / Add Visualizations
   - 进度条用于百分比和范围
   - 图表用于趋势和关系
   - 颜色编码用于快速识别

5. **提供反馈** / Provide Feedback
   - 验证输入值
   - 显示警告和错误
   - 提供有用的提示信息

6. **性能优化** / Performance Optimization
   - 缓存 SerializedProperty
   - 避免每帧重复计算
   - 使用条件渲染

### 最佳实践 / Best Practices

✅ **应该做的 / Do**:
- 使用 CustomEditorStyles 保持一致性
- 添加中英双语标签和说明
- 提供实时数值计算
- 验证配置的合理性
- 使用折叠组组织复杂界面
- 添加有用的图标和表情符号

❌ **不应该做的 / Don't**:
- 硬编码颜色值
- 创建新的样式而不复用
- 忽略边界情况验证
- 创建过于复杂的界面
- 忘记添加注释和文档

---

## 性能考虑 / Performance Considerations

### GUIStyle 缓存 / GUIStyle Caching
- CustomEditorStyles 缓存所有常用样式
- 避免每帧创建新的 GUIStyle 对象
- 减少 GC 压力

### 条件渲染 / Conditional Rendering
- 使用折叠组隐藏不需要的内容
- 减少每帧绘制的控件数量
- 提升编辑器响应速度

### SerializedProperty 缓存 / SerializedProperty Caching
- 在 OnEnable() 中缓存所有属性
- 避免重复调用 FindProperty()
- 提高属性访问速度

---

## 测试检查清单 / Testing Checklist

创建新编辑器后，请检查以下项目：

- [ ] 编辑器在 Unity 中正确显示
- [ ] 所有属性都能正确编辑
- [ ] 可视化元素正确渲染
- [ ] 颜色编码符合规范
- [ ] 折叠状态正确保存
- [ ] 没有控制台错误或警告
- [ ] 性能表现良好（无卡顿）
- [ ] 中英文标签都正确显示
- [ ] 验证逻辑正常工作
- [ ] 帮助信息清晰有用

---

## 故障排除 / Troubleshooting

### 常见问题 / Common Issues

1. **编辑器不显示 / Editor Not Showing**
   - 检查 `#if UNITY_EDITOR` 指令
   - 确认 CustomEditor 特性的目标类型正确
   - 刷新 Unity 编辑器

2. **属性为 null / Property is Null**
   - 检查属性名称拼写
   - 确认属性在目标类中存在
   - 使用 serializedObject.FindProperty() 查找

3. **布局错误 / Layout Errors**
   - 确保 BeginHorizontal/EndHorizontal 配对
   - 检查 BeginColoredBox/EndColoredBox 配对
   - 避免在 EditorGUILayout 中混用 EditorGUI

4. **颜色不一致 / Inconsistent Colors**
   - 始终使用 CustomEditorStyles.Colors
   - 不要硬编码颜色值
   - 使用颜色获取方法（GetTimingColor, GetDamageColor）

---

## 未来改进 / Future Improvements

### 可能的增强 / Possible Enhancements

1. **更多数据类编辑器** / More Data Class Editors
   - BuffData 编辑器
   - StatusEffect 编辑器
   - EquipmentData 编辑器

2. **高级可视化** / Advanced Visualizations
   - 3D 预览窗口
   - 动画时间轴编辑器
   - 技能连招可视化编辑器

3. **批量编辑工具** / Batch Edit Tools
   - 多资产同时编辑
   - 批量参数调整
   - 配置模板系统

4. **导入导出功能** / Import/Export Features
   - JSON 导入导出
   - Excel 批量导入
   - 配置预设系统

---

## 参考资源 / References

- [Unity Custom Editors](https://docs.unity3d.com/Manual/editor-CustomEditors.html)
- [Unity Property Drawers](https://docs.unity3d.com/Manual/editor-PropertyDrawers.html)
- [Unity EditorGUILayout](https://docs.unity3d.com/ScriptReference/EditorGUILayout.html)
- [Unity Handles](https://docs.unity3d.com/ScriptReference/Handles.html)

---

## 更新日志 / Changelog

### 版本 2.1 - 2025-11-11
**新增编辑器 / New Editors**:
- ✨ AttackDefinitionEditor - 攻击定义可视化编辑器
- ✨ AnimationStateConfigEditor - 动画状态配置编辑器
- ✨ PlayerCfgSOEditor - 玩家配置编辑器
- ✨ GameSceneSOEditor - 场景配置编辑器

**功能特点 / Features**:
- 丰富的可视化元素（进度条、图表、徽章）
- 实时数值计算和显示
- 智能颜色编码系统
- 配置验证和反馈
- 完整的中英双语支持

### 版本 2.0 - 2025-11-10
- 初始发布，包含 Ability 和 HitReaction 编辑器
- CustomEditorStyles 统一样式库
- 大量可视化增强

---

**版本 / Version**: 2.1  
**最后更新 / Last Updated**: 2025-11-11  
**作者 / Author**: GitHub Copilot Coding Agent
