using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using rak.world;
using Unity.Burst;

namespace rak.ecs.ThingComponents
{
    public class AgeSystem : JobComponentSystem
    {
        // JOB //
        [BurstCompile]
        struct AgeJob : IJobForEach<Age,Enabled>
        {
            public float delta;

            public void Execute(ref Age age,ref Enabled enabled)
            {
                if (enabled.Value == 1)
                {
                    age.Value += delta;
                    if (age.Value >= age.MaxAge)
                        enabled.Value = 0;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            AgeJob job = new AgeJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this,inputDeps);
        }
    }

    public class ProducesSystem : JobComponentSystem
    {
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
