using Photon.Deterministic;

namespace Quantum
{
    using UnityEngine;

    public unsafe class SkillFieldDebugger : QuantumEntityViewComponent
    {
        [Header("可视化设置")]
        [Tooltip("是否启用技能域调试")]
        public bool EnableDebug = true;
        
        [Tooltip("效果范围颜色")]
        public Color EffectAreaColor = new Color(0f, 1f, 0f, 0.2f);
        
        [Tooltip("效果范围线框颜色")]
        public Color EffectWireframeColor = Color.green;
        
        [Tooltip("Tick时的闪烁颜色")]
        public Color TickFlashColor = new Color(1f, 1f, 1f, 0.6f);
        
        [Tooltip("Tick闪烁持续时间")]
        public float TickFlashDuration = 0.1f;

        [Header("动画效果")]
        [Tooltip("是否启用脉冲动画")]
        public bool EnablePulseAnimation = true;
        
        [Tooltip("脉冲速度")]
        public float PulseSpeed = 2f;
        
        [Tooltip("脉冲强度（0-1）")]
        [Range(0f, 1f)]
        public float PulseIntensity = 0.3f;

        [Header("信息显示")]
        [Tooltip("是否显示技能域信息")]
        public bool ShowFieldInfo = true;
        
        [Tooltip("文字颜色")]
        public Color TextColor = Color.cyan;
        
        [Tooltip("文字背景颜色")]
        public Color TextBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
        
        [Tooltip("字体大小")]
        public int FontSize = 12;

        [Header("不同类型颜色")]
        [Tooltip("伤害类型颜色")]
        public Color DamageColor = new Color(1f, 0f, 0f, 0.2f);
        
        [Tooltip("治疗类型颜色")]
        public Color HealColor = new Color(0f, 1f, 0f, 0.2f);
        
        [Tooltip("减速类型颜色")]
        public Color SlowColor = new Color(0f, 0.5f, 1f, 0.2f);
        
        [Tooltip("推拉力场颜色")]
        public Color PushColor = new Color(1f, 0.5f, 0f, 0.2f);
        
        [Tooltip("旋涡力场颜色")]
        public Color VortexColor = new Color(0.8f, 0f, 0.8f, 0.2f);
        
        [Tooltip("爆炸类型颜色")]
        public Color ExplosionColor = new Color(1f, 0.3f, 0f, 0.3f);

        [Header("爆炸特殊显示")]
        [Tooltip("是否显示爆炸倒计时闪烁")]
        public bool ShowExplosionCountdown = true;

        private SkillFieldComponent* _skillField;
        private Transform2D* _transform;
        private SkillFieldData _skillFieldData;
        private string _cachedInfo;
        private float _tickFlashTimer;
        private float _pulseTime;

        public override void OnActivate(Frame frame)
        {
            if (!EnableDebug) return;
            QuantumEvent.Subscribe<EventOnSkillFieldSpawned>(this, OnSkillFieldSpawned);
            QuantumEvent.Subscribe<EventOnSkillFieldTick>(this, OnSkillFieldTick);
        }

        public override void OnDeactivate()
        {
            QuantumEvent.UnsubscribeListener(this);
        }

        private void OnSkillFieldSpawned(EventOnSkillFieldSpawned e)
        {
            if (e.SkillField != EntityRef || !EnableDebug) return;

            var frame = VerifiedFrame;
            if (frame == null) return;

            _skillField = frame.Unsafe.GetPointer<SkillFieldComponent>(EntityRef);
            _skillFieldData = frame.FindAsset<SkillFieldData>(_skillField->SkillFieldData.Id);
        }

        private void OnSkillFieldTick(EventOnSkillFieldTick e)
        {
            if (e.SkillField != EntityRef || !EnableDebug) return;
            _tickFlashTimer = TickFlashDuration;
        }

        private void Update()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<SkillFieldComponent>(EntityRef, out _skillField)) return;
            _transform = frame.Unsafe.GetPointer<Transform2D>(EntityRef);

            if (_tickFlashTimer > 0)
            {
                _tickFlashTimer -= Time.deltaTime;
            }

            if (EnablePulseAnimation)
            {
                _pulseTime += Time.deltaTime * PulseSpeed;
            }

