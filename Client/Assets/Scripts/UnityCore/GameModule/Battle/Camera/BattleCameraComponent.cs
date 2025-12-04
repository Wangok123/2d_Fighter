using Cinemachine;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Manager.System;
using UnityEngine;

namespace UnityCore.GameModule.Battle.CameraSystem
{
    [DisallowMultipleComponent]
    public class BattleCameraComponent : MonoBehaviour
    {
        [Header("Cinemachine 引用")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineTargetGroup targetGroup;

        [Header("配置")]
        [SerializeField] private BattleCameraConfig config;

        private BattleCameraManager _manager;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _manager = GameModuleManager.GetModule<BattleCameraManager>();
            
            if (config == null)
            {
                Debug.LogWarning("[BattleCameraComponent] 未设置配置文件，使用默认配置");
                config = ScriptableObject.CreateInstance<BattleCameraConfig>();
            }
            
            _manager.Initialize(virtualCamera, targetGroup, config, _mainCamera);
        }

        private void OnDestroy()
        {
            // 场景卸载时清理
            _manager?.ClearAllPlayers();
        }

        // 公共 API
        public void RegisterPlayer(Transform playerTransform, float weight = 1f, float radius = 1f)
        {
            _manager?.RegisterPlayer(playerTransform, weight, radius);
        }

        public void UnregisterPlayer(Transform playerTransform)
        {
            _manager?.UnregisterPlayer(playerTransform);
        }

        public void ClearAllPlayers()
        {
            _manager?.ClearAllPlayers();
        }

        private void OnDrawGizmos()
        {
            if (_manager != null)
            {
                _manager.DrawDebugGizmos();
            }
        }
    }
}