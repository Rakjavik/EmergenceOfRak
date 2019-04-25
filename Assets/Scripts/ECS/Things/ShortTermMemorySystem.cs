using rak.creatures.memory;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public struct ShortTermMemory : IComponentData
    {
        public DynamicBuffer<CreatureMemoryBuf> memoryBuffer;
        public int CurrentMemoryIndex;
        public int MaxShortTermMemories;
    }

    public class ShortTermMemorySystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate(GetEntityQuery(new EntityQueryDesc[] { new EntityQueryDesc {
                Any = new ComponentType[]{typeof(Observe)}
            } }));
            Enabled = true;
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ShortTermMemoryJob job = new ShortTermMemoryJob
            {
                memoryBuffers = GetBufferFromEntity<CreatureMemoryBuf>(false),
                observeBuffers = GetBufferFromEntity<ObserveBuffer>(true)
            };
            return job.Schedule(this, inputDeps);
        }

        struct ShortTermMemoryJob : IJobForEachWithEntity<ShortTermMemory, Observe>
        {
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<CreatureMemoryBuf> memoryBuffers;
            [NativeDisableParallelForRestriction]
            [ReadOnly]
            public BufferFromEntity<ObserveBuffer> observeBuffers;

            public void Execute(Entity creatureEntity, int index, ref ShortTermMemory stm, ref Observe observe)
            {
                if(observe.ObservationAvailable == 1)
                {
                    DynamicBuffer<CreatureMemoryBuf> creatureMemory = memoryBuffers[creatureEntity];
                    DynamicBuffer<ObserveBuffer> observeMemory = observeBuffers[creatureEntity];
                    if (!observeMemory.IsCreated)
                    {
                        return;
                    }
                    int creatureBufferSize = creatureMemory.Length;
                    int observeBufferSize = observeMemory.Length;
                    NativeArray<CreatureMemoryBuf> creatureMemArray = creatureMemory.AsNativeArray();
                    for(int count = 0; count < observeBufferSize; count++)
                    {
                        // Current Memory //
                        MemoryInstance memoryInstance = observe.memoryBuffer[count].memory;
                        if (searchTheseMemoriesFor(ref creatureMemArray, ref memoryInstance) == 0)
                        {
                            creatureMemory[stm.CurrentMemoryIndex].memory.SetNewMemory(memoryInstance);
                            stm.CurrentMemoryIndex++;
                            if (stm.CurrentMemoryIndex >= stm.MaxShortTermMemories)
                                stm.CurrentMemoryIndex = 0;
                        }
                    }
                    stm.memoryBuffer = creatureMemory;
                    observe.ObservationAvailable = 0;
                }
            }

            private byte searchTheseMemoriesFor(ref NativeArray<CreatureMemoryBuf> memories, ref MemoryInstance findThis)
            {
                for (int count = 0; count < memories.Length; count++)
                {
                    if (memories[count].memory.verb.Equals(findThis.verb) && 
                        memories[count].memory.subject.Equals(findThis.subject) &&
                        memories[count].memory.invertVerb == findThis.invertVerb)
                    {
                        return 1;
                    }
                }
                return 0;
            }
        }
    }
}
