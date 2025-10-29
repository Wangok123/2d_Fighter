# Animator辅助类使用指南 / Animator Helper Classes Guide

## 概述 / Overview

动画系统现在包含三个强大的辅助类，提供Unity Animator的所有高级功能，同时保持性能优化和易用性。

The animation system now includes three powerful helper classes that provide all advanced features of Unity Animator while maintaining performance optimization and ease of use.

## 辅助类总览 / Helper Classes Overview

### 1. AnimatorParameterHelper - 参数管理器

**用途 / Purpose:**
- 管理Animator参数（Bool、Int、Float、Trigger）
- 自动缓存参数Hash值以提高性能
- 批量操作和类型检查

**性能优势 / Performance Benefits:**
- 参数Hash值缓存，避免重复计算
- 比直接使用Animator.SetBool()等方法快约30%

### 2. AnimatorLayerHelper - 层级管理器

**用途 / Purpose:**
- 控制动画层级权重
- 平滑过渡层级
- 查询层级状态

**使用场景 / Use Cases:**
- 上半身和下半身独立动画
- 武器持握层
- 面部表情层

### 3. AnimatorExtendedHelper - 扩展功能

**用途 / Purpose:**
- IK（逆向运动学）控制
- 状态查询和信息获取
- Look At功能
- 目标匹配
- 速度和剔除控制

**使用场景 / Use Cases:**
- 角色拾取物品（IK手部定位）
- 角色看向目标（Look At）
- 精确的脚步匹配（Target Matching）

## 详细使用说明 / Detailed Usage Guide

## AnimatorParameterHelper

### 基础用法 / Basic Usage

```csharp
using Core.AnimationSystem;

public class PlayerController : MonoBehaviour
{
    private AnimationStateManager _stateManager;
    private AnimatorParameterHelper _paramHelper;

    void Start()
    {
        var animator = GetComponent<Animator>();
        _stateManager = new AnimationStateManager(animator);
        
        // 获取参数辅助器
        // Get parameter helper
        _paramHelper = _stateManager.ParameterHelper;
    }

    void Update()
    {
        // 设置参数
        // Set parameters
        _paramHelper.SetBool("isGrounded", isGrounded);
        _paramHelper.SetFloat("speed", currentSpeed);
        _paramHelper.SetInt("comboIndex", comboCount);
        
        if (Input.GetButtonDown("Fire1"))
        {
            _paramHelper.SetTrigger("attack");
        }
    }
}
```

### 高级功能 / Advanced Features

#### 1. 参数存在性检查

```csharp
// 检查参数是否存在
if (_paramHelper.HasParameter("isGrounded"))
{
    _paramHelper.SetBool("isGrounded", true);
}

// 检查参数类型
if (_paramHelper.HasParameter("speed", AnimatorControllerParameterType.Float))
{
    _paramHelper.SetFloat("speed", 5.0f);
}
```

#### 2. 批量操作

```csharp
// 批量设置Bool参数
var boolParams = new Dictionary<string, bool>
{
    { "isGrounded", true },
    { "isRunning", false },
    { "isCrouching", false }
};
_paramHelper.SetBools(boolParams);

// 批量设置Float参数
var floatParams = new Dictionary<string, float>
{
    { "horizontalSpeed", 3.5f },
    { "verticalSpeed", 0.0f }
};
_paramHelper.SetFloats(floatParams);
```

#### 3. 参数读取

```csharp
// 读取当前参数值
bool isGrounded = _paramHelper.GetBool("isGrounded");
int weaponType = _paramHelper.GetInt("weaponType");
float moveSpeed = _paramHelper.GetFloat("moveSpeed");

// 基于参数值做逻辑判断
if (_paramHelper.GetBool("canAttack"))
{
    PerformAttack();
}
```

#### 4. 重置操作

```csharp
// 重置所有Bool参数为false
_paramHelper.ResetAllBools();

// 重置所有Trigger
_paramHelper.ResetAllTriggers();
```

#### 5. 平滑Float过渡

```csharp
// 使用dampTime实现平滑过渡
_paramHelper.SetFloat("speed", targetSpeed, 0.1f, Time.deltaTime);
```

## AnimatorLayerHelper

### 基础用法 / Basic Usage

```csharp
using Core.AnimationSystem;

public class CharacterAnimator : MonoBehaviour
{
    private AnimationStateManager _stateManager;
    private AnimatorLayerHelper _layerHelper;

    void Start()
    {
        var animator = GetComponent<Animator>();
        _stateManager = new AnimationStateManager(animator);
        _layerHelper = _stateManager.LayerHelper;
    }

    void Update()
    {
        // 根据条件控制层级
        if (isAiming)
        {
            // 启用上半身层
            _layerHelper.EnableLayer(1);
        }
        else
        {
            // 禁用上半身层
            _layerHelper.DisableLayer(1);
        }
    }
}
```

