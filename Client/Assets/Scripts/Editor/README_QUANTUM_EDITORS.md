# Quantum DB 资源自定义编辑器说明
# Custom Editors for Quantum DB Assets

## 概述 (Overview)

本项目为 `Assets/QuantumUser/Resources/DB` 中的 Quantum 配置资源创建了自定义 Unity Inspector 编辑器，使这些配置在编辑器中更加直观和易于使用。

This project provides custom Unity Inspector editors for Quantum configuration assets in `Assets/QuantumUser/Resources/DB`, making them more intuitive and user-friendly in the editor.

## 功能特性 (Features)

### 1. 双语支持 (Bilingual Support)
- 所有标签都提供中英文双语显示
- All labels display in both Chinese and English

### 2. 颜色编码分组 (Color-Coded Sections)
- 不同配置部分使用不同颜色区分，一目了然
- Different configuration sections use distinct colors for easy identification

### 3. 固定点数值预览 (Fixed-Point Value Preview)
- 在 FP 类型字段旁边实时显示浮点数近似值
- Real-time float approximation displayed next to FP type fields
- 示例: `RawValue: 655360` → `≈ 10.00`

### 4. 智能提示 (Helpful Tooltips)
- 所有字段都包含详细的工具提示
- All fields include detailed tooltips
- 关键配置提供信息框说明
- Info boxes for critical configurations

## 编辑器列表 (Editor List)

### 总览 (Overview)
本项目共提供 **7个** 自定义编辑器，覆盖所有主要的 Quantum 资源类型。

This project provides **7** custom editors covering all major Quantum asset types.

### 1. StatusDataEditor.cs
**用途**: 角色状态数据（CharacterStatusData.asset）

**配置分组**:
- 🔴 生命值设置 (Health Settings)
  - 最大生命值 (Max Health)
  - 最小伤害值 (Minimum Damage)
  
- 🟢 生命恢复设置 (Regeneration Settings)
  - 恢复延迟 (Time Until Regen)
  - 恢复速度 (Regen Rate)
  
- 🔵 重生设置 (Respawn Settings)
  - 重生时间 (Respawn Time)
  - 无敌时间 (Invincible Time)
  
- 🟡 断线设置 (Disconnect Settings)
  - 断线超时 (Time To Disconnect)

**特色功能**:
- 运行时实时数值显示
- Real-time value display during play mode

---

### 2. GameSettingsDataEditor.cs
**用途**: 游戏全局设置（GameSettingsData.asset）

**配置分组**:
- 🟣 物理层设置 (Physics Layer Settings)
  - 玩家层遮罩 (Player Layer Mask)
  - 已选择层列表显示 (Selected layers list)

**特色功能**:
- 自动显示所有选中的物理层名称
- Automatically displays all selected physics layer names

---

### 3. PlayerMovementDataEditor.cs
**用途**: 玩家移动数据（PlayerMovementData.asset）

**配置分组**:
- 🟢 角色控制器设置 (Character Controller Settings)
  - 默认 KCC2D 配置 (Default KCC2D Config)
  - 资源关联状态提示 (Asset link status)

**特色功能**:
- 显示 KCC2D 配置是否正确关联
- Shows if KCC2D config is properly linked
- 提供详细的配置说明信息
- Provides detailed configuration information

---

### 4. AbilityDataEditor.cs
**用途**: 技能数据（AbilityData 及其所有子类）

**配置分组**:
- 🟠 时机设置 (Timing Settings)
  - 输入缓冲、延迟、持续时间、冷却时间
  - Input buffer, delay, duration, cooldown
  
- 🔵 方向设置 (Direction Settings)
  - 施放方向类型、面向设置
  - Cast direction type, facing settings
  
- 🔴 优先级设置 (Priority Settings)
  - 优先级、可打断性
  - Priority, cancellability
  
- 🟢 移动设置 (Movement Settings)
  - 移动禁用选项
  - Movement disable options
  
- 🟣 UI设置 (UI Settings)
  - UI预制体引用
  - UI prefab reference

