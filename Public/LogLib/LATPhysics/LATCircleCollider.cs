using System.Collections.Generic;
using LATLog;
using LATMath;

namespace LATPhysics
{
    public class LATCircleCollider : LATColliderBase
    {
        public LATInt Radius { get; set; } // 半径

        public LATCircleCollider(ColliderConfig config)
        {
            Name = config.Name;
            Position = config.Position;
            Radius = config.Radius;
        }

        public void CalcColliders(List<LATColliderBase> colliders, ref LATVector3 velocity, ref LATVector3 borderAdjust)
        {
            if (velocity == LATVector3.Zero)
            {
                return;
            }
            
            List<CollisionInfo> collisionInfos = new List<CollisionInfo>();

            LATVector3 normal = LATVector3.Zero;
            LATVector3 adjust = LATVector3.Zero;
            foreach (var collider in colliders)
            {
                if (DectectContact(collider, ref normal, ref adjust))
                {
                    collisionInfos.Add(new CollisionInfo()
                    {
                        Collider = collider,
                        Normal = normal,
                        BorderAdjust = adjust
                    });
                }
            }
            
            if (collisionInfos.Count == 1)
            {
                CollisionInfo collisionInfo = collisionInfos[0];
                velocity = CorrectVelocity(velocity, collisionInfo.Normal);
                borderAdjust = collisionInfo.BorderAdjust;
                GameDebug.Log($"碰撞检测成功{velocity}");
            }else if (collisionInfos.Count > 1)
            {
                LATVector3 centerNormal = LATVector3.Zero;
                CollisionInfo info = new CollisionInfo();
                
                LATArgs maxAngle = CalcMaxNormalAngle(collisionInfos, velocity, ref centerNormal, ref info);
                LATArgs angle = LATVector3.Angle(-velocity, centerNormal);
                if (angle > maxAngle)
                {
                    velocity = CorrectVelocity(velocity, centerNormal);
                    GameDebug.Log($"多个碰撞体，碰撞检测成功{velocity}");
                    
                    LATVector3 adj = LATVector3.Zero;
                    foreach (var collisionInfo in collisionInfos)
                    {
                        adj += collisionInfo.BorderAdjust;
                    }
                
                    borderAdjust = adj;
                }
                else
                {
                    velocity = LATVector3.Zero;
                }
            }else
            {
                GameDebug.Log("未检测到碰撞");
            }
        }
        
        private LATArgs CalcMaxNormalAngle(List<CollisionInfo> collisionInfos, LATVector3 velocity, ref LATVector3 centerNormal, ref CollisionInfo info)
        {
            foreach (var collisionInfo in collisionInfos)
            {
                centerNormal += collisionInfo.Normal;
            }
            
            centerNormal /= collisionInfos.Count;

            LATArgs normal = LATArgs.Zero;
            LATArgs velocityAngle = LATArgs.Zero;
            foreach (var collisionInfo in collisionInfos)
            {
                LATArgs tmp = LATVector3.Angle(centerNormal, collisionInfo.Normal);
                if (tmp > normal)
                {
                    normal = tmp;
                }
                
                LATArgs tmpVelocityAngle = LATVector3.Angle(velocity, collisionInfo.Normal);
                if (tmpVelocityAngle > normal)
                {
                    velocityAngle = tmpVelocityAngle;
                    info = collisionInfo;
                }
            }
            
            return normal;
        }
        
        private LATVector3 CorrectVelocity(LATVector3 velocity, LATVector3 normal)
        {
            if (normal == LATVector3.Zero)
            {
                return velocity;
            }
            
            if (LATVector3.Angle(normal, velocity) > LATArgs.HalfPi)
            {
                LATInt dot = LATVector3.Dot(velocity, normal);
                if (dot !=  0)
                {
                    velocity = velocity - dot * normal;
                }
            }
            
            return velocity;
        }

        public override bool DetectBoxCollision(LATBoxCollider boxCollider, ref LATVector3 normal, ref LATVector3 borderAdjust)
        {
            LATVector3 disOffset = Position - boxCollider.Position;
            // 轴向投影
            LATInt dot_disX = LATVector3.Dot(disOffset, boxCollider.Axis[0]);
            LATInt dot_disY = LATVector3.Dot(disOffset, boxCollider.Axis[1]);
            // 限制投影不出检测范围
            LATInt clampX = LATCalculate.Clamp(dot_disX, -boxCollider.Size.x, boxCollider.Size.x);
            LATInt clampY = LATCalculate.Clamp(dot_disY, -boxCollider.Size.y, boxCollider.Size.y);
            // 投影向量
            LATVector3 s_x = clampX * boxCollider.Axis[0];
            LATVector3 s_y = clampY * boxCollider.Axis[1];

            // 碰撞点
            LATVector3 point = boxCollider.Position + s_x + s_y;
            LATVector3 po = Position - point;

            if (LATVector3.SqrMagnitude(po) > Radius * Radius)
            {
                return false;
            }
            else
            {
                normal = po.Normalized;
                LATInt len = po.Magnitude;
                borderAdjust = normal * (Radius - len);
                
                return true;
            }
        }

        public override bool DetectCircleCollision(LATCircleCollider circleCollider, ref LATVector3 normal, ref LATVector3 borderAdjust)
        {
            LATVector3 disOffset = Position - circleCollider.Position;
            if (LATVector3.SqrMagnitude(disOffset) > (Radius + circleCollider.Radius) * (Radius + circleCollider.Radius))
            {
                return false;
            }
            else
            {
                normal = disOffset.Normalized;
                LATInt len = disOffset.Magnitude;
                borderAdjust = normal * (Radius + circleCollider.Radius - len);

                return true;
            }
        }
    }

    public struct CollisionInfo
    {
        public LATColliderBase Collider;
        public LATVector3 Normal;
        public LATVector3 BorderAdjust;
    }
}