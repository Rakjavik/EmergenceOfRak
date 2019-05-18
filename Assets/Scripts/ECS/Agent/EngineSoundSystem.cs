using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public struct EngineSound : IComponentData
    {
        public float CurrentLevel;
        public float TargetLevel;
        public float ChangeSpeed;
    }

    public class EngineSoundSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EngineSoundJob job = new EngineSoundJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        [BurstCompile]
        struct EngineSoundJob : IJobForEach<Velocity, EngineSound>
        {
            public float delta;

            public void Execute(ref Velocity vel, ref EngineSound es)
            {
                es.TargetLevel = vel.GetVelocityMagnitude() / 10;
                if (es.CurrentLevel < es.TargetLevel)
                {
                    es.CurrentLevel += es.ChangeSpeed * delta;
                    if (es.CurrentLevel > es.TargetLevel)
                        es.CurrentLevel = es.TargetLevel;
                }
                else if (es.CurrentLevel > es.TargetLevel)
                {
                    es.CurrentLevel -= es.ChangeSpeed * delta;
                    if (es.CurrentLevel < es.TargetLevel)
                        es.CurrentLevel = es.TargetLevel;
                }
            }
        }
    }
}
