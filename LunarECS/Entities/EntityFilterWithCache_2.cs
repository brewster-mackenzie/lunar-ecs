using LunarECS.Collections;

namespace LunarECS.Entities
{
    public class EntityFilterWithCache<T1, T2> : EntityFilterWithCache
        where T1 : struct
        where T2 : struct
    {
        RecyclableIdCollection<T1> _t1Pool = null!;
        RecyclableIdCollection<T2> _t2Pool = null!;

        readonly static int[] _includeComponentTypeIds = [
            IndexedTypeGroup<EntityComponent>.IndexedType<T1>.TypeId,
            IndexedTypeGroup<EntityComponent>.IndexedType<T2>.TypeId
        ];

        public class Excl<T3> : EntityFilterWithCache<T1, T2> where T3 : struct
        {
            static readonly int[] _excludeComponentTypeIds = [
                 IndexedTypeGroup<EntityComponent>.IndexedType<T3>.TypeId
            ];

            public Excl() : base(_excludeComponentTypeIds)
            { }
        }

        public class Excl<T3, T4> : EntityFilterWithCache<T3, T4> where T3 : struct where T4 : struct
        {
            static readonly int[] _excludeComponentTypeIds = [
                IndexedTypeGroup<EntityComponent>.IndexedType<T3>.TypeId,
                IndexedTypeGroup<EntityComponent>.IndexedType<T4>.TypeId
            ];

            public Excl() : base(_excludeComponentTypeIds) { }
        }

        public EntityFilterWithCache()
            : base(_includeComponentTypeIds, [])
        { }

        private EntityFilterWithCache(int[] excludeComponentTypeIds)
            : base(_includeComponentTypeIds, excludeComponentTypeIds)
        { }

        internal override void Initialize(EntityManager entityManager)
        {
            base.Initialize(entityManager);
            _t1Pool = Owner.GetComponentPool<T1>();
            _t2Pool = Owner.GetComponentPool<T2>();
        }

        public ref T1 GetC1(int index)
        {
            return ref _t1Pool.GetRef(GetComponentId(0, index));
        }

        public ref T2 GetC2(int index)
        {
            return ref _t2Pool.GetRef(GetComponentId(1, index));
        }

    }

}

