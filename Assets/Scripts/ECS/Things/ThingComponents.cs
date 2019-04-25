using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using rak.creatures.memory;
using rak.creatures;

namespace rak.ecs.ThingComponents
{
    public struct Enabled : IComponentData
    {
        public int Value;
    }

    public struct EngineConstantForce : IComponentData
    {
        public float3 CurrentForce;
    }

    public struct CreatureAI : IComponentData
    {
        public ActionStep.Actions CurrentAction;
        public ActionStep.FailReason FailReason;
        public Tasks.CreatureTasks CurrentTask;
        public Tasks.TASK_STATUS CurrentStatus;
        public byte DestinationSet;
        public float ElapsedTime;
        public float MaxAllowedTime;
        public float DistanceForCompletion;
        public Thing.Base_Types TargetBaseType;
        public ConsumptionType ConsumptionType;
    }

    // USED FOR MONO TO WRITE TO ECS VARIABLES FOR HYBRID //
    public struct AgentVariables : IComponentData
    {
        public float3 RelativeVelocity;
        public float3 Position;
        public float3 Velocity;
        public float4 Rotation;
        public float3 AngularVelocity;
        public byte Visible;

        public float GetVelocityMagnitude()
        {
            return Mathf.Abs(Velocity.x) + Mathf.Abs(Velocity.y) + Mathf.Abs(Velocity.z);
        }
        public float GetAngularVelocityMag()
        {
            return Mathf.Abs(AngularVelocity.x) + Mathf.Abs(AngularVelocity.y) + Mathf.Abs(AngularVelocity.z);
        }
    }

    [InternalBufferCapacity(100)]
    public struct CreatureMemoryBuf : IBufferElementData
    {
        public MemoryInstance memory;
    }
}
