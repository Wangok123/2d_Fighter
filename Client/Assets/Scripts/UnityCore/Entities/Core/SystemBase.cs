using UnityCore.Base;

namespace UnityCore.Entities.Core
{
    public abstract class SystemBase : ISystem
    {
        protected EntityComponent World => Game.World;
        
        public abstract void Update();
    }

    public abstract class SystemBase<TComponent> : SystemBase where TComponent : Component, new()
    {
        public override void Update()
        {
            var entities = World.GetEntitiesWith<TComponent>();
            foreach (var entity in entities)
            {
                if (!entity.IsActive) continue;
                
                var component = entity.GetComponent<TComponent>();
                UpdateEntity(entity, component);
            }
        }

        protected abstract void UpdateEntity(Entity entity, TComponent component);
    }

    public abstract class SystemBase<TComponent1, TComponent2> : SystemBase 
        where TComponent1 : Component, new()
        where TComponent2 : Component, new()
    {
        public override void Update()
        {
            var entities = World.GetAllEntities();
            foreach (var entity in entities)
            {
                if (!entity.IsActive) continue;
                
                var comp1 = entity.GetComponent<TComponent1>();
                var comp2 = entity.GetComponent<TComponent2>();
                
                if (comp1 != null && comp2 != null)
                {
                    UpdateEntity(entity, comp1, comp2);
                }
            }
        }

        protected abstract void UpdateEntity(Entity entity, TComponent1 comp1, TComponent2 comp2);
    }
}