using Quantum;
using Quantum.QuantumView;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class SmashBrosDeathZone : MonoBehaviour
    {
        [SerializeField] private SmashBrosCameraController cameraController;
        
        [Header("死亡区设置")]
        [Tooltip("超出摄像机多少距离算死亡")]
        [SerializeField] private float deathZoneExtension = 5f;

        [Header("调试")]
        [SerializeField] private bool showDebugGizmos = true;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            CheckPlayersOutOfBounds();
        }

        private void CheckPlayersOutOfBounds()
        {
            if (_mainCamera == null)
                return;

            PlayerViewController[] allPlayers = FindObjectsOfType<PlayerViewController>();

            foreach (PlayerViewController player in allPlayers)
            {
                if (player == null || player.EntityRef == default)
                    continue;

                Vector3 screenPoint = _mainCamera.WorldToViewportPoint(player.transform.position);
                
                bool isOutOfBounds = screenPoint.x < -deathZoneExtension || screenPoint.x > 1 + deathZoneExtension ||
                                     screenPoint.y < -deathZoneExtension || screenPoint.y > 1 + deathZoneExtension;

                if (isOutOfBounds)
                {
                    OnPlayerOutOfBounds(player.EntityRef);
                }
            }
        }

        private void OnPlayerOutOfBounds(EntityRef playerEntity)
        {
            Debug.Log($"[SmashBrosDeathZone] 玩家 {playerEntity} 超出边界!");
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || _mainCamera == null)
                return;

            Gizmos.color = Color.red;
            
            Vector3[] corners = GetDeathZoneCorners();
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);
        }

        private Vector3[] GetDeathZoneCorners()
        {
            Vector3[] corners = new Vector3[4];
            
            corners[0] = _mainCamera.ViewportToWorldPoint(new Vector3(-deathZoneExtension, -deathZoneExtension, 10f));
            corners[1] = _mainCamera.ViewportToWorldPoint(new Vector3(1 + deathZoneExtension, -deathZoneExtension, 10f));
            corners[2] = _mainCamera.ViewportToWorldPoint(new Vector3(1 + deathZoneExtension, 1 + deathZoneExtension, 10f));
            corners[3] = _mainCamera.ViewportToWorldPoint(new Vector3(-deathZoneExtension, 1 + deathZoneExtension, 10f));

            return corners;
        }
    }
}
