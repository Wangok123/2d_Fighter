using UnityEngine;

namespace Quantum.QuantumView.Base
{
    public class QuantumSmoothTransform : MonoBehaviour
    {
        [Header("平滑设置")]
        [Tooltip("位置平滑速度（越小越平滑但延迟越大）")]
        [Range(0.01f, 0.2f)]
        [SerializeField] private float _positionSmoothTime = 0.05f;

        [Tooltip("回滚修正时间（越小修正越快）")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float _rollbackCorrectionTime = 0.15f;

        [Tooltip("最大瞬移距离（超过此距离直接瞬移）")]
        [SerializeField] private float _maxTeleportDistance = 5f;

        [Tooltip("是否启用位置平滑")]
        [SerializeField] private bool _enableSmoothing = true;

        [Header("调试")]
        [Tooltip("显示调试 Gizmos")]
        [SerializeField] private bool _showDebugGizmos = false;
        
        [Tooltip("显示调试日志")]
        [SerializeField] private bool _showDebugLog = false;

        private Vector3 _smoothedPosition;
        private Vector3 _targetPosition;
        private Vector3 _smoothVelocity;
        private bool _isInitialized;
        private bool _isInRollbackCorrection;
        private float _rollbackStartTime;
        private Vector3 _rollbackStartPosition;

        public bool IsEnabled
        {
            get => _enableSmoothing;
            set => _enableSmoothing = value;
        }

        public void Initialize(Vector3 initialPosition)
        {
            _smoothedPosition = initialPosition;
            _targetPosition = initialPosition;
            transform.position = initialPosition;
            _smoothVelocity = Vector3.zero;
            _isInitialized = true;
            _isInRollbackCorrection = false;

            if (_showDebugLog)
            {
                Debug.Log($"[QuantumSmoothTransform] Initialized at {initialPosition}");
            }
        }

        public void SetTargetPosition(Vector3 newTarget, bool isRollback = false)
        {
            if (!_isInitialized)
            {
                Initialize(newTarget);
                return;
            }

            if (!_enableSmoothing)
            {
                transform.position = newTarget;
                _smoothedPosition = newTarget;
                _targetPosition = newTarget;
                return;
            }

            float distance = Vector3.Distance(_smoothedPosition, newTarget);

            if (distance > _maxTeleportDistance)
            {
                if (_showDebugLog)
                {
                    Debug.Log($"[QuantumSmoothTransform] Teleport: {distance:F2}m");
                }

                _smoothedPosition = newTarget;
                _targetPosition = newTarget;
                transform.position = newTarget;
                _smoothVelocity = Vector3.zero;
                _isInRollbackCorrection = false;
                return;
            }

            if (isRollback && distance > 0.1f)
            {
                if (_showDebugLog)
                {
                    Debug.Log($"[QuantumSmoothTransform] Rollback correction: {distance:F2}m");
                }

                _isInRollbackCorrection = true;
                _rollbackStartTime = Time.time;
                _rollbackStartPosition = _smoothedPosition;
            }

            _targetPosition = newTarget;
        }

        private void Update()
        {
            if (!_isInitialized || !_enableSmoothing)
            {
                return;
            }

            UpdateSmoothing();
        }

        private void UpdateSmoothing()
        {
            if (_isInRollbackCorrection)
            {
                float elapsed = Time.time - _rollbackStartTime;
                float t = Mathf.Clamp01(elapsed / _rollbackCorrectionTime);
                
                float easedT = EaseOutCubic(t);
                _smoothedPosition = Vector3.Lerp(_rollbackStartPosition, _targetPosition, easedT);

                if (t >= 1f)
                {
                    _isInRollbackCorrection = false;
                    _smoothVelocity = Vector3.zero;
                }
            }
            else
            {
                _smoothedPosition = Vector3.SmoothDamp(
                    _smoothedPosition,
                    _targetPosition,
                    ref _smoothVelocity,
                    _positionSmoothTime
                );
            }

            transform.position = _smoothedPosition;
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public Vector3 GetSmoothedPosition()
        {
            return _smoothedPosition;
        }

        public Vector3 GetTargetPosition()
        {
            return _targetPosition;
        }

        public float GetPositionError()
        {
            return Vector3.Distance(_smoothedPosition, _targetPosition);
        }

        public bool IsInRollbackCorrection()
        {
            return _isInRollbackCorrection;
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos || !_isInitialized || !Application.isPlaying) 
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_targetPosition, 0.2f);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_smoothedPosition, 0.15f);

            Gizmos.color = _isInRollbackCorrection ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(_smoothedPosition, _targetPosition);

#if UNITY_EDITOR
            float error = GetPositionError();
            string status = _isInRollbackCorrection ? "ROLLBACK!" : "Normal";
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1.8f,
                $"Smooth Transform\nError: {error:F3}m\n{status}"
            );
#endif
        }
    }
}
