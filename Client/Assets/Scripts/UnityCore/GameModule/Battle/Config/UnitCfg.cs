using LATMath;
using LATPhysics;

namespace UnityCore.GameModule.Battle.Config
{
    public class UnitCfg
    {
        public int ID { get; set; }
        public string UnitName { get; set; }
        public string ResourceName { get; set; }
        public int Hp { get; set; }
        public int Def { get; set; }
        public int MoveSpeed { get; set; }
        
        // 碰撞
        public ColliderConfig ColliderConfig { get; set; }
    }
}