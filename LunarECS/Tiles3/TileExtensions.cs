namespace LunarECS.Tiles3
{
    static class TileExtensions
    {
        public static void Delete(this Tile3 tile)
        {
            tile.Owner.DeleteTile(tile.X, tile.Y, tile.Z);
        }

        public static void Add<T>(this Tile3 tile) where T : struct
        {
            tile.Owner.AddTileComponent<T>(tile.X, tile.Y, tile.Z);
        }

        public static ref T Get<T>(this Tile3 tile) where T : struct
        {
            return ref tile.Owner.GetTileComponent<T>(tile.X, tile.Y, tile.Z);
        }
    }







}
