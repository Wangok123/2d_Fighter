using LATMath;

namespace LATPhysics
{
    public class LATBoxCollider : LATColliderBase
    {
        public LATVector3[] Axis { get; set; }  // 轴向
        public LATVector3 Size { get; set; }  // 尺寸
        
        public LATBoxCollider(ColliderConfig config)
        {
            Name = config.Name;
            Position = config.Position;
            Axis = config.Axis;
            Size = config.Size;
        }

        public override bool DetectBoxCollision(LATBoxCollider boxCollider, ref LATVector3 normal, ref LATVector3 borderAdjust)
        {
            throw new System.NotImplementedException();
        }

        public override bool DetectCircleCollision(LATCircleCollider circleCollider, ref LATVector3 normal, ref LATVector3 borderAdjust)
        {
            LATVector3 tmpNormal = LATVector3.Zero;
            LATVector3 tmpAdjust = LATVector3.Zero;
            bool result = circleCollider.DetectBoxCollision(this, ref tmpNormal, ref tmpAdjust);
            normal = -tmpNormal;
            borderAdjust = -tmpAdjust;
            return result;
        }
    }
}