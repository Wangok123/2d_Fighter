using Photon.Deterministic;

namespace Quantum
{
    using UnityEngine;

    public unsafe class AttackRangeDebugger : QuantumEntityViewComponent
    {
        [Header("可视化设置")]
        [Tooltip("是否启用攻击范围调试")]
        public bool EnableDebug = true;
        
        [Tooltip("攻击范围颜色")]
        public Color AttackRangeColor = new Color(1f, 0f, 0f, 0.3f);
        
        [Tooltip("攻击范围线框颜色")]
        public Color AttackWireframeColor = Color.red;
        
        [Tooltip("默认显示持续时间（当无法获取配置时使用）")]
        public float DefaultDisplayDuration = 0.1f;

        [Tooltip("是否显示激活时间窗口信息")]
        public bool ShowTimingInfo = true;
        
        [Tooltip("文字显示额外延长时间（秒）")]
        public float TextExtraDuration = 0.5f;

        [Header("文字样式")]
        [Tooltip("文字颜色")]
        public Color TextColor = Color.yellow;
        
        [Tooltip("文字背景颜色")]
        public Color TextBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
        
        [Tooltip("字体大小")]
        public int FontSize = 14;
        
        [Header("每段连击独立颜色（可选）")]
        [Tooltip("第一段攻击颜色")]
        public Color Combo1Color = new Color(1f, 0.5f, 0f, 0.3f);
        
        [Tooltip("第二段攻击颜色")]
        public Color Combo2Color = new Color(1f, 0f, 0f, 0.3f);
        
        [Tooltip("第三段攻击颜色")]
        public Color Combo3Color = new Color(0.5f, 0f, 1f, 0.3f);

        private Shape2DConfig _currentAttackShape;
        private Transform2D _attackTransform;
        private float _displayTimer;
        private float _textDisplayTimer;
        private float _maxDisplayDuration;
        private int _currentComboStep;
        private bool _isFacingRight;

        private float _hitboxActiveTime;
        private float _hitboxActiveDuration;
        
        private string _cachedTimingInfo;
        
        public override void OnActivate(Frame frame)
        {
            if (!EnableDebug) return;

            // 订阅攻击事件
            QuantumEvent.Subscribe<EventAttackHitboxActivated>(this, OnAttackHitboxActivated);
            QuantumEvent.Subscribe<EventAbilityEnded>(this, OnAbilityEnded);
        }

        public override void OnDeactivate()
        {
            QuantumEvent.UnsubscribeListener(this);
        }

        private void OnAttackHitboxActivated(EventAttackHitboxActivated e)
        {
            if (e.Entity != EntityRef || !EnableDebug) return;

            _currentComboStep = e.ComboStep;
            CaptureAttackShape();
        }

        private void OnAbilityEnded(EventAbilityEnded e)
        {
            if (e.Entity != EntityRef || !EnableDebug) return;

            if (e.AbilityType == AbilityType.AttackLight || e.AbilityType == AbilityType.AttackHeavy)
            {
                _currentAttackShape = null;
            }
        }

        private void CaptureAttackShape()
        {
            var frame = VerifiedFrame;
            if (frame == null) return;

            var abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(EntityRef);
            var activeAbilityType = abilityInventory->ActiveAbilityInfo.ActiveAbilityType;

            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            if (!dic.TryGetValue(activeAbilityType, out var ability)) return;

            var abilityData = frame.FindAsset<AbilityData>(ability.AbilityData.Id);
            
            if (abilityData is AttackAbilityData attackData)
            {
                _currentAttackShape = attackData.AttackShape;
                
                var transform = frame.Unsafe.GetPointer<Transform2D>(EntityRef);
                _attackTransform = *transform;
                var movementData = frame.Unsafe.GetPointer<MovementComponent>(EntityRef);
                _isFacingRight = movementData->IsFacingRight;
                
                _hitboxActiveTime = attackData.HitboxActiveTime.AsFloat;
                _hitboxActiveDuration = attackData.HitboxActiveDuration.AsFloat;
                _maxDisplayDuration = _hitboxActiveDuration > 0 ? _hitboxActiveDuration : DefaultDisplayDuration;
                _displayTimer = _maxDisplayDuration;
                _textDisplayTimer = _maxDisplayDuration + TextExtraDuration;
                
                _cachedTimingInfo = _currentComboStep > 0 
                    ? $"Combo {_currentComboStep}\nStartup: {_hitboxActiveTime:F3}s | Active: {_hitboxActiveDuration:F3}s"
                    : $"Attack\nStartup: {_hitboxActiveTime:F3}s | Active: {_hitboxActiveDuration:F3}s";
            }
        }

