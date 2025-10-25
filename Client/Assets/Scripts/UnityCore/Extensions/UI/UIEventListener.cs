using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCore.Extensions.UI
{
    public class UIEventListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public event Action<PointerEventData> onPointerDown;
        public event Action<PointerEventData> onPointerUp;
        public event Action<PointerEventData> onDrag;

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(eventData);
        }
    }
}