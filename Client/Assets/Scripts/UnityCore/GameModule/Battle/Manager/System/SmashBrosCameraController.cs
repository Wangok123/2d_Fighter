using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class SmashBrosCameraController : MonoBehaviour
    {
        [Header("Cinemachine 引用")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineTargetGroup targetGroup;

        [Header("缩放设置")]
        [Tooltip("最小正交大小（最大缩放）")]
        [SerializeField] private float minOrthographicSize = 5f;
        
        [Tooltip("最大正交大小（最小缩放）")]
        [SerializeField] private float maxOrthographicSize = 15f;
        
        [Tooltip("边距百分比（0-1），玩家距离边缘的安全距离")]
        [SerializeField] private float screenMargin = 0.15f;

        [Header("平滑设置")]
        [Tooltip("缩放平滑速度")]
        [SerializeField] private float zoomSmoothSpeed = 2f;

        [Header("边界限制")]
        [Tooltip("是否启用摄像机边界")]
        [SerializeField] private bool useCameraBounds = true;
        
        [SerializeField] private Vector2 minCameraBounds = new Vector2(-50f, -10f);
        [SerializeField] private Vector2 maxCameraBounds = new Vector2(50f, 20f);

        [Header("调试")]
        [SerializeField] private bool showDebugGizmos = true;

        private List<Transform> _activePlayerTransforms = new List<Transform>();
        private float _targetOrthographicSize;
        private Vector3 _targetPosition;

        private void Awake()
        {
            if (virtualCamera == null)
            {
                Debug.LogError("[SmashBrosCameraController] CinemachineVirtualCamera 未设置!");
            }

            if (targetGroup == null)
            {
                Debug.LogError("[SmashBrosCameraController] CinemachineTargetGroup 未设置!");
            }

            _targetOrthographicSize = virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : minOrthographicSize;
        }

        public void RegisterPlayer(Transform playerTransform, float weight = 1f, float radius = 1f)
        {
            if (playerTransform == null || _activePlayerTransforms.Contains(playerTransform))
                return;

            _activePlayerTransforms.Add(playerTransform);

            if (targetGroup != null)
            {
                targetGroup.AddMember(playerTransform, weight, radius);
            }

            Debug.Log($"[SmashBrosCameraController] 注册玩家: {playerTransform.name}, 当前玩家数: {_activePlayerTransforms.Count}");
        }

        public void UnregisterPlayer(Transform playerTransform)
        {
            if (playerTransform == null || !_activePlayerTransforms.Contains(playerTransform))
                return;

            _activePlayerTransforms.Remove(playerTransform);

            if (targetGroup != null)
            {
                targetGroup.RemoveMember(playerTransform);
            }

            Debug.Log($"[SmashBrosCameraController] 移除玩家: {playerTransform.name}, 当前玩家数: {_activePlayerTransforms.Count}");
        }

        public void ClearAllPlayers()
        {
            _activePlayerTransforms.Clear();

            if (targetGroup != null)
            {
                for (int i = targetGroup.m_Targets.Length - 1; i >= 0; i--)
                {
                    if (targetGroup.m_Targets[i].target != null)
                    {
                        targetGroup.RemoveMember(targetGroup.m_Targets[i].target);
                    }
                }
            }

            Debug.Log("[SmashBrosCameraController] 清除所有玩家");
        }

        private void LateUpdate()
        {
            if (_activePlayerTransforms.Count == 0 || virtualCamera == null)
                return;

            UpdateCameraZoom();
            
            if (useCameraBounds)
            {
                ClampCameraPosition();
            }
        }

        private void UpdateCameraZoom()
        {
            Bounds playersBounds = CalculatePlayersBounds();

            float requiredHeight = playersBounds.size.y / (1f - screenMargin * 2f);
            float requiredWidth = playersBounds.size.x / (1f - screenMargin * 2f);

            float aspectRatio = Camera.main != null ? Camera.main.aspect : 16f / 9f;
            float requiredOrthographicSize = Mathf.Max(
                requiredHeight * 0.5f,
                requiredWidth / aspectRatio * 0.5f
            );

            _targetOrthographicSize = Mathf.Clamp(requiredOrthographicSize, minOrthographicSize, maxOrthographicSize);

            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                virtualCamera.m_Lens.OrthographicSize,
                _targetOrthographicSize,
                Time.deltaTime * zoomSmoothSpeed
            );
        }

        private Bounds CalculatePlayersBounds()
        {
            if (_activePlayerTransforms.Count == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            Bounds bounds = new Bounds(_activePlayerTransforms[0].position, Vector3.zero);

            foreach (Transform playerTransform in _activePlayerTransforms)
            {
                if (playerTransform != null)
                {
                    bounds.Encapsulate(playerTransform.position);
                }
            }

            return bounds;
        }

        private void ClampCameraPosition()
        {
            if (virtualCamera.Follow == null)
                return;

            Vector3 targetPosition = virtualCamera.Follow.position;
            
            float orthographicSize = virtualCamera.m_Lens.OrthographicSize;
            float aspectRatio = Camera.main != null ? Camera.main.aspect : 16f / 9f;
            float cameraHalfWidth = orthographicSize * aspectRatio;

            float clampedX = Mathf.Clamp(
                targetPosition.x,
                minCameraBounds.x + cameraHalfWidth,
                maxCameraBounds.x - cameraHalfWidth
            );

            float clampedY = Mathf.Clamp(
                targetPosition.y,
                minCameraBounds.y + orthographicSize,
                maxCameraBounds.y - orthographicSize
            );

            if (targetGroup != null && targetGroup.transform != null)
            {
                Vector3 currentGroupPos = targetGroup.transform.position;
                targetGroup.transform.position = new Vector3(clampedX, clampedY, currentGroupPos.z);
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos)
                return;

            if (useCameraBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3(
                    (minCameraBounds.x + maxCameraBounds.x) * 0.5f,
                    (minCameraBounds.y + maxCameraBounds.y) * 0.5f,
                    0f
                );
                Vector3 size = new Vector3(
                    maxCameraBounds.x - minCameraBounds.x,
                    maxCameraBounds.y - minCameraBounds.y,
                    0.1f
                );
                Gizmos.DrawWireCube(center, size);
            }

            if (_activePlayerTransforms.Count > 0)
            {
                Bounds playersBounds = CalculatePlayersBounds();
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(playersBounds.center, playersBounds.size);
            }

            if (virtualCamera != null && virtualCamera.Follow != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(virtualCamera.Follow.position, 0.5f);
            }
        }
    }
}
