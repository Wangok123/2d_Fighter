# Unity Editor 优化文档

## 概述

本项目新增了多个自定义 Unity Editor 脚本，用于优化 Ability 和 HitReaction 配置资产的编辑体验。这些编辑器扩展将复杂的配置数据以更直观、更易理解的方式呈现。

## 新增的编辑器脚本

### 1. AbilityDataEditor.cs
**用途**: 为 `AbilityData` 及其子类提供优化的Inspector界面

**优化内容**:
- ⏱ **时序设置**: 输入缓冲、延迟、持续时间、冷却时间
  - 自动显示 FP 值对应的实际秒数（如 `65536 ≈ 1.00s`）
- 🎯 **移动与方向设置**: 施放方向、面向控制、速度保持
- 🔄 **优先级与取消设置**: 技能优先级、打断逻辑
- 🎨 **UI设置**: UI预制体配置

**功能特点**:
- 所有设置分组折叠，界面更清晰
- FP（Fixed Point）数值自动转换为易读的浮点数显示
- 中文标签和详细的工具提示

**适用于**:
- `AbilityData`
- `JumpAbilityData`
- `DashAbilityData`
- `DoubleJumpAbilityData`
- `WallJumpAbilityData`
- `WallSlideAbilityData`
- 所有其他 AbilityData 子类

---

### 2. AttackAbilityDataEditor.cs
**用途**: 为 `AttackAbilityData` 及其子类提供专门的攻击配置界面

**优化内容**:
- 继承 `AbilityDataEditor` 的所有功能
- 🎯 **攻击范围**: 攻击判定形状配置，带形状预览
- ⏱ **攻击时序**: 判定激活时间、持续时间
  - 可视化时间轴，清晰显示启动帧和判定帧
- 💥 **击退设置**: 击退力度和方向配置
  - 2D方向可视化，直观显示击退方向矢量
- 🛑 **硬直设置**: 硬直时间和受击类型

**可视化功能**:
- **时序可视化**: 在时间轴上显示技能各阶段
- **击退方向可视化**: 用箭头图形显示击退方向和大小

**适用于**:
- `AttackAbilityData`
- `ComboAttackAbilityData`
- `ChargeAttackAbilityData`
- 所有其他 AttackAbilityData 子类

---

### 3. ChargeAttackAbilityDataEditor.cs
**用途**: 为 `ChargeAttackAbilityData` 提供专门的蓄力攻击配置界面

**优化内容**:
- 继承 `AttackAbilityDataEditor` 的所有功能
- ⏱ **蓄力时序**: 最小/最大蓄力时间，蓄力时移动控制
  - 可视化时间轴，显示无效区域、可释放区域、满蓄力区域
- ⚔️ **伤害缩放**: 根据蓄力时间的伤害倍率缩放
  - 线性插值曲线可视化
- 💥 **击退缩放**: 可选的击退力度缩放
  - 缩放曲线可视化
  - 禁用时显示提示信息
- 🎨 **视觉效果**: 可选的攻击范围缩放
  - 清晰显示范围增加百分比

**可视化功能**:
- **蓄力时间轴**: 三段式时间轴（太短/可释放/满蓄力）
- **缩放曲线**: 实时显示伤害和击退的缩放曲线
- **智能禁用**: 未启用的功能灰色显示并提示

**适用于**:
- `ChargeAttackAbilityData`

---

### 4. ComboStepConfigDrawer.cs
**用途**: 为 `ComboStepConfig` 提供结构化的属性绘制

**优化内容**:
- 连招每段的配置以折叠组的形式组织
- ⏱ **时序设置**: 持续时间、判定时间
- 💥 **击退设置**: 每段独立的击退配置
- 🛑 **硬直设置**: 每段的硬直时间和类型
- 🎯 **攻击形状**: 每段可以有不同的攻击判定

**功能特点**:
- 自动计算并显示所有 FP 值的实际数值
- 清晰的分组和图标，易于区分不同类型的设置

**适用于**:
- `ComboAttackAbilityData` 中的 `ComboSteps` 数组

---

### 5. HitReactionDataEditor.cs
**用途**: 为 `HitReactionData` 及其子类提供受击反应配置界面

**优化内容**:
- 🎯 **核心标志**: 可被击退、可被硬直的开关
  - 智能提示：根据配置显示不同的状态警告/信息
- 🛑 **硬直设置**: 轻击/重击的硬直倍率配置
  - 清晰的倍率说明（如 `×1.50` 表示增加50%）
- 💥 **击退配置**: 击退配置文件（KnockbackProfile）
  - 根据当前模式显示相应的说明信息
