using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using System;
using Unity.Collections;

namespace rak.ecs.AgentComponents
{
    public struct Position : IComponentData
    {
        public float3 Value;
    }
    public struct Velocity : IComponentData
    {
        public float3 Value;
    }
    public struct Rotation : IComponentData
    {
        public float4 Value;
    }
    public struct Target : IComponentData
    {
        public float3 Position;
        public System.Guid guid;
    }
    public struct Enabled : IComponentData
    {
        public int Value;
    }
    public struct Speed : IComponentData
    {
        public float Value;
    }
    public struct Age : IComponentData
    {
        public float Value;
        public float MaxAge;
    }

    public class AgeSystem : JobComponentSystem
    {
        // JOB //
        struct AgeJob : IJobProcessComponentData<Age,Enabled>
        {
            public float delta;

            public void Execute(ref Age age,ref Enabled enabled)
            {
                if (enabled.Value == 1)
                {
                    age.Value += delta;
                    if (age.Value >= age.MaxAge)
                        enabled.Value = 0;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            AgeJob job = new AgeJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this,inputDeps);
        }
    }

    public class TurnSystem : JobComponentSystem
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
    }
}
