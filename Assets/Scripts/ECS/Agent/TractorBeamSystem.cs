using Unity.Entities;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;

namespace rak.ecs.ThingComponents
{
    public struct TractorBeam : IComponentData
    {
        public float BeamStrength;
        public byte Locked;
        public float3 NewTargetPosition;
        public float DistanceFromTarget;
    }

    public class TractorBeamSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            TractorBeamJob job = new TractorBeamJob
            {
                delta = Time.deltaTime,
            };
            return job.Schedule(this, inputDeps);
        }

        struct TractorBeamJob : IJobForEach<TractorBeam, Target,CreatureAI,AgentVariables,Position>
        {
            public float delta;

            public void Execute(ref TractorBeam tb, ref Target target,ref CreatureAI ai,ref AgentVariables av, ref Position pos)
            {
                if (ai.CurrentAction == ActionStep.Actions.Add)
                {
                    if (tb.NewTargetPosition.Equals(float3.zero))
                    {
                        tb.NewTargetPosition = target.targetPosition;
                    }
                    if (!target.targetEntity.Equals(Entity.Null))
                    {
                        // Need to lock //
                        if (tb.Locked == 0)
                        {
                            tb.Locked = 1;
                            target.NeedTargetPositionRefresh = 1;
                        }
                    }
                    // Target invalid //
                    else
                    {
                        if (tb.Locked == 1)
                            tb.Locked = 0;
                    }
                    if (tb.Locked == 1)
                    {
                        tb.DistanceFromTarget = Vector3.Distance(target.targetPosition, pos.Value);
                        tb.NewTargetPosition = Vector3.MoveTowards(tb.NewTargetPosition, pos.Value,
                            tb.BeamStrength * delta);
                    }
                }
                else
                    tb.NewTargetPosition = float3.zero;
            }
        }
    }
}