**支持的技能类型**:
- JumpAbility (跳跃)
- DoubleJumpAbility (二段跳)
- DashAbility (冲刺)
- WallJumpAbility (蹬墙跳)
- WallSlideAbility (蹬墙滑行)
- LightAttackAbility / ComboAttackAbility (轻攻击/连击)
- HeavyAttackAbility / ChargeAttackAbility (重攻击/蓄力)
- CommandAttackAbility (指令攻击)

**特色功能**:
- 自动识别技能类型并显示对应的中文名称
- Automatically identifies ability type and shows Chinese name
- 所有 FP 类型字段实时显示秒数
- All FP fields show real-time seconds value
- 自动处理派生类的特殊属性
- Automatically handles special properties of derived classes

---

### 5. ProjectileDataEditor.cs
**用途**: 弹道数据（ProjectileData 及其所有子类）

**配置分组**:
- 🔵 基础设置 (Basic Settings)
  - 生命周期、视觉原型
  - Lifetime, visual prototype
  
- 🔴 伤害设置 (Damage Settings)
  - 基础伤害、受击硬直时间
  - Base damage, hitstun duration
  
- 🟠 击退设置 (Knockback Settings)
  - 击退力度、击退类型、击退方向
  - Knockback force, type, direction
  
- 🟢 碰撞设置 (Collision Settings)
  - 碰撞形状、碰撞层、穿透设置
  - Collision shape, layer, pierce settings

**支持的弹道类型**:
- StraightProjectile (直线弹道)
- ArcProjectile (抛物线弹道)
- HomingProjectile (追踪弹道)
- BoomerangProjectile (回旋弹道)
- GrenadeProjectile (手榴弹弹道)

**特色功能**:
- 击退类型详细说明信息框
- Detailed info box for knockback types
- 固定击退方向的向量预览
- Vector preview for fixed knockback direction
- 穿透设置的条件显示
- Conditional display of pierce settings

---

### 6. HitReactionDataEditor.cs
**用途**: 受击反应数据（HitReactionData 及其子类）

**配置分组**:
- 🟠 核心标志 (Core Flags)
  - 可被击退 (Can Be Knocked Back)
  - 可被硬直 (Can Be Hitstunned)

**特色功能**:
- 详细的受击反应机制说明
- Detailed hit reaction mechanism explanation
- 自动处理派生类的扩展属性
- Automatically handles extended properties of derived classes

---

### 7. SkillFieldDataEditor.cs
**用途**: 技能场数据（SkillFieldData 及其子类）

**配置分组**:
- 🔵 基础设置 (Basic Settings)
  - 持续时间 (Duration)
  - Tick间隔 (Tick Interval)
  - 视觉原型 (Visual Prototype)
  
- 🟢 效果范围 (Effect Range)
  - 范围形状 (Effect Area)
  - 影响层 (Target Layer)
  - 影响友军/敌人 (Affect Allies/Enemies)
  
- 🔴 伤害设置 (Damage Settings)
  - 每次Tick伤害 (Damage Per Tick)
  - 受击硬直时间 (Hitstun Duration)
  
- 🟠 击退设置 (Knockback Settings)
  - 击退力度 (Knockback Force)
  - 击退方向 (Knockback Direction)

**支持的技能场类型**:
- DamageField (伤害场)
- HealField (治疗场)
- SlowField (减速场)
- PushField (推力场)
- VortexField (漩涡场)
- DelayedExplosionField (延迟爆炸场)

**特色功能**:
- 详细的技能场机制说明
- Detailed skill field mechanism explanation
- Tick间隔和伤害的实时预览
- Real-time preview of tick interval and damage
- 向量方向的可视化预览
- Visual preview of vector directions
- 自动处理派生类的特殊属性
- Automatically handles special properties of derived classes

---

## 使用方法 (Usage)

1. **打开Unity编辑器** (Open Unity Editor)
   - 打开项目 (Open the project)

