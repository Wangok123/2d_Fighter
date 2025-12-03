using UnityCore.Base;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Examples
{
    public class BattleCameraExample : MonoBehaviour
    {
        [Header("示例配置")]
        [SerializeField] private Transform player1;
        [SerializeField] private Transform player2;
        [SerializeField] private Transform player3;

        private void Start()
        {
            RegisterPlayers();
        }

        private void RegisterPlayers()
        {
            if (player1 != null)
            {
                Game.BattleCamera.RegisterPlayer(player1, weight: 1f, radius: 2f);
            }

            if (player2 != null)
            {
                Game.BattleCamera.RegisterPlayer(player2, weight: 1f, radius: 2f);
            }

            if (player3 != null)
            {
                Game.BattleCamera.RegisterPlayer(player3, weight: 1f, radius: 2f);
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestRegisterPlayer();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestUnregisterPlayer();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestClearAllPlayers();
            }
        }

        private void TestRegisterPlayer()
        {
            if (player1 != null)
            {
                Game.BattleCamera.RegisterPlayer(player1, weight: 1f, radius: 2f);
                Debug.Log("[Example] 注册玩家1");
            }
        }

        private void TestUnregisterPlayer()
        {
            if (player1 != null)
            {
                Game.BattleCamera.UnregisterPlayer(player1);
                Debug.Log("[Example] 移除玩家1");
            }
        }

        private void TestClearAllPlayers()
        {
            Game.BattleCamera.ClearAllPlayers();
            Debug.Log("[Example] 清除所有玩家");
        }

        private void OnDestroy()
        {
            if (player1 != null)
            {
                Game.BattleCamera.UnregisterPlayer(player1);
            }

            if (player2 != null)
            {
                Game.BattleCamera.UnregisterPlayer(player2);
            }

            if (player3 != null)
            {
                Game.BattleCamera.UnregisterPlayer(player3);
            }
        }
    }
}
