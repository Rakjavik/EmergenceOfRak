using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace rak.ecs.ThingComponents
{
    /*public class TurnSystem : JobComponentSystem
    {
        // JOB //
        struct TurnJob : IJobProcessComponentData<Position,Rotation,Target,Speed>
        {
            public float delta;

            public void Execute(ref Position currentPosition, ref Rotation currentRot, ref Target target, ref Speed turnSpeed)
            {
                Quaternion newRotation;
                Vector3 _direction = target.Position - currentPosition.Value;
                _direction = _direction.normalized;
                if (_direction != Vector3.zero)
                {
                    Quaternion _lookRotation = Quaternion.LookRotation(_direction);
                    Quaternion currentRotation = new Quaternion(currentRot.Value.x, currentRot.Value.y, currentRot.Value.z,
                        currentRot.Value.w);
                    newRotation = Quaternion.Slerp(currentRotation, _lookRotation,
                        turnSpeed.Value);
                    currentRot.Value = new float4(newRotation.x,newRotation.y,newRotation.z,newRotation.w);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            TurnJob job = new TurnJob
            {
                delta = Time.deltaTime
            };
            JobHandle handle = job.Schedule(this,inputDeps);
            return handle;
        }
    }*/
}
