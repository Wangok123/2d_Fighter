using UnityEngine;
using UnityCore.Combat.Components;
using UnityCore.Entities.Core;

namespace UnityCore.Combat.Systems
{
    public class InputProcessingSystem : SystemBase<InputComponent, VelocityComponent>
    {
        protected override void UpdateEntity(Entity entity, InputComponent input, VelocityComponent velocity)
        {
            Vector3 moveDirection = new Vector3(input.MoveInput.x, 0, input.MoveInput.y);
            velocity.Velocity = moveDirection * velocity.MaxSpeed;
        }
    }
}
