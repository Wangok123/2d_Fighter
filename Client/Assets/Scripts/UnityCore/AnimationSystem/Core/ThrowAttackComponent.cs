using UnityEngine;
using System;
using System.Collections.Generic;

namespace UnityCore.AnimationSystem
{
    /// <summary>
    /// 投技攻击组件 - 实现类似街霸桑吉尔夫的投技系统
    /// Throw Attack Component - Implements throw/grapple system like Zangief from Street Fighter
    /// 
    /// 使用说明 / Usage:
    /// 1. 将此组件添加到角色GameObject上
    /// 2. 配置抓取检测范围和投技动画
    /// 3. 调用 TryThrow() 尝试执行投技
    /// 
    /// 投技流程 / Throw Flow:
    /// 1. 检测范围内是否有可抓取目标
    /// 2. 验证目标是否可被抓取（不在无敌、已被抓取等状态）
    /// 3. 播放抓取动画（Throw_Start）
    /// 4. 锁定双方位置和动画
    /// 5. 播放投掷动画序列（Throw_Hold -> Throw_Execute）
    /// 6. 释放目标并应用伤害/击退
    /// 7. 播放恢复动画（Throw_Recovery）
    /// </summary>
    [RequireComponent(typeof(CharacterAnimationManager))]
    public class ThrowAttackComponent : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Throw Detection / 抓取检测")]
        [Tooltip("抓取范围 / Grab range")]
        [SerializeField] private float _grabRange = 1.5f;

        [Tooltip("抓取角度（度数）/ Grab angle in degrees")]
        [SerializeField] private float _grabAngle = 45f;

        [Tooltip("可抓取的层级 / Grabbable layers")]
        [SerializeField] private LayerMask _grabbableLayers;

        [Header("Throw Settings / 投技设置")]
        [Tooltip("投技伤害 / Throw damage")]
        [SerializeField] private float _throwDamage = 50f;

        [Tooltip("投技后的击退力度 / Knockback force after throw")]
        [SerializeField] private float _knockbackForce = 10f;

        [Tooltip("投技动画序列 / Throw animation sequence")]
        [SerializeField] private ThrowAnimationSequence _throwSequence;

        [Tooltip("投技破解窗口时间（秒）/ Throw break window in seconds")]
        [SerializeField] private float _throwBreakWindow = 0.3f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        #endregion

        #region Private Fields

        private CharacterAnimationManager _animationManager;
        private ThrowState _currentState = ThrowState.Idle;
        private GameObject _grabbedTarget;
        private float _throwStartTime;
        private bool _isExecutingThrow;

        // 投技事件 / Throw events
        public event Action<GameObject> OnThrowStarted;
        public event Action<GameObject> OnThrowExecuted;
        public event Action OnThrowBroken;
        public event Action OnThrowCompleted;

        #endregion

        #region Properties

        /// <summary>是否正在执行投技</summary>
        public bool IsThrowActive => _isExecutingThrow;

        /// <summary>当前被抓取的目标</summary>
        public GameObject GrabbedTarget => _grabbedTarget;

        /// <summary>当前投技状态</summary>
        public ThrowState CurrentState => _currentState;

        #endregion

        private void Awake()
        {
            _animationManager = GetComponent<CharacterAnimationManager>();
            
            if (_throwSequence == null)
            {
                _throwSequence = new ThrowAnimationSequence();
            }
        }

        /// <summary>
        /// 尝试执行投技
        /// Try to execute throw attack
        /// </summary>
        /// <returns>是否成功发动投技</returns>
        public bool TryThrow()
        {
            // 检查是否已经在执行投技
            if (_isExecutingThrow)
            {
                Debug.LogWarning("Already executing a throw");
                return false;
            }

            // 检测范围内的目标
            GameObject target = DetectGrabbableTarget();
            
            if (target == null)
            {
                Debug.Log("No valid target for throw");
                return false;
            }

            // 检查目标是否可被抓取
            if (!CanGrabTarget(target))
            {
                Debug.Log("Target cannot be grabbed");
                return false;
            }

            // 开始投技
            StartThrow(target);
            return true;
        }

        /// <summary>
        /// 开始投技
        /// Start throw attack
        /// </summary>
        private void StartThrow(GameObject target)
        {
            _grabbedTarget = target;
            _isExecutingThrow = true;
            _throwStartTime = Time.time;
            _currentState = ThrowState.GrabAttempt;

            // 播放抓取动画
            if (!string.IsNullOrEmpty(_throwSequence.ThrowStartAnimation))
            {
                _animationManager.PlayAnimation(_throwSequence.ThrowStartAnimation, ignoreCancelRules: true);
            }

            // 通知目标被抓取
            var targetThrowable = target.GetComponent<IThrowable>();
            if (targetThrowable != null)
            {
                targetThrowable.OnGrabbed(gameObject);
            }

            // 触发事件
            OnThrowStarted?.Invoke(target);

            Debug.Log($"Started throw on target: {target.name}");
        }

        /// <summary>
        /// 执行投技（在动画事件中调用）
        /// Execute throw (called from animation event)
        /// </summary>
        public void ExecuteThrow()
        {
            if (!_isExecutingThrow || _grabbedTarget == null)
                return;

            _currentState = ThrowState.Executing;

            // 播放投掷执行动画
            if (!string.IsNullOrEmpty(_throwSequence.ThrowExecuteAnimation))
            {
                _animationManager.PlayAnimation(_throwSequence.ThrowExecuteAnimation);
            }

            // 对目标造成伤害
            var targetHealth = _grabbedTarget.GetComponent<IHealthSystem>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(_throwDamage);
            }

            // 应用击退效果
            ApplyKnockback();

            // 触发事件
            OnThrowExecuted?.Invoke(_grabbedTarget);

