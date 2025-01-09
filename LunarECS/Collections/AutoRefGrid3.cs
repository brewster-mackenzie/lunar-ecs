using LunarECS.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LunarECS.Collections
{
    public delegate void GridCellIterator3<T>(int x, int y, int z, ref T value);

    public struct GridCell3<T> where T : struct
    {
        internal AutoRefGrid3<T> Owner { get; set; }

        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Z { get; internal set; }        

        public ref T GetRef()
        {
            return ref Owner.GetCell(X, Y, Z);
        }
    }

    public class AutoRefGrid3<T> where T : struct
    {
        T[][][] _cells;
        int _capacityX, _capacityY, _capacityZ;
        const int MIN_CAPACITY = 16;

        public int CapacityX => _capacityX;
        public int CapacityY => _capacityY;
        public int CapacityZ => _capacityZ;

        public AutoRefGrid3(int capacityX, int capacityY, int capacityZ)
        {
            _cells = [];

            EnsureCellExists(
                _capacityX = Math.Min(capacityX - 1, MIN_CAPACITY),
                _capacityY = Math.Min(capacityY - 1, MIN_CAPACITY),
                _capacityZ = Math.Min(capacityZ - 1, MIN_CAPACITY)
            );
        }

        public void SetCell(int x, int y, int z, T value)
        {
            EnsureCellExists(x, y, z);
            _cells[x][y][z] = value;
        }

        public ref T GetCell(int x, int y, int z)
        {
            return ref _cells[x][y][z];
        }

        private void EnsureCellExists(int x, int y, int z)
        {
            bool grow = x >= _capacityX || y >= _capacityY || z >= _capacityZ;
            if (grow)
            {
                int w = _capacityX;
                while (x >= _capacityX)
                {                          
                    _capacityX <<= 1;

                    Array.Resize(ref _cells, _capacityX);
                    for(int i = w; i < _capacityX; i++)
                    {
                        _cells[i] = new T[_capacityY][];
                        for (int j = 0; j < _capacityY; j++)
                            _cells[i][j] = new T[_capacityZ];
                    }
                }

                while (y >= _capacityY)
                {
                    int h = _capacityY;
                    _capacityY <<= 1;

                    for(int i = 0; i < _capacityX; i++)
                    {
                        Array.Resize(ref _cells[i], _capacityY);
                        for (int j = h; j < _capacityY; j++)
                            _cells[i][j] = new T[_capacityZ];    
                    }    
                }

                while (z >= _capacityZ)
                {
                    int d = _capacityZ;
                    _capacityZ <<= 1;

                    for (int i = 0; i < _capacityX; i++)
                        for (int j = 0; j < _capacityY; j++)
                            Array.Resize(ref _cells[i][j], _capacityZ);
                }
            }            
        }

        private void ClampRange(ref int x1, ref int y1, ref int z1, ref int x2, ref int y2, ref int z2)
        {
            x1 = Math.Max(0, x1);
            y1 = Math.Max(0, y1);
            z1 = Math.Max(0, z1);
            x2 = Math.Min(_capacityX - 1, x2);
            y2 = Math.Min(_capacityY - 1, y2);
            z2 = Math.Min(_capacityZ - 1, z2);
        }

        public IEnumerable<GridCell3<T>> GetRange(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            GridCell3<T> cell = new();
            cell.Owner = this;

            ClampRange(ref x1, ref y1, ref z1, ref x2, ref y2, ref z2);
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    for (int z = z1; z < z2; z++)
                    {
                        cell.X = x;
                        cell.Y = y;
                        cell.Z = z;
                        yield return cell;
                    }
        }

    }


}
