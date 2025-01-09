using LunarECS.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
//using System.Diagnostics.CodeAnalysis;
//using System.Text;


namespace LunarECS.Entities
{

    public abstract class EntitySubscriber
    {
        readonly Dictionary<int, int> _entityIdMap;

        int[] _entities;
        int _count;
        int _capacity;
        int _lockCount;
        int _handles;

        readonly int _includeComponentTypeIdCount, _excludeComponentTypeIdCount;
        readonly int[] _includeComponentTypeIds, _excludeComponentTypeIds;

        int[] _pending;
        int _pendingCount, _pendingSize;
        EntityManager _entityManager;
        readonly ulong _includeFlags, _excludeFlags;

        public int Capacity => _capacity;
        public int Count => _count;

        internal EntityManager Owner => _entityManager;
        internal int Handles => _handles;

        internal int IncludeComponentTypeIdCount => _includeComponentTypeIdCount;
        internal int[] IncludeComponentTypeIds => _includeComponentTypeIds;
        internal int ExcludeComponentTypeIdCount => _excludeComponentTypeIdCount;
        internal int[] ExcludeComponentTypeIds => _excludeComponentTypeIds;
        internal ulong IncludeFlags => _includeFlags;
        internal ulong ExcludeFlags => _excludeFlags;

        public EntitySubscriber(
            int[] includeComponentTypeIds,
            int[] excludeComponentTypeIds
            )
        {
            _entityManager = null!;
            _includeComponentTypeIds = includeComponentTypeIds;
            _excludeComponentTypeIds = excludeComponentTypeIds;
            _includeComponentTypeIdCount = includeComponentTypeIds.Length;
            _excludeComponentTypeIdCount = excludeComponentTypeIds.Length;
            _capacity = 1200;
            _entities = new int[_capacity];
            _entityIdMap = new();
            _pending = new int[_pendingSize = 16];

            _includeFlags = 1ul; // live only
            for (int i = 0; i < includeComponentTypeIds.Length; i++)
                _includeFlags |= IndexedTypeGroup<EntityComponent>.GetRegisteredType(includeComponentTypeIds[i]).TypeFlag;

            _excludeFlags = 0ul;
            for (int i = 0; i < excludeComponentTypeIds.Length; i++)
                _excludeFlags |= IndexedTypeGroup<EntityComponent>.GetRegisteredType(excludeComponentTypeIds[i]).TypeFlag;
        }

        public void Delete()
        {
            _entityManager.DeleteSubscriber(this);

            if (_handles == 0)
            {
                OnDelete();
                _entityManager = null!;
            }
        }

        public bool Contains(int entityId)
        {
            return _entityIdMap.ContainsKey(entityId);
        }

        public bool Contains(int entityId, out int index)
        {
            return _entityIdMap.TryGetValue(entityId, out index);
        }

        internal virtual void Initialize(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public int GetEntityId(int index)
        {
            return _entities[index];
        }

        public Entity GetEntity(int index)
        {
            return _entityManager.GetEntity(_entities[index]);
        }

        internal void IncreaseHandles()
        {
            _handles++;
        }

        internal bool DecreaseHandles()
        {
            return 0 == --_handles;
        }

        internal void Lock()
        {
            _lockCount++;
        }

        internal void Unlock()
        {
            if (--_lockCount == 0)
                ProcessPending();
        }

        bool AddToPending(int entityId)
        {
            if (_lockCount > 0)
            {
                if (_pendingSize == _pendingCount)
                    Array.Resize(ref _pending, _pendingSize <<= 1);
                _pending[_pendingCount++] = entityId;
                return true;
            }
            return false;
        }

        void ProcessPending()
        {
            UpdateEntities(_pending, _pendingCount);
            _pendingCount = 0;
        }

        void UpdateEntities(int[] entityIds, int count)
        {
            for (int i = 0; i < count; i++)
                Filter(ref _entityManager.GetEntityData(entityIds[i]));
        }

        internal void NotifyEntityChanged(ref EntityData entity)
        {
            if (AddToPending(entity.Id))
                return;

            Filter(ref entity);
        }

        internal void ApplyInitialFilter(IEnumerable<int> entities)
        {
            foreach (int entityId in entities)
            {
                ref EntityData entity = ref _entityManager.GetEntityData(entityId);
                Filter(ref entity);
            }
        }

        void Filter(ref EntityData entity)
        {
            if (Wants(entity.Flags))
                Include(ref entity);
            else
                Exclude(ref entity);
        }

        bool Wants(ulong flags)
        {
            return (flags & _includeFlags) == _includeFlags && (flags & _excludeFlags) == 0ul;
        }

        internal virtual void OnResize(int capacity) { }
        internal virtual void OnInclude(int index, ref EntityData entity) { }
        internal virtual void OnExclude(int index, int swapIndex, ref EntityData entity) { }
        internal virtual void OnDelete() { }

        void Include(ref EntityData entity)
        {
            if (_entityIdMap.ContainsKey(entity.Id))
                return;

            if (_count == _capacity)
            {
                Array.Resize(ref _entities, _capacity <<= 1);
                OnResize(_capacity);
            }

            int index = _count++;
            OnInclude(index, ref entity);

            _entityIdMap.Add(entity.Id, index);
            _entities[index] = entity.Id;
        }

        void Exclude(ref EntityData entity)
        {
            if (_entityIdMap.TryGetValue(entity.Id, out int index))
            {
                _entityIdMap.Remove(entity.Id);

                int swapIndex = --_count;
                if (index == swapIndex)
                    return;

                int swapId = _entities[swapIndex];
                _entityIdMap[swapId] = index;
                _entities[index] = swapId;
                OnExclude(index, swapIndex, ref entity);
            }
        }

        internal void NotifyEntityDeleted(ref EntityData entity)
        {
            Exclude(ref entity);
        }
    }
}

