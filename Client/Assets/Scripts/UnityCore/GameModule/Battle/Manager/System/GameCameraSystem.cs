using UnityCore.Base;
using UnityCore.GameModule.Battle.Data;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class GameCameraSystem : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        private Transform _transFollow;

        public void Init(Transform transform)
        {
            if (transform == null)
            {
                throw new NotImplementedException("Transform cannot be null.");
            }
            _transFollow = transform;
        }

        private void Update()
        {
            if (_transFollow != null)
            {
                cameraTransform.position = _transFollow.position;
            }
        }
    }
}