2. **选择资源** (Select Asset)
   - 在 Project 窗口中导航至 `Assets/QuantumUser/Resources/DB`
   - Navigate to `Assets/QuantumUser/Resources/DB` in the Project window
   - 点击任意配置资源 (.asset 文件)
   - Click any configuration asset (.asset file)

3. **查看Inspector** (View Inspector)
   - Inspector 窗口会自动显示自定义编辑器
   - The Inspector window will automatically show the custom editor
   - 享受更直观的配置界面！
   - Enjoy the more intuitive configuration interface!

## 技术细节 (Technical Details)

### 编辑器实现原理 (Editor Implementation)

所有编辑器都使用 Unity 的 `CustomEditor` 特性：

```csharp
[CustomEditor(typeof(YourAssetType), true)]
public class YourAssetEditor : Editor
{
    // true 参数表示也应用于派生类
    // true parameter means it also applies to derived classes
}
```

### FP 值显示 (FP Value Display)

Quantum 使用固定点数（Fixed Point）表示精确的数值：

```csharp
SerializedProperty rawValueProp = property.FindPropertyRelative("RawValue");
long rawValue = rawValueProp.longValue;
FP fpValue = FP.FromRaw(rawValue);
float displayValue = fpValue.AsFloat;
```

### 颜色方案 (Color Scheme)

- 🔴 红色 (Red): 伤害、生命值相关 (Damage, Health-related)
- 🟢 绿色 (Green): 移动、恢复相关 (Movement, Regeneration-related)
- 🔵 蓝色 (Blue): 基础、核心设置 (Basic, Core settings)
- 🟠 橙色 (Orange): 时机、击退相关 (Timing, Knockback-related)
- 🟡 黄色 (Yellow): 特殊、状态相关 (Special, Status-related)
- 🟣 紫色 (Purple): UI、额外功能 (UI, Extra features)

## 扩展指南 (Extension Guide)

如果你需要为新的资源类型添加自定义编辑器：

If you need to add a custom editor for a new asset type:

1. 在 `Assets/Scripts/Editor` 创建新的编辑器脚本
   Create a new editor script in `Assets/Scripts/Editor`

2. 继承 `Editor` 类并使用 `CustomEditor` 特性
   Inherit from `Editor` class and use `CustomEditor` attribute

3. 重写 `OnInspectorGUI()` 方法
   Override the `OnInspectorGUI()` method

4. 参考现有编辑器的样式和布局
   Reference existing editors for style and layout

示例模板：

```csharp
using UnityEditor;
using UnityEngine;
using Quantum;

namespace QuantumEditor
{
    [CustomEditor(typeof(YourAssetType))]
    public class YourAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // 添加你的自定义 UI
            // Add your custom UI here
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
```

## 故障排除 (Troubleshooting)

### 编辑器未显示 (Editor Not Showing)

1. 确保脚本位于 `Editor` 文件夹中
   Make sure scripts are in the `Editor` folder

2. 检查 `CustomEditor` 特性的类型是否正确
   Check if the type in `CustomEditor` attribute is correct

3. 重启 Unity 编辑器
   Restart Unity Editor

### 编译错误 (Compilation Errors)

1. 确保引用了正确的命名空间：
   Make sure correct namespaces are referenced:
   - `UnityEditor`
   - `UnityEngine`
   - `Quantum`
   - `Photon.Deterministic`

2. 确保 Quantum SDK 已正确导入
   Make sure Quantum SDK is properly imported

## 维护说明 (Maintenance Notes)

- 编辑器脚本仅在 Unity 编辑器中运行，不会包含在构建中
  Editor scripts only run in Unity Editor and are not included in builds

- 修改编辑器不会影响运行时行为
  Modifying editors does not affect runtime behavior

- 建议在修改资源结构后同步更新对应的编辑器
  Recommend updating corresponding editors after modifying asset structure

## 版权信息 (Copyright)

这些编辑器脚本是为 2D Fighter 项目定制开发的。

These editor scripts are custom developed for the 2D Fighter project.

---

**最后更新 (Last Updated)**: 2025-11-12
**版本 (Version)**: 1.0.0
