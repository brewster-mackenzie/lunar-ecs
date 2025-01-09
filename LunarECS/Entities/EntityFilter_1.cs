using LunarECS.Collections;

namespace LunarECS.Entities
{

    public class EntityFilter<T1> : EntityFilter where T1 : struct
    {
        RecyclableIdCollection<T1> _t1Pool = null!;

        readonly static int[] _includeComponentTypeIds = [
            IndexedTypeGroup<EntityComponent>.IndexedType<T1>.TypeId
        ];

        public class Excl<T2> : EntityFilter<T1> where T2 : struct
        {
            static readonly int[] _excludeComponentTypeIds = [
                IndexedTypeGroup<EntityComponent>.IndexedType<T2>.TypeId
            ];

            public Excl() : base(_excludeComponentTypeIds)
            { }
        }


        public EntityFilter()
            : base(_includeComponentTypeIds, [])
        { }

        private EntityFilter(int[] excludeComponentTypeIds)
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

