using Core.Utils;
using UnityCore.Base;
using UnityCore.Constant;
using UnityCore.UI.Core;
using UnityEngine;

namespace UnityCore.UI
{
    public class UIRoot : MonoBehaviour
    {
        [SerializeField] private Canvas normalCanvas;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Canvas popUpCanvas;
        [SerializeField] private Canvas guideCanvas;
        [SerializeField] private Canvas topCanvas;

        private void Awake()
        {
            Game.UI.AddUIGroup(ConstantUIGroup.UIGroupNameNormal, 0,new UIGroupHelper(normalCanvas));
            Game.UI.AddUIGroup(ConstantUIGroup.UIGroupNameMain, 200 ,new UIGroupHelper(mainCanvas));
            Game.UI.AddUIGroup(ConstantUIGroup.UIGroupNamePopup, 400 ,new UIGroupHelper(popUpCanvas));
            Game.UI.AddUIGroup(ConstantUIGroup.UIGroupNameGuide, 600 ,new UIGroupHelper(guideCanvas));
            Game.UI.AddUIGroup(ConstantUIGroup.UIGroupNameTop, 800 ,new UIGroupHelper(topCanvas));
        }
    }
}