- ⚔️ **战斗行为**: 受击打断动作等行为设置

**智能提示系统**:
- 当禁用所有受击效果时显示警告
- 针对不同的击退模式显示对应的说明
- 针对可能导致异常的配置显示警告

**适用于**:
- `HitReactionData`
- `PlayerHitReactionData`

---

### 6. Shape2DConfigDrawer.cs
**用途**: 为 `Shape2DConfig` 提供智能化的形状配置界面

**优化内容**:
- 根据选择的形状类型，动态显示相关参数
- **支持的形状类型**:
  - ⭕ Circle (圆形): 半径配置 + 圆形预览
  - ▭ Box (矩形): 半尺寸配置 + 矩形预览
  - ⬭ Capsule (胶囊): 宽高配置
  - ⬟ Polygon (多边形): 碰撞器引用
  - ─ Edge (边缘): 延伸长度
  - ⬢ Compound (复合): 子形状列表

**可视化功能**:
- 圆形和矩形带有实时预览（按比例缩放显示）
- 自动计算并显示实际尺寸
- 所有 FP 值自动转换为易读的浮点数

**通用参数**:
- 位置偏移（FPVector2）
- 旋转偏移（FP，度数）
- 用户标签和持久化选项

**实现细节**:
- 使用 `EditorGUI` 手动布局（PropertyDrawer 必须要求）
- 实现了 `GetPropertyHeight()` 动态计算控件高度
- 所有绘制方法返回更新后的 Y 坐标位置

---

### 7. KnockbackCurveProfileDrawer.cs (已存在)
**用途**: 为 `KnockbackCurveProfile` 提供模式切换的属性绘制

**优化内容**:
- 根据击退模式显示对应参数
- **Physics 模式**: 水平衰减率、重力开关
- **CustomCurve 模式**: 曲线持续时间、水平/垂直曲线
- **LinearDecay 模式**: 线性衰减率

---

## 使用方法

### 自动应用
所有自定义编辑器会自动应用到对应的资产类型上。当你在 Unity 编辑器中选择相应的配置资产时，Inspector 面板会自动使用优化后的界面。

### 配置位置
- **Ability 配置**: `Assets/QuantumUser/Resources/DB/Ability/`
- **HitReaction 配置**: `Assets/QuantumUser/Resources/DB/Hit/`

### 编辑技巧

1. **使用折叠组**: 点击折叠组标题展开/折叠相关设置，保持界面整洁

2. **查看实际数值**: 所有 FP 类型的字段右侧都会显示实际的浮点数值，方便理解

3. **利用可视化**: 
   - 攻击时序会显示时间轴
   - 击退方向会显示方向箭头
   - 形状配置会显示形状预览

4. **阅读提示信息**: 编辑器会根据配置显示相应的帮助信息和警告

---

## FP (Fixed Point) 数值说明

本项目使用 Photon Quantum 的 Fixed Point 数学库。FP 值在内部以 `long` 类型的 `RawValue` 存储。

**转换关系**:
```
FP RawValue → Float Value
65536 → 1.0
32768 → 0.5
131072 → 2.0
```

所有自定义编辑器都会自动显示转换后的浮点数，格式为 `≈ X.XXs` 或 `≈ X.XX`。

---

## 代码组织

所有编辑器脚本位于: `Assets/Scripts/Editor/`

```
Editor/
├── AbilityDataEditor.cs                    # 基础技能编辑器
├── AttackAbilityDataEditor.cs              # 攻击技能编辑器
├── ChargeAttackAbilityDataEditor.cs        # 蓄力攻击编辑器
├── ComboStepConfigDrawer.cs                # 连招步骤属性绘制器
├── HitReactionDataEditor.cs                # 受击反应编辑器
├── Shape2DConfigDrawer.cs                  # 形状配置属性绘制器
├── KnockbackCurveProfileDrawer.cs          # 击退曲线属性绘制器
├── OverrideProtodll.cs                     # (原有)
└── Proto2CSEditor.cs                       # (原有)
```

---

## 扩展指南

### 添加新的编辑器

如果需要为其他数据类添加自定义编辑器：

1. **继承现有编辑器** (如果适用):
```csharp
[CustomEditor(typeof(YourDataClass), true)]
public class YourDataEditor : AbilityDataEditor
{
    // 实现自定义逻辑
}
```

2. **创建新的属性绘制器**:
```csharp
[CustomPropertyDrawer(typeof(YourSerializableClass))]
public class YourDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 实现绘制逻辑
    }
}
```

