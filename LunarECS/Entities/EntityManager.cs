using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;
using LunarECS.Collections;
using LunarECS.Entities;

namespace LunarECS.Entities
{

    public class EntityManager
    {
        readonly IndexedTypeCollections<EntityComponent> _dataPools;
        readonly RecyclableIdCollection<EntityData> _entityPool;
        readonly HashSet<int> _liveEntities;
        readonly Entity _nullEntity;
        readonly Dictionary<Type, EntitySubscriber> _subscribersByType;
        readonly Dictionary<int, List<EntitySubscriber>> _subscribersByCompId;
        Entity[] _entities;

        public EntityManager()
        {
            _dataPools = new();
            _entityPool = new();
            _liveEntities = new();
            _subscribersByType = new();
            _subscribersByCompId = new();
            _entities = new Entity[16];


            _nullEntity = CreateEntity();
            Debug.Assert(_nullEntity.Id == 0);
        }

        #region Entities

        public Entity this[int entityId] => GetEntity(entityId);

        public Entity CreateEntity()
        {
            int entityId = _entityPool.Reserve();

            ref EntityData entityData = ref _entityPool.GetRef(entityId);
            entityData.Id = entityId;
            entityData.Flags = 1ul;
            entityData.Cids = new int[IndexedTypeGroup<EntityComponent>.Count];
            for (int i = 0; i < IndexedTypeGroup<EntityComponent>.Count; i++)
                entityData.Cids[i] = -1;

            if (_entityPool.Capacity > _entities.Length)
                Array.Resize(ref _entities, _entityPool.Capacity);

            ref Entity entity = ref _entities[entityId];
            entity.Owner = this;
            entity.Id = entityId;
            entity.Gen++;
            entityData.Gen = entity.Gen;

            _liveEntities.Add(entityId);

            return entity;
        }

        public Entity GetEntity(int entityId)
        {
            return _entities[entityId];
        }

        internal void DeleteEntity(Entity entity)
        {
            ref EntityData entityData = ref _entityPool.GetRef(entity.Id);

            Debug.Assert(entityData.Gen == entity.Gen);

            for (int i = 0; i < IndexedTypeGroup<EntityComponent>.Count; i++)
            {
                if (entityData.Cids[i] >= 0)
                {
                    _dataPools.Get(i).Release(entityData.Cids[i]);
                    entityData.Cids[i] = -1;
                    NotifySubscribersEntityDeleted(ref entityData, i);
                }
            }

            entityData.Flags = 0ul;
            _entityPool.Release(entity.Id);
            _liveEntities.Remove(entity.Id);
        }

        internal ref EntityData GetEntityData(int entityId)
        {
            return ref _entityPool.GetRef(entityId);
        }

        internal RecyclableIdCollection<T> GetComponentPool<T>() where T : struct
        {
            return _dataPools.Get<T>();
        }

        private ref EntityData GetEntityData(int entityId, int genId)
        {
            ref EntityData entityData = ref _entityPool.GetRef(entityId);
            Debug.Assert(entityData.Gen == genId);
            return ref entityData;
        }

        #endregion

        #region Components

        private bool HasComponent(ref EntityData entityData, ulong componentTypeFlag)
        {
            return (entityData.Flags & componentTypeFlag) > 0;
        }

        private void AddComponent(ref EntityData entityData, int componentTypeId, ulong componentTypeFlag)
        {
            if (entityData.CidSize < IndexedTypeGroup<EntityComponent>.Count)
                Array.Resize(ref entityData.Cids, entityData.CidSize = IndexedTypeGroup<EntityComponent>.Count);

            entityData.Cids[componentTypeId] = _dataPools.Get(componentTypeId).Reserve();
            entityData.Flags |= componentTypeFlag;

            NotifySubscribersEntityChanged(ref entityData, componentTypeId);
        }

        private void RemoveComponent(ref EntityData entityData, int componentTypeId, ulong componentTypeFlag)
        {
            _dataPools.Get(componentTypeId).Release(entityData.Cids[componentTypeId]);
            entityData.Cids[componentTypeId] = -1;
            entityData.Flags &= ~componentTypeFlag;
        }

        private ref T GetComponent<T>(ref EntityData entityData, int componentTypeId) where T : struct
        {
            return ref _dataPools.Get<T>(componentTypeId).GetRef(entityData.Cids[componentTypeId]);
        }

        private void SetComponent<T>(ref EntityData entityData, int componentTypeId, T value) where T : struct
        {
            _dataPools.Get<T>(componentTypeId).Set(entityData.Cids[componentTypeId], value);
        }

