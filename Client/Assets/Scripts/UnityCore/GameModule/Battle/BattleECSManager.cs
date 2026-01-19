using Core;
using UnityEngine;
using UnityCore.Combat.Components;
using UnityCore.Combat.Systems;
using UnityCore.Entities.Core;
using UnityCore.EventDefine;
using UnityCore.EventSystem;

namespace UnityCore.GameModule.Battle
{
    public class BattleECSManager : CoreModule
    {
        public override int Priority => 60;

        private bool _isInitialized;

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!_isInitialized)
            {
                InitializeSystems();
                _isInitialized = true;
            }
            
            Base.Game.World.Update();
        }

        public override void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            Base.Game.World.FixedUpdate();
        }

        private void InitializeSystems()
        {
            Base.Game.World.AddSystem(new InputProcessingSystem(), priority: 100);
            Base.Game.World.AddSystem(new MovementSystem(), priority: 80);
            Base.Game.World.AddSystem(new CombatSystem(), priority: 60);
            Base.Game.World.AddSystem(new DeathSystem(), priority: 40);
        }

        internal override void Shutdown()
        {
            Base.Game.World.Clear();
            _isInitialized = false;
        }

        public Entity CreatePlayer(Vector3 position, int teamId = 0)
        {
            var player = Base.Game.World.CreateEntity();
            
            var transform = player.AddComponent<TransformComponent>();
            transform.Position = position;
            
            var velocity = player.AddComponent<VelocityComponent>();
            velocity.MaxSpeed = 5f;
            
            var health = player.AddComponent<HealthComponent>();
            health.MaxHealth = 100f;
            
            var attack = player.AddComponent<AttackComponent>();
            attack.Damage = 10f;
            attack.AttackRange = 2f;
            attack.AttackCooldown = 1f;
            
            var team = player.AddComponent<TeamComponent>();
            team.TeamId = teamId;
            
            player.AddComponent<InputComponent>();
            
            UniEvent.SendMessage(new EntityCreatedEventArgs(player.ID));
            
            return player;
        }

        public Entity CreateEnemy(Vector3 position, int teamId = 1)
        {
            var enemy = Base.Game.World.CreateEntity();
            
            var transform = enemy.AddComponent<TransformComponent>();
            transform.Position = position;
            
            var health = enemy.AddComponent<HealthComponent>();
            health.MaxHealth = 50f;
            
            var attack = enemy.AddComponent<AttackComponent>();
            attack.Damage = 5f;
            attack.AttackRange = 1.5f;
            attack.AttackCooldown = 1.5f;
            
            var team = enemy.AddComponent<TeamComponent>();
            team.TeamId = teamId;
            
            UniEvent.SendMessage(new EntityCreatedEventArgs(enemy.ID));
            
            return enemy;
        }

        public void DestroyEntity(Entity entity)
        {
            if (entity != null)
            {
                Base.Game.World.DestroyEntity(entity);
            }
        }
    }
}
