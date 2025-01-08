using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace LunarECS.Collections
{
    public interface IIdManagedCollection
    {
        int Count { get; }
        int Capacity { get; }
        int Reserve();
        void Release(int managedId);
    }

    public interface IIdManagedCollection<T> : IIdManagedCollection
    {
        T Get(int managedId);
        ref T GetRef(int managedId);
        int Reserve(T value);
        void Set(int managedId, T value);
    }
}
