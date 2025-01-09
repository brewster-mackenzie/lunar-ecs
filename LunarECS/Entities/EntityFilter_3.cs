using LunarECS.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
//using System.Diagnostics.CodeAnalysis;
//using System.Text;


namespace LunarECS.Entities
{
    public class EntityFilter<T1, T2, T3> : EntityFilter
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        RecyclableIdCollection<T1> _t1Pool = null!;
        RecyclableIdCollection<T2> _t2Pool = null!;
        RecyclableIdCollection<T3> _t3Pool = null!;


        readonly static int[] _includeComponentTypeIds = [
            IndexedTypeGroup<EntityComponent>.IndexedType<T1>.TypeId,
            IndexedTypeGroup<EntityComponent>.IndexedType<T2>.TypeId,
            IndexedTypeGroup<EntityComponent>.IndexedType<T3>.TypeId
        ];

        public class Excl<T4> : EntityFilter<T1, T2, T3> where T4 : struct
        {
            static readonly int[] _excludeComponentTypeIds = [
                IndexedTypeGroup<EntityComponent>.IndexedType<T4>.TypeId
            ];


            public Excl() : base(_excludeComponentTypeIds)
            { }
        }

        public EntityFilter()
            : base(_includeComponentTypeIds, Array.Empty<int>())
        { }

        private EntityFilter(int[] excludeComponentTypeIds)
            : base(_includeComponentTypeIds, excludeComponentTypeIds)
        { }

        internal override void Initialize(EntityManager entityManager)
        {
            base.Initialize(entityManager);
            _t1Pool = Owner.GetComponentPool<T1>();
            _t2Pool = Owner.GetComponentPool<T2>();
            _t3Pool = Owner.GetComponentPool<T3>();
        }

        public ref T1 GetC1(int index)
        {
            return ref _t1Pool.GetRef(GetComponentId(0, index));
        }

        public ref T2 GetC2(int index)
        {
            return ref _t2Pool.GetRef(GetComponentId(1, index));
        }

        public ref T3 GetC3(int index)
        {
            return ref _t3Pool.GetRef(GetComponentId(2, index));
        }
    }
}

