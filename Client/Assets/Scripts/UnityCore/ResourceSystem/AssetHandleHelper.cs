using System.Collections.Generic;
using UnityCore.Base;
using YooAsset;

namespace UnityCore.ResourceSystem
{
    public static class AssetHandleHelper
    {
        private static Dictionary<string, AssetHandle> _assetHandles = new(); // 资源路径与句柄的映射
        private static Dictionary<string, int> _refCounts = new();           // 引用计数
        private static Dictionary<string, RawFileHandle> _rawFileHandles = new(); // 原始文件句柄映射
        private static Dictionary<string, int> _rawRefCounts = new();      // 原始文件引用计数
        

        public static bool TryGetAssetHandle(string assetName, out AssetHandle handle)
        {
            if (_assetHandles.TryGetValue(assetName, out handle))
            {
                _refCounts[assetName]++;
                return true;
            }

            return false;
        }
        
        public static bool TryGetRawAssetHandle(string assetName, out RawFileHandle handle)
        {
            if (_rawFileHandles.TryGetValue(assetName, out RawFileHandle rawHandle))
            {
                handle = rawHandle;
                _rawRefCounts[assetName]++;
                return true;
            }

            handle = null;
            return false;
        }
        
        public static void AddAssetHandle(string assetName, AssetHandle handle)
        {
            if (_assetHandles.ContainsKey(assetName))
            {
                _refCounts[assetName]++;
            }
            else
            {
                _assetHandles[assetName] = handle;
                _refCounts[assetName] = 1;
            }
        }

        public static void AddRawAssetHandle(string assetName, RawFileHandle handle)
        {
            if (_rawFileHandles.ContainsKey(assetName))
            {
                _rawRefCounts[assetName]++;
            }
            else
            {
                _rawFileHandles[assetName] = handle;
                _rawRefCounts[assetName] = 1;
            }
        }

        public static bool ReleaseRawAssetHandle(string assetName)
        {
            if (_rawRefCounts.TryGetValue(assetName, out int count))
            {
                if (count > 1)
                {
                    _rawRefCounts[assetName]--;
                }
                else
                {
                    var rawFileHandle = _rawFileHandles[assetName];
                    _rawFileHandles.Remove(assetName);
                    _rawRefCounts.Remove(assetName);
                    rawFileHandle.Release();
                    // 最后一个引用计数归零后，卸载资源
                    Game.YooAsset.UnloadUnusedAssets();
                }
                
                return true;
            }

            return false;
        }
        
        public static bool ReleaseAssetHandle(string assetName)
        {
            if (_refCounts.TryGetValue(assetName, out int count))
            {
                if (count > 1)
                {
                    _refCounts[assetName]--;
                }
                else
                {
                    var assetHandle = _assetHandles[assetName];
                    _assetHandles.Remove(assetName);
                    _refCounts.Remove(assetName);
                    assetHandle.Release();
                    // 最后一个引用计数归零后，卸载资源
                    Game.YooAsset.UnloadUnusedAssets();
                }
                
                return true;
            }

            return false;
        }
        
        public static void ReleaseAssetHandle(AssetHandle handle)
        {
            foreach (var kvp in _assetHandles)
            {
                if (kvp.Value == handle)
                {
                    ReleaseAssetHandle(kvp.Key);
                    return;
                }
            }
            
            throw new KeyNotFoundException($"AssetHandle not found in UI asset handles: {handle}");
        }
    }
}