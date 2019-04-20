using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public class EngineRotationTurningSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EngineRotationTurningJob job = new EngineRotationTurningJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        struct EngineRotationTurningJob : IJobForEach<EngineRotationTurning, AgentVariables,Target>
        {
            public float delta;

            public void Execute(ref EngineRotationTurning ert, ref AgentVariables av, ref Target target)
            {
                float3 direction = (target.targetPosition - av.Position);
                Quaternion lookRotation = Quaternion.LookRotation(direction,Vector3.up);
                Quaternion currentRot = new Quaternion(av.Rotation.x, av.Rotation.y, av.Rotation.z, av.Rotation.w);
                Quaternion newRotation = Quaternion.Slerp(currentRot, lookRotation, ert.RotationSpeed * delta);
                ert.RotationUpdate = new float4(newRotation.x, newRotation.y, newRotation.z, newRotation.w);
            }
        }
    }
}
