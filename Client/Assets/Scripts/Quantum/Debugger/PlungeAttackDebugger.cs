using Photon.Deterministic;

namespace Quantum
{
    using UnityEngine;

    public unsafe class PlungeAttackDebugger : QuantumEntityViewComponent
    {
        [Header("可视化设置")]
        [Tooltip("是否启用落地猛击范围调试")]
        public bool EnableDebug = true;
        
        [Tooltip("冲击波范围颜色")]
        public Color ShockwaveColor = new Color(1f, 0.3f, 0f, 0.4f);
        
        [Tooltip("冲击波线框颜色")]
        public Color ShockwaveWireframeColor = new Color(1f, 0.5f, 0f, 1f);
        
        [Tooltip("冲击波显示持续时间（秒）")]
        public float DisplayDuration = 1.5f;
        
        [Tooltip("是否显示技能信息")]
        public bool ShowSkillInfo = true;
        
        [Tooltip("文字显示额外延长时间（秒）")]
        public float TextExtraDuration = 0.5f;

        [Header("文字样式")]
        [Tooltip("文字颜色")]
        public Color TextColor = new Color(1f, 0.7f, 0f, 1f);
        
        [Tooltip("文字背景颜色")]
        public Color TextBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
        
        [Tooltip("字体大小")]
        public int FontSize = 14;

        [Header("下落轨迹预测")]
        [Tooltip("是否显示下落轨迹")]
        public bool ShowFallTrajectory = true;
        
        [Tooltip("轨迹颜色")]
        public Color TrajectoryColor = new Color(0f, 1f, 1f, 0.5f);
        
        [Tooltip("轨迹预测步数")]
        public int TrajectorySteps = 20;

        private Shape2DConfig _shockwaveShape;
        private Transform2D _landingTransform;
        private float _displayTimer;
        private float _textDisplayTimer;
        private bool _isFacingRight;
        private string _cachedSkillInfo;
        private bool _isPlunging;
        
        public override void OnActivate(Frame frame)
        {
            if (!EnableDebug) return;

            QuantumEvent.Subscribe<EventSkillActivated>(this, OnSkillActivated);
            QuantumEvent.Subscribe<EventSkillLanded>(this, OnSkillLanded);
        }

        public override void OnDeactivate()
        {
            QuantumEvent.UnsubscribeListener(this);
        }

        private void OnSkillActivated(EventSkillActivated e)
        {
            if (e.Entity != EntityRef || !EnableDebug) return;

            var frame = VerifiedFrame;
            if (frame == null) return;

            if (!e.SkillData.Id.IsValid) return;

            var skillData = frame.FindAsset<SkillData>(e.SkillData.Id);
            
            // 使用 IsFlagSet 检查是否是落地猛击技能
            if (skillData != null && skillData.Flags.IsFlagSet(SkillFlags.ApplyDownwardForce))
            {
                _isPlunging = true;
                _shockwaveShape = null;
                _displayTimer = 0;
                
                _cachedSkillInfo = $"{skillData.SkillName}\n下落中...";
            }
        }

        private void OnSkillLanded(EventSkillLanded e)
        {
            if (e.Entity != EntityRef || !EnableDebug) return;

            var frame = VerifiedFrame;
            if (frame == null) return;

            var skillComponent = frame.Unsafe.GetPointer<SkillComponent>(EntityRef);
            if (!skillComponent->CurrentSkill.Id.IsValid) return;

            var skillData = frame.FindAsset<SkillData>(skillComponent->CurrentSkill.Id);
            
            if (skillData != null && skillData.Flags.IsFlagSet(SkillFlags.LandingShockwave))
            {
                CaptureShockwaveShape(skillData);
                _isPlunging = false;
            }
        }

