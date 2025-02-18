# LunarECS 🌙
_A lightweight Entity Component System implementation for .NET_



## Background

Whether you are a professional game developer or merely an aspiring hobbyist like me,
you probably haven't failed to notice the abundance of ECS advocates
online and have perhaps wondered what all the fuss is about.

I started looking around for a robust ECS library to use with MonoGame, but it became apparent
that many of the existing solutions were designed for much more complex games than I was trying
to create. Furthermore, I found that they would often deviate in some way
from my basic understanding of ECS, or come with a whole bunch of additional features
to increase performance in certain edge cases, or features to assist with some of the heavy lifting
involved when starting a new project, or features designed for enormous game worlds with
millions of entities.

> _Why are there so many different types of System?_

> _Why does it keep talking about Archetypes?_

> _Why are all these performance tweaks necessary for my little game?_

I just wanted a bare-bones implementation, an ECS MVP, a simple toolkit that 
I could use to experiment with this pattern...

So I wrote one.

LunarECS is *not* the best ECS in the world.  
It will never be the fastest, nor the most feature-rich, nor the most scalable.
It is my humble implementation aimed at hobbyists and hackers.  Maybe you can put it
into production use, but it's primarily for having fun, prototyping, picking apart the 
source code and maybe learning a thing or two from some of my design choices and my mistakes.

It is the first step on a journey to ECS mastery.

## Features

### Entity Component System 
- create, update and delete entity components
- subscribe to entity changes
- filter entities based on component type

### Tile Component System
- create, update and delete tile components
- get component data based on tile coordinates

## Getting started

Get the latest release or source code if you want to compile it yourself.

### Examples

#### Entities and filters

```csharp
using LunarECS.Entities;

class Program
{
    struct TransformComponent 
    {
        public Vector3 Position;
    }

    struct MovementComponent
    {
        public Vector3 Velocity;
    }

    static void Main(string[] args)
    {
        var ecs = new EntityManager();

        for(int i = 0; i < 100; i++)
        {
            var entity = ecs.CreateEntity();
            entity.Add<TransformComponent>();
            entity.Add<MovementComponent>();
        }

        var movers = ecs.Subscribe<TransformComponent, MovementComponent>();
        for(int i = 0; i < movers.Count; i++) 
        {
            ref var transform = ref movers.GetC1(i);
            ref var movement = ref movers.GetC2(i);

            transform.Position += movement.Velocity * deltaTime;
        }
    }
}

```



## Limitations

### Component type limit
The component types associated with each Entity and Tile instance are 
stored as bitwise flags in a  `ulong` value, with the first bit reserved for 
indicating whether or not the instance is live, resulting in a hard limit of
63 total components types.  This implemention was decided in the interests of
performance and reducing the complexity of some filtering operations.

### Entity filter type limit 
The entity filters have the following limitations:
- maximum of 4 "included" component types
- maximum of 1 "excluded" component type
- minimum of 1 "included" component type (i.e. no "excluded only" filters)


## Future Plans

### Component ID arrays

Currently the entity/tile data stores an array of live component IDs, 
using the component type ID as an index.  This was a decision made early in 
development to avoid premature optimisation and to help reduce the complexity of
some internal operations.  It trades memory for performance, which I decided was 
reasonable given that LunarECS is intended to be used by hobbyists and for
small projects only (although in reality it can support much more than that).

A better alternative for projects where this might become an issue would be to
store the component index as part of the array, for example:

``` 
[component1_typeId, component1_id, component2_typeId, component2_id ...]
```

### Tile Component System

I like the idea of applying ECS to a tile-based system, but I'm not sold on its advantages
unless serious consideration is given to the underlying data structures when
a large number of tiles (more than 10000) are required.

I have included a very minimal Tile Component System (TCS) for experimentation.  It only
supports 3D spaces at the moment, but I will probably include 2D support at a later date.

