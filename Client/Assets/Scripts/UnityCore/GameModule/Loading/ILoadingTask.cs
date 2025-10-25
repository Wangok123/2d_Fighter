using System.Collections;

namespace UnityCore.GameModule.Loading
{
    public interface ILoadingTask
    {
        /// <summary>
        /// 范围 0~1
        /// </summary>
        float Progress { get; }
        int Weight { get; }
        bool IsDone { get; }
        IEnumerator Execute();
    }
}