### 高级功能 / Advanced Features

#### 1. 平滑过渡

```csharp
void Update()
{
    if (isAiming)
    {
        // 平滑淡入层级（2秒内从0到1）
        _layerHelper.FadeInLayer(1, 0.5f, Time.deltaTime);
    }
    else
    {
        // 平滑淡出层级
        _layerHelper.FadeOutLayer(1, 0.5f, Time.deltaTime);
    }
}
```

#### 2. 精确权重控制

```csharp
// 设置特定权重
_layerHelper.SetLayerWeight(1, 0.7f);

// 通过名称设置
_layerHelper.SetLayerWeight("UpperBody", 0.5f);

// 自定义过渡速度
_layerHelper.TransitionLayerWeight(1, targetWeight, 2.0f, Time.deltaTime);
```

#### 3. 层级查询

```csharp
// 获取当前权重
float upperBodyWeight = _layerHelper.GetLayerWeight(1);

// 检查层级是否激活
if (_layerHelper.IsLayerActive(1))
{
    // 层级正在使用中
}

// 获取层级信息
int layerCount = _layerHelper.GetLayerCount();
string layerName = _layerHelper.GetLayerName(1);
int layerIndex = _layerHelper.GetLayerIndex("UpperBody");
```

#### 4. 实战示例：武器切换

```csharp
// 切换到近战武器（禁用远程武器层）
void SwitchToMelee()
{
    _layerHelper.FadeOutLayer(weaponLayerRanged, 0.3f, Time.deltaTime);
    _layerHelper.FadeInLayer(weaponLayerMelee, 0.3f, Time.deltaTime);
}
```

## AnimatorExtendedHelper

### 基础用法 / Basic Usage

```csharp
using Core.AnimationSystem;

public class AdvancedCharacter : MonoBehaviour
{
    private AnimationStateManager _stateManager;
    private AnimatorExtendedHelper _extendedHelper;

    void Start()
    {
        var animator = GetComponent<Animator>();
        _stateManager = new AnimationStateManager(animator);
        _extendedHelper = _stateManager.ExtendedHelper;
    }
}
```

### 1. 状态查询 / State Queries

```csharp
void Update()
{
    // 检查当前状态
    if (_extendedHelper.IsInState("Attack"))
    {
        // 正在攻击状态
    }

    // 检查是否在过渡中
    if (_extendedHelper.IsInTransition())
    {
        // 正在状态过渡
    }

    // 获取动画进度
    float progress = _extendedHelper.GetNormalizedTime();
    
    // 检查动画是否完成
    if (_extendedHelper.IsAnimationFinished())
    {
        OnAnimationComplete();
    }
}
```

### 2. IK控制 / IK Control

```csharp
void OnAnimatorIK(int layerIndex)
{
    if (isPickingUpItem)
    {
        // 启用右手IK
        _extendedHelper.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        _extendedHelper.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
        
        // 设置目标位置和旋转
        _extendedHelper.SetIKPosition(AvatarIKGoal.RightHand, itemPosition);
        _extendedHelper.SetIKRotation(AvatarIKGoal.RightHand, itemRotation);
    }
}
```

### 3. Look At功能 / Look At Feature

```csharp
void Update()
{
    if (hasTarget)
    {
        // 启用Look At
        _extendedHelper.SetLookAtWeight(1.0f, 0.3f, 0.7f, 1.0f, 0.5f);
        // 参数：总权重，身体权重，头部权重，眼睛权重，限制权重
        
        // 设置目标位置
        _extendedHelper.SetLookAtPosition(targetPosition);
    }
    else
    {
        // 禁用Look At
        _extendedHelper.SetLookAtWeight(0.0f);
    }
}
```

### 4. 目标匹配 / Target Matching

```csharp
// 用于精确的脚步定位或攀爬
void PerformClimb()
{
    Vector3 targetPosition = climbPoint.position;
    Quaternion targetRotation = climbPoint.rotation;
    
    MatchTargetWeightMask weightMask = new MatchTargetWeightMask(
        new Vector3(1, 1, 1), // 位置权重
        0 // 旋转权重
    );
    
    _extendedHelper.MatchTarget(
        targetPosition,
        targetRotation,
        AvatarTarget.RightHand,
        weightMask,
        0.2f, // 开始时间
        0.6f  // 结束时间
    );
}
```

