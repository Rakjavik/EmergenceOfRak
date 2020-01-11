using Unity.Entities;
using rak.world;
using UnityEngine;
using Unity.Collections;
using rak.creatures.memory;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Diagnostics;

namespace rak.ecs.ThingComponents
{
    public struct Observe : IComponentData
    {
        public float ObserveDistance;
        public byte RequestObservation;
        public byte ObservationAvailable;
        public int NumberOfObservations;
    }
    public struct Observable : IComponentData
    {
        public Thing.Base_Types BaseType;
        public float Mass;
    }

    [InternalBufferCapacity(20)]
    public struct ObserveBuffer : IBufferElementData
    {
        public MemoryInstance memory;
    }

    public class ObserveSystem : JobComponentSystem
    {
        [ReadOnly]
        [DeallocateOnJobCompletion]
        private NativeArray<Position> positions;
        [ReadOnly]
        [DeallocateOnJobCompletion]
        private NativeArray<Entity> entities;

        [DeallocateOnJobCompletion]
        private EntityQuery query;


        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            query = EntityManager.CreateEntityQuery(new ComponentType[] { typeof(Observable),typeof(Position) });
            positions = query.ToComponentDataArray<Position>(Allocator.TempJob);
            entities = query.ToEntityArray(Allocator.TempJob);
            ObserveJob job = new ObserveJob
            {
                TimeStamp = Time.time,
                creatureMemBuffers = GetBufferFromEntity<CreatureMemoryBuf>(false),
                positions = positions,
                entities = entities,
                origins = GetComponentDataFromEntity<Position>(true),
                observables = GetComponentDataFromEntity<Observable>(true),
                ObservableThingsLength = positions.Length,
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
        [BurstCompile]
        struct ObserveJob : IJobForEachWithEntity<Observe,Visible,CreatureAI,ShortTermMemory>
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Position> positions;

            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<Entity> entities;

            [ReadOnly]
            public ComponentDataFromEntity<Observable> observables;

            [ReadOnly]
            public ComponentDataFromEntity<Position> origins;


            [NativeDisableParallelForRestriction]
            public BufferFromEntity<CreatureMemoryBuf> creatureMemBuffers;

            public float TimeStamp;
            public int ObservableThingsLength;

            public void Execute(Entity entity, int index, ref Observe ob,ref Visible av, ref CreatureAI ai,
               ref ShortTermMemory stm)
            {
                // Only run if observation was requested, and we won't already have an observation waiting to be read //
                if (ob.RequestObservation == 1)
                {
                    if (stm.Allocated == 0)
                    {
                        DynamicBuffer<CreatureMemoryBuf> creatureMemory = creatureMemBuffers[entity];
                        for (int count = 0; count < stm.MaxShortTermMemories; count++)
                        {
                            creatureMemory.Add(new CreatureMemoryBuf { memory = MemoryInstance.Empty });
                        }
                        stm.Allocated = 1;
                    }
                    // Start from a clean slate //
                    //DynamicBuffer<ObserveBuffer> buffer = observeBuffers[entity];
                    //buffer.Clear();
                    NativeArray<MemoryInstance> withinDistance = new NativeArray<MemoryInstance>(ObservableThingsLength, Allocator.Temp);
                    int withinDistanceCount = 0;
                    for (int count = 0; count < ObservableThingsLength; count++)
                    {
                        float distance = Vector3.Distance(origins[entity].Value, positions[count].Value);
                        if (distance <= ob.ObserveDistance && distance > 0)
                        {
                            MemoryInstance memory = new MemoryInstance
                            {
                                Verb = Verb.SAW,
                                Subject = entities[count],
                                InvertVerb = 0,
                                TimeStamp = TimeStamp,
                                SubjectType = observables[entities[count]].BaseType,
                                Position = positions[count].Value,
                                SubjectMass = observables[entities[count]].Mass,
                            };
                            memory.RefreshEdible(ai.ConsumptionType);
                            withinDistance[withinDistanceCount] = memory;
                            withinDistanceCount++;
                            /*buffer.Add(new ObserveBuffer
                            {
                                memory = memory
                            });*/
                        }
                    }
                    DynamicBuffer<CreatureMemoryBuf> ctmBuffer = creatureMemBuffers[entity];
                    NativeArray<CreatureMemoryBuf> ctmBufferArray = ctmBuffer.ToNativeArray(Allocator.Temp);
                    for (int count = 0; count < withinDistanceCount; count++)
                    { 
                        MemoryInstance searchForThis = withinDistance[count];
                        byte result = searchTheseMemoriesFor(ref ctmBufferArray, ref searchForThis);
                        // Memory not present //
                        if(result == 0)
                        {
                            searchForThis.RefreshEdible(ai.ConsumptionType);
                            ctmBuffer[stm.CurrentMemoryIndex] = new CreatureMemoryBuf
                            {
                                memory = searchForThis
                            };
                            stm.CurrentMemoryIndex++;
                            if(stm.CurrentMemoryIndex >= stm.MaxShortTermMemories)
                            {
                                stm.CurrentMemoryIndex = 0;
                            }
                        }
                    }

                    /*ob.memoryBuffer = buffer;
                    if(ObservableThingsLength > 0)
                        ob.ObservationAvailable = 1;*/
                    ob.RequestObservation = 0;
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
    }
}
