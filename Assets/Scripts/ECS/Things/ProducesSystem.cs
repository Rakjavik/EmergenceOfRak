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
    }

    public class ProducesSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;
        }

        struct ProducesJob : IJobForEach<Produces>
        {
            public float delta;

            public void Execute(ref Produces prod)
            {
                prod.timeSinceLastSpawn += delta;
                if(prod.timeSinceLastSpawn >= prod.spawnThingEvery)
                {
                    // CANT DO THIS FROM NON MAIN THREAD
                    //world.World.CurrentArea.addThingToWorld("fruit");
                    prod.timeSinceLastSpawn = 0;
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ProducesJob job = new ProducesJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }
    }
}
