using LunarECS.Collections;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace LunarECS.Tiles3
{

    public class TileManager3
    {
        readonly IndexedTypeCollections<TileComponent> _componentPools;
        readonly Grid3<Tile3Data> _tileDataGrid;

        public TileManager3()
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

        public void DeleteTile(int x, int y, int z)
        {
            _tileDataGrid.SetCell(x, y, z, default);            
        }

        public bool HasTileComponent<T>(int x, int y, int z) where T : struct
        {
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;
            if (x < 0 || x >= _tileDataGrid.CapacityX || y < 0 || y >= _tileDataGrid.CapacityY || z < 0 || z >= _tileDataGrid.CapacityZ)
                return false;
            return (_tileDataGrid.GetCell(x, y, z).Flags & componentTypeFlag) > 0ul;
        }

        public ref T GetTileComponent<T>(int x, int y, int z) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            return ref _componentPools.Get<T>(componentTypeId).GetRef(_tileDataGrid.GetCell(x, y, z).Cids[componentTypeId]);
        }

        public void AddTileComponent<T>(int x, int y, int z) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;

            int componentId = _componentPools.Get(componentTypeId).Reserve();
            ref var tileData = ref _tileDataGrid.GetCell(x, y, z);
            if (tileData.CidSize < IndexedTypeGroup<TileComponent>.Count)
                Array.Resize(ref tileData.Cids, tileData.CidSize = IndexedTypeGroup<TileComponent>.Count);
            tileData.Cids[componentTypeId] = componentId;
            tileData.Flags |= componentTypeFlag;
        }

        public void RemoveTileComponent<T>(int x, int y, int z) where T : struct
        {
            int componentTypeId = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeId;
            ulong componentTypeFlag = IndexedTypeGroup<TileComponent>.IndexedType<T>.TypeFlag;
            ref var tileData = ref _tileDataGrid.GetCell(x, y, z);
            int componentId = tileData.Cids[componentTypeId];
            _componentPools.Get(componentTypeId).Release(componentId);
            tileData.Cids[componentId] = 0;
            tileData.Flags &= ~componentTypeFlag;
        }
    }

}
