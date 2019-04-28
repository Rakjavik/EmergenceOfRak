using Unity.Entities;
using rak.world;
using UnityEngine;
using Unity.Collections;
using rak.creatures.memory;
using Unity.Jobs;
using Unity.Mathematics;

namespace rak.ecs.ThingComponents
{
    public struct Observe : IComponentData
    {
        public float ObserveDistance;
        public byte RequestObservation;
        public byte ObservationAvailable;
        public DynamicBuffer<ObserveBuffer> memoryBuffer;
        public int NumberOfObservations;
    }

    [InternalBufferCapacity(20)]
    public struct ObserveBuffer : IBufferElementData
    {
        public MemoryInstance memory;
    }

    public class ObserveSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ObserveJob job = new ObserveJob
            {
                TimeStamp = Time.time,
                observeBuffers = GetBufferFromEntity<ObserveBuffer>(false),
                ObservableThings = new NativeArray<ObservableThing>(Area.GetObservableThings(),Allocator.TempJob),
                ObservableThingsLength = Area.ObservableThingsEntriesFilled
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }

        struct ObserveJob : IJobForEachWithEntity<Observe,AgentVariables,CreatureAI>
        {
            //[ReadOnly]
            [NativeDisableParallelForRestriction]
            [DeallocateOnJobCompletion]
            public NativeArray<ObservableThing> ObservableThings;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<ObserveBuffer> observeBuffers;

            public float TimeStamp;
            public int ObservableThingsLength;

            public void Execute(Entity entity, int index, ref Observe ob,ref AgentVariables av, ref CreatureAI ai)
            {
                ob.RequestObservation = 1;
                // Only run if observation was requested, and we won't already have an observation waiting to be read //
                if (ob.RequestObservation == 1 && ob.ObservationAvailable == 0)
                {
                    // Start from a clean slate //
                    DynamicBuffer<ObserveBuffer> buffer = observeBuffers[entity];
                    buffer.Clear();
                    
                    float3 origin = av.Position;
                    for (int count = 0; count < ObservableThingsLength; count++)
                    {
                        float distance = Vector3.Distance(origin, ObservableThings[count].position);
                        if (distance <= ob.ObserveDistance && distance > 0)
                        {
                            buffer.Add(new ObserveBuffer
                            {
                                memory = new MemoryInstance(Verb.SAW, ObservableThings[count].guid, false, TimeStamp,
                                    ObservableThings[count].BaseType, ai.ConsumptionType,ObservableThings[count].position)
                            });
                        }
                    }
                    ob.memoryBuffer = buffer;
                    ob.ObservationAvailable = 1;
                }
            }
        }
    }
}
