using rak.creatures.memory;
using rak.ecs.ThingComponents;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace rak.ecs
{
    /*public struct GridNeighbors
    {
        public Entity left;
        public Entity right;
        public Entity up;
        public Entity down;
        public Entity upLeft;
        public Entity upRight;
        public Entity downLeft;
        public Entity downRight;
    }

    public struct GridSector : IComponentData
    {
        // Static //
        public GridNeighbors Neighbors;
        public float2 Size;
        public float2 Position;

        // Dynamic //
        public DynamicBuffer<ObserveBuffer> Occupants;
    }

    [InternalBufferCapacity(100)]
    public struct EntityBuffer : IBufferElementData
    {
        public Entity entity;
    }

    public class GridSectorSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            float3[] entityPositions;
            GridSectorJob job = new GridSectorJob
            {
                observeBuffers = GetBufferFromEntity<ObserveBuffer>(true),
            };
            return job.Schedule(this, inputDeps);
        }

        struct GridSectorJob : IJobForEachWithEntity<GridSector>
        {
            public BufferFromEntity<ObserveBuffer> observeBuffers;

            public void Execute(Entity entity, int index, ref GridSector gs)
            {
                int numOfEntities = positions.Length;
                DynamicBuffer<EntityBuffer> buffer = entityBuffers[entity];
                for (int count = 0; count < numOfEntities; count++)
                {
                    bool movedLeft = false;
                    bool movedUp = false;
                    bool movedDown = false;
                    bool movedRight = false;
                    if (positions[count].x < gs.Position.x)
                        movedLeft = true;
                    else if (positions[count].x > gs.Position.x + gs.Size.x)
                        movedRight = true;
                    if (positions[count].z < gs.Position.y)
                        movedDown = true;
                    else if (positions[count].z > gs.Position.y + gs.Size.z)
                        movedUp = true;
                    bool removeEntity = true;
                    if (!movedDown && !movedLeft && !movedRight && !movedUp)
                    {
                        removeEntity = false;
                    }
                    if (movedDown && movedLeft)
                    {
                        gs.Neighbors.downLeft.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    else if (movedDown && !movedLeft && !movedRight)
                    {
                        gs.Neighbors.down.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    else if (movedDown && movedRight)
                    {
                        gs.Neighbors.downRight.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    else if (movedRight && !movedDown && !movedUp)
                    {
                        gs.Neighbors.right.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    else if (movedUp && movedRight)
                    {
                        gs.Neighbors.upRight.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    else if (movedUp && !movedRight && !movedLeft)
                    {
                        gs.Neighbors.up.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    else if (movedLeft && movedUp)
                    {
                        gs.Neighbors.upLeft.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    else if (movedLeft && !movedUp && !movedDown)
                    {
                        gs.Neighbors.left.Occupants.Add(new EntityBuffer { entity = gs.Occupants[count] });
                    }
                    if (removeEntity)
                        buffer.RemoveAt(count);
                }
            }
        }
    }*/
}