using LATLog;
using UnityEngine;

namespace UnityCore.Extensions.UI
{
    public static class UGUI
    {
        public static void FillParent(this RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                GameDebug.LogError("[FillParent] RectTransform is null");
                return;
            }

            // 设置锚点为四个角
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            // 设置位置和大小
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            
            rectTransform.localScale = Vector3.one;
        }
    }
}