            UpdateFieldInfo(frame);
        }

        private void UpdateFieldInfo(Frame frame)
        {
            if (!ShowFieldInfo || _skillFieldData == null) return;

            string fieldType = GetFieldTypeName();
            string durationInfo = GetDurationInfo(frame);
            string extraInfo = GetExtraInfo(frame);

            _cachedInfo = $"Skill Field\nType: {fieldType}{durationInfo}{extraInfo}";
        }

        private string GetDurationInfo(Frame frame)
        {
            if (_skillFieldData is DelayedExplosionFieldData explosionData)
            {
                FP elapsed = _skillField->TickTimer.ElapsedSeconds(frame);
                FP remaining = explosionData.ExplosionDelay - elapsed;
                return $"\nDetonation: {remaining.AsFloat:F2}s";
            }
            else
            {
                FP remainingDuration = _skillFieldData.Duration - _skillField->TickTimer.ElapsedSeconds(frame);
                FP tickInterval = _skillFieldData.TickInterval;
                return $"\nTick: {tickInterval.AsFloat:F2}s\nRemaining: {remainingDuration.AsFloat:F2}s";
            }
        }

        private string GetFieldTypeName()
        {
            if (_skillFieldData is DelayedExplosionFieldData)
                return "Explosion";
            if (_skillFieldData is DamageFieldData)
                return "Damage";
            if (_skillFieldData is HealFieldData)
                return "Heal";
            if (_skillFieldData is SlowFieldData)
                return "Slow";
            if (_skillFieldData is PushFieldData pushData)
                return pushData.FieldType == ForceFieldType.Push ? "Push" : "Pull";
            if (_skillFieldData is VortexFieldData)
                return "Vortex";
            
            return "Unknown";
        }

        private string GetExtraInfo(Frame frame)
        {
            if (_skillFieldData is DelayedExplosionFieldData explosionData)
                return $"\nDamage: {explosionData.ExplosionDamage.AsFloat:F0}";
            if (_skillFieldData is DamageFieldData damageData)
                return $"\nDamage: {damageData.DamagePerTick.AsFloat:F1}";
            if (_skillFieldData is HealFieldData healData)
                return $"\nHeal: {healData.HealPerTick.AsFloat:F1}";
            if (_skillFieldData is SlowFieldData slowData)
                return $"\nSlow: {(slowData.SpeedReductionPercent.AsFloat * 100):F0}%";
            if (_skillFieldData is PushFieldData pushData)
                return $"\nForce: {pushData.ForceStrength.AsFloat:F1}";
            if (_skillFieldData is VortexFieldData vortexData)
                return $"\nPull: {vortexData.CentripetalForce.AsFloat:F1} | Spin: {vortexData.TangentialForce.AsFloat:F1}";
            
            return "";
        }

        private void OnDrawGizmos()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<SkillFieldComponent>(EntityRef, out _skillField)) return;
            _transform = frame.Unsafe.GetPointer<Transform2D>(EntityRef);

            DrawEffectArea();

            if (ShowFieldInfo && !string.IsNullOrEmpty(_cachedInfo))
            {
                DrawFieldInfo();
            }
        }

        private void DrawEffectArea()
        {
            if (_skillFieldData == null) return;

            Shape2DConfig shape = _skillFieldData.EffectArea;
            if (shape == null) return;

            Vector3 position = _transform->Position.ToUnityVector3();
            Color fillColor = GetFieldTypeColor();

            if (_skillFieldData is DelayedExplosionFieldData explosionData && ShowExplosionCountdown)
            {
                fillColor = ApplyExplosionCountdownEffect(fillColor, explosionData);
            }
            else
            {
                if (_tickFlashTimer > 0)
                {
                    float flashRatio = _tickFlashTimer / TickFlashDuration;
                    fillColor = Color.Lerp(fillColor, TickFlashColor, flashRatio);
                }

                if (EnablePulseAnimation)
                {
                    float pulse = Mathf.Sin(_pulseTime) * PulseIntensity;
                    fillColor.a *= (1f + pulse);
                }
            }

            switch (shape.ShapeType)
            {
                case Shape2DType.Box:
                    DrawBox(position, shape.BoxExtents, shape.RotationOffset.AsFloat, fillColor, EffectWireframeColor);
                    break;

                case Shape2DType.Circle:
                    DrawCircle(position, shape.CircleRadius.AsFloat, fillColor, EffectWireframeColor);
                    break;

                case Shape2DType.Capsule:
                    DrawCapsule(position, shape.CapsuleSize, shape.RotationOffset.AsFloat, fillColor, EffectWireframeColor);
                    break;
            }
        }

        private Color ApplyExplosionCountdownEffect(Color baseColor, DelayedExplosionFieldData explosionData)
        {
            var frame = VerifiedFrame;
            if (frame == null) return baseColor;

            FP elapsed = _skillField->TickTimer.ElapsedSeconds(frame);
            FP remaining = explosionData.ExplosionDelay - elapsed;

            if (remaining < FP._1)
            {
                float blinkSpeed = 10f;
                float blink = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
                return Color.Lerp(baseColor, Color.red, blink * 0.7f);
            }
            else if (remaining < FP._2)
            {
                float blinkSpeed = 5f;
                float blink = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
                return Color.Lerp(baseColor, Color.yellow, blink * 0.5f);
            }

            return baseColor;
        }

        private Color GetFieldTypeColor()
        {
            if (_skillFieldData is DelayedExplosionFieldData)
                return ExplosionColor;
            if (_skillFieldData is DamageFieldData)
                return DamageColor;
            if (_skillFieldData is HealFieldData)
                return HealColor;
            if (_skillFieldData is SlowFieldData)
                return SlowColor;
            if (_skillFieldData is PushFieldData)
                return PushColor;
            if (_skillFieldData is VortexFieldData)
                return VortexColor;
            
            return EffectAreaColor;
        }

        private void DrawFieldInfo()
        {
#if UNITY_EDITOR
            Vector3 position = _transform->Position.ToUnityVector3();
            Vector3 labelPosition = position + Vector3.up * 0.5f;

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
