using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCore.Extensions.Listener
{
    public class ClickListener : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IDragHandler
    {
        public Action<PointerEventData, object[]> OnClick;
        public Action<PointerEventData, object[]> OnClickUp;
        public Action<PointerEventData, object[]> OnClickDown;
        public Action<PointerEventData, object[]> OnDragEvent;
        
        public object[] Args;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(eventData, Args);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnClickUp?.Invoke(eventData, Args);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnClickDown?.Invoke(eventData, Args);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData, Args);
        }
    }
}