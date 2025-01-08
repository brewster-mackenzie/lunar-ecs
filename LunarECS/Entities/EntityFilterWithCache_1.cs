using LunarECS.Collections;

namespace LunarECS.Entities
{

    public class EntityFilterWithCache<T1> : EntityFilterWithCache where T1 : struct
    {
        RecyclableIdCollection<T1> _t1Pool = null!;

        readonly static int[] _includeComponentTypeIds = [
            IndexedTypeGroup<EntityComponent>.IndexedType<T1>.TypeId
        ];

        public class Excl<T2> : EntityFilterWithCache<T1> where T2 : struct
        {
            static readonly int[] _excludeComponentTypeIds = [
                IndexedTypeGroup<EntityComponent>.IndexedType<T2>.TypeId
            ];

            public Excl() : base(_excludeComponentTypeIds)
            { }
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
        }

        public ref T1 GetC1(int index)
        {
            return ref _t1Pool.GetRef(GetComponentId(0, index));
        }
    }
}

