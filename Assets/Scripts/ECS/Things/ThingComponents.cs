using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using rak.creatures.memory;
using rak.creatures;

namespace rak.ecs.ThingComponents
{
    public struct IsCreature : IComponentData { }

    public struct CreatureState : IComponentData
    {
        public Creature.CreatureState Value;
    }


    public struct Enabled : IComponentData
    {
        public int Value;
    }

    public struct EngineConstantForce : IComponentData
    {
        public float3 CurrentForce;
    }

    public struct Position : IComponentData
    {
        public float3 Value;
    }
    public struct Rotation : IComponentData
    {
        public quaternion Value;
    }

    public struct Velocity : IComponentData
    {
        public float3 RelativeVelocity;
        public float3 NormalVelocity;
        public float3 AngularVelocity;

        public float GetVelocityMagnitude()
        {
            return Mathf.Abs(NormalVelocity.x) + Mathf.Abs(NormalVelocity.y) + Mathf.Abs(NormalVelocity.z);
        }
        public float GetAngularVelocityMag()
        {
            return Mathf.Abs(AngularVelocity.x) + Mathf.Abs(AngularVelocity.y) + Mathf.Abs(AngularVelocity.z);
        }
    }

    public struct RelativeDirections : IComponentData
    {
        public float3 Forward;
        public float3 Right;
    }
    
    public struct Visible : IComponentData
    {
        public byte Value;
    }

    [InternalBufferCapacity(100)]
    public struct CreatureMemoryBuf : IBufferElementData
    {
        public MemoryInstance memory;
    }
}
