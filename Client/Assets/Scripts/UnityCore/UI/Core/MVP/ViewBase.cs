using UniRx;
using UnityEngine;

namespace UnityCore.UI.MVP
{
    [RequireComponent(typeof(RectTransform))]
    public class ViewBase : MonoBehaviour
    {
        private CompositeDisposable disposer = new CompositeDisposable();
        public CompositeDisposable Disposer => disposer;

        private RectTransform _rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }
        
        private void OnEnable()
        {
            BindEvents();
        }

        private void OnDisable()
        {
            UnBindEvents();
        }

        private void OnDestroy()
        {
            Release();
            disposer.Dispose();
        }

        protected virtual void BindEvents()
        {
        }
        
        protected virtual void UnBindEvents()
        {
        }

        protected virtual void Release()
        {
        }
    }
}