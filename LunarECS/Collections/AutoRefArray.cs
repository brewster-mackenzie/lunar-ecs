using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LunarECS.Collections
{
    internal class AutoRefArray<T> where T : struct
    {
        T[] _array;
        int _capacity;
        readonly int _growRate;

        const int DEFAULT_CAPACITY = 64;
        const int DEFAULT_GROWRATE = 64;

        public AutoRefArray(int initialCapacity, int growRate)
        {
            _array = new T[initialCapacity];
            _capacity = initialCapacity;
            _growRate = growRate;
        }

        public AutoRefArray(): this(DEFAULT_CAPACITY, DEFAULT_GROWRATE) { }

        public ref T this[int index]
        {
            get
            {
                int count = index + 1;
                if (count >= _capacity)
                    Array.Resize(ref _array, _capacity = count + (_growRate - (count % _growRate)));
                return ref _array[index];
            }
        }
    }
}
