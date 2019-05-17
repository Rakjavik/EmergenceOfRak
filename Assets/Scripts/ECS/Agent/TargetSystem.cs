using Unity.Entities;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace rak.ecs.ThingComponents
{
    public struct Target : IComponentData
    {
        public Entity targetEntity;
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
        struct TargetJob : IJobForEach<Target, Position>
        {
            public void Execute(ref Target target, ref Position pos)
            {
                target.distance = Vector3.Distance(pos.Value, target.targetPosition);
            }
        }
    }
}
