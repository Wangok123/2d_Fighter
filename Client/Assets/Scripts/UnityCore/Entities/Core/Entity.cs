using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.ReferencePool;

namespace UnityCore.Entities.Core
{
    public class Entity : IReference
    {
        public Guid ID { get; } // 唯一ID
        public Entity Parent { get; set; }
        public List<Entity> Children { get; } = new();

        private Dictionary<Type, Component> _components = new();
        
        public Entity()
        {
            ID = Guid.NewGuid(); // 使用GUID生成唯一ID
        }

        // 添加组件（带生命周期）
        public T AddComponent<T>() where T : Component, new()
        {
            if (_components.ContainsKey(typeof(T)))
                throw new Exception($"Component {typeof(T)} already exists!");

            var component = new T();
            component.Entity = this;
            _components.Add(typeof(T), component);
            component.Awake();
            return component;
        }
        
        public T AddComponent<T>(T component) where T : Component
        {
            if (_components.ContainsKey(typeof(T)))
                throw new Exception($"Component {typeof(T)} already exists!");

            component.Entity = this;
            _components.Add(typeof(T), component);
            component.Awake();
            return component;
        }

        // 获取组件
        public T GetComponent<T>() where T : Component
        {
            if (_components.TryGetValue(typeof(T), out var component))
                return (T)component;
            
            return null;
        }

        // 发送事件
        public void PublishEvent<T>(T eventData) where T : struct 
        {
            foreach (var component in _components.Values)
            {
                if (component is IEventReceiver<T> receiver)
                    receiver.HandleEvent(eventData);
            }
        }

        public void Clear()
        {
            foreach (var component in _components.Values)
            {
                component.Destroy();
            }
            
            _components.Clear();
            Children.Clear();
            Parent = null;
        }

        public List<Component> GetAllComponents()
        {
            return _components.Values.ToList();
        }
    }
}