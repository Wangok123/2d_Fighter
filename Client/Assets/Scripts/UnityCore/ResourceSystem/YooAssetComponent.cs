using System;
using System.Collections;
using System.Threading.Tasks;
using LATLog;
using UnityCore.Base;
using UnityCore.UI.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using YooAsset;

namespace UnityCore.ResourceSystem
{
    public class YooAssetComponent : LatComponent
    {
        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

        private ResourcePackage _package;
        private ResourcePackage _rawPackage;

        private void Start()
        {
            YooAssets.Initialize();
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            // 开始补丁更新流程
#if !UNITY_EDITOR
                PlayMode = EPlayMode.OfflinePlayMode;
#endif

            var operation = new PatchOperation("DefaultPackage", PlayMode);
            YooAssets.StartOperation(operation);
            yield return operation;
            
            var rawOperation = new PatchOperation("RawPackage", PlayMode);
            YooAssets.StartOperation(rawOperation);
            yield return rawOperation;

            // 设置默认的资源包
            _package = YooAssets.GetPackage("DefaultPackage");
            _rawPackage = YooAssets.GetPackage("RawPackage");
            YooAssets.SetDefaultPackage(_package);

            IsInit = true;
        }
        
        public IEnumerator LoadUIAsset(string uiFormAssetName, int priority, LoadAssetCallbacks callbacks,
            OpenUIFormInfo openUIFormInfo)
        {
            if (AssetHandleHelper.TryGetAssetHandle(uiFormAssetName, out AssetHandle result))
            {
                callbacks.LoadAssetSuccessCallback?.Invoke(uiFormAssetName, result, priority, openUIFormInfo);
                yield break;
            }
            
            AssetHandle handle = _package.LoadAssetAsync<GameObject>(uiFormAssetName);
            yield return handle;
            if (handle.Status == EOperationStatus.Succeed)
            {
                AssetHandleHelper.AddAssetHandle(uiFormAssetName, handle);
                callbacks.LoadAssetSuccessCallback?.Invoke(uiFormAssetName, handle, priority, openUIFormInfo);
            }
            else
            {
                GameDebug.LogError($"Load asset failed: {handle}");
                callbacks.LoadAssetFailureCallback?.Invoke(uiFormAssetName, $"LoadError, AssetName:{uiFormAssetName}",
                    openUIFormInfo);
            }
        }

        public async void LoadAudioAsset(string audioAssetName, Action<AudioClip> onLoaded)
        {
            if (AssetHandleHelper.TryGetAssetHandle(audioAssetName, out AssetHandle result))
            {
                onLoaded?.Invoke(result.AssetObject as AudioClip);
                return;
            }

            AssetHandle handle = _package.LoadAssetAsync<AudioClip>(audioAssetName);
            var task = handle.Task;
            await task;
            if (task.Status == TaskStatus.RanToCompletion)
            {
                AssetHandleHelper.AddAssetHandle(audioAssetName, handle);
                onLoaded?.Invoke(handle.AssetObject as AudioClip);
            }
            else
            {
                GameDebug.LogError($"Load audio asset failed: {handle}");
            }
        }
        
        public IEnumerator LoadAtlasAsset(string atlasName, Action<SpriteAtlas> onLoaded)
        {
            if (AssetHandleHelper.TryGetAssetHandle(atlasName, out AssetHandle result))
            {
                onLoaded?.Invoke(result.AssetObject as SpriteAtlas);
                yield break;
            }

            AssetHandle handle = _package.LoadAssetAsync<SpriteAtlas>(atlasName);
            yield return handle;
            if (handle.Status == EOperationStatus.Succeed)
            {
                AssetHandleHelper.AddAssetHandle(atlasName, handle);
                onLoaded?.Invoke(handle.AssetObject as SpriteAtlas);
            }
            else
            {
                GameDebug.LogError($"Load atlas asset failed: {handle}");
            }
        }
        
        // 场景无需缓存，无需回调
        public SceneHandle LoadScene(string sceneName, LoadSceneMode loadSceneMode)
        {
            SceneHandle handle = _package.LoadSceneAsync(sceneName, loadSceneMode);
            return handle;
        }
        
        public IEnumerator LoadTextureAsset(string textureAssetName, Action<Texture> onLoaded)
        {
            if (AssetHandleHelper.TryGetAssetHandle(textureAssetName, out AssetHandle result))
            {
                onLoaded?.Invoke(result.AssetObject as Texture);
                yield break;
            }

            AssetHandle handle = _package.LoadAssetAsync<Texture>(textureAssetName);
            yield return handle;
            if (handle.Status == EOperationStatus.Succeed)
            {
                AssetHandleHelper.AddAssetHandle(textureAssetName, handle);
                onLoaded?.Invoke(handle.AssetObject as Texture);
            }
            else
            {
                GameDebug.LogError($"Load texture asset failed: {handle}");
            }
        }
        
        public RawFileHandle LoadRawAsset(string assetName)
        {
            if (AssetHandleHelper.TryGetRawAssetHandle(assetName, out RawFileHandle result))
            {
                return result;
            }
            

            RawFileHandle handle = _rawPackage.LoadRawFileSync(assetName);
            if (handle.Status == EOperationStatus.Succeed)
            {
                var path = handle.GetRawFilePath();
                AssetHandleHelper.AddRawAssetHandle(assetName, handle);
                return handle;
            }
            else
            {
                GameDebug.LogError($"Load raw asset failed: {handle}");
                return null;
            }
        }
        
        public IEnumerator LoadGameObjectAsync(string goName, Action<GameObject> onLoaded)
        {
            if (AssetHandleHelper.TryGetAssetHandle(goName, out AssetHandle result))
            {
                onLoaded?.Invoke(result.AssetObject as GameObject);
                yield break;
            }

            AssetHandle handle = _package.LoadAssetAsync(goName);
            yield return handle;
            if (handle.Status == EOperationStatus.Succeed)
            {
                AssetHandleHelper.AddAssetHandle(goName, handle);
                onLoaded?.Invoke(handle.AssetObject as GameObject);
            }
            else
            {
                GameDebug.LogError($"Load GameObject failed: {handle}");
            }
        }

        public GameObject LoadGameObjectSync(string goName)
        {
            if (AssetHandleHelper.TryGetAssetHandle(goName, out AssetHandle result))
            {
                return result.AssetObject as GameObject;
            }

            var allInfos = _package.GetAllAssetInfos();
            foreach (var assetInfo in allInfos)
            {
                GameDebug.Log(assetInfo.Address);
            }
            
            AssetHandle handle = _package.LoadAssetSync<GameObject>(goName);
            if (handle.Status == EOperationStatus.Succeed)
            {
                AssetHandleHelper.AddAssetHandle(goName, handle);
                return handle.AssetObject as GameObject;
            }
            else
            {
                GameDebug.LogError($"Load GameObject failed: {handle}");
                return null;
            }
        }

        public void UnloadAsset(string assetName)
        {
            if (AssetHandleHelper.ReleaseAssetHandle(assetName))
            {
                return;
            }

            GameDebug.LogError($"Unload asset failed: {assetName}");
        }

        public void UnloadUIAsset(object assetHandle)
        {
            AssetHandleHelper.ReleaseAssetHandle(assetHandle as AssetHandle);
        }

        public async void UnloadUnusedAssets()
        {
            await YooAssetHelper.UnloadUnusedAssets(_package.PackageName);
        }
    }
}