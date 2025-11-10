# Unity Editor 优化验证指南

## 如何测试新的编辑器扩展

### 前提条件
1. 打开 Unity 编辑器
2. 确保项目已加载完成
3. 等待脚本编译完成（Console 中无错误）

---

## 测试步骤

### 1. 测试 AbilityData 编辑器

**测试资产**: 
- `Assets/QuantumUser/Resources/DB/Ability/JumpAbilityData.asset`
- `Assets/QuantumUser/Resources/DB/Ability/DashAbilityData.asset`

**验证内容**:
1. 在 Project 窗口中选择这些资产
2. 在 Inspector 中应该看到:
   - ✓ 分组的折叠栏（时序设置、移动与方向设置等）
   - ✓ FP 值右侧显示实际秒数（如 `65536 ≈ 1.00s`）
   - ✓ 中文标签和工具提示
   - ✓ 所有设置可以折叠/展开

**预期效果截图位置**: 应该看到类似这样的布局
```
⏱ 时序设置 (Timing Settings)  [▼]
  输入缓冲时间: [FP字段] ≈ 0.15s
  延迟时间: [FP字段] ≈ 0.15s
  持续时间: [FP字段] ≈ 0.25s
  冷却时间: [FP字段] ≈ 0.00s
```

---

### 2. 测试 AttackAbilityData 编辑器

**测试资产**: 
- `Assets/QuantumUser/Resources/DB/Ability/LightAttackAbilityData_Combo.asset`

**验证内容**:
1. 选择该资产
2. 在 Inspector 中应该看到:
   - ✓ 继承自 AbilityData 的所有分组
   - ✓ 攻击特定设置分组（攻击范围、攻击时序、击退设置、硬直设置）
   - ✓ 时序可视化：显示启动帧和判定帧的时间轴
   - ✓ 击退方向可视化：显示方向箭头的小图

**预期效果**: 
- 时间轴应该显示判定的激活时间和持续时间
- 击退方向应该用箭头可视化显示

---

### 3. 测试 ChargeAttackAbilityData 编辑器

**测试资产**: 
- `Assets/QuantumUser/Resources/DB/Ability/HeavyAttackAbilityData_Charge.asset`

**验证内容**:
1. 选择该资产
2. 在 Inspector 中应该看到:
   - ✓ 所有 AttackAbilityData 的功能
   - ✓ 蓄力攻击特定设置分组
   - ✓ 蓄力时间轴：三段式（红色=太短、绿色=可释放、黄色=满蓄力）
   - ✓ 伤害缩放曲线可视化
   - ✓ 击退缩放曲线可视化（如果启用）
   - ✓ 当击退缩放禁用时，应该显示灰色并有提示信息

**预期效果**: 
- 蓄力区间应该清晰显示，例如：`0.25s (可释放) → 1.00s (满蓄力)`
- 缩放曲线应该是青色的线性插值曲线

---

### 4. 测试 ComboStepConfig 属性绘制器

**测试资产**: 
- `Assets/QuantumUser/Resources/DB/Ability/LightAttackAbilityData_Combo.asset`

**验证内容**:
1. 选择该资产
2. 找到 `ComboSteps` 数组
3. 展开数组元素，应该看到:
   - ✓ 每个连招步骤以折叠组形式显示
   - ✓ 分组：时序设置、击退设置、硬直设置、攻击形状
   - ✓ 所有 FP 值右侧显示实际数值

**预期效果**: 
- 每一段连招配置应该清晰分组，便于配置

---

### 5. 测试 HitReactionData 编辑器

**测试资产**: 
- `Assets/QuantumUser/Resources/DB/Hit/PlayerHitReactionData.asset`

**验证内容**:
1. 选择该资产
2. 在 Inspector 中应该看到:
   - ✓ 核心标志分组（可被击退、可被硬直）
   - ✓ 根据配置显示的警告或信息框
   - ✓ 硬直设置分组（轻击/重击倍率）
   - ✓ 击退配置分组（KnockbackProfile）
   - ✓ 根据击退模式显示对应的说明信息
   - ✓ 战斗行为设置

