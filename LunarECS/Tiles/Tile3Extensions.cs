namespace LunarECS.Tiles
{
    static class Tile3Extensions
    {
        public static void Delete(this Tile3 tile)
        {
            tile.Owner.DeleteTile(tile.X, tile.Y, tile.Z);
        }

        public static void Add<T>(this Tile3 tile) where T : struct
        {
            tile.Owner.AddComponent<T>(tile.X, tile.Y, tile.Z);
        }

        public static ref T Get<T>(this Tile3 tile) where T : struct
        {
            return ref tile.Owner.GetComponent<T>(tile.X, tile.Y, tile.Z);
        }

        public static ref T GetOrAdd<T>(this Tile3 tile) where T : struct
        {
            return ref tile.Owner.GetOrAddTileComponent<T>(tile.X, tile.Y, tile.Z);
        }

        public static void Remove<T>(this Tile3 tile) where T : struct
        {
            tile.Owner.RemoveComponent<T>(tile.X, tile.Y, tile.Z);
        }

        public static void Set<T>(this Tile3 tile, T value) where T : struct
        {
            tile.Owner.SetComponent<T>(tile.X, tile.Y, tile.Z, value);
        }
    }
}
