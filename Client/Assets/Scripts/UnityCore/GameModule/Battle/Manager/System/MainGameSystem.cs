using System;
using LATLog;
using UnityCore.Base;
using UnityCore.GameModule.Battle.GamePhysics;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class MainGameSystem : MonoBehaviour
    {
        private MainGameManager _mainGameManager;
        
        private void Awake()
        {
            _mainGameManager = GameModuleManager.GetModule<MainGameManager>();
            var mapRootGo = GameObject.Find("MapRoot");
            if (mapRootGo == null)
            {
                GameDebug.LogError("MapRoot GameObject not found in the scene.");
                return;
            }
            
            var unitRootGo = GameObject.Find("UnitRoot");
            if (unitRootGo == null)
            {
                GameDebug.LogError("UnitRoot GameObject not found in the scene.");
                return;
            }

            _mainGameManager.MapRoot = mapRootGo.GetComponent<MapRoot>();
            _mainGameManager.UnitRoot = unitRootGo;

            _mainGameManager.GameCameraSystem = GetComponent<GameCameraSystem>();
        }

        private void OnEnable()
        {
            _mainGameManager.RegisterEvents();
        }

        private void OnDisable()
        {
            _mainGameManager.UnRegisterEvents();
        }
    }
}