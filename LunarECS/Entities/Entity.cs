using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarECS.Entities
{
    public struct Entity
    {
        public int Id { get; internal set; }

        internal int Gen;
        internal EntityManager Owner;

        internal Entity(EntityManager owner, int id)
        {
            Owner = owner;
            Id = id;
            Gen = 0;
        }
    }
}
