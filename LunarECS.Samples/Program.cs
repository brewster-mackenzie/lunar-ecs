using System.Diagnostics;
using LunarECS;
using LunarECS.Entities;

namespace LunarECS.Samples
{
    internal class Program
    {
        struct Component1 { }
        struct Component2 { }
        struct Component3 { }
        struct Component4 { }
        struct Component5 { }

        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            sw.Restart();
            RunFiltersSample_1(100);
            sw.Stop();
            Console.WriteLine("Filter Sample 1: {0}ms", sw.ElapsedMilliseconds);
        }

        static void RunFiltersSample_1(int entityCount)
        {
            var ecs = new EntityManager();

            for(int i = 0; i < entityCount; i++)
            {
                var entity = ecs.CreateEntity();

                int randomFlags = Random.Shared.Next(0, 1 << 5);

                if ((randomFlags & 1) > 0)
                    entity.Add<Component1>();
                if ((randomFlags & 1 << 1) > 0)
                    entity.Add<Component2>();
                if ((randomFlags & 1 << 2) > 0)
                    entity.Add<Component3>();
                if ((randomFlags & 1 << 3) > 0)
                    entity.Add<Component4>();
                if ((randomFlags & 1 << 4) > 0)
                    entity.Add<Component5>();
            }

            var filter1 = ecs.Subscribe<EntityFilter<Component1>>();
            var filter2 = ecs.Subscribe<EntityFilter<Component1, Component2>>();
            var filter3 = ecs.Subscribe<EntityFilter<Component1, Component2, Component3>>();
            var filter4 = ecs.Subscribe<EntityFilter<Component1, Component2, Component3, Component4>>();

            var filterEx1 = ecs.Subscribe<EntityFilter<Component1>.Excl<Component2>>();
            var filterEx2 = ecs.Subscribe<EntityFilter<Component1, Component2>.Excl<Component3>>();
            var filterEx3 = ecs.Subscribe<EntityFilter<Component1, Component2, Component3>.Excl<Component4>>();
            var filterEx4 = ecs.Subscribe<EntityFilter<Component1, Component2, Component3, Component4>.Excl<Component5>>();

            Console.WriteLine("Filter1 count: {0}", filter1.Count);
            Console.WriteLine("Filter2 count: {0}", filter2.Count);
            Console.WriteLine("Filter3 count: {0}", filter3.Count);
            Console.WriteLine("Filter4 count: {0}", filter4.Count);

            Console.WriteLine("FilterEx1 count: {0}", filterEx1.Count);
            Console.WriteLine("FilterEx2 count: {0}", filterEx2.Count);
            Console.WriteLine("FilterEx3 count: {0}", filterEx3.Count);
            Console.WriteLine("FilterEx4 count: {0}", filterEx4.Count);
        }
    }
}
