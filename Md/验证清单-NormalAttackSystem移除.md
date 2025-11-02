# 验证清单 - NormalAttackSystem 移除
# Verification Checklist - NormalAttackSystem Removal

## 代码生成 (Code Generation)

在Unity中打开项目后，需要重新生成Quantum代码：

After opening the project in Unity, regenerate Quantum code:

### 步骤 (Steps):

1. **打开Unity项目 (Open Unity Project)**
   - 打开 `/Client` 目录
   - 等待Unity编译完成

2. **触发Quantum代码生成 (Trigger Quantum Code Generation)**
   ```
   方法1: Unity菜单 > Quantum > CodeGen > Run Qtn CodeGen
   方法2: 保存任意 .qtn 文件会自动触发
   方法3: 右键点击 Character.qtn > Quantum > Generate
   ```

3. **检查生成的代码 (Check Generated Code)**
   - 查看 `Assets/QuantumUser/Simulation/Generated/` 目录
   - 确认没有编译错误
   - 查找 `AttackData` 组件的生成代码，确认有 `ModularConfig` 字段

## 编译验证 (Compilation Verification)

### 预期结果 (Expected Results):

✅ **成功的编译**
- 没有C#编译错误
- 没有Quantum代码生成错误
- 所有系统类正确编译

❌ **可能的错误和解决方案**

如果出现 "NormalAttackSystem not found" 错误：
- 检查是否有其他文件引用了 `NormalAttackSystem`
- 删除相关引用或更新代码

如果出现 "ModularConfig not found" 错误：
- 确认 Quantum 代码生成已执行
- 检查 `Character.qtn` 文件是否正确

## 功能测试 (Functional Testing)

### 测试场景 1: 使用传统配置的角色

**设置:**
```
AttackData:
  AttackConfig: ✅ LegacyConfig
  ModularConfig: ❌ null/未设置
```

**测试步骤:**
1. 进入游戏场景
2. 选择使用传统配置的角色
3. 尝试输入

**预期结果:**
- ✅ 角色正常移动
- ✅ 指令输入系统正常工作
- ⚠️ 攻击按钮可能不响应（因为没有 NormalAttackSystem 也没有 ModularConfig）
- ✅ 不应该有错误或崩溃

**如果攻击不工作:**
- 这是预期的！需要迁移到 ModularCharacterConfig
- 或者创建一个 ModularCharacterConfig 并引用它

### 测试场景 2: 使用模块化配置的角色

**设置:**
```
AttackData:
  AttackConfig: ✅ SomeConfig (用于输入缓冲)
  ModularConfig: ✅ NewModularConfig
```

**创建测试角色:**

1. **创建轻攻击能力 (Create Light Attack Ability)**
   ```
   路径: Assets/QuantumUser/Resources/Abilities/Attack/TestLightAttack.asset
   
   设置:
   - AbilityName: "测试轻攻击"
   - AttackType: LightMelee
   - Priority: 20
   - BaseDamage: 10
   - Cooldown: 0.3
   - CanCombo: true
   - MaxComboCount: 3
   - ComboWindow: 0.5
   - ComboDamageMultipliers: [1.0, 1.2, 1.5]
   ```

2. **创建重攻击能力 (Create Heavy Attack Ability)**
   ```
   路径: Assets/QuantumUser/Resources/Abilities/Attack/TestHeavyAttack.asset
   
   设置:
   - AbilityName: "测试重攻击"
   - AttackType: HeavyMelee
   - Priority: 50
   - BaseDamage: 25
   - Cooldown: 1.0
   - CanCharge: true
   - MinChargeTime: 0.5
   - MaxChargeTime: 2.0
   - FullChargeDamageMultiplier: 2.0
   ```

3. **创建模块化角色配置 (Create Modular Character Config)**
   ```
   路径: Assets/QuantumUser/Resources/Characters/TestModularCharacter.asset
   
   设置:
   - CharacterId: 999
   - CharacterName: "测试角色"
   - AttackAbilities:
     - Element 0: TestLightAttack
     - Element 1: TestHeavyAttack
   ```

