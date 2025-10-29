using UnityEngine;
using UnityCore.AnimationSystem;

namespace Examples
{
    /// <summary>
    /// 格斗游戏动画系统完整示例
    /// Complete example of fighting game animation system
    /// 
    /// 展示功能 / Features demonstrated:
    /// 1. 动画优先级和取消机制 / Animation priority and cancel mechanism
    /// 2. 投技系统 / Throw attack system
    /// 3. 智能受击反应 / Smart hit reactions
    /// 4. 连招取消窗口 / Combo cancel windows
    /// 
    /// 使用说明 / Usage:
    /// 1. 将此脚本添加到带有Animator的GameObject上
    /// 2. 按键操作测试各种功能
    /// </summary>
    [RequireComponent(typeof(WarriorAnimationManager))]
    [RequireComponent(typeof(ThrowAttackComponent))]
    public class FightingGameAnimationExample : MonoBehaviour
    {
        [Header("References")]
        private WarriorAnimationManager _animManager;
        private ThrowAttackComponent _throwComponent;

        [Header("Settings")]
        [SerializeField] private bool _showDebugInfo = true;

        [Header("Character State")]
        [SerializeField] private bool _isGrounded = true;
        [SerializeField] private float _health = 100f;

        private void Awake()
        {
            _animManager = GetComponent<WarriorAnimationManager>();
            _throwComponent = GetComponent<ThrowAttackComponent>();

            // 订阅投技事件
            if (_throwComponent != null)
            {
                _throwComponent.OnThrowStarted += OnThrowStarted;
                _throwComponent.OnThrowExecuted += OnThrowExecuted;
                _throwComponent.OnThrowBroken += OnThrowBroken;
                _throwComponent.OnThrowCompleted += OnThrowCompleted;
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateCharacterState();
        }

        /// <summary>
        /// 处理输入
        /// Handle input
        /// </summary>
        private void HandleInput()
        {
            // ============ 基础动作 / Basic Actions ============
            
            // 1 - Idle
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LogAction("Playing Idle");
                _animManager.PlayIdle();
            }

            // 2 - Run
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LogAction("Playing Run");
                _animManager.PlayRun();
            }

            // Space - Jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LogAction("Playing Jump");
                _animManager.PlayJump();
                _isGrounded = false;
            }

            // ============ 攻击系统 / Attack System ============

