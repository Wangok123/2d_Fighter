using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace UnityCore.Utils.ObjectPool
{
    public class SimpleObjectPool<T> where T : MonoBehaviour
    {
        private Func<T> m_ActionOnCreate;
        private Action<T> m_ActionOnGet;
        private Action<T> m_ActionOnRelease;
        private Action<T> m_ActionOnDestroy;

        private T m_Prefab;
        private RectTransform m_Parent;
        private ObjectPool<T> m_Pool;

        public SimpleObjectPool(T prefab, RectTransform parent, Func<T> create = null, Action<T> get = null, Action<T> release = null, Action<T> destroy = null)
        {
            m_Prefab = prefab;
            m_Parent = parent;
            m_ActionOnCreate = create;
            m_ActionOnGet = get;
            m_ActionOnRelease = release;
            m_ActionOnDestroy = destroy;
            
            m_Pool = new ObjectPool<T>(CreateFunc, GetFunc, ReleaseFunc, DestroyFunc);
        }

        public T Get()
        {
            return m_Pool.Get();
        }
        
        public void Release(T item)
        {
            m_Pool.Release(item);
        }

        public void Dispose()
        {
            m_Pool.Dispose();
        }
        
        public void Destroy(T item)
        {
            if (m_ActionOnDestroy != null)
            {
                m_ActionOnDestroy(item);
                return;
            }
            
            Object.Destroy(item.gameObject);
        }
        
        private T CreateFunc()
        {
            if (m_ActionOnCreate != null)
            {
                return m_ActionOnCreate();
            }
            
            var go = Object.Instantiate(m_Prefab, m_Parent);
            return go;
        }
        
        private void GetFunc(T item)
        {
            if (m_ActionOnGet != null)
            {
                m_ActionOnGet(item);
                return;
            }
            
            item.gameObject.SetActive(true);
        }
        
        private void ReleaseFunc(T item)
        {
            if (m_ActionOnRelease != null)
            {
                m_ActionOnRelease(item);
                return;
            }
            
            item.gameObject.SetActive(false);
        }
        
        private void DestroyFunc(T item)
        {
            if (m_ActionOnDestroy != null)
            {
                m_ActionOnDestroy(item);
                return;
            }
            
            Object.Destroy(item.gameObject);
        }
    }
}