using Unity.Entities;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;

namespace rak.ecs.ThingComponents
{
    public class TractorBeamSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            TractorBeamJob job = new TractorBeamJob
            {
                delta = Time.deltaTime,
            };
            return job.Schedule(this, inputDeps);
        }

        struct TractorBeamJob : IJobForEach<TractorBeam, Target,CreatureAI,AgentVariables>
        {
            public float delta;

            public void Execute(ref TractorBeam tb, ref Target target,ref CreatureAI ai,ref AgentVariables av)
            {
                if (ai.CurrentAction == ActionStep.Actions.Add)
                {
                    if (tb.NewTargetPosition.Equals(float3.zero))
                    {
                        tb.NewTargetPosition = target.targetPosition;
                    }
                    if (!target.targetGuid.Equals(Guid.Empty))
                    {
                        // Need to lock //
                        if (tb.Locked == 0)
                        {
                            tb.Locked = 1;
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
                        tb.DistanceFromTarget = Vector3.Distance(target.targetPosition, av.Position);
                        tb.NewTargetPosition = Vector3.MoveTowards(tb.NewTargetPosition, av.Position,
                            tb.BeamStrength * delta);
                    }
                }
                else
                    tb.NewTargetPosition = float3.zero;
            }
        }
    }
}
