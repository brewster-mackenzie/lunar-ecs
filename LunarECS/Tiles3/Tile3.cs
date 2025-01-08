namespace LunarECS.Tiles3
{
    public struct Tile3
    {
        internal TileManager3 Owner;
        internal int X;
        internal int Y;
        internal int Z;

        public Tile3(TileManager3 owner, int x, int y, int z)
        {
            Owner = owner;
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct Tile3Data
    {
        internal ulong Flags;
        internal int[] Cids;
        internal int CidSize;
    }

}
