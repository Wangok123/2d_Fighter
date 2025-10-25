using System;
using System.Collections.Generic;
using System.Linq;
using Core.ReferencePool;
using Core.Utils;
using UnityCore.Base;
using UnityCore.Entities.Core;

namespace UnityCore.Entities
{
    public class EntityComponent : LatComponent
    {
        private List<Entity> _entities;
        private List<ISystem> _systems;
        private Dictionary<Type, Entity> _singletonEntities;


        protected override void Awake()
        {
            base.Awake();

            _entities = new List<Entity>();
            _systems = new List<ISystem>();
            _singletonEntities = new Dictionary<Type, Entity>();

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
        
        public void AddSystem(ISystem system)
        {
            if (!_systems.Contains(system))
            {
                _systems.Add(system);
            }
        }

        public void Update()
        {
            foreach (var system in _systems)
            {
                system.Update();
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
            foreach (var entity in _entities)
            {
                DestroyEntity(entity);
            }
            _entities.Clear();
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
    }
}