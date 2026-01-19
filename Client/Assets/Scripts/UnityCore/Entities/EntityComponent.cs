using System;
using System.Collections.Generic;
using System.Linq;
using Core.ReferencePool;
using UnityCore.Base;
using UnityCore.Entities.Core;

namespace UnityCore.Entities
{
    public class EntityComponent : LatComponent
    {
        private List<Entity> _entities;
        private List<SystemInfo> _systems;
        private Dictionary<Type, Entity> _singletonEntities;
        private bool _isDirty;

        private struct SystemInfo
        {
            public ISystem System;
            public int Priority;
        }

        protected override void Awake()
        {
            base.Awake();
            _entities = new List<Entity>();
            _systems = new List<SystemInfo>();
            _singletonEntities = new Dictionary<Type, Entity>();
            _isDirty = false;
            IsInit = true;
        }

        public Entity CreateEntity(Entity parent = null)
        {
            var entity = ReferencePool.Acquire<Entity>();
            _entities.Add(entity);

            if (parent != null)
            {
                parent.Children.Add(entity);
                entity.Parent = parent;
            }

            return entity;
        }
        
        public Entity GetSingletonEntity<T>() where T : Entity
        {
            var type = typeof(T);
            if (_singletonEntities.TryGetValue(type, out var entity))
            {
                return entity;
            }

            entity = CreateEntity();
            _singletonEntities[type] = entity;
            return entity;
        }
        
        public void AddSystem(ISystem system, int priority = 0)
        {
            if (_systems.Any(s => s.System == system))
            {
                return;
            }

            _systems.Add(new SystemInfo { System = system, Priority = priority });
            _isDirty = true;
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.RemoveAll(s => s.System == system);
        }

        private void SortSystems()
        {
            if (!_isDirty) return;
            
            _systems.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            _isDirty = false;
        }

        public void Update()
        {
            SortSystems();
            
            foreach (var systemInfo in _systems)
            {
                systemInfo.System.Update();
            }
        }

        public void FixedUpdate()
        {
            SortSystems();
            
            foreach (var systemInfo in _systems)
            {
                if (systemInfo.System is IFixedUpdateSystem fixedUpdateSystem)
                {
                    fixedUpdateSystem.FixedUpdate();
                }
            }
        }
        
        public void DestroyEntity(Entity entity)
        {
            if (entity.Parent != null)
            {
                entity.Parent.Children.Remove(entity);
            }

            foreach (var child in entity.Children.ToList())
            {
                DestroyEntity(child);
            }

            _entities.Remove(entity);
            entity.Clear();
            ReferencePool.Release(entity);
        }
        
        public void Clear()
        {
            foreach (var entity in _entities.ToList())
            {
                DestroyEntity(entity);
            }
            _entities.Clear();
            _systems.Clear();
            _singletonEntities.Clear();
        }
        
        public IReadOnlyList<Entity> GetAllEntities()
        {
            return _entities;
        }
        
        public IEnumerable<Entity> GetEntitiesWith<T>() where T : Component, new()
        {
            foreach (var entity in _entities)
            {
                if (entity.GetComponent<T>() != null)
                {
                    yield return entity;
                }
            }
        }

        public IEnumerable<Entity> GetEntitiesWithAll(params Type[] componentTypes)
        {
            foreach (var entity in _entities)
            {
                bool hasAll = true;
                foreach (var type in componentTypes)
                {
                    if (entity.GetComponent(type) == null)
                    {
                        hasAll = false;
                        break;
                    }
                }

                if (hasAll)
                {
                    yield return entity;
                }
            }
        }
    }
}