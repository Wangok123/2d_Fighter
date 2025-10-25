using UnityEngine;

namespace UnityCore.EventSystem
{
    public class UniEventDriver : MonoBehaviour
    {
        void Update()
        {
            UniEvent.Update();
        }
    }
}