namespace LunarECS.Collections
{
    public class IndexedHashSet<T> where T : struct
    {
        T[] _items;
        int _count, _capacity;
        Dictionary<T, int> _valueToIndex = new();

        public T this[int index] => _items[index];
        public int Count => _count;
        public int Capacity => _capacity;

        public IndexedHashSet()
        {
            _items = new T[_capacity = 16];
        }

        public bool Add(T value)
        {
            if (_valueToIndex.ContainsKey(value))
                return false;

            if (_capacity == _count)
                Array.Resize(ref _items, _capacity <<= 1);
            int index = _count++;
            _items[index] = value;
            _valueToIndex.Add(value, index);
            return true;
        }
        public bool Contains(T value, out int index)
        {
            return _valueToIndex.TryGetValue(value, out index);
        }

        public bool Remove(T value)
        {
            if (!_valueToIndex.TryGetValue(value, out int index))
                return false;

            _valueToIndex.Remove(_items[index]);

            int swapIndex = --_count;
            if (swapIndex > 0 && swapIndex != index)
            {                
                _items[index] = _items[swapIndex];
                _valueToIndex[_items[index]] = index;
            }

            return true;
        }

    }
}
