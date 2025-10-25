using YooAsset;

namespace UnityCore.ResourceSystem.Utils
{
    public static class AssetHandleExtension
    {
        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        public static AssetHandle WaitForAsyncOperationComplete(this AssetHandle thisHandle)
        {
            thisHandle.WaitForAsyncComplete();
            return thisHandle;
        }
    }
}