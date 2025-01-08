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
    public class EntityFilterWithCache<T1, T2, T3, T4> : EntityFilterWithCache
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        RecyclableIdCollection<T1> _t1Pool = null!;
        RecyclableIdCollection<T2> _t2Pool = null!;
        RecyclableIdCollection<T3> _t3Pool = null!;
        RecyclableIdCollection<T4> _t4Pool = null!;


        readonly static int[] _includeComponentTypeIds = new int[4]
        {
            IndexedTypeGroup<EntityComponent>.IndexedType<T1>.TypeId,
            IndexedTypeGroup<EntityComponent>.IndexedType<T2>.TypeId,
            IndexedTypeGroup<EntityComponent>.IndexedType<T3>.TypeId,
            IndexedTypeGroup<EntityComponent>.IndexedType<T4>.TypeId
        };

        public class Excl<T5> : EntityFilterWithCache<T1, T2, T3, T4> where T5 : struct
        {
            static readonly int[] _excludeComponentTypeIds = [
                IndexedTypeGroup<EntityComponent>.IndexedType<T5>.TypeId
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
            _t2Pool = Owner.GetComponentPool<T2>();
            _t3Pool = Owner.GetComponentPool<T3>();
            _t4Pool = Owner.GetComponentPool<T4>();
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

        public ref T4 GetC4(int index)
        {
            return ref _t4Pool.GetRef(GetComponentId(3, index));
        }
    }
}

