using System;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityCore.Input;
using UnityCore.Input.Module;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class GameInputSystem : MonoBehaviour
    {
        private InputManager _inputManager;
        private PlayerInputModule _playerInputModule;

        private void Awake()
        {
            _inputManager = new InputManager();
            _playerInputModule = Game.Input.GetInputModule<PlayerInputModule>();

            RegisterModules();
        }

        private void Start()
        {
            Game.Input.EnableModule(InputComponent.PlayerInput);
            Game.Input.EnableModule(InputComponent.UIInput);
        }

        private void RegisterModules()
        {            
            _playerInputModule.onMove += _inputManager.OnMove;
        }
        
        private void UnregisterModules()
        {
            _playerInputModule.onMove -= _inputManager.OnMove;
        }
    }
}