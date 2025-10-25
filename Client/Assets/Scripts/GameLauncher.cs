using System;
using System.Collections;
using System.Collections.Generic;
using UnityCore.Base;
using UnityCore.EventDefine;
using UnityCore.SceneManagement;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using YooAsset;

public class GameLauncher : MonoBehaviour
{
    [SerializeField] private GameSceneSO baseScene;
    [SerializeField] private List<GameSceneSO> initialSceneList;

    private int _initialCount;
    
    private IEnumerator Start()
    {
        // 先加载基础场景
        var handler = baseScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
        yield return handler;
        
        // 等待持久化场景加载，初始化完成
        var allBaseComponents = FindObjectsOfType<LatComponent>();
        foreach (var component in allBaseComponents)
        {
            if (component.IsInit) continue;
            yield return new WaitUntil(() => component.IsInit);
        }

        foreach (var sceneSo in initialSceneList)
        {
            sceneSo.sceneReference.LoadSceneAsync(LoadSceneMode.Additive).Completed += LoadSceneComplete;
        }
        
        yield return new WaitUntil(() => _initialCount == initialSceneList.Count);
        SceneEventDefine.ChangeToHomeScene.SendEventMessage();
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    private void LoadSceneComplete(AsyncOperationHandle<SceneInstance> obj)
    {
        _initialCount++;
    }
}