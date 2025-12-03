using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    [CreateAssetMenu(fileName = "BattleCameraConfig", menuName = "Config/Battle Camera Config")]
    public class BattleCameraConfig : ScriptableObject
    {
        [Header("缩放设置")]
        [Tooltip("最小正交大小（最大缩放）")]
        public float minOrthographicSize = 5f;
        
        [Tooltip("最大正交大小（最小缩放）")]
        public float maxOrthographicSize = 15f;
        
        [Tooltip("边距百分比（0-1），玩家距离边缘的安全距离")]
        [Range(0f, 0.5f)]
        public float screenMargin = 0.15f;

        [Header("平滑设置")]
        [Tooltip("缩放平滑时间")]
        public float zoomSmoothTime = 0.5f;

        [Header("边界限制")]
        [Tooltip("是否启用摄像机边界")]
        public bool useCameraBounds = true;
        
        public Vector2 minCameraBounds = new Vector2(-50f, -10f);
        public Vector2 maxCameraBounds = new Vector2(50f, 20f);

        [Header("调试")]
        public bool showDebugGizmos = true;
    }
}
