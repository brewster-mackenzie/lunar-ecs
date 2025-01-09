using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Text;


namespace LunarECS.Entities
{
    public abstract class EntityFilter : EntitySubscriber
    {
        int[][] _componentIds;

        public EntityFilter(int[] includeComponentTypeIds, int[] excludeComponentTypeIds)
            : base(includeComponentTypeIds, excludeComponentTypeIds)
        {
            _componentIds = new int[Capacity][];
        }

        protected int GetComponentId(int typeIndex, int recordIndex)
        {
            return _componentIds[recordIndex][typeIndex];
        }

        internal override void OnDelete()
        {
            _componentIds = null!;
        }

        internal override void OnExclude(int index, int swapIndex, ref EntityData entity)
        {
            _componentIds[index] = _componentIds[swapIndex];
        }

        internal override void OnInclude(int index, ref EntityData entity)
        {
            _componentIds[index] = new int[IncludeComponentTypeIdCount];
            for (int i = 0; i < IncludeComponentTypeIdCount; i++)
                _componentIds[index][i] = entity.Cids[IncludeComponentTypeIds[i]];
        }

        internal override void OnResize(int capacity)
        {
            Array.Resize(ref _componentIds, capacity);
        }
    }
}

