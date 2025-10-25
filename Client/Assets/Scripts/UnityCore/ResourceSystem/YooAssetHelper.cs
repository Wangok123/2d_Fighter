using System;
using System.Threading.Tasks;
using LATLog;
using UnityEngine;
using YooAsset;

namespace UnityCore.ResourceSystem
{
    public static class YooAssetHelper
    {
        #region 资源卸载

        // 卸载所有引用计数为零的资源包。
        // 可以在切换场景之后调用资源释放方法或者写定时器间隔时间去释放。
        public static async Task UnloadUnusedAssets(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UnloadUnusedAssetsAsync();
            await operation.Task;
        }

        // 强制卸载所有资源包，该方法请在合适的时机调用。
        // 注意：Package在销毁的时候也会自动调用该方法。
        public static async Task ForceUnloadAllAssets(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UnloadAllAssetsAsync();
            await operation.Task;
        }

        // 尝试卸载指定的资源对象
        // 注意：如果该资源还在被使用，该方法会无效。
        public static void TryUnloadUnusedAsset(string packageName, string assetPath)
        {
            var package = YooAssets.GetPackage(packageName);
            package.TryUnloadUnusedAsset(assetPath);
        }

        #endregion

        #region 资源移除

        /// <summary>
        /// 清理文件系统所有的缓存资源文件
        /// </summary>
        public static async Task ClearPackageAllCacheBundleFiles(string packageName, Action callback)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //清理成功
                callback?.Invoke();
            }
            else
            {
                //清理失败
                GameDebug.LogError(operation.Error);
            }
        }

        /// <summary>
        /// 清理文件系统未使用的缓存资源文件,以当前激活的资源清单为准，清理该资源清单内未再使用的缓存文件。
        /// </summary>
        public static async Task ClearPackageUnusedCacheBundleFiles(string packageName, Action callback)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //清理成功
                callback?.Invoke();
            }
            else
            {
                //清理失败
                Debug.LogError(operation.Error);
            }
        }

        public static async Task ClearPackageCacheBundleFilesByTags(string packageName, string[] tags, Action callback)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearBundleFilesByTags, tags);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //清理成功
                callback?.Invoke();
            }
            else
            {
                //清理失败
                Debug.LogError(operation.Error);
            }
        }

        /// <summary>
        /// 清理文件系统所有的缓存清单文件
        /// </summary>
        public static async Task ClearPackageAllCacheManifestFiles()
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearAllManifestFiles);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //清理成功
            }
            else
            {
                //清理失败
                Debug.LogError(operation.Error);
            }
        }

        /// <summary>
        /// 清理文件系统未使用的缓存清单文件
        /// </summary>
        public static async Task ClearPackageUnusedCacheManifestFiles()
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //清理成功
            }
            else
            {
                //清理失败
                Debug.LogError(operation.Error);
            }
        }

        #endregion
    }
}