4. **配置实体原型 (Configure Entity Prototype)**
   ```
   在角色的实体原型中:
   
   AttackData:
     AttackConfig: [选择任意一个现有的 CharacterAttackConfig]
     ModularConfig: TestModularCharacter
   ```

**测试步骤:**
1. 进入游戏场景
2. 生成测试角色
3. 测试轻攻击 (LP按钮)
4. 测试重攻击 (HP按钮)
5. 测试连招
6. 测试蓄力

**预期结果:**
- ✅ LP按钮触发轻攻击
- ✅ 连续按LP可以触发连招（3段）
- ✅ 连招伤害递增（1.0x → 1.2x → 1.5x）
- ✅ 按住HP按钮开始蓄力
- ✅ 释放HP按钮触发重攻击
- ✅ 蓄力时间越长，伤害越高（最高2.0x）
- ✅ 控制台显示 "Modular Attack" 日志

**日志检查:**
```
期望看到的日志:
- "Modular Attack: 测试轻攻击 - Type: LightMelee, Damage: 10"
- "Modular Attack: 测试轻攻击 - Type: LightMelee, Damage: 12" (连招第二段)
- "Modular Attack: 测试重攻击 - Type: HeavyMelee, Damage: 50" (满蓄力)
```

### 测试场景 3: 特殊技能测试

**如果有特殊技能配置:**

1. **创建特殊技能 (Create Special Ability)**
   ```
   路径: Assets/QuantumUser/Resources/Abilities/Special/TestSpecial.asset
   
   设置:
   - AbilityName: "测试波动拳"
   - SpecialType: Projectile
   - Priority: 100
   - Damage: 30
   - Cooldown: 3.0
   - InputSequence: [2, 3, 6, 10] // ↓↘→LP
   ```

2. **添加到角色配置**
   ```
   ModularCharacterConfig:
     SpecialAbilities:
       - Element 0: TestSpecial
   ```

**测试步骤:**
1. 输入指令序列: ↓↘→LP
2. 检查是否触发特殊技能

**预期结果:**
- ✅ 指令输入正确识别
- ✅ 触发特殊技能
- ✅ 控制台显示 "Modular Special" 日志
- ✅ 进入冷却时间

## 性能测试 (Performance Testing)

### 帧率检查 (Frame Rate Check)

**测试步骤:**
1. 运行游戏
2. 生成多个角色（4-8个）
3. 同时执行攻击
4. 观察帧率

**预期结果:**
- ✅ 帧率稳定
- ✅ 没有明显的性能下降
- ✅ Quantum模拟正常运行

### 网络同步测试 (Network Sync Test)

**测试步骤:**
1. 启动两个客户端
2. 在一个客户端执行攻击
3. 在另一个客户端观察

**预期结果:**
- ✅ 攻击动作在两个客户端同步
- ✅ 伤害数值一致
- ✅ 没有去同步问题

## 回归测试 (Regression Testing)

### 测试现有功能是否正常

**移动系统 (Movement System):**
- ✅ 基础移动 (WASD/方向键)
- ✅ 跳跃 (Jump按钮)
- ✅ 冲刺 (Dash按钮)
- ✅ 二段跳 (如果已解锁)
- ✅ 面向方向更新

**等级系统 (Level System):**
- ✅ 升级功能
- ✅ 能力解锁
- ✅ LevelUp事件触发

**指令输入系统 (Command Input System):**
- ✅ 输入序列追踪
- ✅ 输入缓冲
- ✅ 输入过期

**事件系统 (Event System):**
- ✅ AttackPerformed 事件
- ✅ SpecialMovePerformed 事件
- ✅ LevelUp 事件

## 边界情况测试 (Edge Case Testing)

### 测试边界条件

1. **没有配置的角色**
   - AttackConfig: null
   - ModularConfig: null
   - 预期: 系统跳过，不崩溃

2. **空的能力数组**
   - ModularConfig 有，但 AttackAbilities: []
   - 预期: 系统跳过，不崩溃

