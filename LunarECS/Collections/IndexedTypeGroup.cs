using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarECS.Collections
{
    class IndexedType
    {
        public int TypeId { get; }
        public ulong TypeFlag { get; }
        public Type Type { get; }
        
        public IndexedType(int typeId, Type type)
        {
            TypeId = typeId;
            TypeFlag = CalculateTypeFlag(typeId);
            Type = type;
        }

        static ulong CalculateTypeFlag(int typeId)
        {
            return 1ul << typeId + 1;
        }
    }

    class IndexedTypeGroup<TCategory>
    {
        static int _count, _capacity;
        static object _lock = new();
        static Func<RecyclableIdCollection>[] _collectionFuncs;
        static IndexedType[] _registered;

        public static int Count => _count;

        public static event EventHandler TypeRegistered;

        static IndexedTypeGroup()
        {
            TypeRegistered = null!;
            _capacity = 16;
            _registered = new IndexedType[_capacity];
            _collectionFuncs = new Func<RecyclableIdCollection>[_capacity];
        }
    
        public static IndexedType Register<T>() where T : struct
        {
            lock (_lock)
            {
                if (_capacity == _count)
                {
                    Array.Resize(ref _registered, _capacity <<= 1);
                    Array.Resize(ref _collectionFuncs, _capacity <<= 1);
                }

                int index = _count++;
                _registered[index] = new IndexedType(index, typeof(T));
                _collectionFuncs[index] = () => new RecyclableIdCollection<T>();
                TypeRegistered?.Invoke(null, EventArgs.Empty);
                return _registered[index];
            }
        }

        public static RecyclableIdCollection CreateDataPool(int typeId)
        {
            return _collectionFuncs[typeId]();
        }

        internal static IndexedType GetRegisteredType(int typeId)
        {
            return _registered[typeId];
        }

        public class IndexedType<T> where T : struct
        {
            public static int TypeId { get; }
            public static ulong TypeFlag { get; }

            static IndexedType()
            {
                var registered = Register<T>();
                TypeId = registered.TypeId;
                TypeFlag = registered.TypeFlag;
            }
        }
    }

  

}
