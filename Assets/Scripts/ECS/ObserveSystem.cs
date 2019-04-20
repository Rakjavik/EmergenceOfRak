using Unity.Entities;
using rak.world;
using UnityEngine;
using Unity.Collections;
using rak.creatures.memory;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public class ObserveSystem : JobComponentSystem
    {
        [DeallocateOnJobCompletion]
        private NativeArray<BlittableThing> allThings;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            allThings = new NativeArray<BlittableThing>(Area.GetBlittableThings(),Allocator.TempJob);
            ObserveJob job = new ObserveJob
            {
                TimeStamp = Time.time,
                AllThings = allThings,
                memoryBuffer = GetBufferFromEntity<MemoryBuffer>(false),
                AllThingsLength = Area.AllThingsCacheEntriesFilled
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
        struct ObserveJob : IJobForEachWithEntity<Observe,AgentVariables>
        {
            [ReadOnly][DeallocateOnJobCompletion]
            public NativeArray<BlittableThing> AllThings;
            [ReadOnly]
            public int AllThingsLength;

            [ReadOnly]
            public float TimeStamp;

            //[NativeDisableParallelForRestriction]
            [ReadOnly]
            public BufferFromEntity<MemoryBuffer> memoryBuffer;

            public void Execute(Entity entity, int index, ref Observe ob,ref AgentVariables av)
            {
                if (ob.RequestObservation == 1)
                {
                    DynamicBuffer<MemoryBuffer> buffer = memoryBuffer[entity];
                    for (int count = 0; count < AllThingsLength; count++)
                    {
                        BlittableThing thing = AllThings[count];
                        if (thing.IsEmpty())
                            return;
                        float distance = Vector3.Distance(av.Position, thing.position);
                        if (distance <= ob.ObserveDistance && distance > 0)
                        {
                            buffer.Add(new MemoryBuffer
                            {
                                memories = new MemoryInstance(Verb.SAW, thing.GetGuid(), false, TimeStamp)
                            });
                        }
                    }
                    ob.ObservationAvailable = 1;
                }
            }
        }
    }
}
