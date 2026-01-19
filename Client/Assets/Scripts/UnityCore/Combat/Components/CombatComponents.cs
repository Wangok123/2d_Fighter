using UnityEngine;
using ECSComponent = UnityCore.Entities.Core.Component;

namespace UnityCore.Combat.Components
{
    public class HealthComponent : ECSComponent
    {
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public override void Awake()
        {
            base.Awake();
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        }

        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }
    }

    public class TransformComponent : ECSComponent
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; } = Vector3.one;
    }

    public class VelocityComponent : ECSComponent
    {
        public Vector3 Velocity { get; set; }
        public float MaxSpeed { get; set; } = 10f;
    }

    public class AttackComponent : ECSComponent
    {
        public float Damage { get; set; }
        public float AttackRange { get; set; }
        public float AttackCooldown { get; set; }
        public float LastAttackTime { get; set; }
        public bool CanAttack => Time.time - LastAttackTime >= AttackCooldown;
    }

    public class InputComponent : ECSComponent
    {
        public Vector2 MoveInput { get; set; }
        public bool AttackPressed { get; set; }
        public bool SkillPressed { get; set; }
    }

    public class TeamComponent : ECSComponent
    {
        public int TeamId { get; set; }
    }
}
