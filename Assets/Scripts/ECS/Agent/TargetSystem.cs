using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace rak.ecs.ThingComponents
{
    public struct Target : IComponentData
    {
        public System.Guid targetGuid;
        public float3 targetPosition;
        public float distance;
        public byte NeedTargetPositionRefresh;
    }

    public class TargetSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            TargetJob job = new TargetJob { };
            return job.Schedule(this, inputDeps);
        }

        //[BurstCompile]
        struct TargetJob : IJobForEach<Target, AgentVariables>
        {
            public void Execute(ref Target target, ref AgentVariables av)
            {
                target.distance = Vector3.Distance(av.Position, target.targetPosition);
            }
        }
    }
}
