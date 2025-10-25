using LATMath;

namespace LATPhysics
{
    /// <summary>
    /// 碰撞体抽象基类
    /// </summary>
    public abstract class LATColliderBase
    {
        public string Name;
        public LATVector3 Position;
        
        public virtual bool DectectContact(LATColliderBase collider, ref LATVector3 normal, ref LATVector3 borderAdjust)
        {
            switch (collider)
            {
                case LATBoxCollider boxCollider:
                    return DetectBoxCollision(boxCollider, ref normal, ref borderAdjust);
                case LATCircleCollider circleCollider:
                    return DetectCircleCollision(circleCollider, ref normal, ref borderAdjust);
                default:
                    return false;
            }
        }
        public abstract bool DetectBoxCollision(LATBoxCollider boxCollider, ref LATVector3 normal, ref LATVector3 borderAdjust);
        public abstract bool DetectCircleCollision(LATCircleCollider circleCollider, ref LATVector3 normal, ref LATVector3 borderAdjust);
    }
}