using Photon.Deterministic;
using System.Collections.Generic;

namespace Quantum
{
    using UnityEngine;

    public unsafe class ProjectileDebugger : QuantumEntityViewComponent
    {
        [Header("可视化设置")]
        [Tooltip("是否启用弹道调试")]
        public bool EnableDebug = true;
        
        [Tooltip("碰撞范围颜色")]
        public Color CollisionShapeColor = new Color(0f, 1f, 1f, 0.3f);
        
        [Tooltip("碰撞范围线框颜色")]
        public Color CollisionWireframeColor = Color.cyan;
        
        [Tooltip("轨迹线颜色")]
        public Color TrajectoryColor = new Color(1f, 1f, 0f, 0.8f);
        
        [Tooltip("轨迹线宽度")]
        public float TrajectoryLineWidth = 2f;
        
        [Tooltip("轨迹点数量")]
        public int TrajectoryPointCount = 20;

        [Header("预测轨迹设置")]
        [Tooltip("是否显示预测轨迹")]
        public bool ShowPredictedTrajectory = true;
        
        [Tooltip("预测时间（秒）")]
        public float PredictionTime = 2f;
        
        [Tooltip("预测轨迹颜色")]
        public Color PredictedTrajectoryColor = new Color(1f, 1f, 0f, 0.2f);

        [Header("信息显示")]
        [Tooltip("是否显示弹道信息")]
        public bool ShowProjectileInfo = true;
        
        [Tooltip("文字颜色")]
        public Color TextColor = Color.yellow;
        
        [Tooltip("文字背景颜色")]
        public Color TextBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
        
        [Tooltip("字体大小")]
        public int FontSize = 12;

        [Header("不同弹道类型颜色")]
        [Tooltip("直线弹道颜色")]
        public Color StraightColor = new Color(0f, 1f, 1f, 0.3f);
        
        [Tooltip("追踪弹道颜色")]
        public Color HomingColor = new Color(1f, 0.5f, 0f, 0.3f);
        
        [Tooltip("抛物线弹道颜色")]
        public Color ArcColor = new Color(0.5f, 1f, 0f, 0.3f);
        
        [Tooltip("回旋镖弹道颜色")]
        public Color BoomerangColor = new Color(1f, 0f, 1f, 0.3f);
        
        [Tooltip("手榴弹弹道颜色")]
        public Color GrenadeColor = new Color(1f, 0.3f, 0f, 0.3f);

        [Header("手榴弹特殊显示")]
        [Tooltip("是否显示爆炸预览")]
        public bool ShowExplosionPreview = true;
        
        [Tooltip("爆炸预览颜色")]
        public Color ExplosionPreviewColor = new Color(1f, 0f, 0f, 0.15f);
        
        [Header("物理调试")]
        [Tooltip("是否显示物理信息")]
        public bool ShowPhysicsInfo = true;
        
        [Tooltip("速度向量颜色")]
        public Color VelocityColor = new Color(0f, 1f, 0f, 0.8f);

        private List<Vector3> _trajectoryPoints = new List<Vector3>(100);
        private ProjectileComponent* _projectile;
        private Transform2D* _transform;
        private ProjectileData _projectileData;
        private string _cachedInfo;

        public override void OnActivate(Frame frame)
        {
            if (!EnableDebug) return;
            QuantumEvent.Subscribe<EventOnProjectileSpawned>(this, OnProjectileSpawned);
        }

        public override void OnDeactivate()
        {
            QuantumEvent.UnsubscribeListener(this);
        }

        private void OnProjectileSpawned(EventOnProjectileSpawned e)
        {
            if (e.Projectile != EntityRef || !EnableDebug) return;
            
            var frame = VerifiedFrame;
            if (frame == null) return;

            _projectile = frame.Unsafe.GetPointer<ProjectileComponent>(EntityRef);
            _projectileData = frame.FindAsset<ProjectileData>(_projectile->ProjectileData.Id);
        }

        private void Update()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<ProjectileComponent>(EntityRef, out _projectile)) return;
            _transform = frame.Unsafe.GetPointer<Transform2D>(EntityRef);

            Vector3 currentPos = _transform->Position.ToUnityVector3();
            
            if (_trajectoryPoints.Count == 0 || Vector3.Distance(_trajectoryPoints[_trajectoryPoints.Count - 1], currentPos) > 0.01f)
            {
                _trajectoryPoints.Add(currentPos);
                
                if (_trajectoryPoints.Count > TrajectoryPointCount)
                {
                    _trajectoryPoints.RemoveAt(0);
                }
            }

