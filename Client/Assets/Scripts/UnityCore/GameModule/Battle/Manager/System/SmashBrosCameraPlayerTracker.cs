using System.Collections.Generic;
using Quantum;
using Quantum.QuantumView;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class SmashBrosCameraPlayerTracker : MonoBehaviour
    {
        [SerializeField] private SmashBrosCameraController cameraController;
        
        [Header("玩家追踪设置")]
        [Tooltip("玩家在TargetGroup中的权重")]
        [SerializeField] private float playerWeight = 1f;
        
        [Tooltip("玩家在TargetGroup中的半径")]
        [SerializeField] private float playerRadius = 2f;

        [Tooltip("更新间隔（秒）")]
        [SerializeField] private float updateInterval = 0.5f;

        private Dictionary<EntityRef, PlayerViewController> _trackedPlayers = new Dictionary<EntityRef, PlayerViewController>();
        private float _lastUpdateTime;

        private void Awake()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<SmashBrosCameraController>();
            }

            if (cameraController == null)
            {
                Debug.LogError("[SmashBrosCameraPlayerTracker] SmashBrosCameraController 未找到!");
            }
        }

        private void Update()
        {
            if (Time.time - _lastUpdateTime < updateInterval)
                return;

            _lastUpdateTime = Time.time;
            UpdateTrackedPlayers();
        }

        private void UpdateTrackedPlayers()
        {
            PlayerViewController[] allPlayers = FindObjectsOfType<PlayerViewController>();

            HashSet<EntityRef> currentPlayerEntities = new HashSet<EntityRef>();

            foreach (PlayerViewController player in allPlayers)
            {
                if (player.EntityRef == default)
                    continue;

                currentPlayerEntities.Add(player.EntityRef);

                if (!_trackedPlayers.ContainsKey(player.EntityRef))
                {
                    RegisterPlayer(player);
                }
            }

            List<EntityRef> playersToRemove = new List<EntityRef>();
            foreach (var kvp in _trackedPlayers)
            {
                if (!currentPlayerEntities.Contains(kvp.Key))
                {
                    playersToRemove.Add(kvp.Key);
                }
            }

            foreach (EntityRef entityRef in playersToRemove)
            {
                UnregisterPlayer(entityRef);
            }
        }

        private void RegisterPlayer(PlayerViewController player)
        {
            if (cameraController == null || player == null)
                return;

            _trackedPlayers[player.EntityRef] = player;
            cameraController.RegisterPlayer(player.transform, playerWeight, playerRadius);

            Debug.Log($"[SmashBrosCameraPlayerTracker] 开始追踪玩家 Entity: {player.EntityRef}");
        }

        private void UnregisterPlayer(EntityRef entityRef)
        {
            if (!_trackedPlayers.TryGetValue(entityRef, out PlayerViewController player))
                return;

            if (player != null && cameraController != null)
            {
                cameraController.UnregisterPlayer(player.transform);
            }

            _trackedPlayers.Remove(entityRef);

            Debug.Log($"[SmashBrosCameraPlayerTracker] 停止追踪玩家 Entity: {entityRef}");
        }

        private void OnDestroy()
        {
            if (cameraController != null)
            {
                cameraController.ClearAllPlayers();
            }

            _trackedPlayers.Clear();
        }
    }
}
