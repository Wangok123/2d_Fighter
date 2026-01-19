using Core;
using UnityCore.Base;
using UnityEngine;
using UnityCore.Combat.Components;
using UnityCore.Entities.Core;
using UnityCore.EventDefine;
using UnityCore.EventSystem;
using UnityCore.GameModule.Battle;

namespace UnityCore.Combat
{
    public class BattleECSExample : MonoBehaviour
    {
        private BattleECSManager _battleManager;
        private Entity _player;
        private Entity _enemy;

        private void Start()
        {
            _battleManager = GameModuleManager.GetModule<BattleECSManager>();
            
            UniEvent.AddListener<EntityDeathEventArgs>(OnEntityDeath);
            UniEvent.AddListener<EntityCreatedEventArgs>(OnEntityCreated);
            
            CreateTestEntities();
        }

        private void CreateTestEntities()
        {
            _player = _battleManager.CreatePlayer(new Vector3(0, 0, 0), teamId: 0);
            _enemy = _battleManager.CreateEnemy(new Vector3(5, 0, 0), teamId: 1);
            
            Debug.Log("Battle entities created!");
        }

        private void Update()
        {
            if (_player != null && _player.IsActive)
            {
                var input = _player.GetComponent<InputComponent>();
                if (input != null)
                {
                    input.MoveInput = new Vector2(
                        UnityEngine.Input.GetAxis("Horizontal"),
                        UnityEngine.Input.GetAxis("Vertical")
                    );
                    input.AttackPressed = UnityEngine.Input.GetKey(KeyCode.Space);
                }
            }
        }

        private void OnEntityCreated(IEventMessage message)
        {
            var e = message as EntityCreatedEventArgs;
            Debug.Log($"Entity created: {e.EntityId}");
        }

        private void OnEntityDeath(IEventMessage message)
        {
            var e = message as EntityDeathEventArgs;
            Debug.Log($"Entity died: {e.EntityId}");
            
            if (_player != null && _player.ID == e.EntityId)
            {
                Debug.Log("Player died!");
            }
            else if (_enemy != null && _enemy.ID == e.EntityId)
            {
                Debug.Log("Enemy died!");
            }
        }

        private void OnDestroy()
        {
            UniEvent.RemoveListener<EntityDeathEventArgs>(OnEntityDeath);
            UniEvent.RemoveListener<EntityCreatedEventArgs>(OnEntityCreated);
        }
    }
}
