using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LunarECS.Collections
{
    public class EnumIndexedArray<TEnum, TItem> 
        where TEnum : Enum
    {
        static readonly int _lower;
        static readonly int _upper;
        static readonly int _capacity;
        readonly TItem[] _array;
        
        static EnumIndexedArray()
        {
            _lower = Convert.ToInt32(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Min());
            _upper = Convert.ToInt32(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Max());
            _capacity = 1 + _upper - _lower;
        }

        public EnumIndexedArray()
        {           
            _array = new TItem[_capacity];
        }

        public int Lower => _lower;
        public int Upper => _upper;

        public TItem this[TEnum index]
        {
            get
            {
                return _array[Convert.ToInt32(index) - _lower];
            }
        }
    }
}
