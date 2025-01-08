namespace LunarECS.Collections
{

    public abstract class RecyclableIdCollection : IIdManagedCollection
    {
        public abstract int Count { get; }
        public abstract int Capacity { get; }

        public abstract void Release(int managedId);
        public abstract int Reserve();
    }

    public class RecyclableIdCollection<T> : RecyclableIdCollection, IIdManagedCollection<T> 
    {
        T[] _items;
        int _count, _capacity;
        readonly Queue<int> _queued;

        public RecyclableIdCollection()
        {
            _items = new T[_capacity = 64];
            _queued = new();
        }

        public override int Capacity => _capacity;
        public override int Count => _count;

        public override int Reserve()
        {
            if (_queued.TryDequeue(out int managedId))
            {
                _items[managedId] = default!;
                return managedId;
            }

            if (_count == _capacity)
                Array.Resize(ref _items, _capacity <<= 1);
            return _count++;
        }

        public int Reserve(T value)
        {
            int managedId = Reserve();
            _items[managedId] = value; // TODO not threadsafe as id->index could change, but this is quicker.
            return managedId;
        }

        public override void Release(int managedId)
        {
            _queued.Enqueue(managedId);
        }

        public T Get(int managedId)
        {
            return _items[managedId];
        }

        public ref T GetRef(int managedId)
        {
            return ref _items[managedId];
        }

        public void Set(int managedId, T value)
        {
            _items[managedId] = value;
        }
    }

}