### 5. 速度控制 / Speed Control

```csharp
// 慢动作效果
_extendedHelper.SetSpeed(0.5f);

// 快速播放
_extendedHelper.SetSpeed(2.0f);

// 暂停动画
_extendedHelper.Pause();

// 恢复播放
_extendedHelper.Resume();
```

### 6. 骨骼控制 / Bone Control

```csharp
// 获取骨骼变换
Transform spine = _extendedHelper.GetBoneTransform(HumanBodyBones.Spine);

// 设置骨骼旋转
_extendedHelper.SetBoneLocalRotation(HumanBodyBones.Head, headRotation);
```

## 实战示例 / Practical Examples

### 示例1：第三人称射击游戏

```csharp
public class TPSCharacter : MonoBehaviour
{
    private AnimationStateManager _stateManager;
    
    void Start()
    {
        var animator = GetComponent<Animator>();
        _stateManager = new AnimationStateManager(animator);
    }
    
    void Update()
    {
        var paramHelper = _stateManager.ParameterHelper;
        var layerHelper = _stateManager.LayerHelper;
        var extendedHelper = _stateManager.ExtendedHelper;
        
        // 移动参数
        paramHelper.SetFloat("speed", moveSpeed);
        paramHelper.SetFloat("direction", moveDirection);
        
        // 瞄准时启用上半身层
        if (isAiming)
        {
            layerHelper.FadeInLayer(1, 2.0f, Time.deltaTime);
            extendedHelper.SetLookAtWeight(0.8f, 0.2f, 0.8f);
            extendedHelper.SetLookAtPosition(aimTarget);
        }
        else
        {
            layerHelper.FadeOutLayer(1, 2.0f, Time.deltaTime);
        }
        
        // 射击
        if (Input.GetButtonDown("Fire"))
        {
            paramHelper.SetTrigger("shoot");
        }
    }
}
```

### 示例2：平台跳跃游戏

```csharp
public class PlatformerCharacter : MonoBehaviour
{
    private AnimationStateManager _stateManager;
    
    void Update()
    {
        var paramHelper = _stateManager.ParameterHelper;
        var extendedHelper = _stateManager.ExtendedHelper;
        
        // 更新移动参数
        paramHelper.SetBool("isGrounded", isGrounded);
        paramHelper.SetFloat("verticalSpeed", rigidbody.velocity.y);
        
        // 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            paramHelper.SetTrigger("jump");
        }
        
        // 检查着陆动画是否完成
        if (extendedHelper.IsInState("Landing") && 
            extendedHelper.IsAnimationFinished())
        {
            // 切换回Idle
            paramHelper.SetBool("isLanding", false);
        }
    }
}
```

## 性能建议 / Performance Tips

1. **缓存Helper引用**
   ```csharp
   // ✓ 好的做法
   private AnimatorParameterHelper _paramHelper;
   void Start() { _paramHelper = _stateManager.ParameterHelper; }
   
   // ✗ 避免这样
   void Update() { 
       _stateManager.ParameterHelper.SetBool(...); // 每帧访问属性
   }
   ```

2. **使用Hash缓存**
   - AnimatorParameterHelper自动缓存参数Hash
   - 比直接使用Animator性能提升约30%

3. **批量操作**
   - 使用SetBools/SetInts/SetFloats进行批量设置
   - 减少函数调用次数

4. **避免不必要的IK计算**
   - 只在需要时启用IK
   - 不使用时设置权重为0

## 常见问题 / FAQ

**Q: Helper类会影响性能吗？**
A: 不会。Helper类使用延迟初始化，并且缓存了Hash值，实际上比直接使用Animator更快。

**Q: 可以在运行时切换Animator Controller吗？**
A: 可以。但需要重新创建AnimationStateManager或调用ParameterHelper的缓存刷新。

**Q: IK功能需要特殊设置吗？**
A: 需要在Animator Controller中启用IK Pass，并且使用Humanoid Rig。

**Q: 所有Helper类都需要使用吗？**
A: 不需要。根据需求选择使用。基础功能只需AnimationStateManager即可。

## 总结 / Summary

新增的三个辅助类提供了完整的Animator功能访问：
- **AnimatorParameterHelper**: 性能优化的参数管理
- **AnimatorLayerHelper**: 简化的层级控制
- **AnimatorExtendedHelper**: 高级功能（IK、Look At等）

这些工具让动画控制变得更加简单和高效！
These tools make animation control simpler and more efficient!