**预期效果**: 
- 当禁用所有受击效果时，应该显示黄色警告框
- 击退模式信息应该根据选择的模式动态显示

---

### 6. 测试 Shape2DConfig 属性绘制器

**测试资产**: 
- 任何包含 `AttackShape` 或 `Shape2DConfig` 字段的资产

**验证内容**:
1. 找到 Shape2DConfig 类型的字段
2. 应该看到:
   - ✓ 根据选择的形状类型，动态显示相关参数
   - ✓ Circle 和 Box 类型有可视化预览
   - ✓ 显示实际尺寸和转换后的数值

**预期效果**: 
- 选择 Circle 时显示半径和圆形预览
- 选择 Box 时显示半尺寸和矩形预览
- 其他形状类型显示对应参数

---

## 常见问题排查

### 问题1: 编辑器没有显示自定义界面
**解决方案**:
1. 检查 Console 是否有编译错误
2. 尝试重新导入脚本：右键 Editor 文件夹 → Reimport
3. 重启 Unity 编辑器

### 问题2: FP 值显示不正确
**说明**: FP (Fixed Point) 值的 RawValue 存储格式是 `value * 65536`
- 如果看到奇怪的数值，这是正常的内部存储格式
- 右侧应该显示转换后的实际数值

### 问题3: 可视化图形显示异常
**解决方案**:
1. 检查数值是否在合理范围内
2. 某些极端值可能导致可视化缩放异常
3. 这不影响实际功能，只是显示问题

### 问题4: 继承层次的编辑器
**说明**: 编辑器遵循继承关系
- `ChargeAttackAbilityDataEditor` → `AttackAbilityDataEditor` → `AbilityDataEditor`
- 子类会自动继承父类的所有显示功能

---

## 验证成功标准

✅ **基础功能**:
- [ ] 所有 FP 值都显示实际数值
- [ ] 所有分组可以正常折叠/展开
- [ ] 中文标签正确显示
- [ ] 工具提示（Tooltip）正常工作

✅ **可视化功能**:
- [ ] 攻击时序时间轴正常显示
- [ ] 击退方向箭头正常显示
- [ ] 蓄力时间轴三段式颜色正确
- [ ] 缩放曲线正常绘制
- [ ] 形状预览正常显示

✅ **智能功能**:
- [ ] 警告和信息框根据配置动态显示
- [ ] 禁用的功能正确灰色显示
- [ ] 根据类型动态显示相关字段

---

## 性能注意事项

- 所有可视化都经过优化，不会影响编辑器性能
- 如果感觉卡顿，可以折叠不需要的分组
- 可视化只在 Inspector 中显示，不会影响运行时性能

---

## 扩展和定制

如果需要添加更多自定义编辑器：

1. 参考现有编辑器的代码结构
2. 使用 `[CustomEditor(typeof(YourClass), true)]` 或 `[CustomPropertyDrawer(typeof(YourClass))]`
3. 遵循相同的命名和分组模式
4. 为 FP 值提供实际数值显示
5. 添加中文标签和详细的工具提示

详细说明请参考 `README_EDITOR_OPTIMIZATION.md`

---

## 反馈和问题

如果发现任何问题或有改进建议：
1. 记录问题的具体表现
2. 记录涉及的资产路径
3. 如果可能，提供截图
4. 检查 Unity Console 中的错误信息

---

**测试完成后，请截图最重要的几个界面，特别是:**
1. AbilityData 的分组展示
2. AttackAbilityData 的时序和击退可视化
3. ChargeAttackAbilityData 的蓄力时间轴和缩放曲线
4. HitReactionData 的智能提示
5. Shape2DConfig 的形状预览

这些截图可以用于文档和展示！
