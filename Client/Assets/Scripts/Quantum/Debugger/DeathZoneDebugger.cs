using Photon.Deterministic;

namespace Quantum
{
    using UnityEngine;

    public unsafe class DeathZoneDebugger : QuantumEntityViewComponent
    {
        [Header("可视化设置")]
        [Tooltip("是否启用死亡区域调试")]
        public bool EnableDebug = true;

        [Tooltip("死亡区域填充颜色")]
        public Color FillColor = new Color(1f, 0f, 0f, 0.3f);

        [Tooltip("死亡区域线框颜色")]
        public Color WireframeColor = Color.red;

        [Header("动画效果")]
        [Tooltip("是否启用脉冲动画")]
        public bool EnablePulseAnimation = true;

        [Tooltip("脉冲速度")]
        public float PulseSpeed = 1.5f;

        [Tooltip("脉冲强度（0-1）")]
        [Range(0f, 1f)]
        public float PulseIntensity = 0.2f;

        [Header("信息显示")]
        [Tooltip("是否显示死亡区域信息")]
        public bool ShowInfo = true;

        [Tooltip("文字颜色")]
        public Color TextColor = Color.red;

        [Tooltip("文字背景颜色")]
        public Color TextBackgroundColor = new Color(0f, 0f, 0f, 0.7f);

        [Tooltip("字体大小")]
        public int FontSize = 12;

        private DeathZone* _deathZone;
        private Transform2D* _transform;
        private PhysicsCollider2D* _collider;
        private float _pulseTime;
        private string _cachedInfo;

        private void Update()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<DeathZone>(EntityRef, out _deathZone)) return;
            if (!frame.Unsafe.TryGetPointer<Transform2D>(EntityRef, out _transform)) return;
            if (!frame.Unsafe.TryGetPointer<PhysicsCollider2D>(EntityRef, out _collider)) return;

            if (EnablePulseAnimation)
            {
                _pulseTime += Time.deltaTime * PulseSpeed;
            }

            if (ShowInfo)
            {
                UpdateInfo();
            }
        }

        private void UpdateInfo()
        {
            string status = _deathZone->IsActive ? "Active" : "Inactive";
            _cachedInfo = $"Death Zone\nStatus: {status}";
        }

        private void OnDrawGizmos()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<DeathZone>(EntityRef, out _deathZone)) return;
            if (!frame.Unsafe.TryGetPointer<Transform2D>(EntityRef, out _transform)) return;
            if (!frame.Unsafe.TryGetPointer<PhysicsCollider2D>(EntityRef, out _collider)) return;

            if (!_deathZone->IsActive) return;

            DrawDeathZone();

            if (ShowInfo && !string.IsNullOrEmpty(_cachedInfo))
            {
                DrawInfo();
            }
        }

        private void DrawDeathZone()
        {
            Vector3 position = _transform->Position.ToUnityVector3();

            Color fillColor = FillColor;
            if (EnablePulseAnimation)
            {
                float pulse = Mathf.Sin(_pulseTime) * PulseIntensity;
                fillColor.a *= (1f + pulse);
            }

            var shape = _collider->Shape;
            if (shape.Type == Shape2DType.Circle)
            {
                float radius = shape.Circle.Radius.AsFloat;
                DrawCircle(position, radius, fillColor, WireframeColor);
            }
            else if (shape.Type == Shape2DType.Box)
            {
                FPVector2 extents = shape.Box.Extents;
                float rotation = _transform->Rotation.AsFloat;
                DrawBox(position, extents, rotation, fillColor, WireframeColor);
            }
        }

        private void DrawCircle(Vector3 center, float radius, Color fillColor, Color wireColor)
        {
            Gizmos.color = fillColor;
            Gizmos.DrawSphere(center, radius);

            Gizmos.color = wireColor;
            DrawWireCircle(center, radius);
        }

        private void DrawBox(Vector3 center, FPVector2 extents, float rotation, Color fillColor, Color wireColor)
        {
            Vector3 size = new Vector3(extents.X.AsFloat * 2, extents.Y.AsFloat * 2, 0.1f);
            Quaternion rot = Quaternion.Euler(0, 0, rotation);

            Gizmos.color = fillColor;
            Gizmos.matrix = Matrix4x4.TRS(center, rot, size);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
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
            Vector3 labelPosition = position + Vector3.up * 0.7f;

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
