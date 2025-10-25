using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UnityCore.Extensions.UI
{
    public class EnhancedScrollerBase: MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] protected EnhancedScroller m_scroller;
        [SerializeField] protected EnhancedScrollerCellView cellView;

        private void Awake()
        {
            m_scroller.Delegate = this;
        }
        
        public virtual void Refresh()
        {
            var lastPos = m_scroller.ScrollPosition;
            m_scroller.ReloadData();
            m_scroller.ScrollPosition = lastPos;
            m_scroller.RefreshActive();
        }
        
        public void JumpTo(int index, float time = 0.5f, System.Action callback = null)
        {
            m_scroller.JumpToDataIndex(index,  time, 0, true, EnhancedScroller.TweenType.linear, time, callback);
        }

        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        {
            throw new System.NotImplementedException();
        }

        public virtual float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            throw new System.NotImplementedException();
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}