using System;
using System.Threading.Tasks;
using LATLog;
using UnityCore.ResourceSystem;
using UnityEngine;
using YooAsset;

namespace UnityCore.UI.Core
{
    public class UIForm  : MonoBehaviour, IUIForm
    {
        private int m_SerialId;
        private string m_UIFormAssetName;
        private IUIGroup m_UIGroup;
        private int m_DepthInUIGroup;
        private bool m_PauseCoveredUIForm;
        private UIFormLogic m_UIFormLogic;
        
        public int SerialId => m_SerialId;
        public string UIFormAssetName => m_UIFormAssetName;
        public object Handle => gameObject;
        public IUIGroup UIGroup => m_UIGroup;
        public int DepthInUIGroup => m_DepthInUIGroup;
        public bool PauseCoveredUIForm => m_PauseCoveredUIForm;
        /// <summary>
        /// 获取界面逻辑。
        /// </summary>
        public UIFormLogic Logic => m_UIFormLogic;

        public void OnInit(int serialId, string uiFormAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm, bool isNewInstance,
            object userData)
        {
            m_SerialId = serialId;
            m_UIFormAssetName = uiFormAssetName;
            m_UIGroup = uiGroup;
            m_DepthInUIGroup = 0;
            m_PauseCoveredUIForm = pauseCoveredUIForm;
            
            if (!isNewInstance)
            {
                return;
            }

            m_UIFormLogic = GetComponent<UIFormLogic>();
            if (m_UIFormLogic == null)
            {
                GameDebug.LogError("UI form '{0}' can not get UI form logic.", uiFormAssetName);
                return;
            }

            try
            {
                m_UIFormLogic.OnInit(userData);
            }
            catch (Exception exception)
            {
                GameDebug.LogError($"UI form '[{m_SerialId.ToString()}]{m_UIFormAssetName}' OnInit with exception '{exception}'.");
            }
        }

        public void OnRecycle()
        {
            try
            {
                string uiFormAssetName = m_UIFormAssetName;
                // Game.YooAsset.UnloadAsset(uiFormAssetName);
                m_UIFormLogic.OnRecycle();
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnRecycle with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }

            m_SerialId = 0;
            m_DepthInUIGroup = 0;
            m_PauseCoveredUIForm = true;
        }

        public void OnOpen(object userData)
        {
            try
            {
                m_UIFormLogic.OnOpen(userData);
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnOpen with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnClose(bool isShutdown, object userData)
        {
            try
            {
                m_UIFormLogic.OnClose(isShutdown, userData);
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnClose with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        public void OnPause()
        {
            try
            {
                m_UIFormLogic.OnPause();
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnPause with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        public void OnResume()
        {
            try
            {
                m_UIFormLogic.OnResume();
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnResume with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        public void OnCover()
        {
            try
            {
                m_UIFormLogic.OnCover();
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnCover with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        public void OnReveal()
        {
            try
            {
                m_UIFormLogic.OnReveal();
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnReveal with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        public void OnRefocus(object userData)
        {
            try
            {
                m_UIFormLogic.OnRefocus(userData);
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnRefocus with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                m_UIFormLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnUpdate with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }

        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            m_DepthInUIGroup = depthInUIGroup;
            try
            {
                m_UIFormLogic.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            }
            catch (Exception exception)
            {
                GameDebug.LogError("UI form '[{0}]{1}' OnDepthChanged with exception '{2}'.", m_SerialId.ToString(), m_UIFormAssetName, exception.ToString());
            }
        }
    }
}