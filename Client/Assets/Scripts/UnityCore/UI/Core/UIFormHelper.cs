using LATLog;
using Unity.VisualScripting;
using UnityCore.Base;
using UnityCore.ResourceSystem;
using UnityEngine;
using YooAsset;

namespace UnityCore.UI.Core
{
    public class UIFormHelper : IUIFormHelper
    {
        private YooAssetComponent _yooAssetComponent = null;

        private YooAssetComponent YooAssetComponent
        {
            get
            {
                if (_yooAssetComponent == null)
                {
                    _yooAssetComponent = GameEntry.GetComponent<YooAssetComponent>();
                }

                return _yooAssetComponent;
            }
        }
        
        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="assetHandle">要实例化的界面资源。</param>
        /// <returns>实例化后的界面。</returns>
        public object InstantiateUIForm(object assetHandle)
        {
            AssetHandle handle = assetHandle as AssetHandle;
            if (handle == null)
            {
                GameDebug.LogError("UI form asset is invalid.");
                return null;
            }

            return handle.InstantiateSync();
        }

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiFormInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        public IUIForm CreateUIForm(object uiFormInstance, IUIGroup uiGroup, object userData)
        {
            GameObject go = uiFormInstance as GameObject;
            if (go != null)
            {
                Transform trans = uiGroup.Helper.Canvas.transform;
                go.transform.SetParent(trans, false);
            }

            return go.GetOrAddComponent<UIForm>();
        }

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="assetHandle">要释放的界面资源。</param>
        /// <param name="uiFormInstance">要释放的界面实例。</param>
        public void ReleaseUIForm(object assetHandle, object uiFormInstance)
        {
            GameObject go = uiFormInstance as GameObject;
            if (go != null)
            {
                Object.Destroy(go);
            }
            YooAssetComponent.UnloadUIAsset(assetHandle);
        }
    }
}