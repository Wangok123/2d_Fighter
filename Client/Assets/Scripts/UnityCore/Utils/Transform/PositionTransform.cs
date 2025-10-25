using UnityEngine;

namespace Utils.Transform
{
    public static class PositionTransform
    {
        /// <summary>
        /// 将一个 RectTransform 的世界坐标转换为另一个 RectTransform 的父级下局部坐标
        /// </summary>
        /// <param name="sourceTransform"></param>
        /// <param name="targetParentTrans"></param>
        /// <returns></returns>
        public static Vector3 ConvertTrans2(UnityEngine.Transform sourceTransform, UnityEngine.Transform targetParentTrans, Camera uiCamera = null)
        {
            Vector2 screenPoint = UIPointToScreenPoint(sourceTransform.position, uiCamera);
            var rt = targetParentTrans.GetComponent<RectTransform>();
            Vector3 uiPoint = ScreenPointToUIPoint(rt, screenPoint, uiCamera);
            return uiPoint;
        }

        // UI 坐标转换为屏幕坐标
        public static Vector2 UIPointToScreenPoint(Vector3 worldPoint, Camera uiCamera)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPoint);
            return screenPoint;
        }
        
        // 屏幕坐标转换为 UGUI 坐标
        public static Vector2 ScreenPointToUIPoint(RectTransform parentRt, Vector2 screenPoint, Camera uiCamera)
        {
            //UI屏幕坐标转换为世界坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screenPoint, uiCamera, out var globalMousePos);
            // 转换后的 globalMousePos 使用下面方法赋值
            // target 为需要使用的 UI RectTransform
            // rt 可以是 target.GetComponent<RectTransform>(), 也可以是 target.parent.GetComponent<RectTransform>()
            // target.transform.position = globalMousePos;
            return globalMousePos;
        }
    }
}