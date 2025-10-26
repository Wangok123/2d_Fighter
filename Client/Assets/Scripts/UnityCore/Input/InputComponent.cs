using System.Collections.Generic;
using LATLog;
using UnityCore.Base;
using UnityCore.Input.Module;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityCore.Input
{
    public class InputComponent : LatComponent
    {
        // todo: 后面要改成类的形式，而非Asset引用
        [SerializeField] private InputActionAsset _inputActions;
        private Dictionary<string, IInputModule> _modules = new Dictionary<string, IInputModule>();
        
        public const string PlayerInput = "Player";
        public const string UIInput = "UI";
        
        protected override void Awake()
        {
            base.Awake();
            
            RegisterModule(PlayerInput, new PlayerInputModule());
            RegisterModule(UIInput, new UIInputModule());
            
            IsInit = true;
        }

        private void RegisterModule(string key, IInputModule module)
        {
            if (!_modules.ContainsKey(key))
            {
                module.Initialize();
                _modules.Add(key, module);
            }
        }

        public void EnableAllModules()
        {
            foreach (var module in _modules)
                module.Value.Enable();
        }
        
        public void EnableModule(string key)
        {
            if (_modules.TryGetValue(key, out var module))
            {
                module.Enable();
            }
            else
            {
                GameDebug.LogError($"Input module '{key}' not found.");
            }
        }

        public void DisableAllModules()
        {
            foreach (var module in _modules)
                module.Value.Disable();
        }
        
        public void DisableModule(string key)
        {
            if (_modules.TryGetValue(key, out var module))
            {
                module.Disable();
            }
            else
            {
                GameDebug.LogError($"Input module '{key}' not found.");
            }
        }

        public InputActionMap GetActionMap(string mapName)
        {
            return _inputActions.FindActionMap(mapName);
        }

        public InputAction GetAction(string mapName, string actionName)
        {
            var actionMap = _inputActions.FindActionMap(mapName);
            if (actionMap == null)
            {
                GameDebug.LogError($"Action map '{mapName}' not found.");
                return null;
            }
            
            return actionMap.FindAction(actionName);
        }

        public T GetInputModule<T>() where T : IInputModule
        {
            foreach (var module in _modules.Values)
            {
                if (module is T typedModule)
                {
                    return typedModule;
                }
            }
            
            GameDebug.LogError($"Input module of type '{typeof(T)}' not found.");
            return default;
        }
    }
}