3. **遵循现有模式**:
   - 使用折叠组组织相关字段
   - 为 FP 值提供实际数值显示
   - 添加中文标签和详细的工具提示
   - 使用图标表情符号区分不同分组

---

## 技术细节

### CustomEditor vs CustomPropertyDrawer

- **CustomEditor**: 用于 ScriptableObject 和 MonoBehaviour，完全控制整个 Inspector
  - `AbilityDataEditor`
  - `AttackAbilityDataEditor`
  - `ChargeAttackAbilityDataEditor`
  - `HitReactionDataEditor`

- **CustomPropertyDrawer**: 用于 Serializable 类，绘制嵌套属性
  - `ComboStepConfigDrawer`
  - `Shape2DConfigDrawer`
  - `KnockbackCurveProfileDrawer`

### EditorGUILayout vs EditorGUI

- **EditorGUILayout**: 自动布局，适合复杂界面
  - 用于 **CustomEditor** (ScriptableObject/MonoBehaviour 编辑器)
  - `AbilityDataEditor`, `AttackAbilityDataEditor`, `ChargeAttackAbilityDataEditor`, `HitReactionDataEditor`
  
- **EditorGUI**: 手动布局，需要计算 Rect 位置
  - **必须**用于 **CustomPropertyDrawer** (属性绘制器)
  - `ComboStepConfigDrawer`, `Shape2DConfigDrawer`, `KnockbackCurveProfileDrawer`
  - PropertyDrawer 的 `OnGUI` 方法接收 `Rect position` 参数，必须使用 `EditorGUI` API
  - 需要实现 `GetPropertyHeight()` 来正确计算控件总高度

### 编译指令

所有编辑器脚本使用 `#if UNITY_EDITOR` 包裹，确保不会编译到最终构建中。

---

## 注意事项

1. **只读字段**: 资源标识符（Identifier）字段为只读，防止意外修改

2. **继承层次**: 编辑器会正确处理继承关系，子类编辑器会继承父类功能

3. **实时更新**: 修改会立即应用到资产，但需要保存场景/项目才会持久化

4. **性能**: 所有可视化都经过优化，不会影响编辑器性能

---

## 效果对比

### 优化前
- 所有字段平铺显示
- FP 值显示为难以理解的 RawValue (如 `65536`)
- 没有分组，难以找到相关设置
- 缺少视觉辅助

### 优化后
- ✅ 相关字段分组折叠，界面清晰
- ✅ FP 值自动显示实际数值 (如 `≈ 1.00s`)
- ✅ 中文标签和详细说明
- ✅ 时间轴、方向箭头、形状预览等可视化
- ✅ 智能提示和警告信息

---

## 维护和更新

如需更新编辑器：

1. 修改对应的 `.cs` 文件
2. Unity 会自动重新编译
3. 刷新 Inspector (选择其他对象再选回来)

如遇到编辑器错误：
- 查看 Unity Console 的错误信息
- 检查属性名是否匹配数据类的字段名
- 确保使用了正确的 `SerializedProperty` API

---

## 参考资源

- [Unity Custom Editors](https://docs.unity3d.com/Manual/editor-CustomEditors.html)
- [Unity Property Drawers](https://docs.unity3d.com/Manual/editor-PropertyDrawers.html)
- [Photon Quantum Documentation](https://doc.photonengine.com/quantum)

---

## 更新日志

### 版本 1.1 - 2025-11-10
**修复内容**:
- 修复了 `Shape2DConfigDrawer` 导致的 Unity Editor 错误
- **问题**: 在 `LightAttackAbilityData_Combo` 资产的 ComboSteps 页签中打开时报错：
  - `ArgumentException: Getting control 64's position in a group with only 64 controls when doing repaint`
- **原因**: PropertyDrawer 错误地使用了 `EditorGUILayout` 自动布局 API
- **解决方案**: 
  - 将 `Shape2DConfigDrawer` 中所有 `EditorGUILayout` 调用改为 `EditorGUI` 手动布局
  - 实现了完整的 `GetPropertyHeight()` 方法来正确计算控件高度
  - 所有绘制方法现在使用 `Rect` 位置参数并返回更新后的 Y 坐标
- 更新了 README 文档，明确了 CustomEditor 和 CustomPropertyDrawer 的 API 使用规范

### 版本 1.0 - 2025-11-10
**初始版本**:
- 添加了所有自定义编辑器和属性绘制器
- 优化了 Ability 和 HitReaction 配置资产的编辑体验

---

**版本**: 1.1  
**最后更新**: 2025-11-10
