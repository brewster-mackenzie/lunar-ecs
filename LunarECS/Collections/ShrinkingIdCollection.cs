using System.Collections.Generic;
using System;
using System.Collections;

namespace LunarECS.Collections
{
    internal class ShrinkingIdCollection<T> : IIdManagedCollection<T>
    {
        T[] _items;
        int _count, _capacity;
        readonly Dictionary<int, int> _idToIndex = new();
        readonly Dictionary<int, int> _indexToId = new();

        public ShrinkingIdCollection()
        {
            _items = new T[_capacity = 16];
        }

        public int Capacity => _capacity;
        public int Count => _count;

        public T Get(int managedId)
        {
            return _items[_idToIndex[managedId]];
        }

        public ref T GetRef(int managedId)
        {
            return ref _items[_idToIndex[managedId]];
        }

        public ref T GetRefByIndex(int index)
        {
            return ref _items[index];
        }

        public int Reserve()
        {
            if (_count == _capacity)
                Array.Resize(ref _items, _capacity <<= 1);

            int index = _count++;
            _idToIndex.Add(index, index);
            _indexToId.Add(index, index);

            return index;
        }

        public int Reserve(T value)
        {
            int index = Reserve();
            _items[index] = value;
            return index;
        }

        public void Release(int managedId)
        {
            int swapIndex = --_count;
            int swapId = _indexToId[swapIndex];
            int index = _idToIndex[managedId];

            _items[index] = _items[swapIndex];
            _idToIndex[swapId] = index;
            _indexToId[index] = swapId;
            _indexToId.Remove(swapIndex);
            _idToIndex.Remove(managedId);
        }

        public void Set(int managedId, T value)
        {
            _items[_idToIndex[managedId]] = value;
        }
    }
}