            // F - Normal Attack (可以连招)
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 尝试攻击，如果在取消窗口内可以连招
                if (_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.Attack))
                {
                    LogAction("Playing Attack (可能是连招)");
                    _animManager.PlayAttack();
                }
                else
                {
                    LogAction("Attack blocked by cancel rules (不在取消窗口)");
                }
            }

            // G - Dash Attack
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.DashAttack))
                {
                    LogAction("Playing Dash Attack");
                    _animManager.PlayDashAttack();
                }
                else
                {
                    LogAction("Dash Attack blocked");
                }
            }

            // ============ 投技系统 / Throw System ============

            // T - Throw Attack (投技)
            if (Input.GetKeyDown(KeyCode.T))
            {
                LogAction("Attempting Throw Attack");
                if (_throwComponent != null)
                {
                    bool success = _throwComponent.TryThrow();
                    if (!success)
                    {
                        LogAction("Throw failed - no valid target or blocked by priority");
                    }
                }
                else
                {
                    // 如果没有ThrowAttackComponent，直接播放动画演示
                    if (_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.ThrowStart))
                    {
                        _animManager.PlayThrowStart();
                    }
                    else
                    {
                        LogAction("Throw blocked by cancel rules");
                    }
                }
            }

            // ============ 受击系统 / Hit System ============

            // H - Take Hit (智能受击 - 根据是否在地面)
            if (Input.GetKeyDown(KeyCode.H))
            {
                LogAction($"Taking Hit ({(_isGrounded ? "Ground" : "Air")})");
                _animManager.PlaySmartHurt(_isGrounded);
                _health -= 10f;
            }

            // K - Knockdown (击倒)
            if (Input.GetKeyDown(KeyCode.K))
            {
                LogAction("Knockdown!");
                _animManager.PlayKnockdown();
                _health -= 20f;
            }

            // ============ 测试优先级系统 / Test Priority System ============

            // Y - 测试优先级 (尝试用低优先级动画打断高优先级)
            if (Input.GetKeyDown(KeyCode.Y))
            {
                LogAction("Testing priority: Try to interrupt with Idle");
                LogAction($"Current Priority: {_animManager.GetCurrentPriority()}");
                
                if (_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.Idle))
                {
                    _animManager.PlayIdle();
                    LogAction("Success: Idle played");
                }
                else
                {
                    LogAction("Failed: Current animation has higher priority");
                }
            }

            // U - 强制播放 (忽略取消规则)
            if (Input.GetKeyDown(KeyCode.U))
            {
                LogAction("Force playing Idle (ignoring cancel rules)");
                _animManager.PlayAnimation(WarriorAnimationManager.AnimationStates.Idle, 
                    forceReplay: false, 
                    ignoreCancelRules: true);
            }

            // ============ 模拟地面状态 / Simulate Ground State ============
            
            // C - Toggle Ground State
            if (Input.GetKeyDown(KeyCode.C))
            {
                _isGrounded = !_isGrounded;
                LogAction($"Ground State: {_isGrounded}");
            }

            // ============ 查询信息 / Query Information ============

            // Q - Show Status
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ShowStatus();
            }
        }

        /// <summary>
        /// 更新角色状态（模拟跳跃落地等）
        /// Update character state (simulate jump landing, etc.)
        /// </summary>
        private void UpdateCharacterState()
        {
            // 模拟跳跃后自动落地
            if (_animManager.IsPlaying(WarriorAnimationManager.AnimationStates.Jump))
            {
                if (_animManager.GetAnimationNormalizedTime() > 0.8f)
                {
                    _animManager.PlayFall();
                }
            }

            // 模拟坠落后着陆
            if (_animManager.IsPlaying(WarriorAnimationManager.AnimationStates.Fall))
            {
                if (_animManager.GetAnimationNormalizedTime() > 0.9f)
                {
                    _isGrounded = true;
                    _animManager.PlayIdle();
                }
            }

            // 攻击完成后返回Idle
            if (_animManager.IsPlaying(WarriorAnimationManager.AnimationStates.Attack))
            {
                if (_animManager.IsCurrentAnimationFinished())
                {
                    _animManager.PlayIdle();
                }
            }
        }

        /// <summary>
        /// 显示当前状态信息
        /// Show current status information
        /// </summary>
        private void ShowStatus()
        {
            Debug.Log("========== Status Info ==========");
            Debug.Log($"Current Animation: {_animManager.CurrentAnimation}");
            Debug.Log($"Current Priority: {_animManager.GetCurrentPriority()}");
            Debug.Log($"Animation Progress: {_animManager.GetAnimationNormalizedTime():F2}");
            Debug.Log($"Is Grounded: {_isGrounded}");
            Debug.Log($"Health: {_health}");
            Debug.Log($"Is Finished: {_animManager.IsCurrentAnimationFinished()}");
            
            // 测试能否播放各种动画
            Debug.Log("\nCan Play Tests:");
            Debug.Log($"  Can Play Idle: {_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.Idle)}");
            Debug.Log($"  Can Play Attack: {_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.Attack)}");
            Debug.Log($"  Can Play Throw: {_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.ThrowStart)}");
            Debug.Log($"  Can Play Hurt: {_animManager.CanPlayAnimation(WarriorAnimationManager.AnimationStates.Hurt)}");
            Debug.Log("================================");
        }

        /// <summary>
        /// 记录动作日志
        /// Log action
        /// </summary>
        private void LogAction(string message)
        {
            if (_showDebugInfo)
            {
                Debug.Log($"[FightingGame] {message}");
            }
        }

        #region Throw Event Handlers

        private void OnThrowStarted(GameObject target)
        {
            LogAction($"Throw Started on: {target.name}");
        }

        private void OnThrowExecuted(GameObject target)
        {
            LogAction($"Throw Executed on: {target.name}");
        }

        private void OnThrowBroken()
        {
            LogAction("Throw was Broken!");
        }

        private void OnThrowCompleted()
        {
            LogAction("Throw Completed");
        }

        #endregion

        #region GUI Display

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 500, 600));
            
            GUILayout.Label("=== Fighting Game Animation System Example ===");
            GUILayout.Label("格斗游戏动画系统示例");
            GUILayout.Space(10);
            
            GUILayout.Label("--- Basic Actions / 基础动作 ---");
            GUILayout.Label("1: Idle (闲置)");
            GUILayout.Label("2: Run (奔跑)");
            GUILayout.Label("Space: Jump (跳跃)");
            GUILayout.Label("C: Toggle Ground State (切换地面状态)");
            GUILayout.Space(10);
            
            GUILayout.Label("--- Attack System / 攻击系统 ---");
            GUILayout.Label("F: Normal Attack (普通攻击 - 可连招)");
            GUILayout.Label("G: Dash Attack (冲刺攻击)");
            GUILayout.Label("T: Throw Attack (投技)");
            GUILayout.Space(10);
            
            GUILayout.Label("--- Hit System / 受击系统 ---");
            GUILayout.Label("H: Take Hit (受击 - 智能判断)");
            GUILayout.Label("K: Knockdown (击倒)");
            GUILayout.Space(10);
            
            GUILayout.Label("--- Priority Tests / 优先级测试 ---");
            GUILayout.Label("Y: Test Priority (测试优先级)");
            GUILayout.Label("U: Force Play Idle (强制播放)");
            GUILayout.Label("Q: Show Status (显示状态)");
            GUILayout.Space(10);
            
            GUILayout.Label("--- Current Status / 当前状态 ---");
            GUILayout.Label($"Animation: {_animManager?.CurrentAnimation ?? "None"}");
            GUILayout.Label($"Priority: {_animManager?.GetCurrentPriority().ToString() ?? "None"}");
            GUILayout.Label($"Progress: {_animManager?.GetAnimationNormalizedTime():F2}");
            GUILayout.Label($"Grounded: {_isGrounded}");
            GUILayout.Label($"Health: {_health:F0}");
            
            if (_throwComponent != null)
            {
                GUILayout.Label($"Throw Active: {_throwComponent.IsThrowActive}");
            }
            
            GUILayout.EndArea();
        }

        #endregion

        private void OnDestroy()
        {
            // 取消订阅事件
            if (_throwComponent != null)
            {
                _throwComponent.OnThrowStarted -= OnThrowStarted;
                _throwComponent.OnThrowExecuted -= OnThrowExecuted;
                _throwComponent.OnThrowBroken -= OnThrowBroken;
                _throwComponent.OnThrowCompleted -= OnThrowCompleted;
            }
        }
    }
}
