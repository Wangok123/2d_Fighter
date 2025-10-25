using YooAsset;

namespace UnityCore.ResourceSystem.Utils
{
    public static class HandleBaseExtension
    {
        public static bool IsSucceed(this HandleBase thisHandle)
        {
            return thisHandle.IsDone && thisHandle.Status == EOperationStatus.Succeed;
        }
    }
}