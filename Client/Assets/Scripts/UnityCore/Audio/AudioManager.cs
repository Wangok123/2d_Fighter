using System.Collections.Generic;
using UnityCore.Base;
using UnityCore.ResourceSystem;
using YooAsset;

namespace UnityCore.Audio
{
    public class AudioManager
    {
        private Dictionary<string, AssetHandle> _assetHandles = new(); // 资源路径与句柄的映射
        private Dictionary<string, int> _refCounts = new();           // 引用计数

        private YooAssetComponent _yooAssetComponent = null;

        public AudioManager()
        {
            _yooAssetComponent = Game.YooAsset;
        }
    }
}