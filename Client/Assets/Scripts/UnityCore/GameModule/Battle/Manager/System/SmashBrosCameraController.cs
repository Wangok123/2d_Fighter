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

        [Header("配置")]
        [SerializeField] private BattleCameraConfig config;

        [Header("掉落追踪设置")]
        [SerializeField] private Transform lowerPlatform;
        [Tooltip("玩家掉落超过此距离后，相机停止追踪。设为0则立即停止追踪")]
        [SerializeField] private float loseSightAtRange = 20f;
        [Tooltip("是否启用掉落追踪功能")]
        [SerializeField] private bool enableFallOffTracking = true;

        private List<Transform> _activePlayerTransforms = new List<Transform>();
        private float _targetOrthographicSize;
        private float _zoomVelocity;

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

            if (config == null)
            {
                Debug.LogError("[SmashBrosCameraController] BattleCameraConfig 未设置!");
            }

            _targetOrthographicSize = virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : (config != null ? config.minOrthographicSize : 5f);
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
            if (_activePlayerTransforms.Count == 0 || virtualCamera == null || config == null)
                return;

            if (enableFallOffTracking && lowerPlatform != null)
            {
                UpdatePlayerWeights();
            }

            UpdateCameraZoom();
            
            if (config.useCameraBounds)
            {
                ClampCameraPosition();
            }
        }

        private void UpdatePlayerWeights()
        {
            if (targetGroup == null)
                return;

            for (int i = 0; i < targetGroup.m_Targets.Length; i++)
            {
                if (targetGroup.m_Targets[i].target == null)
                    continue;

                float distanceBelow = lowerPlatform.position.y - targetGroup.m_Targets[i].target.position.y;

                float weight = Mathf.Clamp(1f - distanceBelow / Mathf.Max(0.001f, loseSightAtRange), 0f, 1f);
                targetGroup.m_Targets[i].weight = weight;
            }
        }

        private void UpdateCameraZoom()
        {
            Bounds playersBounds = CalculatePlayersBounds();

            float requiredHeight = playersBounds.size.y / (1f - config.screenMargin * 2f);
            float requiredWidth = playersBounds.size.x / (1f - config.screenMargin * 2f);

            float aspectRatio = Camera.main != null ? Camera.main.aspect : 16f / 9f;
            float requiredOrthographicSize = Mathf.Max(
                requiredHeight * 0.5f,
                requiredWidth / aspectRatio * 0.5f
            );

            _targetOrthographicSize = Mathf.Clamp(requiredOrthographicSize, config.minOrthographicSize, config.maxOrthographicSize);

            virtualCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(
                virtualCamera.m_Lens.OrthographicSize,
                _targetOrthographicSize,
                ref _zoomVelocity,
                config.zoomSmoothTime
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
                config.minCameraBounds.x + cameraHalfWidth,
                config.maxCameraBounds.x - cameraHalfWidth
            );

            float clampedY = Mathf.Clamp(
                targetPosition.y,
                config.minCameraBounds.y + orthographicSize,
                config.maxCameraBounds.y - orthographicSize
            );

            if (targetGroup != null && targetGroup.transform != null)
            {
                Vector3 currentGroupPos = targetGroup.transform.position;
                targetGroup.transform.position = new Vector3(clampedX, clampedY, currentGroupPos.z);
            }
        }

        private void OnDrawGizmos()
        {
            if (config == null || !config.showDebugGizmos)
                return;

            if (config.useCameraBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3(
                    (config.minCameraBounds.x + config.maxCameraBounds.x) * 0.5f,
                    (config.minCameraBounds.y + config.maxCameraBounds.y) * 0.5f,
                    0f
                );
                Vector3 size = new Vector3(
                    config.maxCameraBounds.x - config.minCameraBounds.x,
                    config.maxCameraBounds.y - config.minCameraBounds.y,
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