        private void Update()
        {
            if (!EnableDebug) return;

            if (_currentAttackShape != null)
            {
                _displayTimer -= Time.deltaTime;
                
                if (_displayTimer <= 0)
                {
                    _currentAttackShape = null;
                }
            }
            
            if (ShowTimingInfo && _textDisplayTimer > 0)
            {
                _textDisplayTimer -= Time.deltaTime;
            }
        }

        private void OnDrawGizmos()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            if (_currentAttackShape != null)
            {
                DrawAttackShape();
            }
            
            if (ShowTimingInfo && _textDisplayTimer > 0 && !string.IsNullOrEmpty(_cachedTimingInfo))
            {
                DrawTimingInfo();
            }
        }

        private void DrawAttackShape()
        {
            Color fillColor = GetComboColor(_currentComboStep);
            
            float fadeRatio = Mathf.Clamp01(_displayTimer / _maxDisplayDuration);
            fillColor.a *= fadeRatio;
            
            Color wireColor = AttackWireframeColor;
            wireColor.a *= fadeRatio;

            Vector3 position = _attackTransform.Position.ToUnityVector3();
            Vector3 offset = _currentAttackShape.PositionOffset.ToUnityVector3();
            
            if (!_isFacingRight)
            {
                offset.x = -offset.x;
            }
            
            Vector3 finalPosition = position + offset;
            
            float shapeRotation = _currentAttackShape.RotationOffset.AsFloat;
            if (!_isFacingRight)
            {
                shapeRotation = 180f - shapeRotation;
            }

            switch (_currentAttackShape.ShapeType)
            {
                case Shape2DType.Box:
                    DrawBox(finalPosition, _currentAttackShape.BoxExtents, shapeRotation, fillColor, wireColor);
                    break;

                case Shape2DType.Circle:
                    DrawCircle(finalPosition, _currentAttackShape.CircleRadius.AsFloat, fillColor, wireColor);
                    break;

                case Shape2DType.Capsule:
                    DrawCapsule(finalPosition, _currentAttackShape.CapsuleSize, shapeRotation, fillColor, wireColor);
                    break;

                case Shape2DType.Edge:
                    DrawEdge(finalPosition, _currentAttackShape.EdgeExtent.AsFloat, shapeRotation, fillColor, wireColor);
                    break;
            }
        }

        private Color GetComboColor(int comboStep)
        {
            return comboStep switch
            {
                1 => Combo1Color,
                2 => Combo2Color,
                3 => Combo3Color,
                _ => AttackRangeColor
            };
        }

        private void DrawBox(Vector3 center, FPVector2 extents, float rotation, Color fillColor, Color wireColor)
        {
            Vector3 size = new Vector3(extents.X.AsFloat * 2, extents.Y.AsFloat * 2, 0.1f);
            Quaternion rot = Quaternion.Euler(0, 0, rotation);

            // 绘制半透明填充
            Gizmos.color = fillColor;
            Gizmos.matrix = Matrix4x4.TRS(center, rot, size);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            // 绘制线框
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void DrawCircle(Vector3 center, float radius, Color fillColor, Color wireColor)
        {
            // 绘制填充圆
            Gizmos.color = fillColor;
            Gizmos.DrawSphere(center, radius);

            // 绘制线框圆
            Gizmos.color = wireColor;
            DrawWireCircle(center, radius);
        }

        private void DrawCapsule(Vector3 center, FPVector2 size, float rotation, Color fillColor, Color wireColor)
        {
            float width = size.X.AsFloat;
            float height = size.Y.AsFloat;
            float radius = width * 0.5f;

            Quaternion rot = Quaternion.Euler(0, 0, rotation);

            // 简化版胶囊体（两个圆+矩形）
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

            // 绘制端点
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
        
        private void DrawTimingInfo()
        {
#if UNITY_EDITOR
            Vector3 position = _attackTransform.Position.ToUnityVector3();
            Vector3 labelPosition = position + Vector3.up * 0.8f;
            
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
            
            GUIContent content = new GUIContent(_cachedTimingInfo);
            Vector2 size = textStyle.CalcSize(content);
            size.x += 16;
            size.y += 8;
            
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
                GUI.Label(backgroundRect, _cachedTimingInfo, textStyle);
                
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
