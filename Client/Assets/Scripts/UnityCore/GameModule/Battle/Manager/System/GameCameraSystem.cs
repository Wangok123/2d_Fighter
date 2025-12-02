using UnityCore.Base;
using UnityCore.GameModule.Battle.Data;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityEngine;
using Cinemachine;
using NotImplementedException = System.NotImplementedException;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class GameCameraSystem : MonoBehaviour
    {
        [Header("传统单人跟随（已弃用）")]
        [SerializeField] private Transform cameraTransform;
        private Transform _transFollow;

        [Header("大乱斗式摄像机")]
        [SerializeField] private SmashBrosCameraController smashBrosCamera;
        [SerializeField] private SmashBrosCameraPlayerTracker playerTracker;
        
        [Tooltip("是否使用大乱斗摄像机模式")]
        [SerializeField] private bool useSmashBrosMode = true;

        private void Awake()
        {
            if (useSmashBrosMode)
            {
                if (smashBrosCamera == null)
                {
                    smashBrosCamera = GetComponent<SmashBrosCameraController>();
                }

                if (playerTracker == null)
                {
                    playerTracker = GetComponent<SmashBrosCameraPlayerTracker>();
                }

                if (smashBrosCamera == null)
                {
                    Debug.LogWarning("[GameCameraSystem] SmashBrosCameraController 未找到，将使用传统跟随模式");
                    useSmashBrosMode = false;
                }
            }
        }

        public void Init(Transform transform)
        {
            if (!useSmashBrosMode)
            {
                if (transform == null)
                {
                    throw new NotImplementedException("Transform cannot be null.");
                }
                _transFollow = transform;
            }
        }

        private void Update()
        {
            if (!useSmashBrosMode && _transFollow != null && cameraTransform != null)
            {
                cameraTransform.position = _transFollow.position;
            }
        }
    }
}