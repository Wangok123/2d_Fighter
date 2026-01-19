using UnityEngine;
using UnityCore.Combat.Components;
using UnityCore.Entities.Core;

namespace UnityCore.Combat.Systems
{
    public class CombatSystem : SystemBase
    {
        public override void Update()
        {
            var attackers = World.GetAllEntities();
            
            foreach (var attacker in attackers)
            {
                if (!attacker.IsActive) continue;
                
                var attackComp = attacker.GetComponent<AttackComponent>();
                var transformComp = attacker.GetComponent<TransformComponent>();
                var inputComp = attacker.GetComponent<InputComponent>();
                var attackerTeam = attacker.GetComponent<TeamComponent>();
                
                if (attackComp == null || transformComp == null) continue;
                
                if (inputComp != null && inputComp.AttackPressed && attackComp.CanAttack)
                {
                    ExecuteAttack(attacker, attackComp, transformComp, attackerTeam);
                }
            }
        }

        private void ExecuteAttack(Entity attacker, AttackComponent attackComp, TransformComponent attackerTransform, TeamComponent attackerTeam)
        {
            attackComp.LastAttackTime = Time.time;
            
            var targets = World.GetEntitiesWith<HealthComponent>();
            foreach (var target in targets)
            {
                if (target == attacker || !target.IsActive) continue;
                
                var targetTeam = target.GetComponent<TeamComponent>();
                if (attackerTeam != null && targetTeam != null && attackerTeam.TeamId == targetTeam.TeamId)
                {
                    continue;
                }
                
                var targetTransform = target.GetComponent<TransformComponent>();
                var targetHealth = target.GetComponent<HealthComponent>();
                
                if (targetTransform == null || targetHealth == null) continue;
                
                float distance = Vector3.Distance(attackerTransform.Position, targetTransform.Position);
                if (distance <= attackComp.AttackRange)
                {
                    targetHealth.TakeDamage(attackComp.Damage);
                    
                    if (targetHealth.IsDead)
                    {
                        target.IsActive = false;
                    }
                }
            }
        }
    }
}
