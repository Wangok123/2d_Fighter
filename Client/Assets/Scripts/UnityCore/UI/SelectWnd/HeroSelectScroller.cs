using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityCore.Entities.User;
using UnityCore.Extensions.UI;

namespace UnityCore.UI.SelectWnd
{
    public class HeroSelectScroller : EnhancedScrollerBase
    {
        public Action<int> OnSelectAction;
        
        private List<HeroData> _dataList;

        public void SetData(List<HeroData> dataList)
        {
            _dataList = dataList;
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _dataList.Count;
        }

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 200f;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cell = scroller.GetCellView(cellView) as HeroSelectCellView;
            if (cell != null)
            {
                cell.Index = dataIndex;
                cell.SetData(_dataList[dataIndex]);
                cell.RefreshCellView();

                cell.OnClickCell = OnClick;
            }

            return cell;
        }
        
        public void SelectDefaultHero(int heroId)
        {
            for (int i = 0; i < _dataList.Count; i++)
            {
                if (_dataList[i].HeroId == heroId)
                {
                    OnClick(heroId, i);
                    return;
                }
            }
        }

        private void OnClick(int heroId, int index)
        {
            var activeCell = m_scroller.AllActiveCellViews;
            for (int i = 0; i < activeCell.Count; i++)
            {
                var cell = activeCell[i] as HeroSelectCellView;
                if (cell != null)
                {
                    cell.SetSelected(cell.Index == index);
                }
            }
            
            OnSelectAction?.Invoke(heroId);
        }
    }
}