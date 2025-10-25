using Core.ReferencePool;
using UnityCore.EventSystem;

namespace UnityCore.EventDefine
{
    public class EventDefineBase : IEventMessage, IReference
    {
        public virtual void Clear()
        {
        }
    }
}