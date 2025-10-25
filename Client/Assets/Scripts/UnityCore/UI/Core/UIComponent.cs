using System.Collections.Generic;
using Core.ObjectPool;
using LATLog;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityCore.UI.Core
{
    public class UIComponent : LatComponent
    {
        [SerializeField] private UISceneSO _sceneData;
        [SerializeField] private int mPriority = 0;
        [SerializeField] private int instanceCapacity = 16;

        [Header("自动释放界面实例对象池对象的时间间隔")] [SerializeField]
        private int instanceAutoReleaseInterval = 60;

        [Header("获取或设置界面实例对象池对象过期秒数")] [SerializeField]
        private float instanceExpireTime = 60f;

        private UIManager _uiManager;
        private Dictionary<int, List<int>> _uiList = new Dictionary<int, List<int>>();

        protected override void Awake()
        {
            base.Awake();

            _uiManager = GameModuleManager.GetModule<UIManager>();
            _uiManager.SetUIFormHelper(new UIFormHelper());
            var poolManager = GameModuleManager.GetModule<ObjectPoolManager>();
            _uiManager.SetObjectPoolManager(poolManager);
            _uiManager.InstancePriority = mPriority;
            _uiManager.InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
            _uiManager.InstanceCapacity = instanceCapacity;
            _uiManager.InstanceExpireTime = instanceExpireTime;

            var handle = _sceneData.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
            handle.WaitForCompletion();

            IsInit = true;
        }

        #region Functions

        public void AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return;
            }

            _uiManager.AddUIGroup(uiGroupName, uiGroupDepth, uiGroupHelper);
        }
        
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return null;
            }

            return _uiManager.GetUIGroup(uiGroupName);
        }

        public int OpenUI(int listID, string uiName, object userData = null,
            string groupName = ConstantUIGroup.UIGroupNameMain)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            if (!_uiList.ContainsKey(listID))
            {
                _uiList.Add(listID, new List<int>());
            }
            
            int serialId = _uiManager.OpenUIForm(uiName, groupName, userData);
            _uiList[listID].Add(serialId);
            return serialId;
        }

        public List<int> GetUIList(int listID)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return null;
            }

            if (!_uiList.ContainsKey(listID))
            {
                GameDebug.LogError($"UI stack with ID {listID} does not exist.");
                return null;
            }
            
            return _uiList[listID];
        }

        public int OpenUI(string uiName, string groupName = ConstantUIGroup.UIGroupNameMain)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiName, groupName);
        }

        public int OpenUI(string uiFormAssetName, string uiGroupName, int priority)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, uiGroupName, priority);
        }

        public int OpenUI(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm);
        }

        public int OpenUI(string uiFormAssetName, object userData)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, ConstantUIGroup.UIGroupNameMain, userData);
        }

        public int OpenUI(string uiFormAssetName, string uiGroupName, object userData)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, uiGroupName, userData);
        }

        public int OpenUI(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm);
        }

        public int OpenUI(string uiFormAssetName, string uiGroupName, int priority, object userData)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, uiGroupName, priority, userData);
        }

        public int OpenUI(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm, userData);
        }

        public int OpenUI(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm,
            object userData)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return -1;
            }

            return _uiManager.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
        }

        public IUIForm GetUIForm(int serialId)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return null;
            }

            return _uiManager.GetUIForm(serialId);
        }

        public IUIForm GetUIForm(string uiName)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return null;
            }

            return _uiManager.GetUIForm(uiName);
        }

        public void CloseUI(int serialId)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return;
            }

            _uiManager.CloseUIForm(serialId);
        }

        public void CloseUI(int serialId, object data)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return;
            }

            _uiManager.CloseUIForm(serialId, data);
        }

        public void CloseUI(IUIForm uiForm)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return;
            }

            _uiManager.CloseUIForm(uiForm);
        }

        public void CloseUI(IUIForm uiForm, object data)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return;
            }

            _uiManager.CloseUIForm(uiForm, data);
        }
        
        public void CloseUIByUIListID(int listID)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return;
            }

            if (!_uiList.ContainsKey(listID))
            {
                GameDebug.LogError($"UI stack with ID {listID} does not exist.");
                return;
            }

            foreach (var serialId in _uiList[listID])
            {
                _uiManager.CloseUIForm(serialId);
            }
            
            _uiList.Remove(listID);
        }

        #region CommonUI

        public void OpenTipsUI(string tips)
        {
            if (_uiManager == null)
            {
                GameDebug.LogError("UIManager is not initialized.");
                return;
            }

            _uiManager.OpenUIForm("UI_TipsWnd", ConstantUIGroup.UIGroupNameTop, tips);
        }

        #endregion

        #endregion
    }
}