        internal bool AddComponent<T>(Entity entity) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeFlag;
            ref var entityData = ref GetEntityData(entity.Id, entity.Gen);

            if (HasComponent(ref entityData, componentTypeFlag))
                return false;

            AddComponent(ref entityData, componentTypeId, componentTypeFlag);
            return true;
        }

        internal bool RemoveComponent<T>(Entity entity) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeFlag;
            ref var entityData = ref GetEntityData(entity.Id, entity.Gen);

            if (HasComponent(ref entityData, componentTypeFlag))
            {

                RemoveComponent(ref entityData, componentTypeId, componentTypeFlag);
                NotifySubscribersEntityChanged(ref entityData, componentTypeId);
                return true;
            }

            return false;
        }

        internal ref T GetComponent<T>(Entity entity) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeId;
            ref var entityData = ref GetEntityData(entity.Id, entity.Gen);
            return ref GetComponent<T>(ref entityData, componentTypeId);
        }

        internal ref T GetOrAddComponent<T>(Entity entity) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeFlag;
            RecyclableIdCollection<T> pool = _dataPools.Get<T>(componentTypeId);

            ref EntityData entityData = ref GetEntityData(entity.Id, entity.Gen);

            if (!HasComponent(ref entityData, componentTypeFlag))
                AddComponent(ref entityData, componentTypeId, componentTypeFlag);

            return ref GetComponent<T>(ref entityData, componentTypeId);
        }

        internal bool HasComponent<T>(Entity entity) where T : struct
        {
            return HasComponent(ref GetEntityData(entity.Id, entity.Gen), IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeFlag);
        }

        internal void SetComponent<T>(Entity entity, T value) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<EntityComponent>.IndexedType<T>.TypeFlag;

            ref EntityData entityData = ref GetEntityData(entity.Id, entity.Gen);
            if (!HasComponent(ref entityData, componentTypeFlag))
                AddComponent(ref entityData, componentTypeId, componentTypeFlag);
            SetComponent(ref entityData, componentTypeId, value);
        }


        #endregion

        #region Filtering

        public T Subscribe<T>() where T : EntitySubscriber, new()
        {
            if (_subscribersByType.TryGetValue(typeof(T), out EntitySubscriber? found))
            {
                found.IncreaseHandles();
                return (T)found;
            }


            var subscriber = new T();

            subscriber.Initialize(this);
            for (int i = 0; i < subscriber.IncludeComponentTypeIdCount; i++)
            {
                if (!_subscribersByCompId.TryGetValue(subscriber.IncludeComponentTypeIds[i], out List<EntitySubscriber>? listByTypeId))
                    _subscribersByCompId.Add(subscriber.IncludeComponentTypeIds[i], listByTypeId = new());

                listByTypeId.Add(subscriber);
            }

            for (int i = 0; i < subscriber.ExcludeComponentTypeIdCount; i++)
            {
                if (!_subscribersByCompId.TryGetValue(subscriber.ExcludeComponentTypeIds[i], out List<EntitySubscriber>? listByTypeId))
                    _subscribersByCompId.Add(subscriber.ExcludeComponentTypeIds[i], listByTypeId = []);

                listByTypeId.Add(subscriber);
            }

            _subscribersByType.Add(typeof(T), subscriber);

            subscriber.ApplyInitialFilter(_liveEntities);
            return subscriber;
        }

        internal void DeleteSubscriber(EntitySubscriber subscriber)
        {
            subscriber.DecreaseHandles();
            if (subscriber.Handles == 0)
            {
                _subscribersByType.Remove(subscriber.GetType());
                for (int i = 0; i < subscriber.IncludeComponentTypeIdCount; i++)
                    _subscribersByCompId[subscriber.IncludeComponentTypeIds[i]].Remove(subscriber);
            }
        }

        private void NotifySubscribersEntityChanged(ref EntityData entity, int componentTypeId)
        {
            if (_subscribersByCompId.TryGetValue(componentTypeId, out List<EntitySubscriber>? subscribers))
                foreach (var subscriber in subscribers)
                    subscriber.NotifyEntityChanged(ref entity);
        }

        private void NotifySubscribersEntityDeleted(ref EntityData entity, int componentTypeId)
        {
            if (_subscribersByCompId.TryGetValue(componentTypeId, out List<EntitySubscriber>? subscribers))
                foreach (var subscriber in subscribers)
                    subscriber.NotifyEntityDeleted(ref entity);
        }

        #endregion             
    }



}
