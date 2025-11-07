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
        
        [Tooltip("显示持续时间")]
        public float DisplayDuration = 0.1f;

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
        private int _currentComboStep;
        private bool _isFacingRight;

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
                _displayTimer = 0f;
            }
        }

        private void CaptureAttackShape()
        {
            var frame = VerifiedFrame;
            if (frame == null) return;

            // 获取当前激活的技能
            var abilityInventory = frame.Unsafe.GetPointer<AbilityInventory>(EntityRef);
            var activeAbilityType = abilityInventory->ActiveAbilityInfo.ActiveAbilityType;

            // 获取技能数据
            var dic = frame.ResolveDictionary(abilityInventory->AbilitiesDic);
            if (!dic.TryGetValue(activeAbilityType, out var ability)) return;

            var abilityData = frame.FindAsset<AbilityData>(ability.AbilityData.Id);
            
            // 尝试转换为攻击技能
            if (abilityData is AttackAbilityData attackData)
            {
                _currentAttackShape = attackData.AttackShape;
                
                // 获取当前位置和朝向
                var transform = frame.Unsafe.GetPointer<Transform2D>(EntityRef);
                _attackTransform = *transform;
                var movementData = frame.Unsafe.GetPointer<MovementData>(EntityRef);
                _isFacingRight = movementData->IsFacingRight;
                
                _displayTimer = DisplayDuration;
            }
        }

        private void Update()
        {
            if (!EnableDebug || _currentAttackShape == null) return;

            _displayTimer -= Time.deltaTime;
            
            if (_displayTimer <= 0)
            {
                _currentAttackShape = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (!EnableDebug || _currentAttackShape == null) return;
            if (!Application.isPlaying) return;

            Color fillColor = GetComboColor(_currentComboStep);
            Color wireColor = AttackWireframeColor;

            Vector3 position = _attackTransform.Position.ToUnityVector3();
            Vector3 offset = _currentAttackShape.PositionOffset.ToUnityVector3();
            
            // ✅ 根据朝向翻转偏移的 X 轴
            if (!_isFacingRight)
            {
                offset.x = -offset.x;
            }
            
            Vector3 finalPosition = position + offset;
            
            // ✅ 旋转角度也需要根据朝向调整
            float shapeRotation = _currentAttackShape.RotationOffset.AsFloat;
            if (!_isFacingRight)
            {
                // 向左时，旋转需要镜像
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
    }
}
