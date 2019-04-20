using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public class TargetSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            TargetJob job = new TargetJob { };
            return job.Schedule(this, inputDeps);
        }

        [BurstCompile]
        struct TargetJob : IJobForEach<Target, AgentVariables>
        {
            public void Execute(ref Target target, ref AgentVariables av)
            {
                target.distance = Vector3.Distance(av.Position, target.targetPosition);
            }
        }
    }
}
