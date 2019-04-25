using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using rak.world;
using Unity.Burst;

namespace rak.ecs.ThingComponents
{
    public struct Age : IComponentData
    {
        public float Value;
        public float MaxAge;
    }

    public class AgeSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            //Enabled = false;
        }

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
}
