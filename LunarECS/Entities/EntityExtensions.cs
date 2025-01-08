namespace LunarECS.Entities
{
    public static class EntityExtensions
    {
        public static void Delete(this Entity entity)
        {
            entity.Owner.DeleteEntity(entity);
            entity.Id = -1;
            entity.Owner = null!;
        }

        public static bool Add<T>(this Entity entity) where T : struct
        {
            return entity.Owner.AddComponent<T>(entity);
        }

        public static bool Has<T>(this Entity entity) where T : struct
        {
            return entity.Owner.HasComponent<T>(entity);
        }


        public static ref T GetOrAdd<T>(this Entity entity) where T : struct
        {
            return ref entity.Owner.GetOrAddComponent<T>(entity);
        }

        public static void Set<T>(this Entity entity, T value) where T : struct
        {
            entity.Owner.SetComponent(entity, value);
        }

        public static ref T Get<T>(this Entity entity) where T : struct
        {
            return ref entity.Owner.GetComponent<T>(entity);
        }

        public static void Remove<T>(this Entity entity) where T : struct
        {
            entity.Owner.RemoveComponent<T>(entity);
        }
    }



}
