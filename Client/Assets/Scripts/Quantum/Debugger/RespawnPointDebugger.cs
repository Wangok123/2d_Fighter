using Photon.Deterministic;

namespace Quantum
{
    using UnityEngine;

    public unsafe class RespawnPointDebugger : QuantumEntityViewComponent
    {
        [Header("可视化设置")]
        [Tooltip("是否启用重生点调试")]
        public bool EnableDebug = true;

        [Tooltip("重生点填充颜色")]
        public Color FillColor = new Color(0f, 1f, 0f, 0.3f);

        [Tooltip("重生点线框颜色")]
        public Color WireframeColor = Color.green;

        [Tooltip("指示器颜色")]
        public Color IndicatorColor = Color.green;

        [Header("动画效果")]
        [Tooltip("是否启用旋转动画")]
        public bool EnableRotationAnimation = true;

        [Tooltip("旋转速度")]
        public float RotationSpeed = 30f;

        [Tooltip("是否启用呼吸动画")]
        public bool EnableBreathingAnimation = true;

        [Tooltip("呼吸速度")]
        public float BreathingSpeed = 2f;

        [Tooltip("呼吸强度")]
        [Range(0f, 0.5f)]
        public float BreathingIntensity = 0.15f;

        [Header("信息显示")]
        [Tooltip("是否显示重生点信息")]
        public bool ShowInfo = true;

        [Tooltip("文字颜色")]
        public Color TextColor = Color.green;

        [Tooltip("文字背景颜色")]
        public Color TextBackgroundColor = new Color(0f, 0f, 0f, 0.7f);

        [Tooltip("字体大小")]
        public int FontSize = 12;

        [Header("队伍颜色")]
        [Tooltip("蓝队颜色")]
        public Color BlueTeamColor = new Color(0f, 0.5f, 1f, 0.4f);

        [Tooltip("红队颜色")]
        public Color RedTeamColor = new Color(1f, 0f, 0f, 0.4f);

        [Tooltip("中立队颜色")]
        public Color NeutralTeamColor = new Color(0f, 1f, 0f, 0.4f);

        private RespawnPoint* _respawnPoint;
        private Transform2D* _transform;
        private float _rotationAngle;
        private float _breathingTime;
        private string _cachedInfo;

        private void Update()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<RespawnPoint>(EntityRef, out _respawnPoint)) return;
            if (!frame.Unsafe.TryGetPointer<Transform2D>(EntityRef, out _transform)) return;

            if (EnableRotationAnimation)
            {
                _rotationAngle += Time.deltaTime * RotationSpeed;
                if (_rotationAngle >= 360f)
                    _rotationAngle -= 360f;
            }

            if (EnableBreathingAnimation)
            {
                _breathingTime += Time.deltaTime * BreathingSpeed;
            }

            if (ShowInfo)
            {
                UpdateInfo();
            }
        }

        private void UpdateInfo()
        {
            string teamName = GetTeamName(_respawnPoint->Team);
            Vector2 pos = _transform->Position.ToUnityVector2();
            _cachedInfo = $"Respawn Point\nTeam: {teamName}\nPos: ({pos.x:F1}, {pos.y:F1})";
        }

        private string GetTeamName(CharacterTeam team)
        {
            return team switch
            {
                CharacterTeam.Blue => "蓝队",
                CharacterTeam.Red => "红队",
                CharacterTeam.Neutral => "中立",
                _ => "None"
            };
        }

        private void OnDrawGizmos()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<RespawnPoint>(EntityRef, out _respawnPoint)) return;
            if (!frame.Unsafe.TryGetPointer<Transform2D>(EntityRef, out _transform)) return;

            DrawRespawnPoint();

            if (ShowInfo && !string.IsNullOrEmpty(_cachedInfo))
            {
                DrawInfo();
            }
        }

        private void DrawRespawnPoint()
        {
            Vector3 position = _transform->Position.ToUnityVector3();

            Color teamColor = GetTeamColor(_respawnPoint->Team);
            float baseRadius = 0.3f;
            float outerRadius1 = 0.5f;
            float outerRadius2 = 0.7f;

            if (EnableBreathingAnimation)
            {
                float breathing = Mathf.Sin(_breathingTime) * BreathingIntensity;
                baseRadius *= (1f + breathing);
                outerRadius1 *= (1f + breathing * 0.5f);
                outerRadius2 *= (1f + breathing * 0.3f);
            }

            Gizmos.color = teamColor;
            Gizmos.DrawSphere(position, baseRadius);

            Gizmos.color = WireframeColor;
            DrawWireCircle(position, outerRadius1);
            DrawWireCircle(position, outerRadius2);

            Gizmos.color = IndicatorColor;
            Gizmos.DrawLine(position + Vector3.down * 0.5f, position + Vector3.up * 0.5f);

            if (EnableRotationAnimation)
            {
                DrawRotatingIndicators(position, outerRadius2);
            }
        }

        private void DrawRotatingIndicators(Vector3 center, float radius)
        {
            int count = 4;
            for (int i = 0; i < count; i++)
            {
                float angle = (_rotationAngle + i * (360f / count)) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                Vector3 indicatorPos = center + offset;

                Gizmos.color = IndicatorColor;
                Gizmos.DrawSphere(indicatorPos, 0.05f);
            }
        }

        private Color GetTeamColor(CharacterTeam team)
        {
            return team switch
            {
                CharacterTeam.Blue => BlueTeamColor,
                CharacterTeam.Red => RedTeamColor,
                CharacterTeam.Neutral => NeutralTeamColor,
                _ => FillColor
            };
        }

        private void DrawWireCircle(Vector3 center, float radius, int segments = 32)
        {
            float angle = 0f;
            Vector3 lastPoint = center + new Vector3(radius, 0, 0);

            for (int i = 0; i <= segments; i++)
            {
                angle += (2 * Mathf.PI / segments);
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
            }
        }

        private void DrawInfo()
        {
#if UNITY_EDITOR
            Vector3 position = _transform->Position.ToUnityVector3();
            Vector3 labelPosition = position + Vector3.up * 0.8f;

            GUIStyle backgroundStyle = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = CreateBackgroundTexture(TextBackgroundColor)
                },
                padding = new RectOffset(6, 6, 3, 3),
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle textStyle = new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = TextColor },
                fontSize = FontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            GUIContent content = new GUIContent(_cachedInfo);
            Vector2 size = textStyle.CalcSize(content);
            size.x += 12;
            size.y += 6;

            Vector3 screenPos = UnityEditor.SceneView.currentDrawingSceneView?.camera.WorldToScreenPoint(labelPosition) ?? Vector3.zero;
            if (screenPos.z > 0)
            {
                UnityEditor.Handles.BeginGUI();

                Rect backgroundRect = new Rect(
                    screenPos.x - size.x * 0.5f,
                    UnityEditor.SceneView.currentDrawingSceneView.camera.pixelHeight - screenPos.y - size.y * 0.5f,
                    size.x,
                    size.y
                );

                GUI.Box(backgroundRect, "", backgroundStyle);
                GUI.Label(backgroundRect, _cachedInfo, textStyle);

                UnityEditor.Handles.EndGUI();
            }
#endif
        }

#if UNITY_EDITOR
        private Texture2D CreateBackgroundTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
#endif
    }
}
