using System;
using System.Collections.Generic;
using System.Linq;
using Core.ReferencePool;

namespace UnityCore.Entities.Core
{
    public class Entity : IReference
    {
        public Guid ID { get; }
        public Entity Parent { get; set; }
        public List<Entity> Children { get; } = new();
        public bool IsActive { get; set; } = true;

        private Dictionary<Type, Component> _components = new();
        
        public Entity()
        {
            ID = Guid.NewGuid();
        }

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

        public T GetComponent<T>() where T : Component
        {
            if (_components.TryGetValue(typeof(T), out var component))
                return (T)component;
            
            return null;
        }

        public Component GetComponent(Type type)
        {
            _components.TryGetValue(type, out var component);
            return component;
        }

        public bool HasComponent<T>() where T : Component
        {
            return _components.ContainsKey(typeof(T));
        }

        public bool HasComponent(Type type)
        {
            return _components.ContainsKey(type);
        }

        public bool RemoveComponent<T>() where T : Component
        {
            if (_components.TryGetValue(typeof(T), out var component))
            {
                component.Destroy();
                _components.Remove(typeof(T));
                return true;
            }
            return false;
        }

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
            IsActive = true;
        }

        public List<Component> GetAllComponents()
        {
            return _components.Values.ToList();
        }
    }
}