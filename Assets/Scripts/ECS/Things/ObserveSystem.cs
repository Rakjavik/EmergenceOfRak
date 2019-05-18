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
                observeBuffers = GetBufferFromEntity<ObserveBuffer>(false),
                positions = positions,
                entities = entities,
                origins = GetComponentDataFromEntity<Position>(true),
                observables = GetComponentDataFromEntity<Observable>(true),
                ObservableThingsLength = positions.Length,
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }

        struct ObserveJob : IJobForEachWithEntity<Observe,AgentVariables,CreatureAI>
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
            public BufferFromEntity<ObserveBuffer> observeBuffers;
            

            public float TimeStamp;
            public int ObservableThingsLength;

            public void Execute(Entity entity, int index, ref Observe ob,ref AgentVariables av, ref CreatureAI ai)
            {
                // Only run if observation was requested, and we won't already have an observation waiting to be read //
                if (ob.RequestObservation == 1 && ob.ObservationAvailable == 0)
                {
                    // Start from a clean slate //
                    DynamicBuffer<ObserveBuffer> buffer = observeBuffers[entity];
                    buffer.Clear();

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
                            buffer.Add(new ObserveBuffer
                            {
                                memory = memory
                            });
                        }
                    }
                    ob.memoryBuffer = buffer;
                    ob.ObservationAvailable = 1;
                    ob.RequestObservation = 0;
                }
            }
        }
    }
}
