using System;
using UnityCore.Base;
using UnityCore.GameModule.Battle.Manager.MainGame;
using UnityCore.Input.Module;
using UnityCore.UI.Common;
using UnityEngine;

namespace UnityCore.GameModule.Battle.Manager.System
{
    public class UIInputSystem : MonoBehaviour
    {
        [SerializeField] private LatJoyStick joystick;
        
        private InputManager _inputManager;
        private UIInputModule _playerInputModule;

        private void Awake()
        {
            _inputManager = new InputManager();
            RegisterModules();
        }

        private void OnDestroy()
        {
            UnregisterModules();
        }

        private void RegisterModules()
        {            
            joystick.OnDirectionChanged += _inputManager.OnMove;
        }
        
        private void UnregisterModules()
        {
            joystick.OnDirectionChanged -= _inputManager.OnMove;
        }
    }
}