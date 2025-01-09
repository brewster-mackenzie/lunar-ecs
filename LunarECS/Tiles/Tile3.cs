namespace LunarECS.Tiles
{
    public struct Tile3
    {        
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        internal Tile3Manager Owner { get; set; }

        internal Tile3(Tile3Manager owner, int x, int y, int z)
        {
            Owner = owner;
            X = x;
            Y = y;
            Z = z;
        }
    }

}