            UpdateProjectileInfo(frame);
        }

        private void UpdateProjectileInfo(Frame frame)
        {
            if (!ShowProjectileInfo || _projectileData == null) return;

            FP remainingLifetime = _projectileData.Lifetime - _projectile->LifetimeTimer.ElapsedSeconds(frame);
            FP speed = _projectile->Speed;
            
            string projectileType = GetProjectileTypeName();
            string extraInfo = GetExtraInfo(frame);

            _cachedInfo = $"Projectile\nType: {projectileType}\nSpeed: {speed.AsFloat:F2}\nLifetime: {remainingLifetime.AsFloat:F2}s{extraInfo}";
        }

        private string GetProjectileTypeName()
        {
            if (_projectileData is GrenadeProjectileData)
                return "Grenade";
            if (_projectileData is StraightProjectileData)
                return "Straight";
            if (_projectileData is HomingProjectileData)
                return "Homing";
            if (_projectileData is ArcProjectileData)
                return "Arc";
            if (_projectileData is BoomerangProjectileData)
                return "Boomerang";
            
            return "Unknown";
        }

        private string GetExtraInfo(Frame frame)
        {
            // 修改：优先从PhysicsBody2D读取，其次从KCC读取
            string physicsInfo = "";
            
            if (ShowPhysicsInfo)
            {
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    physicsInfo = $"\n[Body] Vel: ({body->Velocity.X.AsFloat:F2}, {body->Velocity.Y.AsFloat:F2})";
                    physicsInfo += $"\nAngVel: {body->AngularVelocity.AsFloat:F2}";
                }
                else if (frame.Unsafe.TryGetPointer<CharacterController2D>(EntityRef, out var kcc))
                {
                    physicsInfo = $"\n[KCC] Vel: ({kcc->Velocity.X.AsFloat:F2}, {kcc->Velocity.Y.AsFloat:F2})";
                    physicsInfo += $"\nGrounded: {kcc->Grounded}";
                }
            }
    
            if (_projectileData is GrenadeProjectileData grenadeData)
            {
                FP currentHeight = _transform->Position.Y;
                bool hasDetonated = false;
        
                if (frame.Unsafe.TryGetPointer<GrenadeRuntimeComponent>(EntityRef, out var grenadeRuntime))
                {
                    hasDetonated = grenadeRuntime->HasDetonated;
                }
        
                return $"\nHeight: {currentHeight.AsFloat:F2}\nDetonated: {hasDetonated}{physicsInfo}";
            }
    
            if (_projectileData is BoomerangProjectileData boomerangData)
            {
                string phase = "Forward";
                FP distanceTraveled = FP._0;
        
                if (frame.Unsafe.TryGetPointer<BoomerangRuntimeComponent>(EntityRef, out var boomerangRuntime))
                {
                    phase = boomerangRuntime->CurrentPhase == BoomerangPhase.Forward ? "Forward" : "Returning";
                    distanceTraveled = boomerangRuntime->DistanceTraveled;
                }
        
                return $"\nPhase: {phase}\nDistance: {distanceTraveled.AsFloat:F2}{physicsInfo}";
            }
    
            // 修改：Arc弹道从物理组件读取
            if (_projectileData is ArcProjectileData arcData)
            {
                FP currentHeight = _transform->Position.Y;
                FPVector2 velocity = FPVector2.Zero;
        
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    velocity = body->Velocity;
                }
                else if (frame.Unsafe.TryGetPointer<CharacterController2D>(EntityRef, out var kcc))
                {
                    velocity = kcc->Velocity;
                }
        
                return $"\nHeight: {currentHeight.AsFloat:F2}\nVelocity Y: {velocity.Y.AsFloat:F2}{physicsInfo}";
            }
    
            return physicsInfo;
        }

        private void OnDrawGizmos()
        {
            if (!EnableDebug) return;
            if (!Application.isPlaying) return;

            var frame = VerifiedFrame;
            if (frame == null || !frame.Exists(EntityRef)) return;

            if (!frame.Unsafe.TryGetPointer<ProjectileComponent>(EntityRef, out _projectile)) return;
            _transform = frame.Unsafe.GetPointer<Transform2D>(EntityRef);

            DrawTrajectory();
            DrawCollisionShape();
            
            // 修改：显示速度向量（支持PhysicsBody和KCC）
            if (ShowPhysicsInfo)
            {
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    DrawVelocityVector(body->Velocity);
                }
                else if (frame.Unsafe.TryGetPointer<CharacterController2D>(EntityRef, out var kcc))
                {
                    DrawVelocityVector(kcc->Velocity);
                }
            }
            
            if (ShowPredictedTrajectory)
            {
                DrawPredictedTrajectory();
            }

            if (_projectileData is GrenadeProjectileData && ShowExplosionPreview)
            {
                DrawGrenadeExplosionPreview();
            }

            if (ShowProjectileInfo && !string.IsNullOrEmpty(_cachedInfo))
            {
                DrawProjectileInfo();
            }
        }

        // 修改：统一的速度向量绘制方法
        private void DrawVelocityVector(FPVector2 velocity)
        {
            Vector3 position = _transform->Position.ToUnityVector3();
            Vector3 velocityVector = new Vector3(velocity.X.AsFloat, velocity.Y.AsFloat, 0);
            
            if (velocityVector.magnitude < 0.01f) return;
            
#if UNITY_EDITOR
            UnityEditor.Handles.color = VelocityColor;
            UnityEditor.Handles.DrawLine(position, position + velocityVector, 3f);
            UnityEditor.Handles.ArrowHandleCap(0, position + velocityVector, Quaternion.LookRotation(Vector3.forward, velocityVector), 0.2f, EventType.Repaint);
#endif
        }

        private void DrawTrajectory()
        {
            if (_trajectoryPoints.Count < 2) return;

#if UNITY_EDITOR
            UnityEditor.Handles.color = TrajectoryColor;
            for (int i = 0; i < _trajectoryPoints.Count - 1; i++)
            {
                UnityEditor.Handles.DrawLine(_trajectoryPoints[i], _trajectoryPoints[i + 1], TrajectoryLineWidth);
            }
#endif
        }

        private void DrawCollisionShape()
        {
            if (_projectileData == null) return;

            Shape2DConfig shape = _projectileData.CollisionShape;
            if (shape == null) return;

            Vector3 position = _transform->Position.ToUnityVector3();
            Color fillColor = GetProjectileTypeColor();

            switch (shape.ShapeType)
            {
                case Shape2DType.Box:
                    DrawBox(position, shape.BoxExtents, _transform->Rotation.AsFloat, fillColor, CollisionWireframeColor);
                    break;

                case Shape2DType.Circle:
                    DrawCircle(position, shape.CircleRadius.AsFloat, fillColor, CollisionWireframeColor);
                    break;

                case Shape2DType.Capsule:
                    DrawCapsule(position, shape.CapsuleSize, _transform->Rotation.AsFloat, fillColor, CollisionWireframeColor);
                    break;
            }
        }

        private Color GetProjectileTypeColor()
        {
            if (_projectileData is GrenadeProjectileData)
                return GrenadeColor;
            if (_projectileData is StraightProjectileData)
                return StraightColor;
            if (_projectileData is HomingProjectileData)
                return HomingColor;
            if (_projectileData is ArcProjectileData)
                return ArcColor;
            if (_projectileData is BoomerangProjectileData)
                return BoomerangColor;
            
            return CollisionShapeColor;
        }

        private void DrawPredictedTrajectory()
        {
            if (_projectileData == null) return;

            var frame = VerifiedFrame;
            if (frame == null) return;

            Vector3 position = _transform->Position.ToUnityVector3();
            Vector2 direction = new Vector2(_projectile->Direction.X.AsFloat, _projectile->Direction.Y.AsFloat);
            float speed = _projectile->Speed.AsFloat;

            List<Vector3> predictedPoints = new List<Vector3>();
            predictedPoints.Add(position);

            float timeStep = 0.1f;
            int steps = Mathf.CeilToInt(PredictionTime / timeStep);

            if (_projectileData is StraightProjectileData straightData)
            {
                // 修改：优先从PhysicsBody读取速度
                Vector2 velocity = direction * straightData.MoveSpeed.AsFloat;
                
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    velocity = new Vector2(body->Velocity.X.AsFloat, body->Velocity.Y.AsFloat);
                }
                
                DrawStraightPrediction(position, velocity, timeStep, steps, predictedPoints);
            }
            else if (_projectileData is ArcProjectileData arcData)
            {
                Vector2 currentVelocity = direction * speed;
                float gravity = 10f;
                
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    currentVelocity = new Vector2(body->Velocity.X.AsFloat, body->Velocity.Y.AsFloat);
                    gravity = frame.PhysicsSceneSettings->Gravity.Y.AsFloat;
                }
    
                DrawArcPrediction(position, currentVelocity, gravity, arcData.MinimumHeight.AsFloat, arcData.EnableGroundClamp, timeStep, steps, predictedPoints);
            }
            else if (_projectileData is GrenadeProjectileData grenadeData)
            {
                Vector2 currentVelocity = direction * speed;
                float gravity = 10f;
                
                // 修改：优先从PhysicsBody读取
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    currentVelocity = new Vector2(body->Velocity.X.AsFloat, body->Velocity.Y.AsFloat);
                    gravity = frame.PhysicsSceneSettings->Gravity.Y.AsFloat;
                }
                
                DrawGrenadePrediction(position, currentVelocity, gravity, 0f, timeStep, steps, predictedPoints);
            }
            else if (_projectileData is HomingProjectileData homingData)
            {
                Vector2 velocity = direction * homingData.MoveSpeed.AsFloat;
                
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    velocity = new Vector2(body->Velocity.X.AsFloat, body->Velocity.Y.AsFloat);
                }
                DrawStraightPrediction(position, velocity, timeStep, steps, predictedPoints);
            }
            else if (_projectileData is BoomerangProjectileData boomerangData)
            {
                Vector2 velocity = direction * speed;
                
                if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(EntityRef, out var body))
                {
                    velocity = new Vector2(body->Velocity.X.AsFloat, body->Velocity.Y.AsFloat);
                }
                else if (boomerangData.UseKCC && frame.Unsafe.TryGetPointer<CharacterController2D>(EntityRef, out var kcc))
                {
                    velocity = new Vector2(kcc->Velocity.X.AsFloat, kcc->Velocity.Y.AsFloat);
                }
                
                DrawStraightPrediction(position, velocity, timeStep, steps, predictedPoints);
            }

