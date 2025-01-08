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
    internal class EntityManager
    {
        readonly IndexedTypeCollections<EntityComponent> _dataPools;
        readonly RecyclableIdCollection<EntityData> _entityPool;
        readonly HashSet<int> _liveEntities;
        readonly Entity _nullEntity;
        readonly Dictionary<Type, EntityFilter> _filtersByType;
        readonly Dictionary<int, List<EntityFilter>> _filtersByCompTypeId;
        Entity[] _entities;

        public EntityManager()
        {
            _dataPools = new();
            _entityPool = new();
            _liveEntities = new();
            _filtersByType = new();
            _filtersByCompTypeId = new();
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
                    NotifyFiltersEntityDeleted(ref entityData, i);
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

            NotifyFiltersEntityChanged(ref entityData, componentTypeId);
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
                NotifyFiltersEntityChanged(ref entityData, componentTypeId);
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

        public EntityFilterWithCache<T1> CreateFilter<T1>() where T1 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1>>();
        }

        public EntityFilterWithCache<T1> CreateFilterExcl<T1, TExcl1>() where T1 : struct where TExcl1 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1>.Excl<TExcl1>>();
        }

        public EntityFilterWithCache<T1, T2> CreateFilter<T1, T2>() where T1 : struct where T2 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1, T2>>();
        }

        public EntityFilterWithCache<T1, T2> CreateFilterExcl<T1, T2, TExcl1>() where T1 : struct where T2 : struct where TExcl1 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1, T2>.Excl<TExcl1>>();
        }

        public EntityFilterWithCache<T1, T2, T3> CreateFilter<T1, T2, T3>() where T1 : struct where T2 : struct where T3 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1, T2, T3>>();
        }

        public EntityFilterWithCache<T1, T2, T3> CreateFilterExcl<T1, T2, T3, TExcl1>() where T1 : struct where T2 : struct where T3 : struct where TExcl1 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1, T2, T3>.Excl<TExcl1>>();
        }

        public EntityFilterWithCache<T1, T2, T3, T4> CreateFilter<T1, T2, T3, T4>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1, T2, T3, T4>>();
        }

        public EntityFilterWithCache<T1, T2, T3, T4> CreateFilterExcl<T1, T2, T3, T4, TExcl1>() where T1 : struct where T2 : struct where T3 : struct where T4 : struct where TExcl1 : struct
        {
            return InternalCreateFilter<EntityFilterWithCache<T1, T2, T3, T4>.Excl<TExcl1>>();
        }


        private T InternalCreateFilter<T>() where T : EntityFilter, new()
        {
            if (_filtersByType.TryGetValue(typeof(T), out EntityFilter? found))
            {
                found.IncreaseHandles();
                return (T)found;
            }


            var filter = new T();

            filter.Initialize(this);
            for (int i = 0; i < filter.IncludeComponentTypeIdCount; i++)
            {
                if (!_filtersByCompTypeId.TryGetValue(filter.IncludeComponentTypeIds[i], out List<EntityFilter>? listByTypeId))
                    _filtersByCompTypeId.Add(filter.IncludeComponentTypeIds[i], listByTypeId = new());

                listByTypeId.Add(filter);
            }

            for (int i = 0; i < filter.ExcludeComponentTypeIdCount; i++)
            {
                if (!_filtersByCompTypeId.TryGetValue(filter.ExcludeComponentTypeIds[i], out List<EntityFilter>? listByTypeId))
                    _filtersByCompTypeId.Add(filter.ExcludeComponentTypeIds[i], listByTypeId = []);

                listByTypeId.Add(filter);
            }

            _filtersByType.Add(typeof(T), filter);

            filter.ApplyInitialFilter(_liveEntities);
            return filter;
        }

        internal void DeleteFilter(EntityFilter filter)
        {
            filter.DecreaseHandles();
            if (filter.Handles == 0)
            {
                _filtersByType.Remove(filter.GetType());
                for (int i = 0; i < filter.IncludeComponentTypeIdCount; i++)
                    _filtersByCompTypeId[filter.IncludeComponentTypeIds[i]].Remove(filter);
            }
        }

        private void NotifyFiltersEntityChanged(ref EntityData entity, int componentTypeId)
        {
            if (_filtersByCompTypeId.TryGetValue(componentTypeId, out List<EntityFilter>? filters))
                foreach (var filter in filters)
                    filter.NotifyEntityChanged(ref entity);
        }

        private void NotifyFiltersEntityDeleted(ref EntityData entity, int componentTypeId)
        {
            if (_filtersByCompTypeId.TryGetValue(componentTypeId, out List<EntityFilter>? filters))
                foreach (var filter in filters)
                    filter.NotifyEntityDeleted(ref entity);
        }

        #endregion             
    }



}
