namespace UnityCore.Entities.Core
{
    public class Component
    {
        public Entity Entity { get; set; }
        public virtual void Awake() { }
        public virtual void Destroy() { }
    }
}