        private void CaptureShockwaveShape(SkillData skillData)
        {
            var frame = VerifiedFrame;
            if (frame == null) return;

            _shockwaveShape = skillData.ShockwaveShape;
            
            var transform = frame.Unsafe.GetPointer<Transform2D>(EntityRef);
            _landingTransform = *transform;
            
            var movementData = frame.Unsafe.GetPointer<MovementComponent>(EntityRef);
            _isFacingRight = movementData->IsFacingRight;
            
            _displayTimer = DisplayDuration;
            _textDisplayTimer = DisplayDuration + TextExtraDuration;
            
            _cachedSkillInfo = $"{skillData.SkillName}\n冲击波范围";
        }

        private void Update()
        {
            if (!EnableDebug) return;

            if (_shockwaveShape != null)
            {
                _displayTimer -= Time.deltaTime;
                
                if (_displayTimer <= 0)
                {
                    _shockwaveShape = null;
                }
            }
            
            if (ShowSkillInfo && _textDisplayTimer > 0)
            {
                _textDisplayTimer -= Time.deltaTime;
            }
        }

        private void OnDrawGizmos()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            // 绘制下落轨迹
            if (_isPlunging && ShowFallTrajectory)
            {
                DrawFallTrajectory();
            }

            // 绘制冲击波范围
            if (_shockwaveShape != null)
            {
                DrawShockwaveShape();
            }
            
            // 绘制技能信息
            if (ShowSkillInfo && _textDisplayTimer > 0 && !string.IsNullOrEmpty(_cachedSkillInfo))
            {
                DrawSkillInfo();
            }
        }

        private void DrawFallTrajectory()
        {
            var frame = VerifiedFrame;
            if (frame == null) return;

            if (!frame.Unsafe.TryGetPointer<Transform2D>(EntityRef, out var transform)) return;
            if (!frame.Unsafe.TryGetPointer<SkillComponent>(EntityRef, out var skillComponent)) return;
            if (!skillComponent->CurrentSkill.Id.IsValid) return;

            var skillData = frame.FindAsset<SkillData>(skillComponent->CurrentSkill.Id);
            if (skillData == null) return;

            Vector3 currentPos = transform->Position.ToUnityVector3();
            
            // 简化的轨迹预测（只显示向下的直线）
            Gizmos.color = TrajectoryColor;
            
            for (int i = 0; i < TrajectorySteps; i++)
            {
                float t = i / (float)TrajectorySteps;
                Vector3 predictedPos = currentPos + Vector3.down * (t * 5f);
                
                if (i > 0)
                {
                    float prevT = (i - 1) / (float)TrajectorySteps;
                    Vector3 prevPos = currentPos + Vector3.down * (prevT * 5f);
                    Gizmos.DrawLine(prevPos, predictedPos);
                }
            }
            
            // 绘制预测着陆点标记
            Vector3 landingPoint = currentPos + Vector3.down * 5f;
            Gizmos.DrawWireSphere(landingPoint, 0.2f);
        }

