using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public struct EngineRotationTurning : IComponentData
    {
        public quaternion RotationUpdate; // Requested rotation to update transform to
        public float RotationSpeed; // Speed modifier for slerp between current and dest rotation
    }

    public class EngineRotationTurningSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            //Enabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EngineRotationTurningJob job = new EngineRotationTurningJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        struct EngineRotationTurningJob : IJobForEach<EngineRotationTurning, Rotation,Target,Agent,Position>
        {
            public float delta;

            public void Execute(ref EngineRotationTurning ert, ref Rotation rot, ref Target target, ref Agent agent,
                ref Position pos)
            {
                // Disabled turning if we're avoiding obstacles //
                if (agent.DistanceFromFirstZHit <= 3)
                    return;
                float3 direction = (target.targetPosition - pos.Value);
                if (direction.Equals(float3.zero)) return;
                Quaternion lookRotation = Quaternion.LookRotation(direction,Vector3.up);
                Quaternion currentRot = new Quaternion(rot.Value.value.x, rot.Value.value.y, rot.Value.value.z,
                    rot.Value.value.w);
                Quaternion newRotation = Quaternion.Slerp(currentRot, lookRotation, ert.RotationSpeed * delta);
                ert.RotationUpdate = new float4(newRotation.x, newRotation.y, newRotation.z, newRotation.w);
            }
        }
    }
}
