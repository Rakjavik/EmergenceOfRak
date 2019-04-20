using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public class EngineSoundSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EngineSoundJob job = new EngineSoundJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        [BurstCompile]
        struct EngineSoundJob : IJobForEach<AgentVariables, EngineSound>
        {
            public float delta;

            public void Execute(ref AgentVariables av, ref EngineSound es)
            {
                es.TargetLevel = av.GetVelocityMagnitude() / 10;
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
