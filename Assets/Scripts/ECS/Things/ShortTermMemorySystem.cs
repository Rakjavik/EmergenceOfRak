using rak.creatures.memory;
using System.Diagnostics;
using Unity.Burst;
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
        public byte Allocated;
    }

    /*public class ShortTermMemorySystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem EndSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            EndSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            Enabled = false;
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ShortTermMemoryJob job = new ShortTermMemoryJob
            {
                memoryBuffers = GetBufferFromEntity<CreatureMemoryBuf>(false),
                observeBuffers = GetBufferFromEntity<ObserveBuffer>(true),
                commandBuffer = EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(handle);
            handle.Complete();
            long elapsed = sw.ElapsedMilliseconds;
            if (elapsed > 0)
                UnityEngine.Debug.Log("STM - " + elapsed);
            return handle;
        }

        struct ShortTermMemoryJob : IJobForEachWithEntity<ShortTermMemory, Observe,CreatureAI>
        {
            public EntityCommandBuffer.Concurrent commandBuffer;
            
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<CreatureMemoryBuf> memoryBuffers;

            [ReadOnly]
            public BufferFromEntity<ObserveBuffer> observeBuffers;

            public void Execute(Entity creatureEntity, int index, ref ShortTermMemory stm, ref Observe observe,
                ref CreatureAI ai)
            {
                DynamicBuffer<CreatureMemoryBuf> creatureMemory = memoryBuffers[creatureEntity];

                if (stm.Allocated == 0)
                {
                    for(int count = 0; count < stm.MaxShortTermMemories; count++)
                    {
                        creatureMemory.Add(new CreatureMemoryBuf { memory = MemoryInstance.Empty });
                    }
                    stm.Allocated = 1;
                }
                if(observe.ObservationAvailable == 1)
                {
                    DynamicBuffer<ObserveBuffer> observeMemory = observeBuffers[creatureEntity];
                    int creatureBufferSize = creatureMemory.Length;
                    int observeBufferSize = observeMemory.Length;
                    if (observeBufferSize == 0) 
                    {
                        observe.ObservationAvailable = 0;
                        commandBuffer.RemoveComponent(index, creatureEntity, typeof(PerformObservation));
                        return;
                    }
                    NativeArray<CreatureMemoryBuf> creatureMemArray = creatureMemory.AsNativeArray();
                    for(int count = 0; count < observeBufferSize; count++)
                    {
                        // Current Memory //
                        MemoryInstance memoryInstance = observe.memoryBuffer[count].memory;
                        if (searchTheseMemoriesFor(ref creatureMemArray, ref memoryInstance) == 0)
                        {
                            memoryInstance.RefreshEdible(ai.ConsumptionType);
                            creatureMemory[stm.CurrentMemoryIndex] = new CreatureMemoryBuf { memory = memoryInstance };
                            stm.CurrentMemoryIndex++;
                            if (stm.CurrentMemoryIndex >= stm.MaxShortTermMemories)
                                stm.CurrentMemoryIndex = 0;
                        }
                        else
                        {
                            creatureMemory[stm.CurrentMemoryIndex].memory.AddIteration();
                        }
                    }
                    stm.memoryBuffer = creatureMemory;
                    observe.ObservationAvailable = 0;
                    commandBuffer.RemoveComponent(index, creatureEntity, typeof(PerformObservation));
                }
            }

            private byte searchTheseMemoriesFor(ref NativeArray<CreatureMemoryBuf> memories, ref MemoryInstance findThis)
            {
                for (int count = 0; count < memories.Length; count++)
                {
                    if (memories[count].memory.Verb == findThis.Verb && 
                        memories[count].memory.Subject == findThis.Subject &&
                        memories[count].memory.InvertVerb == findThis.InvertVerb)
                    {
                        return 1;
                    }
                }
                return 0;
            }
        }
    }*/
}
