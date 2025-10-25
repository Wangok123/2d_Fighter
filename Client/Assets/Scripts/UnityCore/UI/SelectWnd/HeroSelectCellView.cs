using System;
using EnhancedUI.EnhancedScroller;
using UnityCore.Entities.User;
using UnityCore.Extensions.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCore.UI.SelectWnd
{
    public class HeroSelectCellView : EnhancedScrollerCellView
    {
        [SerializeField] private LatButton cellButton;
        [SerializeField] private LatImage icon;
        [SerializeField] private LatImage selectIcon;
        [SerializeField] private Text nameText;

        public Action<int, int> OnClickCell;

        public int Index { get; set; }
        
        private int _heroId;
        private HeroData _heroData;

        private void OnEnable()
        {
            cellButton.onClick.AddListener(OnCellClick);
        }

        private void OnDisable()
        {
            cellButton.onClick.RemoveListener(OnCellClick);
        }

        public void SetData(HeroData data)
        {
            _heroData = data; 
            _heroId = data.HeroId;
        }

        public override void RefreshCellView()
        {
            icon.LoadImage("HeadIcon", $"{_heroData.HeroIcon}_head");
            nameText.text = _heroData.HeroName;
        }

        private void OnCellClick()
        {
            OnClickCell?.Invoke(_heroId, Index); 
        }

        public void SetSelected(bool selected)
        {
            selectIcon.Image.gameObject.SetActive(selected);
        }
    }
}