using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunarECS.Tiles;

namespace LunarECS.Collections
{
    class IndexedTypeCollections<TCategory>
    {
        RecyclableIdCollection[] _collections;
        int _count;

        public IndexedTypeCollections()
        {
            _collections = [];
            RefreshCollectionsArray();
            IndexedTypeGroup<TCategory>.TypeRegistered += TypedCollectionIndexer_TypeRegistered;
        }

        private void TypedCollectionIndexer_TypeRegistered(object? sender, EventArgs e)
        {
            RefreshCollectionsArray();
        }
 
        public RecyclableIdCollection Get(int typeId)
        {
            return _collections[typeId];
        }

        public RecyclableIdCollection<T> Get<T>(int typeId) where T : struct
        {
            return (RecyclableIdCollection<T>)_collections[typeId];
        }

        public RecyclableIdCollection<T> Get<T>() where T : struct
        {
            return (RecyclableIdCollection<T>)_collections[IndexedTypeGroup<TCategory>.IndexedType<T>.TypeId];
        }

        private void RefreshCollectionsArray()
        {
            int typeCount = IndexedTypeGroup<TCategory>.Count;
            if (_count < typeCount)
            {
                Array.Resize(ref _collections, typeCount);
                for (int i = _count; i < typeCount; i++)
                    _collections[i] = IndexedTypeGroup<TCategory>.CreateDataPool(i);
                _count = IndexedTypeGroup<TCategory>.Count;
            }
        }
    }


}
