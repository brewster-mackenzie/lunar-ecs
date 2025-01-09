using LunarECS.Collections;
using LunarECS.Entities;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace LunarECS.Tiles
{

    public class Tile3Manager
    {
        readonly IndexedTypeCollections<TileComponent> _componentPools;
        readonly AutoRefGrid3<Tile3Data> _tileDataGrid;

        public Tile3Manager()
        {
            _componentPools = new();
            _tileDataGrid = new(8, 8, 8);
        }

        public Tile3 CreateTile(int x, int y, int z)
        {
            _tileDataGrid.SetCell(x, y, z, new Tile3Data()
            {
                Cids = new int[4],
                CidSize = 4,
                Flags = 1ul
            });

            return new Tile3(this, x, y, z);
        }

        public Tile3 GetTile(int x, int y, int z)
        {
            return new Tile3(this, x, y, z);
        }

        internal void DeleteTile(int x, int y, int z)
        {
            _tileDataGrid.SetCell(x, y, z, default);
        }

        private bool HasComponent(ref Tile3Data tileData, ulong componentTypeFlag)
        {
            return (tileData.Flags & componentTypeFlag) > 0;
        }

        private void AddComponent(ref Tile3Data tileData, int componentTypeId, ulong componentTypeFlag)
        {
            if (tileData.CidSize < IndexedTypeGroup<EntityComponent>.Count)
                Array.Resize(ref tileData.Cids, tileData.CidSize = IndexedTypeGroup<TileComponent>.Count);

            tileData.Cids[componentTypeId] = _componentPools.Get(componentTypeId).Reserve();
            tileData.Flags |= componentTypeFlag;
        }

        private void RemoveComponent(ref Tile3Data tileData, int componentTypeId, ulong componentTypeFlag)
        {
            _componentPools.Get(componentTypeId).Release(tileData.Cids[componentTypeId]);
            tileData.Cids[componentTypeId] = -1;
            tileData.Flags &= ~componentTypeFlag;
        }

        private ref T GetComponent<T>(ref Tile3Data tileData, int componentTypeId) where T : struct
        {
            return ref _componentPools.Get<T>(componentTypeId).GetRef(tileData.Cids[componentTypeId]);
        }

        private void SetComponent<T>(ref Tile3Data tileData, int componentTypeId, T value) where T : struct
        {
            _componentPools.Get<T>(componentTypeId).Set(tileData.Cids[componentTypeId], value);
        }


        internal bool HasComponent<T>(int x, int y, int z) where T : struct
        {
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;
            if (x < 0 || x >= _tileDataGrid.CapacityX || y < 0 || y >= _tileDataGrid.CapacityY || z < 0 || z >= _tileDataGrid.CapacityZ)
                return false;
            ref Tile3Data tileData = ref _tileDataGrid.GetCell(x, y, z);
            return HasComponent(ref tileData, componentTypeFlag);
        }

        public ref T GetComponent<T>(int x, int y, int z) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            ref Tile3Data tileData = ref _tileDataGrid.GetCell(x, y, z);
            return ref GetComponent<T>(ref tileData, componentTypeId);
        }

        internal void AddComponent<T>(int x, int y, int z) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;
            ref var tileData = ref _tileDataGrid.GetCell(x, y, z);
            AddComponent(ref tileData, componentTypeId, componentTypeFlag);
        }

        internal void RemoveComponent<T>(int x, int y, int z) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;
            ref var tileData = ref _tileDataGrid.GetCell(x, y, z);
            RemoveComponent(ref tileData, componentTypeId, componentTypeFlag);
        }

        internal ref T GetOrAddTileComponent<T>(int x, int y, int z) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;
            RecyclableIdCollection<T> pool = _componentPools.Get<T>(componentTypeId);

            ref Tile3Data tileData = ref _tileDataGrid.GetCell(x, y, z);
            if (!HasComponent(ref tileData, componentTypeFlag))
                AddComponent(ref tileData, componentTypeId, componentTypeFlag);

            return ref GetComponent<T>(ref tileData, componentTypeId);
        }

        internal void SetComponent<T>(int x, int y, int z, T value) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;

            ref Tile3Data tileData = ref _tileDataGrid.GetCell(x, y, z);
            if (!HasComponent(ref tileData, componentTypeFlag))
                AddComponent(ref tileData, componentTypeId, componentTypeFlag);
            SetComponent(ref tileData, componentTypeId, value);
        }
    }

}
