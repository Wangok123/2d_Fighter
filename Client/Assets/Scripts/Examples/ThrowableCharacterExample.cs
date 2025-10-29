using UnityEngine;
using UnityCore.AnimationSystem;

namespace Examples
{
    /// <summary>
    /// 可被投技的角色示例
    /// Example of a throwable character
    /// 
    /// 此脚本演示如何实现IThrowable接口，使角色可以被投技攻击
    /// This script demonstrates how to implement IThrowable interface to make a character grabbable
    /// </summary>
    [RequireComponent(typeof(CharacterAnimationManager))]
    public class ThrowableCharacterExample : MonoBehaviour, IThrowable, IHealthSystem
    {
        [Header("State")]
        [SerializeField] private bool _canBeGrabbed = true;
        [SerializeField] private bool _isInvincible = false;
        [SerializeField] private bool _isBeingGrabbed = false;

        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;

        private CharacterAnimationManager _animManager;

        private void Awake()
        {
            _animManager = GetComponent<CharacterAnimationManager>();
            _currentHealth = _maxHealth;
        }

        #region IThrowable Implementation

        /// <summary>
        /// 检查是否可以被抓取
        /// Check if can be grabbed
        /// </summary>
        public bool CanBeGrabbed()
        {
            // 如果已经被抓取、无敌、或死亡，则不能再被抓取
            if (_isBeingGrabbed)
            {
                Debug.Log($"{name}: Already being grabbed");
                return false;
            }

            if (_isInvincible)
            {
                Debug.Log($"{name}: Invincible - cannot be grabbed");
                return false;
            }

            if (_currentHealth <= 0)
            {
                Debug.Log($"{name}: Dead - cannot be grabbed");
                return false;
            }

            // 如果正在播放某些特殊动画，也不能被抓取
            if (_animManager != null)
            {
                // 如果角色当前优先级太高（比如正在播放死亡或其他高优先级动画），不能被抓取
                var currentPriority = _animManager.GetCurrentPriority();
                if (currentPriority >= AnimationPriority.Knockdown)
                {
                    Debug.Log($"{name}: Priority too high - cannot be grabbed");
                    return false;
                }
            }

            return _canBeGrabbed;
        }

        /// <summary>
        /// 被抓取时调用
        /// Called when grabbed
        /// </summary>
        public void OnGrabbed(GameObject attacker)
        {
            Debug.Log($"{name}: Was grabbed by {attacker.name}!");
            
            _isBeingGrabbed = true;

            // 播放被抓取动画（如果有的话）
            // Play grabbed animation if exists
            if (_animManager != null)
            {
                // 可以播放一个被抓取的动画
                // You can play a grabbed animation here
                // _animManager.PlayAnimation("Grabbed", ignoreCancelRules: true);
                
                // 或者暂时保持当前动画，等待投技执行
                // Or keep current animation until throw is executed
            }

            // 可以在这里添加其他效果，比如：
            // - 显示被抓取的视觉效果
            // - 播放音效
            // - 触发粒子效果
        }

        /// <summary>
        /// 投技被破解时调用
        /// Called when throw is broken
        /// </summary>
        public void OnThrowBroken()
        {
            Debug.Log($"{name}: Broke the throw!");
            
            _isBeingGrabbed = false;

            // 播放破解成功的动画
            if (_animManager != null)
            {
                _animManager.PlayAnimation("ThrowBreak", ignoreCancelRules: true);
                // 如果没有专门的破解动画，可以返回Idle
                // _animManager.PlayDefaultState();
            }

            // 可以添加破解成功的奖励：
            // - 短暂无敌时间
            // - 反击机会
            _isInvincible = true;
            Invoke(nameof(RemoveInvincibility), 0.5f);
        }