3. **等级不足**
   - 能力 RequiredLevel: 5
   - 角色 CurrentLevel: 1
   - 预期: 能力不执行

4. **冷却期间连续按键**
   - 连续快速按攻击键
   - 预期: 冷却期间不执行新攻击

5. **优先级冲突**
   - 两个能力有相同优先级
   - 预期: 列表顺序决定执行顺序

## 文档验证 (Documentation Verification)

### 检查文档完整性

- ✅ `ModularCharacterSystem.md` 已更新
- ✅ `迁移指南-NormalAttackSystem移除.md` 已创建
- ✅ `系统架构-统一攻击系统.md` 已创建
- ✅ 代码注释完整
- ✅ 示例配置清晰

## 问题排查 (Troubleshooting)

### 常见问题和解决方案

**问题 1: 攻击不生效**
```
排查步骤:
1. 检查 ModularConfig 是否设置
2. 检查 AttackAbilities 数组是否有元素
3. 查看控制台是否有 "Modular Attack" 日志
4. 确认输入按钮映射正确 (LP/HP)
```

**问题 2: 编译错误**
```
排查步骤:
1. 运行 Quantum CodeGen
2. 检查 Character.qtn 语法
3. 清理并重新编译 (Clean → Build)
```

**问题 3: 能力不解锁**
```
排查步骤:
1. 检查 CharacterLevel 组件
2. 确认 RequiredLevel 设置
3. 查看 AbilityUnlocks 配置
```

**问题 4: 特殊技能不触发**
```
排查步骤:
1. 检查 CommandInputData 组件存在
2. 确认 InputSequence 配置正确
3. 测试简单的输入序列 (如 [10] 仅LP)
4. 查看输入缓冲日志
```

## 验收标准 (Acceptance Criteria)

完成以下所有检查项才算验证通过:

### 必需 (Required)
- [ ] Quantum 代码生成成功
- [ ] 没有编译错误
- [ ] 模块化配置的角色可以正常攻击
- [ ] 轻攻击、重攻击正常工作
- [ ] 连招系统正常
- [ ] 蓄力系统正常
- [ ] 事件正确触发
- [ ] 现有移动功能不受影响

### 推荐 (Recommended)
- [ ] 特殊技能测试通过
- [ ] 网络同步测试通过
- [ ] 性能测试通过
- [ ] 回归测试全部通过
- [ ] 边界情况测试通过

### 可选 (Optional)
- [ ] 创建示例角色配置
- [ ] 录制演示视频
- [ ] 编写额外的单元测试

## 测试报告模板 (Test Report Template)

```markdown
# 测试报告 - NormalAttackSystem 移除验证

**测试日期:** YYYY-MM-DD
**测试人员:** [姓名]
**Unity版本:** [版本号]
**Quantum版本:** [版本号]

## 环境信息
- 操作系统: 
- Unity版本: 
- Quantum SDK版本: 

## 测试结果总结
- 代码生成: ✅ / ❌
- 编译: ✅ / ❌
- 功能测试: ✅ / ❌
- 性能测试: ✅ / ❌
- 回归测试: ✅ / ❌

## 详细测试结果

### 1. 代码生成
- [ ] Character.qtn 代码生成成功
- [ ] AttackData 包含 ModularConfig 字段
- [ ] 没有生成错误

备注: 

### 2. 编译验证
- [ ] C# 代码编译成功
- [ ] 没有编译错误
- [ ] 没有警告

备注: 

### 3. 功能测试
- [ ] 轻攻击 (LP)
- [ ] 重攻击 (HP)
- [ ] 连招系统
- [ ] 蓄力系统
- [ ] 特殊技能

备注: 

### 4. 发现的问题
1. 
2. 
3. 

### 5. 建议改进
1. 
2. 
3. 

## 结论
[ ] 通过验证，可以合并
[ ] 需要修改
```

---

**注意事项:**
- 这是一个重大的架构变更，请仔细测试
- 建议在测试分支进行完整测试
- 确保现有游戏内容不受影响
- 保留备份以便回滚
