using System.Runtime.InteropServices;

namespace LunarECS.Entities
{
    internal struct EntityData
    {
        public int Id;
        public ulong Flags;
        public int[] Cids;
        public int CidSize;

        public int Gen { get; internal set; }
    }
}
