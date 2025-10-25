using System.Collections.Generic;

namespace LATPhysics
{
    public class EnvColliders
    {
        public List<ColliderConfig> EnvColliderConfigs { get; set; }
        
        List<LATColliderBase> EnvCollidersList { get; set; }
        
        public void Init()
        {
            EnvCollidersList = new List<LATColliderBase>();
            foreach (var config in EnvColliderConfigs)
            {
                switch (config.ColliderType)
                {
                    case ColliderType.Box:
                        EnvCollidersList.Add(new LATBoxCollider(config));
                        break;
                    case ColliderType.Circle:
                        EnvCollidersList.Add(new LATCircleCollider(config));
                        break;
                }
            }
        }
        
        public List<LATColliderBase> GetEnvColliders()
        {
            return EnvCollidersList;
        }
    }
}