#if UNITY_EDITOR
            if (predictedPoints.Count > 1)
            {
                UnityEditor.Handles.color = PredictedTrajectoryColor;
                for (int i = 0; i < predictedPoints.Count - 1; i++)
                {
                    UnityEditor.Handles.DrawDottedLine(predictedPoints[i], predictedPoints[i + 1], 3f);
                }
            }
#endif
        }

        private void DrawStraightPrediction(Vector3 position, Vector2 velocity, float timeStep, int steps, List<Vector3> points)
        {
            for (int i = 0; i < steps; i++)
            {
                position += new Vector3(velocity.x, velocity.y, 0) * timeStep;
                points.Add(position);
            }
        }

        private void DrawArcPrediction(Vector3 position, Vector2 velocity, float gravity, float minHeight, bool clampGround, float timeStep, int steps, List<Vector3> points)
        {
            for (int i = 0; i < steps; i++)
            {
                velocity.y -= gravity * timeStep;
                position += new Vector3(velocity.x, velocity.y, 0) * timeStep;
                
                if (clampGround && position.y < minHeight)
                {
                    position.y = minHeight;
                    if (velocity.y < 0)
                    {
                        velocity.y = 0;
                    }
                }
                
                points.Add(position);
            }
        }

        private void DrawGrenadePrediction(Vector3 position, Vector2 velocity, float gravity, float groundHeight, float timeStep, int steps, List<Vector3> points)
        {
            for (int i = 0; i < steps; i++)
            {
                velocity.y -= gravity * timeStep;
                position += new Vector3(velocity.x, velocity.y, 0) * timeStep;
                points.Add(position);

                if (position.y <= groundHeight && velocity.y <= 0)
                {
                    break;
                }
            }
        }

        private void DrawGrenadeExplosionPreview()
        {
            if (!(_projectileData is GrenadeProjectileData grenadeData)) return;
            if (!grenadeData.ExplosionFieldData.Id.IsValid) return;

            var frame = VerifiedFrame;
            if (frame == null) return;

            var explosionData = frame.FindAsset<SkillFieldData>(grenadeData.ExplosionFieldData.Id);
            if (explosionData == null || explosionData.EffectArea == null) return;

            Vector3 currentPos = _transform->Position.ToUnityVector3();
            Vector3 explosionPos = currentPos;

            Shape2DConfig explosionShape = explosionData.EffectArea;

            switch (explosionShape.ShapeType)
            {
                case Shape2DType.Circle:
                    DrawCircle(explosionPos, explosionShape.CircleRadius.AsFloat, ExplosionPreviewColor, new Color(1f, 0f, 0f, 0.3f));
                    break;

                case Shape2DType.Box:
                    DrawBox(explosionPos, explosionShape.BoxExtents, explosionShape.RotationOffset.AsFloat, ExplosionPreviewColor, new Color(1f, 0f, 0f, 0.3f));
                    break;
            }

#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.5f);
            UnityEditor.Handles.DrawDottedLine(currentPos, explosionPos, 2f);
#endif
        }

        private void DrawProjectileInfo()
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
