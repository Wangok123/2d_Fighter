using UnityCore.Base;
using UnityEngine;

namespace UnityCore.GameModule.Coroutine
{
    [DisallowMultipleComponent]
    public class CoroutineComponent : LatComponent
    {
        private CoroutineManager _manager;

        protected override void Awake()
        {
            base.Awake();
            _manager = GameModuleManager.GetModule<CoroutineManager>();
            _manager.Initialize();
        }

        public CoroutineManager GetManager()
        {
            return _manager;
        }
    }
}
