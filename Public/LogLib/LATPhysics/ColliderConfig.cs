using LATMath;

namespace LATPhysics
{
    public class ColliderConfig
    {
        public string Name { get; set; }
        public ColliderType ColliderType { get; set; }
        public LATVector3 Position { get; set; }
        
        // Box
        public LATVector3[] Axis { get; set; }  // 轴向
        public LATVector3 Size { get; set; }  // 尺寸
        
        // Sphere
        public LATInt Radius { get; set; }  // 半径
    }
    
    public enum ColliderType
    {
        Box = 1,
        Circle = 2
    }
}