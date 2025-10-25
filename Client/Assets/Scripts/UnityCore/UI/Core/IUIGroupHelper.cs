using UnityEngine;

namespace UnityCore.UI.Core
{
    public interface IUIGroupHelper
    {
        Canvas Canvas { get; }
        
        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        void SetDepth(int depth);
    }
}