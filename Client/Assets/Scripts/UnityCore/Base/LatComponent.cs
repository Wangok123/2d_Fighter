using UnityEngine;

namespace UnityCore.Base
{
    [DisallowMultipleComponent]
    public abstract class LatComponent : MonoBehaviour
    {
        public bool IsInit { get; set; } = false;
        
        protected virtual void Awake()
        {
            GameEntry.RegisterComponent(this);
        }
    }
}