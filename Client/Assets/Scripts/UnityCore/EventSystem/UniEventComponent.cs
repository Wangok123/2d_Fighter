using UnityCore.Base;

namespace UnityCore.EventSystem
{
    public class UniEventComponent : LatComponent
    {
        protected override void Awake()
        {
            base.Awake();

            UniEvent.Initialize();

            IsInit = true;
        }
    }
}