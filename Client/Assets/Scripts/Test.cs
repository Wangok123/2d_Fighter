using Cysharp.Threading.Tasks;
using UnityCore.Base;
using UnityCore.UI.Core;
using UnityEngine;

public class Test : MonoBehaviour
{
    private UIComponent _uiComponent;
    
    void Start()
    {
        _uiComponent = GameEntry.GetComponent<UIComponent>();
        if (_uiComponent == null)
        {
            Debug.LogError("UIComponent not found on this GameObject.");
            return;
        }
    }
}
