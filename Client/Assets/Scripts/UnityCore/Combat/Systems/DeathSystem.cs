using UnityCore.Combat.Components;
using UnityCore.Entities.Core;
using UnityCore.EventDefine;
using UnityCore.EventSystem;

namespace UnityCore.Combat.Systems
{
    public class DeathSystem : SystemBase<HealthComponent>
    {
        protected override void UpdateEntity(Entity entity, HealthComponent health)
        {
            if (health.IsDead && entity.IsActive)
            {
                entity.IsActive = false;
                UniEvent.SendMessage(new EntityDeathEventArgs(entity.ID));
            }
        }
    }
}
