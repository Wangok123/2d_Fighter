using UnityEngine;
using UnityCore.Combat.Components;
using UnityCore.Entities.Core;

namespace UnityCore.Combat.Systems
{
    public class MovementSystem : SystemBase<TransformComponent, VelocityComponent>
    {
        protected override void UpdateEntity(Entity entity, TransformComponent transform, VelocityComponent velocity)
        {
            transform.Position += velocity.Velocity * Time.deltaTime;
            
            if (velocity.Velocity.magnitude > velocity.MaxSpeed)
            {
                velocity.Velocity = velocity.Velocity.normalized * velocity.MaxSpeed;
            }
        }
    }
}