        private void DrawShockwaveShape()
        {
            Color fillColor = ShockwaveColor;
            
            float fadeRatio = Mathf.Clamp01(_displayTimer / DisplayDuration);
            fillColor.a *= fadeRatio;
            
            Color wireColor = ShockwaveWireframeColor;
            wireColor.a *= fadeRatio;

            Vector3 position = _landingTransform.Position.ToUnityVector3();
            Vector3 offset = _shockwaveShape.PositionOffset.ToUnityVector3();
            
            if (!_isFacingRight)
            {
                offset.x = -offset.x;
            }
            
            Vector3 finalPosition = position + offset;
            
            float shapeRotation = _shockwaveShape.RotationOffset.AsFloat;
            if (!_isFacingRight)
            {
                shapeRotation = 180f - shapeRotation;
            }

            switch (_shockwaveShape.ShapeType)
            {
                case Shape2DType.Box:
                    DrawBox(finalPosition, _shockwaveShape.BoxExtents, shapeRotation, fillColor, wireColor);
                    break;

                case Shape2DType.Circle:
                    DrawCircle(finalPosition, _shockwaveShape.CircleRadius.AsFloat, fillColor, wireColor);
                    break;

                case Shape2DType.Capsule:
                    DrawCapsule(finalPosition, _shockwaveShape.CapsuleSize, shapeRotation, fillColor, wireColor);
                    break;

                case Shape2DType.Edge:
                    DrawEdge(finalPosition, _shockwaveShape.EdgeExtent.AsFloat, shapeRotation, fillColor, wireColor);
                    break;
            }
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

        private void DrawCircle(Vector3 center, float radius, Color fillColor, Color wireColor)
        {
            Gizmos.color = fillColor;
            Gizmos.DrawSphere(center, radius);

            Gizmos.color = wireColor;
            DrawWireCircle(center, radius);
        }

        private void DrawCapsule(Vector3 center, FPVector2 size, float rotation, Color fillColor, Color wireColor)
        {
            float width = size.X.AsFloat;
            float height = size.Y.AsFloat;
            float radius = width * 0.5f;

            Quaternion rot = Quaternion.Euler(0, 0, rotation);

            Vector3 top = center + rot * new Vector3(0, (height - width) * 0.5f, 0);
            Vector3 bottom = center + rot * new Vector3(0, -(height - width) * 0.5f, 0);

            Gizmos.color = fillColor;
            Gizmos.DrawSphere(top, radius);
            Gizmos.DrawSphere(bottom, radius);

            Gizmos.color = wireColor;
            DrawWireCircle(top, radius);
            DrawWireCircle(bottom, radius);
        }

        private void DrawEdge(Vector3 center, float extent, float rotation, Color fillColor, Color wireColor)
        {
            Quaternion rot = Quaternion.Euler(0, 0, rotation);
            Vector3 start = center + rot * new Vector3(-extent, 0, 0);
            Vector3 end = center + rot * new Vector3(extent, 0, 0);

            Gizmos.color = wireColor;
            Gizmos.DrawLine(start, end);

            Gizmos.DrawSphere(start, 0.05f);
            Gizmos.DrawSphere(end, 0.05f);
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
        
        private void DrawSkillInfo()
        {
#if UNITY_EDITOR
            Vector3 position = _landingTransform.Position.ToUnityVector3();
            Vector3 labelPosition = position + Vector3.up * 1.2f;
            
            GUIStyle backgroundStyle = new GUIStyle()
            {
                normal = new GUIStyleState() 
                { 
                    background = CreateBackgroundTexture(TextBackgroundColor)
                },
                padding = new RectOffset(8, 8, 4, 4),
                alignment = TextAnchor.MiddleCenter
            };
            
            GUIStyle textStyle = new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = TextColor },
                fontSize = FontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            float textFadeRatio = Mathf.Clamp01(_textDisplayTimer / TextExtraDuration);
            Color adjustedTextColor = TextColor;
            adjustedTextColor.a *= textFadeRatio;
            textStyle.normal.textColor = adjustedTextColor;
            
            GUIContent content = new GUIContent(_cachedSkillInfo);
            Vector2 size = textStyle.CalcSize(content);
            size.x += 16;
            size.y += 8;
            
            var sceneView = UnityEditor.SceneView.currentDrawingSceneView;
            if (sceneView == null) return;
            
            Vector3 screenPos = sceneView.camera.WorldToScreenPoint(labelPosition);
            if (screenPos.z > 0)
            {
                UnityEditor.Handles.BeginGUI();
                
                Rect backgroundRect = new Rect(
                    screenPos.x - size.x * 0.5f, 
                    sceneView.camera.pixelHeight - screenPos.y - size.y * 0.5f, 
                    size.x, 
                    size.y
                );
                
                GUI.Box(backgroundRect, "", backgroundStyle);
                GUI.Label(backgroundRect, _cachedSkillInfo, textStyle);
                
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
