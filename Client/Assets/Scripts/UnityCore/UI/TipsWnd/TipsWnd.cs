using UnityCore.Extensions.UI;
using UnityCore.UI.Core;
using UnityEngine;

namespace UnityCore.UI.TipsWnd
{
    public class TipsWnd : UIFormLogic
    {
        [SerializeField] private GameObject tipsRoot;
        [SerializeField] private LatText tipsText;
        [SerializeField] private Animator tipsAnimator;
        
        private string _tips;
        
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            _tips = userData as string;
        }
        
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            
            if (string.IsNullOrEmpty(_tips))
            {
                _tips = "No tips provided.";
            }
            
            tipsText.text = _tips;
            
            tipsAnimator.Play("TipsWindow");
        }
    }
}