            Debug.Log($"Executed throw on: {_grabbedTarget.name}, Damage: {_throwDamage}");
        }

        /// <summary>
        /// 完成投技
        /// Complete throw
        /// </summary>
        public void CompleteThrow()
        {
            if (!_isExecutingThrow)
                return;

            _currentState = ThrowState.Recovery;

            // 播放恢复动画
            if (!string.IsNullOrEmpty(_throwSequence.ThrowRecoveryAnimation))
            {
                _animationManager.PlayAnimation(_throwSequence.ThrowRecoveryAnimation);
            }

            // 释放目标
            ReleaseTarget();

            // 触发事件
            OnThrowCompleted?.Invoke();

            _isExecutingThrow = false;
            _currentState = ThrowState.Idle;

            Debug.Log("Throw completed");
        }

        /// <summary>
        /// 破解投技（被抓取者调用）
        /// Break throw (called by grabbed character)
        /// </summary>
        public bool TryBreakThrow()
        {
            if (!_isExecutingThrow)
                return false;

            // 检查是否在破解窗口内
            float timeSinceGrab = Time.time - _throwStartTime;
            if (timeSinceGrab > _throwBreakWindow)
            {
                Debug.Log("Throw break window expired");
                return false;
            }

            // 成功破解
            BreakThrow();
            return true;
        }

        /// <summary>
        /// 破解投技
        /// Break throw
        /// </summary>
        private void BreakThrow()
        {
            Debug.Log("Throw broken!");

            // 通知目标破解成功
            var targetThrowable = _grabbedTarget?.GetComponent<IThrowable>();
            if (targetThrowable != null)
            {
                targetThrowable.OnThrowBroken();
            }

            // 触发事件
            OnThrowBroken?.Invoke();

            // 释放目标
            ReleaseTarget();

            _isExecutingThrow = false;
            _currentState = ThrowState.Idle;

            // 播放破解后的动画（可选）
            _animationManager.PlayDefaultState();
        }

        /// <summary>
        /// 检测可抓取的目标
        /// Detect grabbable target
        /// </summary>
        private GameObject DetectGrabbableTarget()
        {
            Vector3 origin = transform.position;
            Vector3 direction = transform.right; // 假设面朝右方

            // 使用球形检测
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, _grabRange, _grabbableLayers);

            GameObject closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                // 跳过自己
                if (hit.gameObject == gameObject)
                    continue;

                // 检查角度
                Vector3 directionToTarget = (hit.transform.position - origin).normalized;
                float angle = Vector3.Angle(direction, directionToTarget);

                if (angle > _grabAngle)
                    continue;

                // 找到最近的目标
                float distance = Vector3.Distance(origin, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hit.gameObject;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// 检查目标是否可被抓取
        /// Check if target can be grabbed
        /// </summary>
        private bool CanGrabTarget(GameObject target)
        {
            if (target == null)
                return false;

            // 检查目标是否实现了IThrowable接口
            var throwable = target.GetComponent<IThrowable>();
            if (throwable != null)
            {
                return throwable.CanBeGrabbed();
            }

            // 如果没有实现接口，默认可以抓取
            return true;
        }

        /// <summary>
        /// 应用击退效果
        /// Apply knockback effect
        /// </summary>
        private void ApplyKnockback()
        {
            if (_grabbedTarget == null)
                return;

            var rb = _grabbedTarget.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockbackDirection = (transform.right * _knockbackForce);
                rb.AddForce(knockbackDirection, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// 释放目标
        /// Release target
        /// </summary>
        private void ReleaseTarget()
        {
            if (_grabbedTarget != null)
            {
                var throwable = _grabbedTarget.GetComponent<IThrowable>();
                if (throwable != null)
                {
                    throwable.OnReleased();
                }

                _grabbedTarget = null;
            }
        }

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos)
                return;

            // 绘制抓取范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _grabRange);

            // 绘制抓取角度
            Vector3 direction = transform.right;
            Vector3 rightBound = Quaternion.Euler(0, 0, _grabAngle) * direction;
            Vector3 leftBound = Quaternion.Euler(0, 0, -_grabAngle) * direction;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, rightBound * _grabRange);
            Gizmos.DrawRay(transform.position, leftBound * _grabRange);
        }

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// 投技状态枚举
    /// Throw state enum
    /// </summary>
    public enum ThrowState
    {
        Idle,           // 空闲
        GrabAttempt,    // 尝试抓取
        Holding,        // 抓取中
        Executing,      // 执行投技
        Recovery        // 恢复中
    }

    /// <summary>
    /// 投技动画序列
    /// Throw animation sequence
    /// </summary>
    [Serializable]
    public class ThrowAnimationSequence
    {
        [Tooltip("抓取开始动画")]
        public string ThrowStartAnimation = "Throw_Start";

        [Tooltip("抓取保持动画")]
        public string ThrowHoldAnimation = "Throw_Hold";

        [Tooltip("投技执行动画")]
        public string ThrowExecuteAnimation = "Throw_Execute";

        [Tooltip("投技恢复动画")]
        public string ThrowRecoveryAnimation = "Throw_Recovery";
    }

    /// <summary>
    /// 可被投技的接口
    /// Throwable interface
    /// </summary>
    public interface IThrowable
    {
        /// <summary>是否可以被抓取</summary>
        bool CanBeGrabbed();

        /// <summary>被抓取时调用</summary>
        void OnGrabbed(GameObject attacker);

        /// <summary>投技被破解时调用</summary>
        void OnThrowBroken();

        /// <summary>被释放时调用</summary>
        void OnReleased();
    }

    /// <summary>
    /// 生命系统接口（简化版）
    /// Health system interface (simplified)
    /// </summary>
    public interface IHealthSystem
    {
        void TakeDamage(float damage);
    }

    #endregion
}