        /// <summary>
        /// 从投技中释放时调用
        /// Called when released from throw
        /// </summary>
        public void OnReleased()
        {
            Debug.Log($"{name}: Released from throw");
            
            _isBeingGrabbed = false;

            // 播放倒地或恢复动画
            if (_animManager != null)
            {
                if (_currentHealth > 0)
                {
                    _animManager.PlayAnimation("Knockdown", ignoreCancelRules: true);
                }
                else
                {
                    _animManager.PlayAnimation("Death", ignoreCancelRules: true);
                }
            }
        }

        #endregion

        #region IHealthSystem Implementation

        /// <summary>
        /// 受到伤害
        /// Take damage
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (_isInvincible)
            {
                Debug.Log($"{name}: Invincible - no damage taken");
                return;
            }

            _currentHealth -= damage;
            Debug.Log($"{name}: Took {damage} damage. Health: {_currentHealth}/{_maxHealth}");

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }
            else
            {
                // 播放受击动画（如果不是被抓取状态）
                if (!_isBeingGrabbed && _animManager != null)
                {
                    // 假设有一个检测地面的方法
                    bool isGrounded = CheckIfGrounded();
                    
                    // 如果WarriorAnimationManager有PlaySmartHurt方法
                    var warriorAnim = _animManager as WarriorAnimationManager;
                    if (warriorAnim != null)
                    {
                        warriorAnim.PlaySmartHurt(isGrounded);
                    }
                    else
                    {
                        // 否则播放普通受击动画
                        _animManager.PlayAnimation("Hurt", ignoreCancelRules: true);
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 死亡处理
        /// Handle death
        /// </summary>
        private void Die()
        {
            Debug.Log($"{name}: Died!");
            
            _canBeGrabbed = false;
            
            if (_animManager != null)
            {
                _animManager.PlayAnimation("Death", ignoreCancelRules: true);
            }

            // 可以在这里添加死亡相关逻辑
            // - 禁用控制
            // - 触发死亡效果
            // - 通知游戏管理器
        }

        /// <summary>
        /// 移除无敌状态
        /// Remove invincibility
        /// </summary>
        private void RemoveInvincibility()
        {
            _isInvincible = false;
            Debug.Log($"{name}: Invincibility ended");
        }

        /// <summary>
        /// 检查是否在地面（示例实现）
        /// Check if grounded (example implementation)
        /// </summary>
        private bool CheckIfGrounded()
        {
            // 这里应该实现实际的地面检测逻辑
            // 可以使用Raycast、Physics2D.OverlapCircle等
            // This should implement actual ground detection logic
            // Can use Raycast, Physics2D.OverlapCircle, etc.
            
            // 简单示例：使用Raycast检测
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
            return hit.collider != null;
        }

        /// <summary>
        /// 尝试破解投技（由玩家输入触发）
        /// Try to break throw (triggered by player input)
        /// </summary>
        public void TryBreakThrow()
        {
            if (!_isBeingGrabbed)
            {
                Debug.Log($"{name}: Not being grabbed - cannot break throw");
                return;
            }

            // 这里需要获取攻击者的ThrowAttackComponent
            // 实际应用中，应该在OnGrabbed时保存攻击者引用
            // In practice, should save attacker reference in OnGrabbed
            
            Debug.Log($"{name}: Attempting to break throw...");
            // 实际调用应该是：attackerThrowComponent.TryBreakThrow()
        }

        #endregion

        #region Debug & Testing

        private void Update()
        {
            // 测试用：按E键尝试破解投技
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryBreakThrow();
            }

            // 测试用：按I键切换无敌状态
            if (Input.GetKeyDown(KeyCode.I))
            {
                _isInvincible = !_isInvincible;
                Debug.Log($"{name}: Invincibility: {_isInvincible}");
            }
        }

        private void OnGUI()
        {
            // 显示状态信息
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
            
            if (screenPos.z > 0)
            {
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y, 100, 60),
                    $"{name}\n" +
                    $"HP: {_currentHealth:F0}/{_maxHealth:F0}\n" +
                    $"Grabbed: {_isBeingGrabbed}\n" +
                    $"Invincible: {_isInvincible}");
            }
        }

        #endregion
    }
}
