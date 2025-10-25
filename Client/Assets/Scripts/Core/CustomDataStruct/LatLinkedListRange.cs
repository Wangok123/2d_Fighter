using System;
using System.Collections;
using System.Collections.Generic;

namespace Core.CustomDataStruct
{
    public struct LatLinkedListRange<T>: IEnumerable<T>, IEnumerable
    {
        private readonly LinkedListNode<T> m_First;
        private readonly LinkedListNode<T> m_Terminal;
        
        public LatLinkedListRange(LinkedListNode<T> first, LinkedListNode<T> terminal)
        {
            if (first == null || terminal == null || first == terminal)
            {
                throw new Exception("Range is invalid.");
            }

            m_First = first;
            m_Terminal = terminal;
        }
        
        public bool IsValid => m_First != null && m_Terminal != null && m_First != m_Terminal;
        
        public LinkedListNode<T> First => m_First;
        public LinkedListNode<T> Terminal => m_Terminal;
        
        public int Count
        {
            get
            {
                if (!IsValid)
                {
                    return 0;
                }

                int count = 0;
                for (LinkedListNode<T> current = m_First; current != null && current != m_Terminal; current = current.Next)
                {
                    count++;
                }

                return count;
            }
        }
        
        public bool Contains(T value)
        {
            for (LinkedListNode<T> current = m_First; current != null && current != m_Terminal; current = current.Next)
            {
                if (current.Value.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }
        
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        
        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 返回循环访问集合的枚举数。
        /// </summary>
        /// <returns>循环访问集合的枚举数。</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly LatLinkedListRange<T> m_GameFrameworkLinkedListRange;
            private LinkedListNode<T> m_Current;
            private T m_CurrentValue;

            internal Enumerator(LatLinkedListRange<T> range)
            {
                if (!range.IsValid)
                {
                    throw new Exception("Range is invalid.");
                }

                m_GameFrameworkLinkedListRange = range;
                m_Current = m_GameFrameworkLinkedListRange.m_First;
                m_CurrentValue = default(T);
            }

            /// <summary>
            /// 获取当前结点。
            /// </summary>
            public T Current => m_CurrentValue;

            /// <summary>
            /// 获取当前的枚举数。
            /// </summary>
            object IEnumerator.Current => m_CurrentValue;

            /// <summary>
            /// 清理枚举数。
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// 获取下一个结点。
            /// </summary>
            /// <returns>返回下一个结点。</returns>
            public bool MoveNext()
            {
                if (m_Current == null || m_Current == m_GameFrameworkLinkedListRange.m_Terminal)
                {
                    return false;
                }

                m_CurrentValue = m_Current.Value;
                m_Current = m_Current.Next;
                return true;
            }

            /// <summary>
            /// 重置枚举数。
            /// </summary>
            void IEnumerator.Reset()
            {
                m_Current = m_GameFrameworkLinkedListRange.m_First;
                m_CurrentValue = default(T);
            }
        }
    }
}