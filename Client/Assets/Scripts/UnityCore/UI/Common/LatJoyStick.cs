using System;
using UnityCore.Extensions.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCore.UI.Common
{
    public class LatJoyStick : MonoBehaviour
    {
        private UIEventListener eventListener;
        [SerializeField] private LatImage dirImg;
        [SerializeField] private LatImage dirPointImg;
        [SerializeField] private Transform arrowRoot;
        
        public event Action<Vector2> OnDirectionChanged; 

        private Vector2 _startPos = Vector2.zero;
        private Vector2 _defaultPos = Vector2.zero;

        // 限制拖动范围
        private float _maxDistance = 100f; // 最大拖动距离
        
        private void Awake()
        {
            eventListener = GetComponent<UIEventListener>();
        }

        private void OnEnable()
        {
            eventListener.onPointerDown += OnPointerDown;
            eventListener.onPointerUp += OnPointerUp;
            eventListener.onDrag += OnDrag;
        }
        
        private void OnDisable()
        {
            eventListener.onPointerDown -= OnPointerDown;
            eventListener.onPointerUp -= OnPointerUp;
            eventListener.onDrag -= OnDrag;
        }

        private void Start()
        {
            _maxDistance = dirImg.Image.rectTransform.sizeDelta.x / 2f; // 根据方向图片大小计算最大拖动距离
            _defaultPos = dirImg.transform.position;
            arrowRoot.gameObject.SetActive(false); // 初始隐藏箭头
        }

        private void OnPointerDown(PointerEventData eventData)
        {
            _startPos = eventData.position;
            dirPointImg.Image.color = Color.white; // 显示方向指示点
            dirImg.transform.position = eventData.position;
        }
        
        private void OnPointerUp(PointerEventData eventData)
        {
            dirImg.transform.position = _defaultPos; // 重置方向图片位置
            dirPointImg.Image.color = new Color(1, 1, 1, 0.5f); // 隐藏方向指示点
            dirPointImg.transform.localPosition = Vector3.zero; // 重置方向指示点位置
            arrowRoot.gameObject.SetActive(false); // 隐藏箭头
            
            OnDirectionChanged?.Invoke(Vector2.zero); // 通知方向已重置
        }

        private void OnDrag(PointerEventData eventData)
        {
            Vector2 dir = eventData.position - _startPos;
            Vector2 currentPos = eventData.position;
            Vector2 delta = currentPos - _startPos;


            if (delta.magnitude > _maxDistance)
            {
                delta = delta.normalized * _maxDistance;
            }
            
            // 更新方向指示点位置
            dirPointImg.transform.position = _startPos + delta;

            // 更新箭头方向
            if (dir != Vector2.zero)
            {
                arrowRoot.gameObject.SetActive(true);
                float angle = Vector2.SignedAngle(new Vector2(1, 0), dir);
                arrowRoot.localEulerAngles = new Vector3(0, 0, angle);
            }
            
            OnDirectionChanged?.Invoke(dir.normalized); // 通知方向变化
        }
    }
}
