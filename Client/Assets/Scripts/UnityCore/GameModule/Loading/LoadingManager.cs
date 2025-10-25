using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using LATLog;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.Tools;
using UnityCore.UI.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace UnityCore.GameModule.Loading
{
    public class LoadingManager : CoreModule
    {
        private int _taskFinishedCount = 0;
        private List<ILoadingTask> _tasks = new();
        
        private ILoadingUI _loadingUI;
        private string _loadingUIName;
        private float _currentProgress;
        private int _loadingUIInstanceId;
        private SceneHandle _loadingSceneHandle;

        public void AddTask(ILoadingTask task)
        {
            if (task == null)
            {
                GameDebug.LogError("Loading task cannot be null.");
                return;
            }

            _tasks.Add(task);
        }

        public void Load(string loadingUIName, object userData = null, Action onStart = null, Action onComplete = null)
        {
            _taskFinishedCount = 0;
            _currentProgress = 0f;
            _loadingUIName = loadingUIName;
            CoroutineManager.Instance.StartCoroutine(LoadCoroutine(userData, onStart,onComplete));
        }

        public void Load(Action onStart = null, Action onComplete = null)
        {
            _taskFinishedCount = 0;
            _currentProgress = 0f;
            CoroutineManager.Instance.StartCoroutine(LoadCoroutine(onStart,onComplete));
        }

        private IEnumerator LoadCoroutine(Action onStart, Action onComplete)
        {
            onStart?.Invoke();
            _loadingSceneHandle = Game.YooAsset.LoadScene("Loading", LoadSceneMode.Additive);
            yield return _loadingSceneHandle;

            float totalWeight = 0f;
            foreach (var task in _tasks)
                totalWeight += task.Weight;

            // 执行所有加载任务
            foreach (var task in _tasks)
            {
                CoroutineManager.Instance.Coroutine(ExecuteTask(task, totalWeight));
                // 等待当前任务完成
                yield return new WaitUntil(() => task.IsDone);
            }

            onComplete?.Invoke();
        }

        private IEnumerator LoadCoroutine(object userData, Action onStart, Action onComplete)
        {
            onStart?.Invoke();
            _loadingSceneHandle = Game.YooAsset.LoadScene("Loading", LoadSceneMode.Additive);
            yield return _loadingSceneHandle;

            var loadingUIGroup = GetLoadingUIGroup();
            _loadingUIInstanceId = Game.UI.OpenUI(_loadingUIName, loadingUIGroup.Name, userData);
            yield return new WaitUntil(() => Game.UI.GetUIForm(_loadingUIInstanceId) != null);
            var uiForm = Game.UI.GetUIForm(_loadingUIInstanceId);
            _loadingUI = ((GameObject)uiForm.Handle).GetComponent<ILoadingUI>();

            _loadingUI?.OnLoadingStarted();

            float totalWeight = 0f;
            foreach (var task in _tasks)
                totalWeight += task.Weight;

            // 执行所有加载任务
            foreach (var task in _tasks)
            {
                CoroutineManager.Instance.Coroutine(ExecuteTask(task, totalWeight));
                // 等待当前任务完成
                yield return new WaitUntil(() => task.IsDone);
            }

            // 所有任务完成
            _loadingUI?.SetProgress(1f);
            _loadingUI?.OnLoadingComplete();

            onComplete?.Invoke();
        }

        private IEnumerator ExecuteTask(ILoadingTask task, float totalWeight)
        {
            var cachedProgress = _currentProgress;
            CoroutineManager.Instance.Coroutine(task.Execute());
            
            // 设置总体进度
            while (!task.IsDone)
            {
                float taskProgress = task.Progress;
                _currentProgress = cachedProgress + (taskProgress * task.Weight / totalWeight);
                _loadingUI?.SetProgress(_currentProgress);
                yield return null;
            }
            
            // 进度条更新为当前任务完成的权重占比

            _currentProgress = cachedProgress + (task.Weight / totalWeight);
            _loadingUI?.SetProgress(_currentProgress);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            SceneManager.UnloadSceneAsync(_loadingSceneHandle.SceneObject);
            _loadingSceneHandle.Release();
            if (!string.IsNullOrEmpty(_loadingUIName))
            {
                Game.YooAsset.UnloadAsset(_loadingUIName);
            }
            GameDebug.Log("LoadingManager Shutdown. Unloaded Loading UI and Scene.");
        }

        private IUIGroup GetLoadingUIGroup()
        {
            // 获取Loading UIGroup
            IUIGroup uiGroup = Game.UI.GetUIGroup(ConstantUIGroup.UIGroupNameLoading);
            if (uiGroup == null)
            {
                var loadingUIObject = GameObject.Find("LoadingRoot"); // 假设场景中有一个名为"LoadingUI"的对象
                var canvas = loadingUIObject.GetComponent<Canvas>();
                Game.UI.AddUIGroup(ConstantUIGroup.UIGroupNameLoading, 1000, new UIGroupHelper(canvas));
                uiGroup = Game.UI.GetUIGroup(ConstantUIGroup.UIGroupNameLoading);
            }

            return uiGroup;
        }
    }
}