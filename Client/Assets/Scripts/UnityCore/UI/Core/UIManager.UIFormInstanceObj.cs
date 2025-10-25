using System;
using Core.ObjectPool;
using Core.ReferencePool;

namespace UnityCore.UI.Core
{
    public partial class UIManager
    {
        /// <summary>
        /// 界面实例对象。
        /// </summary>
        private sealed class UIFormInstanceObject : ObjectBase
        {
            private object m_UIFormAssetHandle;
            private IUIFormHelper m_UIFormHelper;

            public UIFormInstanceObject()
            {
                m_UIFormAssetHandle = null;
                m_UIFormHelper = null;
            }

            public static UIFormInstanceObject Create(string name, object assetHandle, object uiFormInstance, IUIFormHelper uiFormHelper)
            {
                if (assetHandle == null)
                {
                    throw new Exception("UI form asset is invalid.");
                }

                if (uiFormHelper == null)
                {
                    throw new Exception("UI form helper is invalid.");
                }

                UIFormInstanceObject uiFormInstanceObject = ReferencePool.Acquire<UIFormInstanceObject>();
                uiFormInstanceObject.Initialize(name, uiFormInstance);
                uiFormInstanceObject.m_UIFormAssetHandle = assetHandle;
                uiFormInstanceObject.m_UIFormHelper = uiFormHelper;
                return uiFormInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                m_UIFormAssetHandle = null;
                m_UIFormHelper = null;
            }

            protected internal override void Release(bool isShutdown)
            {
                m_UIFormHelper.ReleaseUIForm(m_UIFormAssetHandle, Target);
            }
        }
    }
}