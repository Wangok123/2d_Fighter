using System;
using UnityCore.Base;
using UnityCore.GameModule;
using UnityCore.Tools;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    private void Awake()
    {
        CoroutineManager.Instance.Behaviour = this;
    }
}
