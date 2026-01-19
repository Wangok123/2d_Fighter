using System;

namespace UnityCore.EventDefine
{
    public class EntityDeathEventArgs : EventDefineBase
    {
        public Guid EntityId { get; private set; }

        public EntityDeathEventArgs(Guid entityId)
        {
            EntityId = entityId;
        }

        public override void Clear()
        {
            EntityId = Guid.Empty;
        }
    }

    public class EntityCreatedEventArgs : EventDefineBase
    {
        public Guid EntityId { get; private set; }

        public EntityCreatedEventArgs(Guid entityId)
        {
            EntityId = entityId;
        }

        public override void Clear()
        {
            EntityId = Guid.Empty;
        }
    }

    public class EntityDamageEventArgs : EventDefineBase
    {
        public Guid AttackerId { get; private set; }
        public Guid TargetId { get; private set; }
        public float Damage { get; private set; }

        public EntityDamageEventArgs(Guid attackerId, Guid targetId, float damage)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            Damage = damage;
        }

        public override void Clear()
        {
            AttackerId = Guid.Empty;
            TargetId = Guid.Empty;
            Damage = 0f;
        }
    }
}
