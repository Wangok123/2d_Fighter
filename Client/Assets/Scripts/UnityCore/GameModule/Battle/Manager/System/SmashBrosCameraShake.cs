using System.Collections;
using Cinemachine;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class SmashBrosCameraShake : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        private CinemachineBasicMultiChannelPerlin _noise;

        private void Awake()
        {
            if (virtualCamera != null)
            {
                _noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        public void ShakeCamera(float intensity, float duration)
        {
            if (_noise == null)
            {
                Debug.LogWarning("[SmashBrosCameraShake] CinemachineBasicMultiChannelPerlin 未找到，请在 Virtual Camera 上添加 Noise 组件");
                return;
            }

            StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        public void ShakeCameraOnHit(float damage)
        {
            float intensity = Mathf.Clamp(damage * 0.1f, 0.2f, 2f);
            ShakeCamera(intensity, 0.2f);
        }

        private IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            _noise.m_AmplitudeGain = intensity;

            yield return new WaitForSeconds(duration);

            _noise.m_AmplitudeGain = 0f;
        }
    }
}
