using System;
using Wjybxx.Commons.Fx;

namespace UnityCore.GameModule.Components
{
    /// <summary>
    /// 数据组件示例：列表组件
    /// </summary>
    [ComponentDefine(Kind = ComponentKind.Data)]
    public abstract class ListComponent<E> : GComponent
    {
        private E[] _elements;
        private int _count;
        private int _capacity;
    
        public ListComponent(int initialCapacity = 4)
        {
            _capacity = initialCapacity;
            _elements = new E[initialCapacity];
            _count = 0;
        }
    
        public int Count => _count;
    
        public E this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();
                return _elements[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();
                _elements[index] = value;
            }
        }
    
        public void Add(E element)
        {
            if (_count >= _capacity)
            {
                _capacity *= 2;
                Array.Resize(ref _elements, _capacity);
            }
        
            _elements[_count++] = element;
        }
    
        public bool Remove(E element)
        {
            for (int i = 0; i < _count; i++)
            {
                if (Equals(_elements[i], element))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            
            // 移动元素
            for (int i = index; i < _count - 1; i++)
            {
                _elements[i] = _elements[i + 1];
            }
        
            _count--;
            _elements[_count] = default;
        }
    
        public void Clear()
        {
            Array.Clear(_elements, 0, _count);
            _count = 0;
        }
    
        public override void Reset()
        {
            base.Reset();
            Clear();
        }
    }
}