using UnityEngine;

namespace UnityCore.UI.Core
{
    public class UIGroupHelper : IUIGroupHelper
    {
        private readonly Canvas _canvas;
        public Canvas Canvas => _canvas;
        
        public UIGroupHelper(Canvas canvas)
        {
            _canvas = canvas;
        }
        
        public void SetDepth(int depth)
        {
            _canvas.sortingOrder = depth;
        }
    }
}