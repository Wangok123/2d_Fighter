using System.Collections.Generic;
using Cinemachine;
using Core;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class BattleCameraManager : CoreModule
    {
        private CinemachineVirtualCamera _virtualCamera;
        private CinemachineTargetGroup _targetGroup;
        private CinemachineConfiner2D _confiner;
        private Camera _mainCamera;
        private BattleCameraConfig _config;

        private readonly List<Transform> _activePlayerTransforms = new List<Transform>();
        private float _targetOrthographicSize;
        private float _zoomVelocity;

        public override int Priority => 20;

        public void Initialize(CinemachineVirtualCamera virtualCamera, CinemachineTargetGroup targetGroup, BattleCameraConfig config, Camera mainCamera)
        {
            _virtualCamera = virtualCamera;
            _targetGroup = targetGroup;
            _config = config;
            _mainCamera = mainCamera;

            if (_virtualCamera == null)
            {
                Debug.LogError("[BattleCameraManager] CinemachineVirtualCamera 未设置!");
                return;
            }

            if (_targetGroup == null)
            {
                Debug.LogError("[BattleCameraManager] CinemachineTargetGroup 未设置!");
                return;
            }

            _targetOrthographicSize = _virtualCamera.m_Lens.OrthographicSize;
            
            _confiner = _virtualCamera.GetComponent<CinemachineConfiner2D>();
        }

        public void RegisterPlayer(Transform playerTransform, float weight = 1f, float radius = 1f)
        {
            if (playerTransform == null || _activePlayerTransforms.Contains(playerTransform))
                return;

            _activePlayerTransforms.Add(playerTransform);

            if (_targetGroup != null)
            {
                _targetGroup.AddMember(playerTransform, weight, radius);
            }

            Debug.Log($"[BattleCameraManager] 注册玩家: {playerTransform.name}, 当前玩家数: {_activePlayerTransforms.Count}");
        }

        public void UnregisterPlayer(Transform playerTransform)
        {
            if (playerTransform == null || !_activePlayerTransforms.Contains(playerTransform))
                return;

            _activePlayerTransforms.Remove(playerTransform);

            if (_targetGroup != null)
            {
                _targetGroup.RemoveMember(playerTransform);
            }

            Debug.Log($"[BattleCameraManager] 移除玩家: {playerTransform.name}, 当前玩家数: {_activePlayerTransforms.Count}");
        }

        public void ClearAllPlayers()
        {
            _activePlayerTransforms.Clear();

            if (_targetGroup != null)
            {
                for (int i = _targetGroup.m_Targets.Length - 1; i >= 0; i--)
                {
                    if (_targetGroup.m_Targets[i].target != null)
                    {
                        _targetGroup.RemoveMember(_targetGroup.m_Targets[i].target);
                    }
                }
            }

            Debug.Log("[BattleCameraManager] 清除所有玩家");
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_activePlayerTransforms.Count == 0 || _virtualCamera == null || _config == null)
                return;

            UpdateCameraZoom(elapseSeconds);
            
            if (_config.useCameraBounds && _confiner == null)
            {
                ClampCameraPosition();
            }
        }

        private void UpdateCameraZoom(float deltaTime)
        {
            Bounds playersBounds = CalculatePlayersBounds();

            float requiredHeight = playersBounds.size.y / (1f - _config.screenMargin * 2f);
            float requiredWidth = playersBounds.size.x / (1f - _config.screenMargin * 2f);

            float aspectRatio = _mainCamera != null ? _mainCamera.aspect : 16f / 9f;
            float requiredOrthographicSize = Mathf.Max(
                requiredHeight * 0.5f,
                requiredWidth / aspectRatio * 0.5f
            );

            _targetOrthographicSize = Mathf.Clamp(requiredOrthographicSize, _config.minOrthographicSize, _config.maxOrthographicSize);

            _virtualCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(
                _virtualCamera.m_Lens.OrthographicSize,
                _targetOrthographicSize,
                ref _zoomVelocity,
                _config.zoomSmoothTime
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
                else
                {
                    Debug.LogWarning("[BattleCameraManager] 检测到空的玩家Transform，请检查玩家是否已被销毁");
                }
            }

            _activePlayerTransforms.RemoveAll(t => t == null);

            return bounds;
        }

        private void ClampCameraPosition()
        {
            if (_virtualCamera.Follow == null)
                return;

            Vector3 targetPosition = _virtualCamera.Follow.position;
            
            float orthographicSize = _virtualCamera.m_Lens.OrthographicSize;
            float aspectRatio = _mainCamera != null ? _mainCamera.aspect : 16f / 9f;
            float cameraHalfWidth = orthographicSize * aspectRatio;

            float clampedX = Mathf.Clamp(
                targetPosition.x,
                _config.minCameraBounds.x + cameraHalfWidth,
                _config.maxCameraBounds.x - cameraHalfWidth
            );

            float clampedY = Mathf.Clamp(
                targetPosition.y,
                _config.minCameraBounds.y + orthographicSize,
                _config.maxCameraBounds.y - orthographicSize
            );

            if (_targetGroup != null && _targetGroup.transform != null)
            {
                Vector3 currentGroupPos = _targetGroup.transform.position;
                _targetGroup.transform.position = new Vector3(clampedX, clampedY, currentGroupPos.z);
            }
        }

        public void DrawDebugGizmos()
        {
            if (_config == null || !_config.showDebugGizmos)
                return;

            if (_config.useCameraBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3(
                    (_config.minCameraBounds.x + _config.maxCameraBounds.x) * 0.5f,
                    (_config.minCameraBounds.y + _config.maxCameraBounds.y) * 0.5f,
                    0f
                );
                Vector3 size = new Vector3(
                    _config.maxCameraBounds.x - _config.minCameraBounds.x,
                    _config.maxCameraBounds.y - _config.minCameraBounds.y,
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

            if (_virtualCamera != null && _virtualCamera.Follow != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_virtualCamera.Follow.position, 0.5f);
            }
        }

        internal override void Shutdown()
        {
            ClearAllPlayers();
            _virtualCamera = null;
            _targetGroup = null;
            _confiner = null;
            _mainCamera = null;
            _config = null;
        }
    }
}
