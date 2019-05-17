using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace rak.ecs.ThingComponents
{
    public struct Produces : IComponentData
    {
        public Thing.Thing_Types thingToProduce;
        public float spawnThingEvery;
        public float timeSinceLastSpawn;
        public byte ProductionAvailable;
    }

    public class ProducesSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ProducesJob job = new ProducesJob
            {
                delta = Time.deltaTime,
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }

        struct ProducesJob : IJobForEachWithEntity<Produces>
        {
            public float delta;

            public void Execute(Entity entity, int index, ref Produces prod)
            {
                prod.timeSinceLastSpawn += delta;
                if(prod.timeSinceLastSpawn >= prod.spawnThingEvery)
                {
                    prod.ProductionAvailable = 1;
                }
            }
        }
    }
}
